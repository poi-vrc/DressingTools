/*
 * File: LegacyPresenter.cs
 * Project: DressingTools
 * Created Date: Wednesday, September 10th 2023, 2:45:04 pm
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
using Chocopoi.DressingFramework;
using Chocopoi.DressingFramework.Detail.DK.Logging;
using Chocopoi.DressingTools.Components.Generic;
using Chocopoi.DressingTools.Components.Modifiers;
using Chocopoi.DressingTools.Dresser;
using Chocopoi.DressingTools.Dresser.Default;
using Chocopoi.DressingTools.Passes;
using Chocopoi.DressingTools.UI.Views;
using Chocopoi.DressingTools.UI.Views.Modules;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Presenters
{
    internal class LegacyPresenter
    {
        private const string TestModeAnimationControllerPath = "Packages/com.chocopoi.vrc.dressingtools/Animations/TestModeAnimationController.controller";
        private static readonly DefaultDresser DefaultDresser = new DefaultDresser();
        private static AnimatorController s_testModeAnimationController;

        private ILegacyView _view;
        private DKReport _report;

        public LegacyPresenter(ILegacyView view)
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
            _view.RenameClothesNameButtonClick += OnRenameClothesNameButtonClick;
            _view.CheckAndPreviewButtonClick += OnCheckAndPreviewButtonClick;
            _view.TestNowButtonClick += OnTestNowButtonClick;
            _view.DressNowButtonClick += OnDressNowButtonClick;
        }

        private void UnsubscribeEvents()
        {
            _view.Load -= OnLoad;
            _view.Unload -= OnUnload;

            _view.ForceUpdateView -= OnForceUpdateView;
            _view.TargetAvatarOrWearableChange -= OnTargetAvatarOrWearableChange;
            _view.RenameClothesNameButtonClick -= OnRenameClothesNameButtonClick;
            _view.CheckAndPreviewButtonClick -= OnCheckAndPreviewButtonClick;
            _view.TestNowButtonClick -= OnTestNowButtonClick;
            _view.DressNowButtonClick -= OnDressNowButtonClick;
        }

        private void OnTargetAvatarOrWearableChange()
        {
            _view.Prefix = "";
            if (_view.TargetClothes != null)
            {
                _view.Suffix = $" ({_view.TargetClothes.name})";
                _view.NewClothesName = _view.TargetClothes.name;
            }

            if (!_view.UseCustomArmatureName)
            {
                _view.AvatarArmatureObjectName = "Armature";
                _view.ClothesArmatureObjectName = "Armature";
            }
        }

        private void OnRenameClothesNameButtonClick()
        {
            if (_view.NewClothesName != null && _view.NewClothesName != "" && _view.TargetClothes != null)
            {
                _view.TargetClothes.name = _view.NewClothesName;
            }
        }

        private bool VerifySettings()
        {
            // TODO: update these logs
            if (_view.TargetAvatar == null || _view.TargetClothes == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(_view.ClothesArmatureObjectName))
            {
                return false;
            }

            var sourceArmature = _view.TargetClothes.transform.Find(_view.ClothesArmatureObjectName);

            if (sourceArmature == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(_view.AvatarArmatureObjectName))
            {
                return false;
            }

            return true;
        }

        private void GenerateMappings(out List<ObjectMapping> objectMappings, out List<ITag> tags)
        {
            if (!VerifySettings())
            {
                objectMappings = null;
                tags = null;
                return;
            }

            _report = new DKReport();
            var sourceArmature = _view.TargetClothes.transform.Find(_view.ClothesArmatureObjectName);
            var dresserSettings = new DefaultDresserSettings()
            {
                DynamicsOption = (DefaultDresserSettings.DynamicsOptions)_view.DynamicsOption,
                SourceArmature = sourceArmature,
                TargetArmaturePath = _view.AvatarArmatureObjectName,
            };
            DefaultDresser.Execute(_report, _view.TargetAvatar, dresserSettings, out objectMappings, out tags);
            UpdateReportViewData();
        }

        private void CleanUpPreviewObjects()
        {
            var allObjects = Object.FindObjectsOfType<GameObject>();
            foreach (var obj in allObjects)
            {
                if (obj != null && obj.name.StartsWith("DTPreview_"))
                {
                    Object.DestroyImmediate(obj);
                }
            }
        }

        private void GenerateMappingsAndPreview()
        {
            if (_view.TargetAvatar == null || _view.TargetClothes == null)
            {
                return;
            }

            if (PrefabUtility.IsPartOfAnyPrefab(_view.TargetClothes))
            {
                // clone the prefab
                GameObject clonedPrefab;

                // check if in scene or not
                if (PrefabUtility.GetPrefabInstanceStatus(_view.TargetClothes) != PrefabInstanceStatus.NotAPrefab)
                {
                    // if in scene, we cannot clone with prefab connection or the overrides will be gone
                    clonedPrefab = Object.Instantiate(_view.TargetClothes);

                    // rename and disable original
                    clonedPrefab.name = _view.TargetClothes.name;
                    _view.TargetClothes.name += "-Prefab";
                    _view.TargetClothes.SetActive(false);

                    // set to original parent
                    clonedPrefab.transform.SetParent(_view.TargetClothes.transform.parent);

                    if (DKEditorUtils.IsGrandParent(_view.TargetAvatar.transform, _view.TargetClothes.transform))
                    {
                        // if the original clothes is already inside the avatar, unparent it to prevent confusion
                        _view.TargetClothes.transform.SetParent(null);
                    }
                }
                else
                {
                    // create prefab connection
                    clonedPrefab = (GameObject)PrefabUtility.InstantiatePrefab(_view.TargetClothes);
                    clonedPrefab.name = _view.TargetClothes.name;

                    // unpack the outermost root of the prefab
                    PrefabUtility.UnpackPrefabInstance(clonedPrefab, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
                    clonedPrefab.transform.SetParent(_view.TargetAvatar.transform);
                }

                // set the clone to be the target clothes to dress
                _view.TargetClothes = clonedPrefab;
            }

            // clean up old objects
            CleanUpPreviewObjects();

            // replicate the v1 behaviour to generate a preview GameObject
            var avatarNewName = "DTPreview_" + _view.TargetAvatar.name;

            // create a copy of the avatar and wearable
            var targetAvatar = Object.Instantiate(_view.TargetAvatar);
            targetAvatar.name = avatarNewName;

            var newAvatarPosition = targetAvatar.transform.position;
            newAvatarPosition.x -= 20;
            targetAvatar.transform.position = newAvatarPosition;

            // if clothes is not inside avatar, we instantiate a new copy
            GameObject targetClothes;
            if (!DKEditorUtils.IsGrandParent(_view.TargetAvatar.transform, _view.TargetClothes.transform))
            {
                targetClothes = Object.Instantiate(_view.TargetClothes);

                var newClothesPosition = targetClothes.transform.position;
                newClothesPosition.x -= 20;
                targetClothes.transform.position = newClothesPosition;
            }
            else
            {
                // otherwise, we find the inner wearable and use it
                targetClothes = targetAvatar.transform.Find(_view.TargetClothes.name).gameObject;
            }

            //add animation controller
            if (targetAvatar.TryGetComponent<Animator>(out var animator))
            {
                animator.runtimeAnimatorController = s_testModeAnimationController;
            }

            //add dummy focus sceneview script
            targetAvatar.AddComponent<DummyFocusSceneViewScript>();

            // parent to avatar
            targetClothes.name = _view.TargetClothes.name;
            targetClothes.transform.SetParent(targetAvatar.transform);

            // dry run to see if can generate first
            GenerateMappings(out var objectMappings, out var tags);
            if (objectMappings == null || tags == null)
            {
                Debug.LogError("[DressingToolsLegacy] No mappings generated in dry run, aborting");
                return;
            }

            WriteChanges(targetAvatar, targetClothes);

            Selection.activeGameObject = targetAvatar;
            SceneView.FrameLastActiveSceneView();
        }

        private void WriteChanges(GameObject targetAvatar, GameObject targetClothes)
        {
            if (!VerifySettings())
            {
                return;
            }

            var sourceArmature = targetClothes.transform.Find(_view.ClothesArmatureObjectName);

            var ca = new ComponentApplier(targetAvatar);

            if (_view.GroupDynamics)
            {
                var groupContainer = new GameObject("DT_Dynamics");
                groupContainer.transform.SetParent(targetClothes.transform);

                var groupDynComp = groupContainer.AddComponent<DTGroupDynamics>();
                groupDynComp.SearchMode = DTGroupDynamics.DynamicsSearchMode.ControlRoot;
                groupDynComp.IncludeTransforms.Add(targetClothes.transform);
                groupDynComp.enabled = false;
                groupDynComp.SetToCurrentState = true;
                groupDynComp.SeparateGameObjects = _view.GroupDynamicsSeparateGameObjects;

                if (!ca.Apply(groupDynComp, true))
                {
                    Debug.LogError("[DressingTools] Error applying group dynamics component");
                    return;
                }
            }

            var armMapComp = targetClothes.AddComponent<DTArmatureMapping>();
            armMapComp.DresserType = DTArmatureMapping.DresserTypes.Default;
            armMapComp.DresserDefaultConfig.DynamicsOption = (DTArmatureMapping.AMDresserDefaultConfig.DynamicsOptions)_view.DynamicsOption;
            armMapComp.Mode = DTArmatureMapping.MappingMode.Auto;
            armMapComp.SourceArmature = sourceArmature;
            armMapComp.TargetArmaturePath = _view.AvatarArmatureObjectName;
            armMapComp.GroupBones = _view.GroupBones;
            armMapComp.Prefix = _view.Prefix;
            armMapComp.Suffix = _view.Suffix;
            armMapComp.PreventDuplicateNames = _view.PreventDuplicateNames;

            if (!ca.Apply(armMapComp, true))
            {
                Debug.LogError("[DressingTools] Error applying armature mapping component");
                return;
            }
        }

        private void OnCheckAndPreviewButtonClick()
        {
            if (_view.TargetClothes == null || _view.TargetClothes.name == "")
            {
                return;
            }

            GenerateMappingsAndPreview();
        }

        private void OnTestNowButtonClick()
        {
            EditorApplication.EnterPlaymode();
        }

        private void OnDressNowButtonClick()
        {
            // dry run to see if can generate first
            GenerateMappings(out var objectMappings, out var tags);
            if (objectMappings == null || tags == null)
            {
                return;
            }

            if (!_view.ShowDressConfirmDialog())
            {
                return;
            }

            WriteChanges(_view.TargetAvatar, _view.TargetClothes);

            // reset
            _view.TargetClothes = null;
            _report = null;
            _view.ReportData = null;
            CleanUpPreviewObjects();
            UpdateView();

            _view.ShowCompletedDialog();
        }

        private void OnForceUpdateView()
        {
            UpdateView();
        }

        private void UpdateReportViewData()
        {
            if (_report != null)
            {
                _view.ReportData = new ReportData();
                var logEntries = _report.GetLogEntriesAsDictionary();

                _view.ReportData.errorMsgs.Clear();
                if (logEntries.ContainsKey(DressingFramework.Logging.LogType.Error))
                {
                    foreach (var logEntry in logEntries[DressingFramework.Logging.LogType.Error])
                    {
                        _view.ReportData.errorMsgs.Add(logEntry.message);
                    }
                }

                _view.ReportData.warnMsgs.Clear();
                if (logEntries.ContainsKey(DressingFramework.Logging.LogType.Warning))
                {
                    foreach (var logEntry in logEntries[DressingFramework.Logging.LogType.Warning])
                    {
                        _view.ReportData.warnMsgs.Add(logEntry.message);
                    }
                }

                _view.ReportData.infoMsgs.Clear();
                if (logEntries.ContainsKey(DressingFramework.Logging.LogType.Info))
                {
                    foreach (var logEntry in logEntries[DressingFramework.Logging.LogType.Info])
                    {
                        _view.ReportData.infoMsgs.Add(logEntry.message);
                    }
                }
            }
            else
            {
                _report = null;
            }
        }

        private void UpdateView()
        {
            _view.CurrentVersion = UpdateChecker.CurrentVersion;

            UpdateReportViewData();

            if (!_view.UseCustomArmatureName)
            {
                _view.AvatarArmatureObjectName = "Armature";
                _view.ClothesArmatureObjectName = "Armature";
            }
        }

        private void OnLoad()
        {
            if (s_testModeAnimationController == null)
            {
                s_testModeAnimationController = AssetDatabase.LoadAssetAtPath<AnimatorController>(TestModeAnimationControllerPath);
                if (s_testModeAnimationController == null)
                {
                    Debug.LogError("[DressingToolsLegacy] Could not load \"TestModeAnimationController\" from \"Assets/chocopoi/DressingTools/Animations\". Did you move it to another location?");
                }
            }

            UpdateView();
        }

        private void OnUnload()
        {
            UnsubscribeEvents();
        }
    }
}
