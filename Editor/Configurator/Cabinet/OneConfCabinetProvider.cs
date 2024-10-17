/*
 * Copyright (c) 2024 chocopoi
 * 
 * This file is part of DressingTools.
 * 
 * DressingTools is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 * 
 * DressingTools is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with DressingFramework. If not, see <https://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;
using Chocopoi.DressingTools.Localization;
using Chocopoi.DressingFramework.Localization;
using Chocopoi.DressingTools.OneConf;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using Chocopoi.DressingTools.OneConf.Serialization;
using Chocopoi.DressingTools.OneConf.Cabinet.Modules.BuiltIn;
using Chocopoi.DressingTools.OneConf.Cabinet;

namespace Chocopoi.DressingTools.Configurator.Cabinet
{
    internal class OneConfCabinetProvider : IWardrobeProvider
    {
        private static readonly I18nTranslator t = I18n.ToolTranslator;

        private readonly GameObject _avatarGameObject;

        public OneConfCabinetProvider(GameObject avatarGameObject)
        {
            _avatarGameObject = avatarGameObject;
        }

        public VisualElement CreateView()
        {
            var container = new VisualElement();

            var cabinet = OneConfUtils.GetAvatarCabinet(_avatarGameObject);
            if (cabinet == null || !CabinetConfigUtility.TryDeserialize(cabinet.ConfigJson, out var config))
            {
                // TODO: show message
                return container;
            }

            var armatureNameField = new TextField(t._("editor.main.avatar.settings.oneConf.textField.avatarArmatureName")) { value = config.avatarArmatureName };
            container.Add(armatureNameField);
            var groupDynToggle = new Toggle(t._("editor.main.avatar.settings.oneConf.toggle.groupDynamics")) { value = config.groupDynamics };
            container.Add(groupDynToggle);
            var groupDynSepToggle = new Toggle(t._("editor.main.avatar.settings.oneConf.toggle.groupDynamicsSeparate")) { value = config.groupDynamicsSeparateGameObjects };
            container.Add(groupDynSepToggle);

            var cabAnimConfig = config.FindModuleConfig<CabinetAnimCabinetModuleConfig>();
            if (cabAnimConfig == null)
            {
                cabAnimConfig = new CabinetAnimCabinetModuleConfig();
                config.modules.Add(new CabinetModule()
                {
                    config = cabAnimConfig,
                    moduleName = CabinetAnimCabinetModuleConfig.ModuleIdentifier
                });
            }

            var useThumbnailsToggle = new Toggle(t._("editor.main.avatar.settings.oneConf.toggle.useThumbnailsAsMenuIcons")) { value = cabAnimConfig.thumbnails };
            container.Add(useThumbnailsToggle);
            var resetCustomizablesOnSwitch = new Toggle(t._("editor.main.avatar.settings.oneConf.toggle.resetCustomizablesOnSwitch")) { value = cabAnimConfig.resetCustomizablesOnSwitch };
            container.Add(resetCustomizablesOnSwitch);

            container.Add(new IMGUIContainer(() => EditorGUILayout.HelpBox(t._("editor.main.avatar.settings.oneConf.helpbox.installPathDescription"), MessageType.Info)));

            var installPathField = new TextField(t._("editor.main.avatar.settings.oneConf.textField.menuInstallPath")) { value = cabAnimConfig.menuInstallPath };
            container.Add(installPathField);
            var itemNameField = new TextField(t._("editor.main.avatar.settings.oneConf.textField.menuItemName")) { value = cabAnimConfig.menuItemName };
            container.Add(itemNameField);

            var networkSyncedToggle = new Toggle(t._("editor.main.avatar.settings.oneConf.toggle.networkSynced")) { value = cabAnimConfig.networkSynced };
            container.Add(networkSyncedToggle);
            var savedToggle = new Toggle(t._("editor.main.avatar.settings.oneConf.toggle.saved")) { value = cabAnimConfig.saved };
            container.Add(savedToggle);

            return container;
        }

        public List<IConfigurableOutfit> GetOutfits()
        {
            var wearables = OneConfUtils.GetCabinetWearables(_avatarGameObject);
            var outfits = new List<IConfigurableOutfit>();
            foreach (var wearable in wearables)
            {
                outfits.Add(new OneConfConfigurableOutfit(_avatarGameObject, wearable));
            }
            return outfits;
        }

        public void RemoveOutfit(IConfigurableOutfit outfit)
        {
            throw new System.NotImplementedException();
        }
    }
}
