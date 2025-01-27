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
            EditorGUILayout.LabelField("label.ignore".G(), Utils.BoldLabel2);
            EditorGUILayout.Separator();
            Localization.SelectLanguageGUI();
        }
    }
}