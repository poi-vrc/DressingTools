/*
 * File: MoveRootModuleEditor.cs
 * Project: DressingTools
 * Created Date: Wednesday, August 9th 2023, 8:34:36 pm
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
using Chocopoi.DressingTools.UI.Presenters.Modules;
using Chocopoi.DressingTools.UIBase.Views;
using Chocopoi.DressingTools.Wearable.Modules;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Views.Modules
{
    [CustomModuleEditor(typeof(MoveRootModule))]
    internal class MoveRootModuleEditor : ModuleEditor, IMoveRootModuleEditorView
    {
        private static Localization.I18n t = Localization.I18n.GetInstance();

        public event Action MoveToGameObjectFieldChange;
        public bool ShowSelectAvatarFirstHelpBox { get; set; }
        public bool IsGameObjectInvalid { get; set; }
        public GameObject MoveToGameObject { get => _moveToGameObject; set => _moveToGameObject = value; }

        private MoveRootModuleEditorPresenter _presenter;
        private IModuleEditorViewParent _parentView;
        private GameObject _moveToGameObject;

        public MoveRootModuleEditor(IModuleEditorViewParent parentView, DTWearableModuleBase target) : base(parentView, target)
        {
            _parentView = parentView;
            _presenter = new MoveRootModuleEditorPresenter(this, parentView, (MoveRootModule)target);
            ShowSelectAvatarFirstHelpBox = true;
            IsGameObjectInvalid = true;
        }

        public override void OnGUI()
        {
            if (ShowSelectAvatarFirstHelpBox)
            {
                HelpBox("Please select an avatar first.", MessageType.Error);
            }
            else
            {
                if (IsGameObjectInvalid)
                {
                    HelpBox("The selected GameObject is not inside the avatar.", MessageType.Error);
                }
                GameObjectField("Move To", ref _moveToGameObject, true, MoveToGameObjectFieldChange);
            }
        }

        public override bool IsValid()
        {
            return !IsGameObjectInvalid;
        }
    }
}
