using __yky.MisoShadowNDMF.Runtime;
using UnityEditor;

namespace __yky.MisoShadowNDMF.Editor
{
    [CustomEditor(typeof(MisoShadowIgnore))]
    public class MisoShadowIgnoreEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            Utils.ShowTitle();
            EditorGUILayout.HelpBox("label.ignore".L(), MessageType.Info, true);
            EditorGUILayout.Separator();
            Localization.SelectLanguageGUI();
        }
    }
}