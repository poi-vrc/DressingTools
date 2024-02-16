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

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Chocopoi.DressingFramework.Localization;
using Chocopoi.DressingTools.Localization;
using Chocopoi.DressingTools.OneConf.Wearable.Modules.BuiltIn.ArmatureMapping;
using Chocopoi.DressingTools.UI.Views;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI
{
    [ExcludeFromCodeCoverage]
    internal class DTMappingEditorWindow : EditorWindow
    {
        private static readonly I18nTranslator t = I18n.ToolTranslator;

        internal class ViewData
        {
            public event Action MappingEditorChanged;

            public GameObject targetAvatar;
            public GameObject targetWearable;
            public BoneMappingMode boneMappingMode;
            public List<BoneMapping> generatedBoneMappings;
            public List<BoneMapping> outputBoneMappings;

            public ViewData()
            {
                Reset();
            }

            internal void RaiseMappingEditorChangedEvent()
            {
                MappingEditorChanged?.Invoke();
            }

            public void Reset()
            {
                targetAvatar = null;
                targetWearable = null;
                boneMappingMode = BoneMappingMode.Auto;
                generatedBoneMappings = null;
                outputBoneMappings = new List<BoneMapping>();
            }
        }

        internal static ViewData Data { get; private set; } = new ViewData();

        public static void StartMappingEditor()
        {
            var boneMappingEditorWindow = GetWindow<DTMappingEditorWindow>();
            boneMappingEditorWindow.titleContent = new GUIContent(t._("modules.wearable.armatureMapping.editor.title"));
            boneMappingEditorWindow.Show();
        }

        private MappingEditorView _view;

        public void OnEnable()
        {
            _view = new MappingEditorView();
            rootVisualElement.Add(_view);
            _view.OnEnable();
        }

        public void OnDisable()
        {
            _view.OnDisable();
        }

        public MappingEditorView GetView()
        {
            return _view;
        }
    }
}
