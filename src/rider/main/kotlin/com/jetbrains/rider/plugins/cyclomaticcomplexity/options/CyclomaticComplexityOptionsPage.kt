package com.jetbrains.rider.plugins.cyclomaticcomplexity.options

import com.jetbrains.rider.plugins.cyclomaticcomplexity.CyclomaticComplexityBundle
import com.jetbrains.rider.settings.simple.SimpleOptionsPage

class CyclomaticComplexityOptionsPage : SimpleOptionsPage(
    name = CyclomaticComplexityBundle.message("configurable.name.cyclomaticcomplexity.options.title"),
    pageId = "PowerToys.CyclomaticComplexity")
{
    override fun getId(): String {
        return "PowerToys.CyclomaticComplexity"
    }
}
