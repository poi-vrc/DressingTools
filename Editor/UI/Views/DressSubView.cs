/*
 * File: DressSubView.cs
 * Project: DressingTools
 * Created Date: Saturday, August 12th 2023, 1:22:09 am
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
using System.Diagnostics.CodeAnalysis;
using Chocopoi.DressingFramework.Localization;
using Chocopoi.DressingTools.Localization;
using Chocopoi.DressingTools.OneConf.Wearable;
using Chocopoi.DressingTools.UI.Presenters;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Chocopoi.DressingTools.UI.Views
{
    [ExcludeFromCodeCoverage]
    internal class DressSubView : ElementView, IDressSubView
    {
        private static readonly I18nTranslator t = I18n.ToolTranslator;

        public event Action StartButtonClick;

        public int SelectedTab { get => _mainView.SelectedTab; set => _mainView.SelectedTab = value; }
        public GameObject SelectedAvatarGameObject { get => _mainView.SelectedAvatarGameObject; set => _mainView.SelectedAvatarGameObject = value; }
        public GameObject SelectedOutfitGameObject { get => (GameObject)_outfitObjectField.value; set => _outfitObjectField.value = value; }

        private DressPresenter _presenter;
        private IMainView _mainView;
        private ObjectField _outfitObjectField;

        public DressSubView(IMainView mainView)
        {
            _mainView = mainView;
            _presenter = new DressPresenter(this);
        }

        public void StartDressing(GameObject targetAvatar = null, GameObject targetOutfit = null)
        {
            SelectedAvatarGameObject = targetAvatar;
            SelectedOutfitGameObject = targetOutfit;
            StartButtonClick?.Invoke();
        }

        public override void OnEnable()
        {
            InitVisualTree();
            t.LocalizeElement(this);
            RaiseLoadEvent();
        }

        public override void OnDisable()
        {
            base.OnDisable();
        }

        private void InitVisualTree()
        {
            var tree = Resources.Load<VisualTreeAsset>("DressSubView");
            tree.CloneTree(this);
            var styleSheet = Resources.Load<StyleSheet>("DressSubViewStyles");
            if (!styleSheets.Contains(styleSheet))
            {
                styleSheets.Add(styleSheet);
            }

            _outfitObjectField = Q<ObjectField>("outfit-objfield").First();
            var startBtn = Q<Button>("start-btn").First();
            startBtn.RegisterCallback<ClickEvent>(e => StartButtonClick?.Invoke());
        }

        public override void Repaint()
        {
        }
    }
}
