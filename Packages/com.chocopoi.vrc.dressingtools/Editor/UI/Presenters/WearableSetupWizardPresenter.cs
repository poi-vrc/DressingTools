using System.Collections.Generic;
using Chocopoi.DressingTools.UIBase.Views;
using Chocopoi.DressingTools.Wearable;
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
            }

            // TODO: show message

            // attempt to find wearable armature using avatar armature name
            var armature = DTRuntimeUtils.GuessArmature(_view.TargetWearable, armatureName);

            if (armature == null)
            {
                _view.UseArmatureMapping = false;
                _view.UseMoveRoot = true;

                // TODO: show message
            }
            else
            {
                if (armature.name != armatureName)
                {
                    // TODO: show message
                }

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
                        var child = _view.TargetWearable.transform.GetChild(i);
                        if (child != wearableArmature)
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
                        transforms.Add(_view.TargetWearable.transform.GetChild(i));
                    }
                }

                var toggles = _view.AnimationGenerationModule.wearableAnimationOnWear.toggles;
                var blendshapes = _view.AnimationGenerationModule.wearableAnimationOnWear.blendshapes;
                toggles.Clear();
                blendshapes.Clear();

                var wearableToggles = new List<DTAnimationToggle>();
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
                var avatarSmrs = _view.TargetAvatar.GetComponentsInChildren<SkinnedMeshRenderer>();
                var avatarSmrCache = new Dictionary<SkinnedMeshRenderer, string[]>();
                foreach (var avatarSmr in avatarSmrs)
                {
                    avatarSmrCache.Add(avatarSmr, GetBlendshapeNames(avatarSmr));
                }
                // TODO: skip existing wearables

                // pair wearable blendshape names with avatar
                var wearableSmrs = _view.TargetWearable.GetComponentsInChildren<SkinnedMeshRenderer>();
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
        }

        private void OnPreviousButtonClick()
        {
            if (_view.CurrentStep > 0)
            {
                _view.CurrentStep -= 1;
            }
        }

        private void AddTargetAvatarConfig(DTWearableConfig config)
        {
            var cabinet = DTEditorUtils.GetAvatarCabinet(_view.TargetAvatar);

            // try obtain armature name from cabinet
            if (cabinet == null)
            {
                // leave it empty
                config.targetAvatarConfig.armatureName = "";
            }
            else
            {
                config.targetAvatarConfig.armatureName = cabinet.avatarArmatureName;
            }

            // can't do anything
            if (_view.TargetAvatar == null || _view.TargetWearable == null)
            {
                return;
            }

            var avatarPrefabGuid = DTEditorUtils.GetGameObjectOriginalPrefabGuid(_view.TargetAvatar);
            var invalidAvatarPrefabGuid = avatarPrefabGuid == null || avatarPrefabGuid == "";

            config.targetAvatarConfig.guids.Clear();
            if (!invalidAvatarPrefabGuid)
            {
                // TODO: multiple guids
                config.targetAvatarConfig.guids.Add(avatarPrefabGuid);
            }

            var deltaPos = _view.TargetWearable.transform.position - _view.TargetAvatar.transform.position;
            var deltaRotation = _view.TargetWearable.transform.rotation * Quaternion.Inverse(_view.TargetAvatar.transform.rotation);
            config.targetAvatarConfig.worldPosition = new DTAvatarConfigVector3(deltaPos);
            config.targetAvatarConfig.worldRotation = new DTAvatarConfigQuaternion(deltaRotation);
            config.targetAvatarConfig.avatarLossyScale = new DTAvatarConfigVector3(_view.TargetAvatar.transform.lossyScale);
            config.targetAvatarConfig.wearableLossyScale = new DTAvatarConfigVector3(_view.TargetWearable.transform.lossyScale);
        }

        private void AddMetaInfo(DTWearableConfig config)
        {
            if (_view.TargetWearable == null)
            {
                return;
            }

            config.info.name = _view.TargetWearable.name;
            config.info.author = "";
            config.info.description = "";
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

                var config = new DTWearableConfig();

                config.configVersion = DTWearableConfig.CurrentConfigVersion;

                AddMetaInfo(config);
                AddTargetAvatarConfig(config);

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
                _view.RaiseDoAddToCabinetEvent();
            }
        }

        private void UpdateView()
        {

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
