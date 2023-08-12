/*
 * File: DTDefaultDresserSettings.cs
 * Project: DressingTools
 * Created Date: Saturday, July 22nd 2023, 12:36:56 am
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

using UnityEditor;

namespace Chocopoi.DressingTools.Dresser.Default
{
    public enum DTDefaultDresserDynamicsOption
    {
        RemoveDynamicsAndUseParentConstraint = 0,
        KeepDynamicsAndUseParentConstraintIfNecessary = 1,
        IgnoreTransform = 2,
        CopyDynamics = 3,
        IgnoreAll = 4,
    }

    public class DTDefaultDresserSettings : DTDresserSettings
    {
        public DTDefaultDresserDynamicsOption dynamicsOption;

        public DTDefaultDresserSettings()
        {
            // default settings
            dynamicsOption = DTDefaultDresserDynamicsOption.RemoveDynamicsAndUseParentConstraint;
        }

        private DTDefaultDresserDynamicsOption ConvertIntToDynamicsOption(int dynamicsOption)
        {
            switch (dynamicsOption)
            {
                case 1:
                    return DTDefaultDresserDynamicsOption.KeepDynamicsAndUseParentConstraintIfNecessary;
                case 2:
                    return DTDefaultDresserDynamicsOption.IgnoreTransform;
                case 3:
                    return DTDefaultDresserDynamicsOption.CopyDynamics;
                case 4:
                    return DTDefaultDresserDynamicsOption.IgnoreAll;
                default:
                case 0:
                    return DTDefaultDresserDynamicsOption.RemoveDynamicsAndUseParentConstraint;
            }
        }

#if UNITY_EDITOR
        public override bool DrawEditorGUI()
        {
            var modified = base.DrawEditorGUI();

            // Dynamics Option
            var newDynamicsOption = ConvertIntToDynamicsOption(EditorGUILayout.Popup("Dynamics Option", (int)dynamicsOption, new string[] {
                        "Remove wearable dynamics and ParentConstraint",
                        "Keep wearable dynamics and ParentConstraint if needed",
                        "Remove wearable dynamics and IgnoreTransform",
                        "Copy avatar dynamics data to wearable",
                        "Ignore all dynamics"
                    }));

            modified |= dynamicsOption != newDynamicsOption;
            dynamicsOption = newDynamicsOption;

            return modified;
        }
#endif 
    }
}
