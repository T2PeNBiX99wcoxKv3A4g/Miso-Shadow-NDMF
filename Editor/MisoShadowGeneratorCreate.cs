using __yky.MisoShadowNDMF.Runtime;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace __yky.MisoShadowNDMF.Editor
{
    internal class MisoShadowGeneratorCreate : EditorWindow
    {
        private const string MenuPath = "GameObject/Miso Shadow/Add Shadow (NDMF)";
        private const string MenuPath2 = "GameObject/Miso Shadow/Add Ignore (NDMF)";
        private const string ObjName = "MisoShadow";

        [MenuItem(MenuPath, false, 9)]
        private static void CreateGenerator(MenuCommand menuCommand)
        {
            var avatar = menuCommand.context as GameObject;

            if (avatar == null)
            {
                EditorUtility.DisplayDialog("Warning", "No objects getting selected!", "Use Correct Avatar Object");
                return;
            }

            var desc = avatar.GetComponent<VRCAvatarDescriptor>();

            if (desc == null)
            {
                EditorUtility.DisplayDialog("Warning", "There is no VRCAvatarDescriptor Component",
                    "Use Correct Avatar Object");
                return;
            }

            var newObj = new GameObject(ObjName)
            {
                transform = { parent = avatar.transform }
            };

            newObj.AddComponent<MisoShadowGenerate>();

            EditorUtility.DisplayDialog("Success", "Miso Shadow Apply Complete", "OK");
        }

        [MenuItem(MenuPath2, false, 12)]
        private static void CreateIgnore(MenuCommand menuCommand)
        {
            var obj = menuCommand.context as GameObject;
            
            if (obj == null)
            {
                EditorUtility.DisplayDialog("Warning", "No objects getting selected!", "Use Correct Object");
                return;
            }
            
            obj.AddComponent<MisoShadowIgnore>();
        }
    }
}