using __yky.MisoShadowController.Runtime;
using UnityEditor;

namespace __yky.MisoShadowController.Editor
{
    [CustomEditor(typeof(MisoShadowGenerate))]
    public class MisoShadowGenerateEditor : UnityEditor.Editor
    {
        private SerializedProperty _menuRoot;
        private SerializedProperty _enableByDefault;
        private SerializedProperty _useWriteDefaults;
        private SerializedProperty _savableShadowStrength;
        private SerializedProperty _savableShadowAngle;

        private static bool showAdvanced;

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
            EditorGUILayout.Separator();

            EditorGUILayout.LabelField(Utils.Label("Options"), Utils.BoldLabel2);

            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUILayout.VerticalScope())
                {
                    EditorGUILayout.PropertyField(_enableByDefault, Utils.Label("Enable by Default"));
                    EditorGUILayout.PropertyField(_useWriteDefaults, Utils.Label("Use Write Defaults"));
                    EditorGUILayout.PropertyField(_savableShadowStrength, Utils.Label("Savable Shadow Strength"));
                    EditorGUILayout.PropertyField(_savableShadowAngle, Utils.Label("Savable Shadow Angle"));

                    showAdvanced = EditorGUILayout.Foldout(showAdvanced, Utils.Label("Advanced"));

                    if (showAdvanced)
                        EditorGUILayout.PropertyField(_menuRoot, Utils.Label("Menu Root"));
                }
            }

            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        }
    }
}