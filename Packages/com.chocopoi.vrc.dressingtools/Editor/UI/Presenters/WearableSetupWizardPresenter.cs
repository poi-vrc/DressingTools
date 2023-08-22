/*
 * File: WearableSetupWizardPresenter.cs
 * Project: DressingTools
 * Created Date: Saturday, August 12th 2023, 1:22:09 am
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
using Chocopoi.DressingTools.Lib.Cabinet;
using Chocopoi.DressingTools.Lib.Wearable;
using Chocopoi.DressingTools.Lib.Wearable.Modules;
using Chocopoi.DressingTools.UIBase.Views;
using Chocopoi.DressingTools.Wearable.Modules;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Presenters
{
    internal class WearableSetupWizardPresenter
    {
        private IWearableSetupWizardView _view;

        public WearableSetupWizardPresenter(IWearableSetupWizardView view)
        {
            _view = view;

            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            _view.Load += OnLoad;
            _view.Unload += OnUnload;

            _view.ForceUpdateView += OnForceUpdateView;
            _view.TargetAvatarOrWearableChange += OnTargetAvatarOrWearableChange;
            _view.PreviousButtonClick += OnPreviousButtonClick;
            _view.NextButtonClick += OnNextButtonClick;
        }

        private void UnsubscribeEvents()
        {
            _view.Load -= OnLoad;
            _view.Unload -= OnUnload;

            _view.ForceUpdateView -= OnForceUpdateView;
            _view.TargetAvatarOrWearableChange -= OnTargetAvatarOrWearableChange;
            _view.PreviousButtonClick -= OnPreviousButtonClick;
            _view.NextButtonClick -= OnNextButtonClick;
        }

        private void OnForceUpdateView()
        {
            UpdateView();
        }

        private void OnTargetAvatarOrWearableChange()
        {
            UpdateView();
            AutoSetup();
        }

        private void AutoSetupMapping()
        {
            // cabinet
            var cabinet = DTEditorUtils.GetAvatarCabinet(_view.TargetAvatar);

            var armatureName = "Armature";
            if (cabinet != null)
            {
                armatureName = cabinet.AvatarArmatureName;
                _view.ShowAvatarNoCabinetHelpBox = false;
            }
            else
            {
                _view.ShowAvatarNoCabinetHelpBox = true;
            }

            // attempt to find wearable armature using avatar armature name
            var armature = DTRuntimeUtils.GuessArmature(_view.TargetWearable, armatureName);

            if (armature == null)
            {
                _view.UseArmatureMapping = false;
                _view.UseMoveRoot = true;

                _view.ShowArmatureNotFoundHelpBox = true;
                _view.ShowArmatureGuessedHelpBox = false;
            }
            else
            {
                _view.ShowArmatureNotFoundHelpBox = false;
                _view.ShowArmatureGuessedHelpBox = armature.name != armatureName;

                _view.UseArmatureMapping = true;
                _view.UseMoveRoot = false;
            }
        }

        private string[] GetBlendshapeNames(SkinnedMeshRenderer smr)
        {
            if (smr.sharedMesh == null)
            {
                return new string[0];
            }

            var names = new List<string>();
            for (var i = 0; i < smr.sharedMesh.blendShapeCount; i++)
            {
                names.Add(smr.sharedMesh.GetBlendShapeName(i));
            }

            return names.ToArray();
        }

        private void AutoSetupAnimationGeneration()
        {
            _view.UseAnimationGeneration = true;

            // generate wearable toggles
            {
                var transforms = new List<Transform>();

                if (_view.UseArmatureMapping)
                {
                    // skip the armature if used armature mapping
                    var wearableArmature = DTRuntimeUtils.GuessArmature(_view.TargetWearable, _view.ArmatureMappingModuleConfig.wearableArmatureName);
                    for (var i = 0; i < _view.TargetWearable.transform.childCount; i++)
                    {
                        // do not auto add wearable toggle if state is disabled initially
                        var child = _view.TargetWearable.transform.GetChild(i);
                        if (child != wearableArmature && child.gameObject.activeSelf)
                        {
                            transforms.Add(child);
                        }
                    }
                }
                else
                {
                    // add all
                    for (var i = 0; i < _view.TargetWearable.transform.childCount; i++)
                    {
                        // do not auto add wearable toggle if state is disabled initially
                        var child = _view.TargetWearable.transform.GetChild(i);
                        if (child.gameObject.activeSelf)
                        {
                            transforms.Add(child);
                        }
                    }
                }

                var toggles = _view.AnimationGenerationModuleConfig.wearableAnimationOnWear.toggles;
                var blendshapes = _view.AnimationGenerationModuleConfig.wearableAnimationOnWear.blendshapes;
                toggles.Clear();
                blendshapes.Clear();

                foreach (var trans in transforms)
                {
                    toggles.Add(new AnimationToggle()
                    {
                        path = DTRuntimeUtils.GetRelativePath(trans, _view.TargetWearable.transform),
                        state = true
                    });
                }
            }

            // generate blendshape syncs
            {
                // clear old syncs
                var blendshapeSyncs = _view.BlendshapeSyncModuleConfig.blendshapeSyncs;
                blendshapeSyncs.Clear();

                // find all avatar blendshapes
                var avatarSmrs = _view.TargetAvatar.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                var avatarSmrCache = new Dictionary<SkinnedMeshRenderer, string[]>();
                foreach (var avatarSmr in avatarSmrs)
                {
                    // transverse up to see if it is originated from ours or an existing wearable
                    if (DTRuntimeUtils.IsOriginatedFromAnyWearable(_view.TargetWearable.transform, avatarSmr.transform))
                    {
                        // skip this SMR
                        continue;
                    }

                    // add to cache
                    avatarSmrCache.Add(avatarSmr, GetBlendshapeNames(avatarSmr));
                }

                // pair wearable blendshape names with avatar
                var wearableSmrs = _view.TargetWearable.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                foreach (var wearableSmr in wearableSmrs)
                {
                    var wearableBlendshapes = GetBlendshapeNames(wearableSmr);

                    if (wearableBlendshapes.Length == 0)
                    {
                        // skip no wearable blendshapes
                        continue;
                    }

                    // search if have pairs
                    var found = false;
                    foreach (var avatarSmr in avatarSmrs)
                    {
                        if (!avatarSmrCache.ContainsKey(avatarSmr))
                        {
                            continue;
                        }

                        var avatarBlendshapes = avatarSmrCache[avatarSmr];
                        foreach (var wearableBlendshape in wearableBlendshapes)
                        {
                            if (System.Array.IndexOf(avatarBlendshapes, wearableBlendshape) != -1)
                            {
                                blendshapeSyncs.Add(new AnimationBlendshapeSync()
                                {
                                    avatarBlendshapeName = wearableBlendshape,
                                    avatarFromValue = 0,
                                    avatarToValue = 100,
                                    wearableFromValue = 0,
                                    wearableToValue = 100,
                                    wearableBlendshapeName = wearableBlendshape,
                                    avatarPath = DTRuntimeUtils.GetRelativePath(avatarSmr.transform, _view.TargetAvatar.transform),
                                    wearablePath = DTRuntimeUtils.GetRelativePath(wearableSmr.transform, _view.TargetWearable.transform)
                                });
                                found = true;
                                break;
                            }
                        }

                        if (found)
                        {
                            // don't process the same blendshape anymore if found
                            break;
                        }
                    }
                }

                // enable module if count > 0
                _view.UseBlendshapeSync = blendshapeSyncs.Count > 0;
            }
        }

        private void AutoSetup()
        {
            if (_view.TargetAvatar == null || _view.TargetWearable == null)
            {
                return;
            }

            AutoSetupMapping();
            AutoSetupAnimationGeneration();
            UpdateView();
        }

        private void OnPreviousButtonClick()
        {
            if (_view.CurrentStep > 0)
            {
                _view.CurrentStep -= 1;
            }
        }

        public void GenerateConfig()
        {
            var config = new WearableConfig();

            DTEditorUtils.PrepareWearableConfig(config, _view.TargetAvatar, _view.TargetWearable);

            if (_view.UseArmatureMapping)
            {
                config.Modules.Add(new WearableModule()
                {
                    moduleName = ArmatureMappingModuleProvider.Identifier,
                    config = _view.ArmatureMappingModuleConfig,
                });
            }

            if (_view.UseMoveRoot)
            {
                config.Modules.Add(new WearableModule()
                {
                    moduleName = MoveRootModuleProvider.Identifier,
                    config = _view.MoveRootModuleConfig,
                });
            }

            if (_view.UseAnimationGeneration)
            {
                config.Modules.Add(new WearableModule()
                {
                    moduleName = AnimationGenerationModuleProvider.Identifier,
                    config = _view.AnimationGenerationModuleConfig,
                });
            }

            if (_view.UseBlendshapeSync)
            {
                config.Modules.Add(new WearableModule()
                {
                    moduleName = BlendshapeSyncModuleProvider.Identifier,
                    config = _view.BlendshapeSyncModuleConfig,
                });
            }

            _view.Config = config;
        }

        private void OnNextButtonClick()
        {
            // progress step if not last step
            if (_view.CurrentStep < 3)
            {
                _view.CurrentStep += 1;
            }
            else
            {
                if (_view.TargetAvatar == null || _view.TargetWearable == null)
                {
                    Debug.Log("null target avatar/wearable");
                    return;
                }

                if (!_view.IsValid())
                {
                    Debug.Log("Invalid");
                    return;
                }

                GenerateConfig();
                _view.RaiseDoAddToCabinetEvent();
            }
        }

        private void UpdateView()
        {
            _view.ArmatureMappingModuleEditor.RaiseForceUpdateViewEvent();
            _view.MoveRootModuleEditor.RaiseForceUpdateViewEvent();
            _view.AnimationGenerationModuleEditor.RaiseForceUpdateViewEvent();
            _view.BlendshapeSyncModuleEditor.RaiseForceUpdateViewEvent();
        }

        private void OnLoad()
        {
            UpdateView();
        }

        private void OnUnload()
        {
            UnsubscribeEvents();
        }
    }
}
