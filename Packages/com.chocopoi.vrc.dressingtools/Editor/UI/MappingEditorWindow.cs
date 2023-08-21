/*
 * File: DTMappingEditorWindow.cs
 * Project: DressingTools
 * Created Date: Saturday, August 12th 2023, 12:21:25 am
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
using System.Diagnostics.CodeAnalysis;
using Chocopoi.DressingTools.Lib.Wearable;
using Chocopoi.DressingTools.UI.View;
using Chocopoi.DressingTools.Wearable.Modules;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI
{
    [ExcludeFromCodeCoverage]
    internal class DTMappingEditorContainer
    {
        public GameObject targetAvatar;
        public GameObject targetWearable;
        public BoneMappingMode boneMappingMode;
        public List<BoneMapping> generatedBoneMappings;
        public List<BoneMapping> outputBoneMappings;

        public DTMappingEditorContainer()
        {
            targetAvatar = null;
            targetWearable = null;
            boneMappingMode = BoneMappingMode.Auto;
            generatedBoneMappings = null;
            outputBoneMappings = null;
        }
    }

    internal class DTMappingEditorWindow : EditorWindow
    {
        private MappingEditorView _view;
        private DTMappingEditorContainer _container;

        public DTMappingEditorWindow()
        {
            _view = new MappingEditorView();
            _container = null;
        }

        public void SetContainer(DTMappingEditorContainer container)
        {
            _container = container;
            _view.SetContainer(container);
        }

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
            if (_container == null)
            {
                Close();
            }

            _view.OnGUI();
        }
    }
}
