/*
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
using Chocopoi.DressingFramework.Extensibility.Sequencing;
using Chocopoi.DressingFramework.Localization;
using Chocopoi.DressingFramework.Serialization;
using Chocopoi.DressingTools.Event;
using Chocopoi.DressingTools.Localization;
using Chocopoi.DressingTools.OneConf;
using Chocopoi.DressingTools.OneConf.Cabinet;
using Chocopoi.DressingTools.OneConf.Serialization;
using Chocopoi.DressingTools.OneConf.Wearable;
using Chocopoi.DressingTools.OneConf.Wearable.Modules;
using Chocopoi.DressingTools.OneConf.Wearable.Modules.BuiltIn;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.OneConf.Wearable.Modules
{
    [InitializeOnLoad]
    internal class CabinetAnimWearableModuleProvider : WearableModuleProvider
    {
        internal class EventAdapter : ToolEventAdapter
        {
            public override void OnAddWearableToCabinet(CabinetConfig cabinetConfig, GameObject avatarGameObject, WearableConfig wearableConfig, GameObject wearableGameObject)
            {
                var wearableModule = wearableConfig.FindModule(CabinetAnimWearableModuleConfig.ModuleIdentifier);
                if (wearableModule == null) return;

                var agm = wearableConfig.FindModuleConfig<CabinetAnimWearableModuleConfig>();
                if (agm.invertAvatarToggleOriginalStates) InvertAvatarToggleStates(agm, avatarGameObject);
                if (agm.invertWearableToggleOriginalStates) InvertWearableToggleStates(agm, wearableGameObject);
            }
        }

        private static readonly I18nTranslator t = I18n.ToolTranslator;

        [ExcludeFromCodeCoverage] public override string Identifier => CabinetAnimWearableModuleConfig.ModuleIdentifier;
        [ExcludeFromCodeCoverage] public override string FriendlyName => t._("modules.wearable.cabinetAnim.friendlyName");
        [ExcludeFromCodeCoverage] public override bool AllowMultiple => false;

        [ExcludeFromCodeCoverage]
        public override BuildConstraint Constraint =>
            InvokeAtStage(BuildStage.Transpose)
                .AfterPass(ArmatureMappingWearableModuleConfig.ModuleIdentifier, true)
                .AfterPass(MoveRootWearableModuleConfig.ModuleIdentifier, true)
                .Build();

        public override IModuleConfig DeserializeModuleConfig(JObject jObject)
        {
            // TODO: do schema check

            var version = jObject["version"].ToObject<SerializationVersion>();
            if (version.Major > CabinetAnimWearableModuleConfig.CurrentConfigVersion.Major)
            {
                throw new System.Exception("Incompatible CabinetAnimWearableModuleConfig version: " + version.Major + " > " + CabinetAnimWearableModuleConfig.CurrentConfigVersion.Major);
            }

            return jObject.ToObject<CabinetAnimWearableModuleConfig>();
        }

        public override IModuleConfig NewModuleConfig() => new CabinetAnimWearableModuleConfig();

        private static void InvertAvatarToggleStates(CabinetAnimWearableModuleConfig agm, GameObject avatarGameObject)
        {
            // invert avatar toggles
            foreach (var toggle in agm.avatarAnimationOnWear.toggles)
            {
                var avatarToggleObj = avatarGameObject.transform.Find(toggle.path);
                if (avatarToggleObj == null)
                {
                    Debug.LogWarning("[DressingTools] [CabinetAnimModule] Avatar toggle GameObject not found at path: " + toggle.path);
                    continue;
                }
                avatarToggleObj.gameObject.SetActive(!toggle.state);
            }
        }

        private static void InvertWearableToggleStates(CabinetAnimWearableModuleConfig agm, GameObject wearableGameObject)
        {
            // invert wearable toggles
            foreach (var toggle in agm.wearableAnimationOnWear.toggles)
            {
                var wearableToggleObj = wearableGameObject.transform.Find(toggle.path);
                if (wearableToggleObj == null)
                {
                    Debug.LogWarning("[DressingTools] [CabinetAnimModule] Wearable toggle GameObject not found at path: " + toggle.path);
                    continue;
                }
                wearableToggleObj.gameObject.SetActive(!toggle.state);
            }
        }

        private void ApplyAnimationPreset(GameObject go, CabinetAnimWearableModuleConfig.Preset preset)
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

        private static void SetDynamicsInactive(WearableContext wearCtx)
        {
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
                // dynamics.GameObject.SetActive(false);
                if (dynamics.Component is Behaviour b)
                {
                    b.enabled = false;
                }

                // mark as visited
                visitedDynamicsTransforms.Add(dynamics.Transform);
            }
        }

        public override bool Invoke(CabinetContext cabCtx, WearableContext wearCtx, ReadOnlyCollection<WearableModule> modules, bool isPreview)
        {
            if (modules.Count == 0) return true;
            var agm = (CabinetAnimWearableModuleConfig)modules[0].config;

            // we only apply preset in preview
            if (isPreview)
            {
                ApplyAnimationPreset(cabCtx.dkCtx.AvatarGameObject, agm.avatarAnimationOnWear);
                ApplyAnimationPreset(wearCtx.wearableGameObject, agm.wearableAnimationOnWear);

                return true;
            }

            // Now we don't invert states on entering play to avoid user confusion
            // We only switch the dynamics inactive
            if (agm.setWearableDynamicsInactive) SetDynamicsInactive(wearCtx);
            return true;
        }
    }

}
