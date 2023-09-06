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
using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Dresser;
using Chocopoi.DressingTools.Dresser.Default;
using Chocopoi.DressingTools.Lib.Cabinet;
using Chocopoi.DressingTools.Lib.Dresser;
using Chocopoi.DressingTools.Lib.Logging;
using Chocopoi.DressingTools.Lib.Wearable;
using Chocopoi.DressingTools.Lib.Wearable.Modules;
using Chocopoi.DressingTools.UIBase.Views;
using Chocopoi.DressingTools.Wearable.Modules;
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
        private DTReport _report;

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
            var cabinet = DTEditorUtils.GetAvatarCabinet(_view.TargetAvatar, false);
            if (cabinet != null)
            {
                if (!CabinetConfig.TryDeserialize(cabinet.configJson, out var cabinetConfig))
                {
                    Debug.LogWarning("[DressingToolsLegacy] Unable to deserialize cabinet config, ignoring and creating a new config");
                    cabinet.configJson = new CabinetConfig().Serialize();
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

        private List<BoneMapping> GenerateMappings()
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
            if (!DTEditorUtils.IsGrandParent(_view.TargetAvatar.transform, _view.TargetClothes.transform))
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

            if (targetWearable.TryGetComponent<DTCabinetWearable>(out var existingComp))
            {
                if (!EditorUtility.DisplayDialog("DressingTools", "The clothes has already a wearable configuration attached.\nContinuing will ignore existing configuration. Are you sure?", "Yes", "No"))
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
            var cabinet = DTEditorUtils.GetAvatarCabinet(targetAvatar, true);
            AddClothesToCabinet(cabinet, targetAvatar, targetWearable);

            // run cabinet applier
            var report = new DTReport();
            new CabinetApplier(report, cabinet).Execute();
            if (report.HasLogType(DTReportLogType.Error))
            {
                ReportWindow.ShowWindow(report);
            }

            Selection.activeGameObject = targetAvatar;
            SceneView.FrameLastActiveSceneView();
        }

        private void AddClothesToCabinet(DTCabinet cabinet, GameObject targetAvatar, GameObject targetWearable)
        {
            if (!CabinetConfig.TryDeserialize(cabinet.configJson, out var cabinetConfig))
            {
                Debug.LogWarning("[DressingToolsLegacy] Unable to deserialize cabinet config, ignoring and create a new one");
                cabinetConfig = new CabinetConfig();
                cabinet.configJson = cabinetConfig.Serialize();
            }

            // write cabinet config
            cabinetConfig.groupDynamics = _view.GroupDynamics;
            cabinetConfig.groupDynamicsSeparateGameObjects = _view.GroupDynamicsSeparateGameObjects;

            // create a wearable config
            var wearableConfig = new WearableConfig();
            DTEditorUtils.PrepareWearableConfig(wearableConfig, targetAvatar, targetWearable);
            wearableConfig.modules.Add(new WearableModule()
            {
                moduleName = ArmatureMappingWearableModuleProvider.MODULE_IDENTIFIER,
                config = new ArmatureMappingWearableModuleConfig()
                {
                    dresserName = DefaultDresser.GetType().FullName,
                    wearableArmatureName = _view.ClothesArmatureObjectName,
                    boneMappingMode = BoneMappingMode.Auto,
                    boneMappings = null,
                    serializedDresserConfig = JsonConvert.SerializeObject(GetDresserSettings()),
                    removeExistingPrefixSuffix = _view.RemoveExistingPrefixSuffix,
                    groupBones = _view.GroupBones
                }
            });

            // add the wearable config to the preview avatar
            if (!DTEditorUtils.AddCabinetWearable(cabinetConfig, targetAvatar, wearableConfig, targetWearable))
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

            _view.DressNowConfirm = false;
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
                EditorUtility.DisplayDialog("DressingTools", "Error validating before adding to cabinet: No mappings generated.", "OK");
                return;
            }

            if (!EditorUtility.DisplayDialog("DressingTools", "Are you sure to add this clothes?\nYou can remove it later from the cabinet editor or remove directly from the hierachy.", "Yes", "No"))
            {
                return;
            }

            var cabinet = DTEditorUtils.GetAvatarCabinet(_view.TargetAvatar, true);
            AddClothesToCabinet(cabinet, _view.TargetAvatar, _view.TargetClothes);
            Selection.activeGameObject = _view.TargetAvatar;
            SceneView.FrameLastActiveSceneView();


            // reset
            _view.TargetClothes = null;
            _report = null;
            _view.ReportData = null;
            CleanUpPreviewObjects();
            UpdateView();

            EditorUtility.DisplayDialog("DressingTools", "Completed.", "OK");

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
                if (logEntries.ContainsKey(DTReportLogType.Error))
                {
                    foreach (var logEntry in logEntries[DTReportLogType.Error])
                    {
                        _view.ReportData.errorMsgs.Add(logEntry.message);
                    }
                }

                _view.ReportData.warnMsgs.Clear();
                if (logEntries.ContainsKey(DTReportLogType.Warning))
                {
                    foreach (var logEntry in logEntries[DTReportLogType.Warning])
                    {
                        _view.ReportData.warnMsgs.Add(logEntry.message);
                    }
                }

                _view.ReportData.infoMsgs.Clear();
                if (logEntries.ContainsKey(DTReportLogType.Info))
                {
                    foreach (var logEntry in logEntries[DTReportLogType.Info])
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
                    Debug.LogError("[DressingTools] Could not load \"TestModeAnimationController\" from \"Assets/chocopoi/DressingTools/Animations\". Did you move it to another location?");
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
