using Chocopoi.DressingTools.Wearable;
using Chocopoi.DressingTools.Wearable.Modules;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Modules
{
    public class ModuleEditor
    {
        public virtual string FriendlyName => GetType().Name;

        public bool foldout;

        protected DTWearableModuleBase target;

        protected DTWearableConfig config;

        public ModuleEditor(DTWearableModuleBase target, DTWearableConfig config)
        {
            this.target = target;
            this.config = config;
        }

        public virtual bool OnGUI(GameObject avatarGameObject, GameObject wearableGameObject)
        {
            // TODO: default module editor?
            return false;
        }

        public virtual bool IsValid()
        {
            return true;
        }
    }
}
