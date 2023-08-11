using System;
using System.Collections.Generic;
using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.UIBase.Views;
using Chocopoi.DressingTools.Wearable;
using UnityEditor;
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
                armatureName = cabinet.avatarArmatureName;
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
                    var wearableArmature = DTRuntimeUtils.GuessArmature(_view.TargetWearable, _view.ArmatureMappingModule.wearableArmatureName);
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

                var toggles = _view.AnimationGenerationModule.wearableAnimationOnWear.toggles;
                var blendshapes = _view.AnimationGenerationModule.wearableAnimationOnWear.blendshapes;
                toggles.Clear();
                blendshapes.Clear();

                foreach (var trans in transforms)
                {
                    toggles.Add(new DTAnimationToggle()
                    {
                        path = DTRuntimeUtils.GetRelativePath(trans, _view.TargetWearable.transform),
                        state = true
                    });
                }
            }

            // generate blendshape syncs
            {
                // clear old syncs
                var blendshapeSyncs = _view.BlendshapeSyncModule.blendshapeSyncs;
                blendshapeSyncs.Clear();

                // find all avatar blendshapes
                var avatarSmrs = _view.TargetAvatar.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                var avatarSmrCache = new Dictionary<SkinnedMeshRenderer, string[]>();
                foreach (var avatarSmr in avatarSmrs)
                {
                    // transverse up to see if it is originated from ours or an existing wearable
                    var transform = avatarSmr.transform;
                    var cabinetWearableFound = false;
                    while (transform != null)
                    {
                        transform = transform.parent;
                        if (transform != null && (transform == _view.TargetWearable.transform || transform.TryGetComponent<DTCabinetWearable>(out var _)))
                        {
                            cabinetWearableFound = true;
                            break;
                        }

                        if (cabinetWearableFound)
                        {
                            break;
                        }
                    }

                    if (cabinetWearableFound)
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
                                blendshapeSyncs.Add(new DTAnimationBlendshapeSync()
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
            var config = new DTWearableConfig();

            DTEditorUtils.PrepareWearableConfig(config, _view.TargetAvatar, _view.TargetWearable);

            if (_view.UseArmatureMapping)
            {
                config.modules.Add(_view.ArmatureMappingModule);
            }

            if (_view.UseMoveRoot)
            {
                config.modules.Add(_view.MoveRootModule);
            }

            if (_view.UseAnimationGeneration)
            {
                config.modules.Add(_view.AnimationGenerationModule);
            }

            if (_view.UseBlendshapeSync)
            {
                config.modules.Add(_view.BlendshapeSyncModule);
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
