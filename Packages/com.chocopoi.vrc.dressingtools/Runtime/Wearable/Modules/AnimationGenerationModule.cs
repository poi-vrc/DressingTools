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
using System.Diagnostics.CodeAnalysis;
using Chocopoi.DressingTools.Lib;
using Chocopoi.DressingTools.Lib.Cabinet;
using Chocopoi.DressingTools.Lib.Logging;
using Chocopoi.DressingTools.Lib.Proxy;
using Chocopoi.DressingTools.Lib.Wearable;
using Chocopoi.DressingTools.Lib.Wearable.Modules;
using Chocopoi.DressingTools.Lib.Wearable.Modules.Providers;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.Wearable.Modules
{
    internal class AnimationGenerationModuleConfig : IModuleConfig
    {
        public AnimationPreset avatarAnimationOnWear; // execute on wear
        public AnimationPreset wearableAnimationOnWear;
        public List<WearableCustomizable> wearableCustomizables; // items that show up in action menu for customization

        public AnimationGenerationModuleConfig()
        {
            avatarAnimationOnWear = new AnimationPreset();
            wearableAnimationOnWear = new AnimationPreset();
            wearableCustomizables = new List<WearableCustomizable>();
        }
    }

    [InitializeOnLoad]
    internal class AnimationGenerationModuleProvider : ModuleProviderBase
    {
        public const string Identifier = "com.chocopoi.dressingtools.built-in.animation-generation";

        [ExcludeFromCodeCoverage] public override string ModuleIdentifier => Identifier;
        [ExcludeFromCodeCoverage] public override string FriendlyName => "Animation Generation";
        [ExcludeFromCodeCoverage] public override int ApplyOrder => 4;
        [ExcludeFromCodeCoverage] public override bool AllowMultiple => false;

        static AnimationGenerationModuleProvider()
        {
            ModuleProviderLocator.Instance.Register(new AnimationGenerationModuleProvider());
        }

        public override IModuleConfig DeserializeModuleConfig(JObject jObject) => jObject.ToObject<AnimationGenerationModuleConfig>();

        public override IModuleConfig NewModuleConfig() => new AnimationGenerationModuleConfig();

        private static void InvertToggleStates(ICabinet cabinet, WearableConfig config, GameObject wearableGameObject, WearableModule module)
        {
            var agm = (AnimationGenerationModuleConfig)module.config;
            var avatarGameObject = cabinet.AvatarGameObject;

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
                var wearableToggleObj = wearableGameObject.transform.Find(toggle.path);
                if (wearableGameObject == null)
                {
                    Debug.LogWarning("[DressingTools] [AnimationGenerationModule] Wearable toggle GameObject not found at path: " + toggle.path);
                    continue;
                }
                wearableToggleObj.gameObject.SetActive(!toggle.state);
            }
        }

        public override bool OnAddWearableToCabinet(ICabinet cabinet, WearableConfig config, GameObject wearableGameObject, WearableModule module)
        {
            if (module == null)
            {
                // we need the wearable to have our module installed
                return true;
            }

            InvertToggleStates(cabinet, config, wearableGameObject, module);
            return true;
        }

        public override bool OnAfterApplyCabinet(ApplyCabinetContext ctx)
        {
            var wearables = ctx.cabinet.GetWearables();

            foreach (var wearable in wearables)
            {
                var config = WearableConfig.Deserialize(wearable.configJson);

                if (config == null)
                {
                    Debug.LogWarning("[DressingTools] [AnimationGenerationModule] Unable to deserialize one of the wearable configuration: " + wearable.name);
                    return false;
                }

                var module = DTRuntimeUtils.FindWearableModule(config, Identifier);

                if (module == null)
                {
                    // no animation generation module, skipping
                    continue;
                }

                InvertToggleStates(ctx.cabinet, config, wearable.wearableGameObject, module);

                // set wearable dynamics inactive
                var wearableDynamics = DTRuntimeUtils.ScanDynamics(wearable.wearableGameObject, false);
                var visitedDynamicsTransforms = new List<Transform>();
                foreach (var dynamics in wearableDynamics)
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
    }

}
