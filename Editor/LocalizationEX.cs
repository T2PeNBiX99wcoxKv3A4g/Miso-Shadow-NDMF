using UnityEditor;
using UnityEngine;

// ReSharper disable UnusedMember.Global

namespace __yky.MisoShadowNDMF.Editor
{
    // ReSharper disable once InconsistentNaming
    public static class LocalizationEX
    {
        internal static string L(this string str) => Localization.L(str);
        internal static GUIContent G(this string str, string tooltip) => Localization.G(str, tooltip);
        internal static GUIContent G(this string str) => Localization.G(str, str + ".tooltip");
        internal static GUIContent G(this SerializedProperty property) => Localization.G(property);
    }
}