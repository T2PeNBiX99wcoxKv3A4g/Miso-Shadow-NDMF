using System;
using System.Linq;
using __yky.MisoShadowNDMF.Editor;
using __yky.MisoShadowNDMF.Runtime;
using AnimatorAsCode.V1;
using AnimatorAsCode.V1.ModularAvatar;
using AnimatorAsCode.V1.VRC;
using Miso.Utility;
using nadena.dev.ndmf;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

[assembly: ExportsPlugin(typeof(MisoShadowGenerator))]

namespace __yky.MisoShadowNDMF.Editor
{
    internal class MisoShadowGenerator : Plugin<MisoShadowGenerator>
    {
        public override string QualifiedName => "io.github.yky.animator-as-code.miso-shadow";
        public override string DisplayName => "Miso Shadow";

        private const string SystemName = "MisoShadow";
        private const string LayerName = "MisoShadow/ShadowStrength";
        private const string LayerName2 = "MisoShadow/ShadowAngle";
        private const string LayerName3 = "MisoShadow/ShadowReset";
        private const string ShadowAngle = "Assets/MISO/Animation/Shadow_angle.anim";
        private const string ShadowBody = "Assets/MISO/Animation/Shadow_Strength_Body.anim";
        private const string ShadowETC = "Assets/MISO/Animation/Shadow_Strength_Etc.anim";
        private const string Dummy = "Assets/MISO/Animation/Dummy.anim";
        private const string ParamShadowStrength = "MisoShadow/ShadowStrength";
        private const string ParamShadowAngle = "MisoShadow/ShadowAngle";
        private const string ParamToggleAngle = "MisoShadow/ToggleAngle";
        private const string ParamReset = "MisoShadow/Reset";
        private const string MenuShadowStrength = "menu.shadow_strength";
        private const string MenuShadowAngle = "menu.shadow_angle";
        private const string MenuToggleAngle = "menu.toggle_angle";
        private const string MenuReset = "menu.reset";

        protected override void Configure() => InPhase(BuildPhase.Generating).Run($"Generate {DisplayName}", Generate);

        private static void Log(object message) => Debug.Log($"[{nameof(MisoShadowGenerator)}] {message}");

        private static void LogError(object message) => Debug.LogError($"[{nameof(MisoShadowGenerator)}] {message}");

        private void Generate(BuildContext ctx)
        {
            var avatar = ctx.AvatarRootObject;
            var generate = avatar.GetComponentInChildren<MisoShadowGenerate>();

            if (generate == null) return;

            var nameList = MisoUtils.CheckShader(avatar);

            if (nameList == null || nameList.Count == 0)
            {
                LogError("NameList is null or empty.");
                return;
            }

            var ignoreList = Utils.CheckIgnore(avatar);

            foreach (var ignore in from ignore in ignoreList
                     let success = nameList.Remove(ignore)
                     where success
                     select ignore)
            {
                Log("Ignore transform: " + ignore);
            }

            // Initialize Animator As Code.
            var aac = AacV1.Create(new AacConfiguration
            {
                SystemName = SystemName,
                AnimatorRoot = ctx.AvatarRootTransform,
                DefaultValueRoot = ctx.AvatarRootTransform,
                AssetKey = GUID.Generate().ToString(),
                AssetContainer = ctx.AssetContainer,
                ContainerMode = AacConfiguration.Container.OnlyWhenPersistenceRequired,
                // States will be created with Write Defaults set to ON or OFF based on whether UseWriteDefaults is true or false.
                DefaultsProvider = new AacDefaultsProvider(generate.useWriteDefaults)
            });

            // Create a new animator controller.
            // This will be merged with the rest of the playable layer at the end of this function.
            var ctrl = aac.NewAnimatorController();
            var layer = ctrl.NewLayer(LayerName);

            var originalClip = aac.NewClip().Animating(clip =>
            {
                var originalClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(ShadowBody);

                if (originalClip == null)
                {
                    LogError("Original animation clip not found.");
                    return;
                }

                var curveBindings = AnimationUtility.GetCurveBindings(originalClip);

                foreach (var name in nameList)
                {
                    var objTransform = ctx.AvatarRootTransform.Find(name);
                    if (objTransform == null)
                    {
                        LogError("Could not find transform: " + name);
                        continue;
                    }

                    var transformPath = AnimationUtility.CalculateTransformPath(objTransform, avatar.transform);
                    var obj = objTransform.gameObject;
                    var smr = obj.GetComponent<SkinnedMeshRenderer>();
                    var mr = obj.GetComponent<MeshRenderer>();
                    Material mat = null;
                    Type currentBindingType = null;
                    if (smr == null && mr != null)
                    {
                        currentBindingType = typeof(MeshRenderer);
                        mat = mr.sharedMaterial;
                    }
                    else if (mr == null && smr != null)
                    {
                        currentBindingType = typeof(SkinnedMeshRenderer);
                        mat = smr.sharedMaterial;
                    }
                    else
                    {
                        LogError($"SkinnedMeshRenderer or MeshRenderer is not exist for GameObject with name {name}.");
                    }

                    if (mat == null)
                    {
                        LogError($"Material is null for GameObject with name {name}.");
                        continue;
                    }

                    foreach (var binding in curveBindings)
                    {
                        var newBinding = binding;
                        if (currentBindingType == typeof(MeshRenderer) && binding.type == typeof(SkinnedMeshRenderer))
                            newBinding.type = typeof(MeshRenderer);

                        if (newBinding.type != currentBindingType) continue;
                        var propertyName = binding.propertyName.Split('.')[1];
                        if (!mat.HasProperty(propertyName)) continue;
                        var basePropertyName = binding.propertyName.Split('.')[1];
                        var curveName = "material." + basePropertyName;

                        if (propertyName.ToLower().Contains("color"))
                        {
                            var colorValue = mat.GetColor(basePropertyName);
                            foreach (var channel in new[] { 'r', 'g', 'b', 'a' })
                            {
                                float channelValue;
                                switch (channel)
                                {
                                    case 'r':
                                        channelValue = colorValue.r;
                                        break;
                                    case 'g':
                                        channelValue = colorValue.g;
                                        break;
                                    case 'b':
                                        channelValue = colorValue.b;
                                        break;
                                    case 'a':
                                        channelValue = colorValue.a;
                                        break;
                                    default:
                                        continue;
                                }

                                clip.Animates(transformPath, currentBindingType, $"{curveName}.{channel}")
                                    .WithFrameCountUnit(
                                        keyframe =>
                                            keyframe.Easing(0f, channelValue)
                                    );
                            }
                        }
                        else
                        {
                            var floatValue = mat.GetFloat(propertyName);
                            clip.Animates(transformPath, currentBindingType, $"{curveName}.{propertyName}")
                                .WithFrameCountUnit(keyframe => keyframe.Easing(0f, floatValue));
                        }
                    }
                }
            });
            var original = layer.NewState("Original").WithAnimation(originalClip);

            var strengthClip = aac.NewClip().Animating(clip =>
            {
                var strengthClips = new AnimationClip[2];
                strengthClips[0] = AssetDatabase.LoadAssetAtPath<AnimationClip>(ShadowBody);
                strengthClips[1] = AssetDatabase.LoadAssetAtPath<AnimationClip>(ShadowETC);

                Utils.CopyClip(ctx.AvatarRootTransform, nameList, strengthClips[0], clip);
                Utils.CopyClip(ctx.AvatarRootTransform, nameList, strengthClips[1], clip, true);
            });

            var strength = layer.NewState("Strength").WithAnimation(strengthClip);

            // Creates a Bool parameter in the animator.
            var itemBool = layer.BoolParameter(ParamShadowStrength);

            original.TransitionsTo(strength).When(itemBool.IsTrue());
            strength.TransitionsTo(original).When(itemBool.IsFalse());

            var layer2 = ctrl.NewLayer(LayerName2);
            var dummyClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(Dummy);
            var dummy = layer2.NewState("Dummy").WithAnimation(dummyClip);

            var angleClip = aac.NewClip().Animating(clip =>
            {
                var angleClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(ShadowAngle);

                Utils.CopyClip(ctx.AvatarRootTransform, nameList, angleClip, clip, false, true);
            });

            var itemFloat = layer.FloatParameter(ParamShadowAngle);
            var angle = layer2.NewState("Angle").WithAnimation(angleClip).WithMotionTime(itemFloat);

            var itemBool2 = layer.BoolParameter(ParamToggleAngle);

            dummy.TransitionsTo(angle).When(itemBool2.IsTrue());
            angle.TransitionsTo(dummy).When(itemBool2.IsFalse());

            var layer3 = ctrl.NewLayer(LayerName3);
            var itemBool3 = layer3.BoolParameter(ParamReset);
            var empty = layer3.NewState("Empty").WithAnimation(dummyClip);
            var reset = layer3.NewState("Reset").WithAnimation(dummyClip).Driving(driver =>
            {
                driver.Locally();
                driver.Sets(itemBool, generate.enableByDefault);
                driver.Sets(itemFloat, 0);
                driver.Sets(itemBool2, false);
                driver.Sets(itemBool3, false);
            });

            empty.TransitionsTo(reset).When(itemBool3.IsTrue());
            reset.Exits().Automatically();

            // Create a new object in the scene. We will add Modular Avatar components inside it.
            var modularAvatar = MaAc.Create(new GameObject(SystemName)
            {
                transform = { parent = generate.transform }
            });

            // By creating a Modular Avatar Merge Animator component,
            // our animator controller will be added to the avatar's FX layer.
            if (generate.savableShadowStrength)
                modularAvatar.NewParameter(itemBool).WithDefaultValue(generate.enableByDefault);
            else
                modularAvatar.NewParameter(itemBool).WithDefaultValue(generate.enableByDefault).NotSaved();

            if (generate.savableShadowAngle)
            {
                modularAvatar.NewParameter(itemFloat);
                modularAvatar.NewParameter(itemBool2);
            }
            else
            {
                modularAvatar.NewParameter(itemFloat).NotSaved();
                modularAvatar.NewParameter(itemBool2).NotSaved();
            }

            modularAvatar.NewParameter(itemBool3).NotSaved().NotSynced();
            modularAvatar.NewMergeAnimator(ctrl.AnimatorController, VRCAvatarDescriptor.AnimLayerType.FX);

            var menuRootObj = generate.menuRoot.gameObject;
            var menuObj = new GameObject(MenuShadowStrength.L())
            {
                transform = { parent = menuRootObj.transform }
            };
            var menu = modularAvatar.EditMenuItem(menuObj);
            menu.Name(MenuShadowStrength.L());
            menu.Toggle(itemBool);

            var menuObj2 = new GameObject(MenuShadowAngle.L())
            {
                transform = { parent = menuRootObj.transform }
            };
            var menu2 = modularAvatar.EditMenuItem(menuObj2);
            menu2.Name(MenuShadowAngle.L());
            menu2.Radial(itemFloat);

            var menuObj3 = new GameObject(MenuToggleAngle.L())
            {
                transform = { parent = menuRootObj.transform }
            };
            var menu3 = modularAvatar.EditMenuItem(menuObj3);
            menu3.Name(MenuToggleAngle.L());
            menu3.Toggle(itemBool2);

            var menuObj4 = new GameObject(MenuReset.L())
            {
                transform = { parent = menuRootObj.transform }
            };
            var menu4 = modularAvatar.EditMenuItem(menuObj4);
            menu4.Name(MenuReset.L());
            menu4.Button(itemBool3);
        }
    }
}