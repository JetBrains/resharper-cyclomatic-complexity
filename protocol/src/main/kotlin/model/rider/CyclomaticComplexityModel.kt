package model.rider

import com.jetbrains.rider.model.nova.ide.SolutionModel
import com.jetbrains.rider.generator.nova.*
import com.jetbrains.rider.generator.nova.PredefinedType.*
import com.jetbrains.rider.generator.nova.csharp.CSharp50Generator
import com.jetbrains.rider.generator.nova.kotlin.Kotlin11Generator

@Suppress("unused")
object CyclomaticComplexityModel : Ext(SolutionModel.Solution) {

    val MyEnum = enum {
        +"FirstValue"
        +"SecondValue"
    }

    val MyStructure = structdef {
        field("projectFile", string)
        field("target", string)
    }

    init {
        //setting(CSharp50Generator.Namespace, "ReSharperPlugin.CyclomaticComplexity.Rider.Model")
        //setting(Kotlin11Generator.Namespace, "com.jetbrains.rider.cyclomaticcomplexity.model")

        property("myString", string)
        property("myBool", bool)
        property("myEnum", MyEnum.nullable)

        map("data", string, string)

        signal("myStructure", MyStructure)
    }
}