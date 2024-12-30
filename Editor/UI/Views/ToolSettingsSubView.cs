/*
 * File: SettingsSubView.cs
 * Project: DressingTools
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
 * You should have received a copy of the GNU General Public License along with DressingTools. If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Chocopoi.DressingFramework.Localization;
using Chocopoi.DressingTools.Localization;
using Chocopoi.DressingTools.UI.Presenters;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Chocopoi.DressingTools.UI.Views
{
    [ExcludeFromCodeCoverage]
    internal class ToolSettingsSubView : ElementView, IToolSettingsSubView
    {
        private static readonly I18nTranslator t = I18n.ToolTranslator;

        public event Action UpdaterCheckUpdateButtonClicked;
        public event Action ResetToDefaultsButtonClicked;

        public string UpdaterCurrentVersion { get; set; }
        public bool UpdaterShowHelpboxUpdateNotChecked { get; set; }

        private readonly IMainView _mainView;
        private readonly ToolSettingsPresenter _presenter;
        private Label _updaterCurrentVerLabel;
        private VisualElement _updaterHelpboxContainer;

        public ToolSettingsSubView(IMainView mainView)
        {
            _mainView = mainView;

            UpdaterCurrentVersion = "";
            UpdaterShowHelpboxUpdateNotChecked = true;

            _presenter = new ToolSettingsPresenter(this);
        }

        private void InitVisualTree()
        {
            var tree = Resources.Load<VisualTreeAsset>("ToolSettingsSubView");
            tree.CloneTree(this);
            var styleSheet = Resources.Load<StyleSheet>("ToolSettingsSubViewStyles");
            if (!styleSheets.Contains(styleSheet))
            {
                styleSheets.Add(styleSheet);
            }
        }

        private void InitUpdateChecker()
        {
            _updaterCurrentVerLabel = Q<Label>("updater-current-ver-label").First();
            _updaterHelpboxContainer = Q<VisualElement>("updater-helpbox-container").First();

            var updaterCheckUpdateBtn = Q<Button>("updater-check-update-btn").First();
            updaterCheckUpdateBtn.clicked += UpdaterCheckUpdateButtonClicked;

            var resetToDefaultsBtn = Q<Button>("reset-defaults-btn").First();
            resetToDefaultsBtn.clicked += ResetToDefaultsButtonClicked;
        }

        private void RepaintUpdateChecker()
        {
            _updaterCurrentVerLabel.text = UpdaterCurrentVersion;

            _updaterHelpboxContainer.Clear();
            if (UpdaterShowHelpboxUpdateNotChecked)
            {
                _updaterHelpboxContainer.Add(CreateHelpBox(t._("editor.main.toolSettings.updaterChecker.helpbox.msg.updateNotChecked"), MessageType.Warning));
            }
        }

        public override void Repaint()
        {
            RepaintUpdateChecker();
        }

        public override void OnEnable()
        {
            InitVisualTree();
            InitUpdateChecker();

            t.LocalizeElement(this);

            RaiseLoadEvent();
        }

        public override void OnDisable()
        {
            base.OnDisable();
        }
    }
}
