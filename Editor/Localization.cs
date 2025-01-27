using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace __yky.MisoShadowNDMF.Editor
{
    // Referenced from VRCEmoteManager and jp.lilxyzw.ndmfmeshsimplifier
    [InitializeOnLoad]
    internal class Localization
    {
        private const string LocalizationFolderGuid = "07d6c164912b916428a5b7bdffb6242f";
        private const string Ext = ".json";
        private const string TooltipExt = ".tooltip";
        private const string DisplayNameKey = "display_name";
        private const string LanguageLabelKey = "label.language";
        private const string DefaultLangKey = "en-US";
        private const string PrefsLangKey = "MisoShadowNDMF_Language";
        private const string Null = "--null--";
        private static string LocalizationFolder => AssetDatabase.GUIDToAssetPath(LocalizationFolderGuid);
        private static string[] _languageKeyList;
        private static string[] _languageKeyNames;

        private static readonly Dictionary<string, ImmutableSortedDictionary<string, string>>
            LanguageDictionary = new();

        private static readonly Dictionary<string, Dictionary<string, GUIContent>> GuiContents = new();

        private static string SelectedLanguage
        {
            get => EditorPrefs.GetString(PrefsLangKey, DefaultLangKey);
            set
            {
                if (LanguageDictionary.ContainsKey(value))
                    EditorPrefs.SetString(PrefsLangKey, value);
            }
        }

        // ReSharper disable once MemberCanBePrivate.Global
        internal static string L(string key)
        {
            return LanguageDictionary.TryGetValue(SelectedLanguage, out var contents)
                ? CollectionExtensions.GetValueOrDefault(contents, key, key)
                : key;
        }

        private static GUIContent G(string key) => G(key, null, "");
        private static GUIContent G(string[] key) => key.Length == 2 ? G(key[0], null, key[1]) : G(key[0], null, null);

        // ReSharper disable once MemberCanBePrivate.Global
        internal static GUIContent G(string key, string tooltip) => G(key, null, tooltip); // From EditorToolboxSettings
        private static GUIContent G(string key, Texture image) => G(key, image, "");
        internal static GUIContent G(SerializedProperty property) => G(property.name, $"{property.name}{TooltipExt}");

        private static GUIContent G(string key, Texture image, string tooltip)
        {
            if (GuiContents.TryGetValue(SelectedLanguage, out var contents))
            {
                if (contents.TryGetValue(key, out var content))
                    return content;
            }

            if (!GuiContents.ContainsKey(SelectedLanguage))
                GuiContents[SelectedLanguage] = new Dictionary<string, GUIContent>();
            return GuiContents[SelectedLanguage][key] = new GUIContent(L(key), image, L(tooltip));
        }

        internal static void SelectLanguageGUI()
        {
            EditorGUI.BeginChangeCheck();
            var newIndex = EditorGUILayout.Popup(G(LanguageLabelKey, LanguageLabelKey + TooltipExt),
                Array.IndexOf(_languageKeyList, SelectedLanguage),
                _languageKeyNames);
            if (EditorGUI.EndChangeCheck())
                SelectedLanguage = _languageKeyList[newIndex];
        }

        private static void Load()
        {
            var filePaths = Directory.GetFiles(LocalizationFolder);
            var langDisplayNames = new Dictionary<string, string>();
            var langKeyList = new List<string>();

            Debug.Log("Loading Test");

            foreach (var filePath in filePaths.Where(f => f.EndsWith(Ext)))
            {
                var lang = Path.GetFileNameWithoutExtension(filePath);
                var content = File.ReadAllText(filePath);
                var langauge = LanguageDictionary[lang] = JsonConvert
                    .DeserializeObject<Dictionary<string, string>>(content).ToImmutableSortedDictionary()
                    .WithComparers(StringComparer.OrdinalIgnoreCase);

                langDisplayNames.Add(lang, langauge[DisplayNameKey] ?? Null);
                langKeyList.Add(lang);
                Debug.Log($"Loading Localization file: {filePath} {lang}");
            }

            var languageDisplayNames = langDisplayNames.ToImmutableSortedDictionary()
                .WithComparers(StringComparer.OrdinalIgnoreCase);
            langDisplayNames.Clear();

            _languageKeyList = langKeyList.ToArray();
            _languageKeyNames = new string[_languageKeyList.Length];
            for (var i = 0; i < _languageKeyList.Length; i++)
                _languageKeyNames[i] = languageDisplayNames[_languageKeyList[i]];

            langKeyList.Clear();
        }

        static Localization() => Load();
    }
}