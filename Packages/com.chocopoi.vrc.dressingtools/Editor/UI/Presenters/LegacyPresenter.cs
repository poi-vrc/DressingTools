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
using Chocopoi.DressingFramework.Cabinet;
using Chocopoi.DressingFramework.Dresser;
using Chocopoi.DressingFramework.Logging;
using Chocopoi.DressingFramework.Serialization;
using Chocopoi.DressingFramework.UI;
using Chocopoi.DressingFramework.Wearable;
using Chocopoi.DressingFramework.Wearable.Modules;
using Chocopoi.DressingFramework.Wearable.Modules.BuiltIn;
using Chocopoi.DressingTools.Dresser;
using Chocopoi.DressingTools.Dresser.Default;
using Chocopoi.DressingTools.UIBase.Views;
using Newtonsoft.Json;
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
            _view.ConfigChange += OnConfigChange;
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
            _view.ConfigChange -= OnConfigChange;
            _view.CheckAndPreviewButtonClick -= OnCheckAndPreviewButtonClick;
            _view.TestNowButtonClick -= OnTestNowButtonClick;
            _view.DressNowButtonClick -= OnDressNowButtonClick;
        }

        private void OnTargetAvatarOrWearableChange()
        {
            // attempt to get settings from cabinet
            var cabinet = DKEditorUtils.GetAvatarCabinet(_view.TargetAvatar, false);
            if (cabinet != null)
            {
                if (!CabinetConfigUtility.TryDeserialize(cabinet.ConfigJson, out var cabinetConfig))
                {
                    Debug.LogWarning("[DressingToolsLegacy] Unable to deserialize cabinet config, ignoring and creating a new config");
                    cabinet.ConfigJson = CabinetConfigUtility.Serialize(new CabinetConfig());
                }

                _view.ShowHasCabinetHelpbox = true;
                _view.AvatarArmatureObjectName = cabinetConfig.avatarArmatureName;
                _view.GroupDynamics = cabinetConfig.groupDynamics;
                _view.GroupDynamicsSeparateGameObjects = cabinetConfig.groupDynamicsSeparateGameObjects;
            }
            else
            {
                _view.ShowHasCabinetHelpbox = false;
            }

            if (!_view.UseCustomArmatureName)
            {
                _view.AvatarArmatureObjectName = "Armature";
                _view.ClothesArmatureObjectName = "Armature";
            }

            GenerateMappings();
        }

        private void OnRenameClothesNameButtonClick()
        {
            if (_view.NewClothesName != null && _view.NewClothesName != "" && _view.TargetClothes != null)
            {
                _view.TargetClothes.name = _view.NewClothesName;
            }
        }

        private void OnConfigChange()
        {
            GenerateMappings();
        }

        private DresserSettings GetDresserSettings()
        {
            return new DefaultDresserSettings()
            {
                targetAvatar = _view.TargetAvatar,
                targetWearable = _view.TargetClothes,
                dynamicsOption = (DefaultDresserDynamicsOption)_view.DynamicsOption,
                avatarArmatureName = _view.AvatarArmatureObjectName,
                wearableArmatureName = _view.ClothesArmatureObjectName
            };
        }

        private List<ArmatureMappingWearableModuleConfig.BoneMapping> GenerateMappings()
        {
            var dresserSettings = GetDresserSettings();
            _report = DefaultDresser.Execute(dresserSettings, out var boneMappings);
            UpdateReportViewData();
            return boneMappings;
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
            GameObject targetWearable;
            if (!DKEditorUtils.IsGrandParent(_view.TargetAvatar.transform, _view.TargetClothes.transform))
            {
                targetWearable = Object.Instantiate(_view.TargetClothes);

                var newClothesPosition = targetWearable.transform.position;
                newClothesPosition.x -= 20;
                targetWearable.transform.position = newClothesPosition;
            }
            else
            {
                // otherwise, we find the inner wearable and use it
                targetWearable = targetAvatar.transform.Find(_view.TargetClothes.name).gameObject;
            }

            //add animation controller
            if (targetAvatar.TryGetComponent<Animator>(out var animator))
            {
                animator.runtimeAnimatorController = s_testModeAnimationController;
            }

            //add dummy focus sceneview script
            targetAvatar.AddComponent<DummyFocusSceneViewScript>();

            // parent to avatar
            targetWearable.name = _view.TargetClothes.name;
            targetWearable.transform.SetParent(targetAvatar.transform);

            if (targetWearable.TryGetComponent<DTWearable>(out var existingComp))
            {
                if (!_view.ShowExistingWearableConfigIgnoreConfirmDialog())
                {
                    return;
                }

                Object.DestroyImmediate(existingComp);
            }

            // dry run to see if can generate first
            if (GenerateMappings() == null)
            {
                Debug.LogError("[DressingToolsLegacy] No mappings generated in dry run, aborting");
                return;
            }

            // create a cabinet or use existing
            var cabinet = DKEditorUtils.GetAvatarCabinet(targetAvatar, true);
            AddClothesToCabinet(cabinet, targetAvatar, targetWearable);

            // run cabinet applier
            var report = new DKReport();
            new CabinetApplier(report, cabinet).RunStages();
            if (report.HasLogType(DressingFramework.Logging.LogType.Error))
            {
                ReportWindow.ShowWindow(report);
            }

            Selection.activeGameObject = targetAvatar;
            SceneView.FrameLastActiveSceneView();
        }

        private void AddClothesToCabinet(DTCabinet cabinet, GameObject targetAvatar, GameObject targetWearable)
        {
            if (!CabinetConfigUtility.TryDeserialize(cabinet.ConfigJson, out var cabinetConfig))
            {
                Debug.LogWarning("[DressingToolsLegacy] Unable to deserialize cabinet config, ignoring and create a new one");
                cabinetConfig = new CabinetConfig();
                cabinet.ConfigJson = CabinetConfigUtility.Serialize(cabinetConfig);
            }

            // write cabinet config
            cabinetConfig.groupDynamics = _view.GroupDynamics;
            cabinetConfig.groupDynamicsSeparateGameObjects = _view.GroupDynamicsSeparateGameObjects;

            // create a wearable config
            var wearableConfig = new WearableConfig();
            DTEditorUtils.PrepareWearableConfig(wearableConfig, targetAvatar, targetWearable);
            wearableConfig.modules.Add(new WearableModule()
            {
                moduleName = ArmatureMappingWearableModuleConfig.ModuleIdentifier,
                config = new ArmatureMappingWearableModuleConfig()
                {
                    dresserName = DefaultDresser.GetType().FullName,
                    wearableArmatureName = _view.ClothesArmatureObjectName,
                    boneMappingMode = ArmatureMappingWearableModuleConfig.BoneMappingMode.Auto,
                    boneMappings = null,
                    serializedDresserConfig = JsonConvert.SerializeObject(GetDresserSettings()),
                    removeExistingPrefixSuffix = _view.RemoveExistingPrefixSuffix,
                    groupBones = _view.GroupBones
                }
            });

            // add the wearable config to the preview avatar
            if (!cabinet.AddWearable(wearableConfig, targetWearable))
            {
                Debug.LogWarning("[DressingToolsLegacy] Unable to add cabinet wearable to dummy avatar");
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
            if (GenerateMappings() == null)
            {
                return;
            }

            if (!_view.ShowDressConfirmDialog())
            {
                return;
            }

            var cabinet = DKEditorUtils.GetAvatarCabinet(_view.TargetAvatar, true);
            AddClothesToCabinet(cabinet, _view.TargetAvatar, _view.TargetClothes);
            Selection.activeGameObject = _view.TargetAvatar;
            SceneView.FrameLastActiveSceneView();


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
