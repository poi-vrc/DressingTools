/*
 * File: NoMissingScriptsHook.cs
 * Project: DressingTools
 * Created Date: Saturday, July 29th 2023, 10:31:11 am
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
using Chocopoi.DressingFramework.Detail.DK.Logging;
using Chocopoi.DressingFramework.Localization;
using Chocopoi.DressingTools.Localization;
using Chocopoi.DressingTools.OneConf.Wearable.Modules.BuiltIn.ArmatureMapping;
using UnityEngine;

namespace Chocopoi.DressingTools.Dresser.Default.Hooks
{
    // TODO: replace by reading missing scripts Unity files
    internal class NoMissingScriptsHook : IDefaultDresserHook
    {
        private static readonly I18nTranslator t = I18n.ToolTranslator;

        public bool ScanGameObject(DKReport report, string errorCode, GameObject gameObject)
        {
            var components = gameObject.GetComponents<Component>();
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    report.LogErrorLocalized(t, DefaultDresser.LogLabel, errorCode, gameObject.name);
                    return false;
                }
            }

            foreach (Transform child in gameObject.transform)
            {
                if (!ScanGameObject(report, errorCode, child.gameObject))
                {
                    return false;
                }
            }

            return true;
        }

        public bool Evaluate(DKReport report, DresserSettings settings, List<BoneMapping> boneMappings)
        {
            //scan avatar missing scripts
            var avatarResult = ScanGameObject(report, DefaultDresser.MessageCode.MissingScriptsDetectedInAvatar, settings.targetAvatar);

            if (!avatarResult)
            {
                return false;
            }

            //scan wearable missing scripts
            var clothesResult = ScanGameObject(report, DefaultDresser.MessageCode.MissingScriptsDetectedInWearable, settings.targetWearable);

            if (!clothesResult)
            {
                return false;
            }

            return avatarResult && clothesResult;
        }
    }
}
