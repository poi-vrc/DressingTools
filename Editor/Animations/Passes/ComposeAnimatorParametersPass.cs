/*
 * Copyright (c) 2024 chocopoi
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
using System.Text.RegularExpressions;
using Chocopoi.DressingFramework;
using Chocopoi.DressingFramework.Animations;
using Chocopoi.DressingFramework.Extensibility.Sequencing;
using Chocopoi.DressingTools.Components.Animations;
using Chocopoi.DressingTools.Components.Menu;
using Chocopoi.DressingTools.Menu.Passes;
using UnityEngine;

#if DT_VRCSDK3A
using Chocopoi.DressingFramework.Detail.DK.Passes.VRChat;
#endif

namespace Chocopoi.DressingTools.Animations.Passes
{
    internal class ComposeAnimatorParametersPass : BuildPass
    {
        public override BuildConstraint Constraint =>
            InvokeAtStage(BuildStage.Transpose)
#if DT_VRCSDK3A
                .BeforePass<ApplyVRCExParamsPass>()
#endif
                .AfterPass<ComposeAndInstallMenuPass>()
                .Build();

        private readonly Dictionary<string, DTAnimatorParameters.ParameterConfig> _configs;

        public ComposeAnimatorParametersPass()
        {
            _configs = new Dictionary<string, DTAnimatorParameters.ParameterConfig>();
        }

        private void TraverseDown(Transform transform)
        {
            // the idea is to go to the very deep first to find child parameters
            // and then replace them with parents or grand-parents' config

            for (var i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                TraverseDown(child);
            }

            if (transform.TryGetComponent<DTAnimatorParameters>(out var comp))
            {
                foreach (var config in comp.Configs)
                {
                    _configs[config.ParameterName] = config;
                }
            }
        }

        private void AddItemControllerConfig(DTMenuItem.ItemController controller)
        {
            if (controller.Type == DTMenuItem.ItemController.ControllerType.AnimatorParameter &&
                !string.IsNullOrEmpty(controller.AnimatorParameterName))
            {
                // defaults to all network synced and saved
                _configs[controller.AnimatorParameterName] = new DTAnimatorParameters.ParameterConfig()
                {
                    ParameterName = controller.AnimatorParameterName,
                    ParameterDefaultValue = 0.0f,
                    NetworkSynced = true,
                    Saved = true
                };
            }
        }

        private void FindMenuItemsAndDraftParams(Context ctx)
        {
            var comps = ctx.AvatarGameObject.GetComponentsInChildren<DTMenuItem>();
            foreach (var comp in comps)
            {
                // controller on open or single controller
                AddItemControllerConfig(comp.Controller);

                var subControllersCount = 0;
                switch (comp.Type)
                {
                    case DTMenuItem.ItemType.TwoAxis:
                        subControllersCount = 2;
                        break;
                    case DTMenuItem.ItemType.FourAxis:
                        subControllersCount = 4;
                        break;
                    case DTMenuItem.ItemType.Radial:
                        subControllersCount = 1;
                        break;
                }

                if (subControllersCount > 0)
                {
                    if (comp.SubControllers.Length < subControllersCount)
                    {
                        continue;
                    }

                    for (var i = 0; i < subControllersCount; i++)
                    {
                        AddItemControllerConfig(comp.SubControllers[i]);
                    }
                }
            }
        }

        private void WriteConfigs(Context ctx)
        {
            var animParams = ctx.Feature<AnimatorParameters>();
            foreach (var config in _configs.Values)
            {
                // TODO: at DK side, the nature is using regex and the order is not considered
                animParams.AddConfig(new AnimatorParameters.ParameterConfig($"^{Regex.Escape(config.ParameterName)}$")
                {
                    networkSynced = config.NetworkSynced,
                    saved = config.Saved,
                    defaultValue = config.ParameterDefaultValue
                });
            }
        }

        public override bool Invoke(Context ctx)
        {
            FindMenuItemsAndDraftParams(ctx);
            TraverseDown(ctx.AvatarGameObject.transform);
            WriteConfigs(ctx);
            return true;
        }
    }
}
