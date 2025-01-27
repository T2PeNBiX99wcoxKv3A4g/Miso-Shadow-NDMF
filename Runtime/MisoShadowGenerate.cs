using nadena.dev.modular_avatar.core;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace __yky.MisoShadowNDMF.Runtime
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ModularAvatarMenuInstaller))]
    [RequireComponent(typeof(ModularAvatarMenuGroup))]
    [AddComponentMenu("yky/Miso Shadow/Miso Shadow Generate")]
    public class MisoShadowGenerate : MisoShadowEditorComponent
    {
        public bool enableByDefault;
        public bool useWriteDefaults = true;
        public bool savableShadowStrength = true;
        public bool savableShadowAngle;
        public ModularAvatarMenuItem menuRoot;
        
        private const string MenuRootName = "Miso Shadow";
        private void OnValidate()
        {
            if (menuRoot != null) return;
            var menuRootObj = new GameObject(MenuRootName)
            {
                transform = { parent = transform}
            };
            menuRoot = menuRootObj.AddComponent<ModularAvatarMenuItem>();
            menuRoot.Control = new VRCExpressionsMenu.Control
            {
                type = VRCExpressionsMenu.Control.ControlType.SubMenu
            };
            menuRoot.MenuSource = SubmenuSource.Children;
            menuRoot.menuSource_otherObjectChildren = null;
        }
    }
}