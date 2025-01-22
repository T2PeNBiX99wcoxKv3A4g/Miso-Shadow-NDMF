using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using __yky.MisoShadowController.Runtime;
using AnimatorAsCode.V1;
using Miso.Utility;
using UnityEditor;
using UnityEngine;

namespace __yky.MisoShadowController.Editor
{
    public static class Utils
    {
        private const string ShaderShortName = "lil";

        public static List<string> CheckShader(GameObject avatar)
        {
            var renderers = avatar.GetComponentsInChildren<Renderer>(true);

            return (from ren in renderers
                let materials = ren.sharedMaterials
                where materials != null && materials.Length != 0
                from mat in materials
                where mat != null
                where mat.shader.name.Contains(ShaderShortName)
                select MisoUtils.GetPartialPath(ren.transform)).ToList();
        }

        public static List<string> CheckIgnore(GameObject avatar)
        {
            var ignores = avatar.GetComponentsInChildren<MisoShadowIgnore>(true);
            return ignores.Select(ignore => MisoUtils.GetPartialPath(ignore.transform)).ToList();
        }

        public static void CopyClip(Transform avatar, List<string> nameList, AnimationClip sourceClip,
            AacFlEditClip destClip, bool excludeFilter = false, bool ignoreFilter = false)
        {
            foreach (var binding in AnimationUtility.GetCurveBindings(sourceClip))
            {
                var curve = AnimationUtility.GetEditorCurve(sourceClip, binding);
                ApplyToAllChildren(avatar, nameList, avatar, binding, curve, destClip, excludeFilter, ignoreFilter);
            }
        }

        private static void ApplyToAllChildren(Transform parent, List<string> nameList, Transform avatar,
            EditorCurveBinding binding, AnimationCurve curve, AacFlEditClip destClip, bool excludeFilter,
            bool ignoreFilter)
        {
            var bodyRegex = new Regex(".*body.*", RegexOptions.IgnoreCase);

            for (var i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                var fullPath = AnimationUtility.CalculateTransformPath(child, avatar);
                var apply = nameList.Contains(fullPath);

                if (apply && !ignoreFilter)
                {
                    if (excludeFilter)
                        apply = !bodyRegex.IsMatch(child.gameObject.name);
                    else
                        apply = bodyRegex.IsMatch(child.gameObject.name);
                }

                if (apply)
                {
                    System.Type typeToUse = null;
                    if (child.GetComponent<SkinnedMeshRenderer>() != null)
                        typeToUse = typeof(SkinnedMeshRenderer);
                    else if (child.GetComponent<MeshRenderer>() != null)
                        typeToUse = typeof(MeshRenderer);

                    if (typeToUse != null)
                    {
                        var newBinding = new EditorCurveBinding
                        {
                            type = typeToUse,
                            path = AnimationUtility.CalculateTransformPath(child.transform, avatar),
                            propertyName = binding.propertyName
                        };
                        AnimationUtility.SetEditorCurve(destClip.Clip, newBinding, curve);
                    }
                }

                // Recursive call for child objects
                ApplyToAllChildren(child, nameList, avatar, binding, curve, destClip, excludeFilter, ignoreFilter);
            }
        }
    }
}