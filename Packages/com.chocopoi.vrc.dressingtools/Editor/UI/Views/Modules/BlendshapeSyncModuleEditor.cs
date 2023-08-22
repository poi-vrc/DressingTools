/*
 * File: BlendshapeSyncModuleEditor.cs
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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Chocopoi.DressingTools.Lib.UI;
using Chocopoi.DressingTools.Lib.Wearable.Modules;
using Chocopoi.DressingTools.Lib.Wearable.Modules.Providers;
using Chocopoi.DressingTools.UI.Presenters.Modules;
using Chocopoi.DressingTools.UIBase.Views;
using Chocopoi.DressingTools.Wearable.Modules;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Views.Modules
{
    [ExcludeFromCodeCoverage]
    [CustomModuleEditor(typeof(BlendshapeSyncModuleProvider))]
    internal class BlendshapeSyncModuleEditor : ModuleEditor, IBlendshapeSyncModuleEditorView
    {
        private static Localization.I18n t = Localization.I18n.GetInstance();

        public event Action AddBlendshapeSyncButtonClick;
        public GameObject TargetAvatar { get => _parentView.TargetAvatar; }
        public GameObject TargetWearable { get => _parentView.TargetWearable; }
        public bool ShowCannotRenderWithoutTargetAvatarAndWearableHelpBox { get; set; }
        public List<BlendshapeSyncData> BlendshapeSyncs { get; set; }

        private IModuleEditorViewParent _parentView;
        private BlendshapeSyncModuleEditorPresenter _presenter;

        public BlendshapeSyncModuleEditor(IModuleEditorViewParent parentView, ModuleProviderBase provider, ModuleConfig target) : base(parentView, provider, target)
        {
            _parentView = parentView;
            _presenter = new BlendshapeSyncModuleEditorPresenter(this, (BlendshapeSyncModuleConfig)target);

            ShowCannotRenderWithoutTargetAvatarAndWearableHelpBox = true;
            BlendshapeSyncs = new List<BlendshapeSyncData>();
        }

        public override void OnGUI()
        {
            if (ShowCannotRenderWithoutTargetAvatarAndWearableHelpBox)
            {
                HelpBox("Cannot render blendshape sync editor without a target avatar and a target wearble selected.", MessageType.Error);
            }
            else
            {
                HelpBox("The object must be a child or grand-child of the root. Or it will not be selected.", MessageType.Info);

                Button("+ Add", AddBlendshapeSyncButtonClick, GUILayout.ExpandWidth(false));

                var copy = new List<BlendshapeSyncData>(BlendshapeSyncs);
                foreach (var blendshapeSync in copy)
                {
                    if (blendshapeSync.isAvatarGameObjectInvalid)
                    {
                        HelpBox("The avatar GameObject is invalid.", MessageType.Error);
                    }
                    if (blendshapeSync.isWearableGameObjectInvalid)
                    {
                        HelpBox("The wearable GameObject is invalid.", MessageType.Error);
                    }

                    BeginHorizontal();
                    {
                        GameObjectField(ref blendshapeSync.avatarGameObject, true, blendshapeSync.avatarGameObjectFieldChangeEvent);

                        if (!blendshapeSync.isAvatarGameObjectInvalid)
                        {
                            Popup(ref blendshapeSync.avatarSelectedBlendshapeIndex, blendshapeSync.avatarAvailableBlendshapeNames, blendshapeSync.avatarBlendshapeNameChangeEvent);
                        }
                        else
                        {
                            // empty placeholder
                            BeginDisabled(true);
                            int fakeInt = 0;
                            Popup(ref fakeInt, new string[] { "---" });
                            EndDisabled();
                        }

                        GameObjectField(ref blendshapeSync.wearableGameObject, true, blendshapeSync.wearableGameObjectFieldChangeEvent);

                        if (!blendshapeSync.isWearableGameObjectInvalid)
                        {
                            Popup(ref blendshapeSync.wearableSelectedBlendshapeIndex, blendshapeSync.wearableAvailableBlendshapeNames, blendshapeSync.wearableBlendshapeNameChangeEvent);
                        }
                        else
                        {
                            // empty placeholder
                            BeginDisabled(true);
                            int fakeInt = 0;
                            Popup(ref fakeInt, new string[] { "---" });
                            EndDisabled();
                        }

                        Toggle(ref blendshapeSync.inverted, blendshapeSync.invertedToggleChangeEvent);

                        Button("x", blendshapeSync.removeButtonClickEvent, GUILayout.ExpandWidth(false));
                    }
                    EndHorizontal();
                }
            }
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
