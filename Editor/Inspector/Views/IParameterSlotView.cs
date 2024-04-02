﻿/*
 * Copyright (c) 2024 chocopoi
 * 
 * This file is part of DressingTools.
 * 
 * DressingTools is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 * 
 * DressingTools is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with DressingTools. If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using Chocopoi.DressingTools.Components.Animations;
using Chocopoi.DressingTools.UI.Views;

namespace Chocopoi.DressingTools.Inspector.Views
{
    internal class ParameterSlotMapping
    {
        public DTSmartControl ctrl;
        public Action<float> onChange;
        public Action onRemove;
    }

    internal interface IParameterSlotView : IEditorView
    {
        event Action ConfigChanged;
        event Action<DTSmartControl> AddMapping;

        DTParameterSlot Target { get; set; }
        int ValueType { get; set; }
        List<ParameterSlotMapping> Mappings { get; set; }
        bool ShowAvatarRootNotFoundHelpbox { get; set; }
        bool ShowNoDefaultValueMappingHelpbox { get; set; }
        bool ShowDuplicateMappingsHelpbox { get; set; }
        float ParameterDefaultValue { get; set; }

        void RepaintMappingHelpboxes();
    }
}
