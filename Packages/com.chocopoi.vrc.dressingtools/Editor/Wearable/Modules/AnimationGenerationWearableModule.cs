/*
 * File: AnimationGenerationModule.cs
 * Project: DressingTools
 * Created Date: Tuesday, August 1st 2023, 12:37:10 am
 * Author: chocopoi (poi@chocopoi.com)
 * -----
 * Copyright (c) 2023 chocopoi
 * 
 * This file is part of DressingTools.
 * 
 * DressingTools is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 * 
 * DressingTools is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with DressingTools. If not, see <https://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Chocopoi.DressingTools.Lib;
using Chocopoi.DressingTools.Lib.Cabinet;
using Chocopoi.DressingTools.Lib.Extensibility.Providers;
using Chocopoi.DressingTools.Lib.Serialization;
using Chocopoi.DressingTools.Lib.Wearable;
using Chocopoi.DressingTools.Lib.Wearable.Modules;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.Wearable.Modules
{
    internal class AnimationGenerationWearableModuleConfig : IModuleConfig
    {
        public static readonly SerializationVersion CurrentConfigVersion = new SerializationVersion(1, 0, 0);
        public SerializationVersion version;
        public AnimationPreset avatarAnimationOnWear; // execute on wear
        public AnimationPreset wearableAnimationOnWear;
        public List<WearableCustomizable> wearableCustomizables; // items that show up in action menu for customization

        public AnimationGenerationWearableModuleConfig()
        {
            version = CurrentConfigVersion;
            avatarAnimationOnWear = new AnimationPreset();
            wearableAnimationOnWear = new AnimationPreset();
            wearableCustomizables = new List<WearableCustomizable>();
        }
    }

    [InitializeOnLoad]
    internal class AnimationGenerationWearableModuleProvider : WearableModuleProviderBase
    {
        private static readonly Localization.I18n t = Localization.I18n.Instance;
        public const string MODULE_IDENTIFIER = "com.chocopoi.dressingtools.built-in.wearable.animation-generation";

        [ExcludeFromCodeCoverage] public override string ModuleIdentifier => MODULE_IDENTIFIER;
        [ExcludeFromCodeCoverage] public override string FriendlyName => t._("modules.wearable.animGen.friendlyName");
        [ExcludeFromCodeCoverage] public override int CallOrder => 4;
        [ExcludeFromCodeCoverage] public override bool AllowMultiple => false;

        static AnimationGenerationWearableModuleProvider()
        {
            WearableModuleProviderLocator.Instance.Register(new AnimationGenerationWearableModuleProvider());
        }

        public override IModuleConfig DeserializeModuleConfig(JObject jObject)
        {
            // TODO: do schema check

            var version = jObject["version"].ToObject<SerializationVersion>();
            if (version.Major > AnimationGenerationWearableModuleConfig.CurrentConfigVersion.Major)
            {
                throw new System.Exception("Incompatible AnimationGenerationWearableModuleConfig version: " + version.Major + " > " + AnimationGenerationWearableModuleConfig.CurrentConfigVersion.Major);
            }

            return jObject.ToObject<AnimationGenerationWearableModuleConfig>();
        }

        public override IModuleConfig NewModuleConfig() => new AnimationGenerationWearableModuleConfig();

        private static void InvertToggleStates(GameObject avatarGameObject, WearableConfig config, GameObject wearableGameObject, WearableModule module)
        {
            var agm = (AnimationGenerationWearableModuleConfig)module.config;

            // invert avatar toggles
            foreach (var toggle in agm.avatarAnimationOnWear.toggles)
            {
                var avatarToggleObj = avatarGameObject.transform.Find(toggle.path);
                if (avatarToggleObj == null)
                {
                    Debug.LogWarning("[DressingTools] [AnimationGenerationModule] Avatar toggle GameObject not found at path: " + toggle.path);
                    continue;
                }
                avatarToggleObj.gameObject.SetActive(!toggle.state);
            }

            // invert wearable toggles
            foreach (var toggle in agm.wearableAnimationOnWear.toggles)
            {
                Debug.Log("toggle: " + toggle != null);
                var wearableToggleObj = wearableGameObject.transform.Find(toggle.path);
                if (wearableToggleObj == null)
                {
                    Debug.LogWarning("[DressingTools] [AnimationGenerationModule] Wearable toggle GameObject not found at path: " + toggle.path);
                    continue;
                }
                wearableToggleObj.gameObject.SetActive(!toggle.state);
            }
        }

        public override bool OnAddWearableToCabinet(CabinetConfig cabinetConfig, GameObject avatarGameObject, WearableConfig wearableConfig, GameObject wearableGameObject, ReadOnlyCollection<WearableModule> modules)
        {
            if (modules.Count == 0)
            {
                // we need the wearable to have our module installed
                return true;
            }

            InvertToggleStates(avatarGameObject, wearableConfig, wearableGameObject, modules[0]);
            return true;
        }

        public override bool OnAfterApplyCabinet(ApplyCabinetContext cabCtx)
        {
            var wearables = DTEditorUtils.GetCabinetWearables(cabCtx.avatarGameObject);

            foreach (var wearable in wearables)
            {
                var wearCtx = cabCtx.wearableContexts[wearable];
                var config = wearCtx.wearableConfig;
                var module = DTEditorUtils.FindWearableModule(config, ModuleIdentifier);

                if (module == null)
                {
                    // no animation generation module, skipping
                    continue;
                }

                InvertToggleStates(cabCtx.avatarGameObject, config, wearable.wearableGameObject, module);

                // set wearable dynamics inactive
                var visitedDynamicsTransforms = new List<Transform>();
                foreach (var dynamics in wearCtx.wearableDynamics)
                {
                    if (visitedDynamicsTransforms.Contains(dynamics.Transform))
                    {
                        // skip duplicates since it's meaningless
                        continue;
                    }

                    // we toggle GameObjects instead of components
                    dynamics.GameObject.SetActive(false);

                    // mark as visited
                    visitedDynamicsTransforms.Add(dynamics.Transform);
                }
            }

            return true;
        }

        public override bool OnPreviewWearable(ApplyCabinetContext cabCtx, ApplyWearableContext wearCtx, ReadOnlyCollection<WearableModule> modules)
        {
            if (modules.Count == 0)
            {
                return true;
            }

            var agm = (AnimationGenerationWearableModuleConfig)modules[0].config;

            ApplyAnimationPreset(cabCtx.avatarGameObject, agm.avatarAnimationOnWear);
            ApplyAnimationPreset(wearCtx.wearableGameObject, agm.wearableAnimationOnWear);

            return true;
        }

        private void ApplyAnimationPreset(GameObject go, AnimationPreset preset)
        {
            foreach (var toggle in preset.toggles)
            {
                var obj = go.transform.Find(toggle.path);
                if (obj != null)
                {
                    obj.gameObject.SetActive(toggle.state);
                }
            }

            foreach (var blendshape in preset.blendshapes)
            {
                var obj = go.transform.Find(blendshape.path);
                if (obj != null)
                {
                    var smr = obj.GetComponent<SkinnedMeshRenderer>();
                    if (smr == null || smr.sharedMesh == null)
                    {
                        continue;
                    }

                    var index = smr.sharedMesh.GetBlendShapeIndex(blendshape.blendshapeName);

                    if (index == -1)
                    {
                        continue;
                    }

                    smr.SetBlendShapeWeight(index, blendshape.value);
                }
            }
        }
    }

}
