using __yky.MisoShadowNDMF.Runtime;
using UnityEditor;

namespace __yky.MisoShadowNDMF.Editor
{
    [CustomEditor(typeof(MisoShadowGenerate))]
    internal class MisoShadowGenerateEditor : UnityEditor.Editor
    {
        private SerializedProperty _menuRoot;
        private SerializedProperty _enableByDefault;
        private SerializedProperty _useWriteDefaults;
        private SerializedProperty _savableShadowStrength;
        private SerializedProperty _savableShadowAngle;

        private static bool _showDevs;

        private void OnEnable()
        {
            _menuRoot = serializedObject.FindProperty(nameof(MisoShadowGenerate.menuRoot));
            _enableByDefault = serializedObject.FindProperty(nameof(MisoShadowGenerate.enableByDefault));
            _useWriteDefaults = serializedObject.FindProperty(nameof(MisoShadowGenerate.useWriteDefaults));
            _savableShadowStrength = serializedObject.FindProperty(nameof(MisoShadowGenerate.savableShadowStrength));
            _savableShadowAngle = serializedObject.FindProperty(nameof(MisoShadowGenerate.savableShadowAngle));
        }

        public override void OnInspectorGUI()
        {
            Utils.ShowTitle();
            EditorGUILayout.LabelField("label.setting".G(), Utils.BoldLabel2);
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUILayout.VerticalScope())
                {
                    EditorGUILayout.PropertyField(_enableByDefault, "label.setting.enable_by_default".G());
                    EditorGUILayout.PropertyField(_useWriteDefaults, "label.setting.use_write_defaults".G());
                    EditorGUILayout.PropertyField(_savableShadowStrength, "label.setting.savable_shadow_strength".G());
                    EditorGUILayout.PropertyField(_savableShadowAngle, "label.setting.savable_shadow_angle".G());

                    _showDevs = EditorGUILayout.Foldout(_showDevs, "label.dev".G());

                    if (_showDevs)
                        EditorGUILayout.PropertyField(_menuRoot, "label.dev.menu_root".G());
                }
            }

            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
            
            Localization.SelectLanguageGUI();
        }
    }
}