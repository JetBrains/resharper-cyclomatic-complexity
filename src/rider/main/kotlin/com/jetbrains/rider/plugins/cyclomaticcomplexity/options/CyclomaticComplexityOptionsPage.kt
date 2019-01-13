package com.jetbrains.rider.plugins.cyclomaticcomplexity.options

import com.jetbrains.rider.settings.simple.SimpleOptionsPage

class CyclomaticComplexityOptionsPage : SimpleOptionsPage("Cyclomatic Complexity", "PowerToys.CyclomaticComplexity") {
    override fun getId(): String {
        return "PowerToys.CyclomaticComplexity"
    }
}