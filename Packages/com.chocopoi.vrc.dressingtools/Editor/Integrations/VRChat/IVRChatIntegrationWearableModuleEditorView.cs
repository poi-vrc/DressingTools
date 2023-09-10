/*
 * File: IVRChatIntegrationModuleEditorView.cs
 * Project: DressingTools
 * Created Date: Wednesday, August 23th 2023, 7:56:36 pm
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

#if VRC_SDK_VRCSDK3
using System;
using Chocopoi.DressingTools.Lib.UI;

namespace Chocopoi.DressingTools.Integrations.VRChat
{
    internal interface IVRChatIntegrationWearableModuleEditorView : IEditorView
    {
        event Action ConfigChange;

        bool UseCustomCabinetToggleName { get; set; }
        string CustomCabinetToggleName { get; set; }
        bool UseCabinetThumbnails { get; set; }
    }
}
#endif
