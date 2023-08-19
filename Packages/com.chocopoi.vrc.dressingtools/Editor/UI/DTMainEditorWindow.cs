/*
 * File: DTMainEditorWindow.cs
 * Project: DressingTools
 * Created Date: Thursday, August 10th 2023, 12:27:04 am
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

using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Dresser;
using Chocopoi.DressingTools.Dresser.Default;
using Chocopoi.DressingTools.UI.View;
using Chocopoi.DressingTools.Wearable;
using Chocopoi.DressingTools.Wearable.Modules;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI
{
    public class DTMainEditorWindow : EditorWindow
    {
        // note that in order for a menu item in "GameObject/" to be propagated to the
        // hierarchy Create dropdown and hierarchy context menu, it must be grouped with
        // the other GameObject creation menu items. This can be achieved by setting its priority to 10 
        private const int MenuItemPriority = 20;
        private MainView _view;

        [MenuItem("Tools/chocopoi/DressingTools", false, 0)]
        static void ShowWindow()
        {
            var window = (DTMainEditorWindow)GetWindow(typeof(DTMainEditorWindow));
            window.titleContent = new GUIContent("DressingTools");
            window.Show();
        }

        [MenuItem("GameObject/DressingTools/Auto-setup wearable (Mappings Only)", false, MenuItemPriority)]
        static void QuickAutoSetup(MenuCommand menuCommand)
        {
            if (!(menuCommand.context is GameObject))
            {
                return;
            }

            var wearable = (GameObject)menuCommand.context;

            // find the avatar
            var avatarTransform = wearable.transform.parent;

            if (avatarTransform == null || !avatarTransform.TryGetComponent(out DTCabinet cabinet))
            {
                // no parent or grandparents has the cabinet
                EditorUtility.DisplayDialog("DressingTools", "The avatar has no cabinet attached.", "OK");
                return;
            }

            var config = new WearableConfig();
            DTEditorUtils.PrepareWearableConfig(config, cabinet.avatarGameObject, wearable);

            var armatureName = cabinet.avatarArmatureName;

            // attempt to find wearable armature using avatar armature name
            var armature = DTRuntimeUtils.GuessArmature(wearable, armatureName);

            if (armature == null)
            {
                // TODO: ask to select a location for move to
                EditorUtility.DisplayDialog("DressingTools", "Cannot detect Armature GameObject automatically, please use the wizard instead.", "OK");
                return;
            }
            else
            {
                if (armature.name != armatureName)
                {
                    // TODO: show message
                }

                var dresserSettings = new DefaultDresserSettings()
                {
                    targetAvatar = cabinet.avatarGameObject,
                    targetWearable = wearable,
                    dynamicsOption = DefaultDresserDynamicsOption.RemoveDynamicsAndUseParentConstraint
                };

                var dresser = new DefaultDresser();
                var report = dresser.Execute(dresserSettings, out _);

                if (report.HasLogType(Logging.DTReportLogType.Error))
                {
                    ReportWindow.ShowWindow(report);
                    EditorUtility.DisplayDialog("DressingTools", "Default dresser has errors processing this wearable automatically, please use the wizard instead.", "OK");
                    return;
                }

                var armatureMappingModule = new ArmatureMappingModule
                {
                    dresserName = typeof(DefaultDresser).FullName,
                    wearableArmatureName = armature.name,
                    boneMappingMode = BoneMappingMode.Auto,
                    boneMappings = null,
                    serializedDresserConfig = JsonConvert.SerializeObject(dresserSettings),
                    removeExistingPrefixSuffix = true,
                    groupBones = true
                };

                config.modules.Add(armatureMappingModule);
            }

            DTEditorUtils.AddCabinetWearable(cabinet, config, wearable);
        }

        [MenuItem("GameObject/DressingTools/Setup wearable with wizard", true, MenuItemPriority)]
        [MenuItem("GameObject/DressingTools/Auto-setup wearable (Mappings Only)", true, MenuItemPriority)]
        public static bool ValidateSetupMenus()
        {
            // no selected objects
            if (Selection.objects.Length == 0)
            {
                return false;
            }

            foreach (var obj in Selection.objects)
            {
                if (!(obj is GameObject go))
                {
                    // not a GameObject
                    return false;
                }

                // find the avatar
                var avatarTransform = go.transform.parent;

                if (avatarTransform == null || !avatarTransform.TryGetComponent(out DTCabinet _))
                {
                    // no parent or grandparents has the cabinet
                    return false;
                }
            }

            return true;
        }

        [MenuItem("GameObject/DressingTools/Setup wearable with wizard", false, MenuItemPriority)]
        public static void StartSetupWizard(MenuCommand menuCommand)
        {
            if (!(menuCommand.context is GameObject))
            {
                return;
            }

            var wearable = (GameObject)menuCommand.context;

            // find the avatar
            var avatarTransform = wearable.transform.parent;

            if (avatarTransform == null || !avatarTransform.TryGetComponent(out DTCabinet cabinet))
            {
                // no parent or grandparents has the cabinet
                EditorUtility.DisplayDialog("DressingTools", "The avatar has no cabinet attached.", "OK");
                return;
            }

            var window = (DTMainEditorWindow)GetWindow(typeof(DTMainEditorWindow));
            window.titleContent = new GUIContent("DressingTools");
            //window.Show();
            window._view.SelectedTab = 1;
            window._view.StartSetupWizard(avatarTransform.gameObject, wearable);
        }

        public DTMainEditorWindow()
        {
            _view = new MainView();
        }

        public void SelectCabinet(DTCabinet cabinet) => _view.SelectCabinet(cabinet);

        public void OnEnable()
        {
            _view.OnEnable();
        }

        public void OnDisable()
        {
            _view.OnDisable();
        }

        public void OnGUI()
        {
            _view.OnGUI();
        }
    }
}
