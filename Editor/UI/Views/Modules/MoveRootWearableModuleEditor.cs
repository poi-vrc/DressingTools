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
using System.Diagnostics.CodeAnalysis;
using Chocopoi.DressingFramework.Localization;
using Chocopoi.DressingTools.Localization;
using Chocopoi.DressingTools.OneConf.Serialization;
using Chocopoi.DressingTools.OneConf.Wearable.Modules;
using Chocopoi.DressingTools.OneConf.Wearable.Modules.BuiltIn;
using Chocopoi.DressingTools.UI.Presenters.Modules;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Views.Modules
{
    [ExcludeFromCodeCoverage]
    [CustomWearableModuleEditor(typeof(MoveRootWearableModuleProvider))]
    internal class MoveRootWearableModuleEditor : WearableModuleEditorIMGUI, IMoveRootWearableModuleEditorView
    {
        private static readonly I18nTranslator t = I18n.ToolTranslator;

        public event Action MoveToGameObjectFieldChange;
        public bool ShowSelectAvatarFirstHelpBox { get; set; }
        public bool IsGameObjectInvalid { get; set; }
        public GameObject MoveToGameObject { get => _moveToGameObject; set => _moveToGameObject = value; }

        private readonly MoveRootWearableModuleEditorPresenter _presenter;
        private readonly IWearableModuleEditorViewParent _parentView;
        private GameObject _moveToGameObject;

        public MoveRootWearableModuleEditor(IWearableModuleEditorViewParent parentView, MoveRootWearableModuleProvider provider, IModuleConfig target) : base(parentView, provider, target)
        {
            _parentView = parentView;
            _presenter = new MoveRootWearableModuleEditorPresenter(this, parentView, (MoveRootWearableModuleConfig)target);
            ShowSelectAvatarFirstHelpBox = true;
            IsGameObjectInvalid = true;
        }

        public override void OnGUI()
        {
            if (ShowSelectAvatarFirstHelpBox)
            {
                HelpBox(t._("modules.wearable.moveRoot.editor.helpbox.selectAvatarFirst"), MessageType.Error);
            }
            else
            {
                if (IsGameObjectInvalid)
                {
                    HelpBox(t._("modules.wearable.moveRoot.editor.helpbox.objectNotInsideAvatar"), MessageType.Error);
                }
                GameObjectField(t._("modules.wearable.moveRoot.editor.gameObjectField.moveTo"), ref _moveToGameObject, true, MoveToGameObjectFieldChange);
            }
        }

        public override bool IsValid()
        {
            return !IsGameObjectInvalid;
        }
    }
}
