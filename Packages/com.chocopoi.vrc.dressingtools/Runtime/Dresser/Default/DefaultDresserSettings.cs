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

using System.Diagnostics.CodeAnalysis;
using UnityEditor;

namespace Chocopoi.DressingTools.Dresser.Default
{
    internal enum DefaultDresserDynamicsOption
    {
        RemoveDynamicsAndUseParentConstraint = 0,
        KeepDynamicsAndUseParentConstraintIfNecessary = 1,
        IgnoreTransform = 2,
        CopyDynamics = 3,
        IgnoreAll = 4,
    }

    internal class DefaultDresserSettings : DTDresserSettings
    {
        public DefaultDresserDynamicsOption dynamicsOption;

        public DefaultDresserSettings()
        {
            // default settings
            dynamicsOption = DefaultDresserDynamicsOption.RemoveDynamicsAndUseParentConstraint;
        }

#if UNITY_EDITOR
        [ExcludeFromCodeCoverage]
        public override bool DrawEditorGUI()
        {
            var modified = base.DrawEditorGUI();

            // Dynamics Option
            var newDynamicsOption = (DefaultDresserDynamicsOption)EditorGUILayout.Popup("Dynamics Option", (int)dynamicsOption, new string[] {
                        "Remove wearable dynamics and ParentConstraint",
                        "Keep wearable dynamics and ParentConstraint if needed",
                        "Remove wearable dynamics and IgnoreTransform",
                        "Copy avatar dynamics data to wearable",
                        "Ignore all dynamics"
                    });

            modified |= dynamicsOption != newDynamicsOption;
            dynamicsOption = newDynamicsOption;

            return modified;
        }
#endif 
    }
}
