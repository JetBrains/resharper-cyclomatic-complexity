// /*
//  * Copyright 2007-2015 JetBrains
//  *
//  * Licensed under the Apache License, Version 2.0 (the "License");
//  * you may not use this file except in compliance with the License.
//  * You may obtain a copy of the License at
//  *
//  * http://www.apache.org/licenses/LICENSE-2.0
//  *
//  * Unless required by applicable law or agreed to in writing, software
//  * distributed under the License is distributed on an "AS IS" BASIS,
//  * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  * See the License for the specific language governing permissions and
//  * limitations under the License.
//  */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Xml.Linq;
using NuGet.Versioning;
using Nuke.Common.IO;
using Nuke.Core;

class VersionUpdater
{
  public static void UpdateToLatestVersion (string projectFile, string package)
  {
    XmlTasks.XmlPoke (
      projectFile,
      $"//PackageReference[@Include='{package}']/@Version",
      GetLatestVersion (package));
  }

  public static string GetLatestVersion (string package)
  {
    XNamespace dataservices = "http://schemas.microsoft.com/ado/2007/08/dataservices";
    XNamespace dataservicesMetadata = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";

    return GetNuGetPackageEntries($"https://www.nuget.org/api/v2/Packages?$filter=Id%20eq%20%27{package}%27")
        .Select(x => x.Element(dataservicesMetadata + "properties").NotNull())
        .Select(x => NuGetVersion.Parse(x.Element(dataservices + "Version").NotNull().Value))
        .OrderByDescending(x => x)
        .First().ToString();
  }

  static IEnumerable<XElement> GetNuGetPackageEntries (string url)
  {
    XNamespace atom = "http://www.w3.org/2005/Atom";

    var clientHandler = new HttpClientHandler { UseDefaultCredentials = true };

    using (var httpClient = new HttpClient(clientHandler))
    {
      while (true)
      {
        using (var response = httpClient.GetAsync(url).Result)
        {
          var content = response.Content.ReadAsStringAsync().Result;

          if (!response.IsSuccessStatusCode)
            throw new Exception($"Status: '{response.StatusCode}'. Resonse: {Environment.NewLine}{content}");

          var xml = XDocument.Parse(content);
          var feedElement = xml.Element(atom + "feed");

          foreach (var element in feedElement.NotNull().Elements(atom + "entry"))
            yield return element;

          var nextLink = feedElement.Elements(atom + "link").SingleOrDefault(x => x.Attribute("rel").NotNull().Value == "next");
          if (nextLink == null)
            yield break;

          url = nextLink.Attribute("href").NotNull().Value;
        }
      }
    }
  }
}