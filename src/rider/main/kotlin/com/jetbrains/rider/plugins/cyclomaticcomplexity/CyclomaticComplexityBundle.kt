package com.jetbrains.rider.plugins.cyclomaticcomplexity

import com.intellij.DynamicBundle
import org.jetbrains.annotations.Nls
import org.jetbrains.annotations.NonNls
import org.jetbrains.annotations.PropertyKey

class CyclomaticComplexityBundle : DynamicBundle(BUNDLE) {
    companion object {
        @NonNls
        private const val BUNDLE = "messages.CyclomaticComplexityBundle"
        private val INSTANCE: CyclomaticComplexityBundle = CyclomaticComplexityBundle()

        @Nls
        fun message(
            @PropertyKey(resourceBundle = BUNDLE) key: String,
            vararg params: Any
        ): String {
            return INSTANCE.getMessage(key, *params)
        }
    }
}
