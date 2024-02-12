/*
 * File: WearableModuleEditorIMGUI.cs
 * Project: DressingFramework
 * Created Date: Tuesday, August 1st 2023, 12:37:10 am
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
 * You should have received a copy of the GNU General Public License along with DressingFramework. If not, see <https://www.gnu.org/licenses/>.
 */

using Chocopoi.DressingTools.OneConf;
using Chocopoi.DressingTools.OneConf.Serialization;

namespace Chocopoi.DressingTools.UI.Views.Modules
{
    /// <summary>
    /// Wearable module editor interface
    /// </summary>
    internal interface IWearableModuleEditor : IEditorView
    {
        /// <summary>
        /// Human-readable friendly name of this module editor
        /// </summary>
        string FriendlyName { get; }

        /// <summary>
        /// Used internally. A temporary status for the UI to store the foldout state.
        /// </summary>
        bool FoldoutState { get; set; }

        /// <summary>
        /// Parent view
        /// </summary>
        IWearableModuleEditorViewParent ParentView { get; set; }

        /// <summary>
        /// Module provider
        /// </summary>
        WearableModuleProvider Provider { get; set; }

        /// <summary>
        /// Target module
        /// </summary>
        IModuleConfig Target { get; set; }

        /// <summary>
        /// Check if the module editor content is valid
        /// </summary>
        /// <returns>Is valid</returns>
        bool IsValid();
    }
}
