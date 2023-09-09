/*
 * File: VRCMergeAnimLayerWearableModuleEditor.cs
 * Project: DressingTools
 * Created Date: Tuesday, 29th Aug 2023, 02:53:11 pm
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
using System.Diagnostics.CodeAnalysis;
using Chocopoi.DressingTools.Integration.VRChat.Modules;
using Chocopoi.DressingTools.Lib.Extensibility.Providers;
using Chocopoi.DressingTools.Lib.UI;
using Chocopoi.DressingTools.Lib.Wearable.Modules;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.Integrations.VRChat
{
    [ExcludeFromCodeCoverage]
    [CustomWearableModuleEditor(typeof(VRCMergeAnimLayerWearableModuleProvider))]
    internal class VRCMergeAnimLayerWearableModuleEditor : WearableModuleEditor, IVRCMergeAnimLayerWearableModuleEditorView
    {
        private static readonly Localization.I18n t = Localization.I18n.Instance;

        public event Action ConfigChange;

        public string[] AnimLayerKeys { get; set; }
        public int SelectedAnimLayerIndex { get => _selectedAnimLayerIndex; set => _selectedAnimLayerIndex = value; }
        public int SelectedPathMode { get => _selectedPathMode; set => _selectedPathMode = value; }
        public bool RemoveAnimatorAfterApply { get => _removeAnimatorAfterApply; set => _removeAnimatorAfterApply = value; }
        public bool MatchLayerWriteDefaults { get => _matchLayerWriteDefaults; set => _matchLayerWriteDefaults = value; }
        public GameObject AnimatorObject { get => _animatorObj; set => _animatorObj = value; }
        public bool ShowNoTargetAvatarOrWearableHelpbox { get; set; }
        public bool ShowAnimatorObjectPathNotFoundHelpbox { get; set; }
        public bool ShowNoAnimatorHelpbox { get; set; }
        public bool ShowNotInWearableHelpbox { get; set; }

        private int _selectedAnimLayerIndex;
        private int _selectedPathMode;
        private bool _removeAnimatorAfterApply;
        private bool _matchLayerWriteDefaults;
        private GameObject _animatorObj;

        private IWearableModuleEditorViewParent _parentView;
        private VRCMergeAnimLayerWearableModuleEditorPresenter _presenter;

        public VRCMergeAnimLayerWearableModuleEditor(IWearableModuleEditorViewParent parentView, VRCMergeAnimLayerWearableModuleProvider provider, IModuleConfig target) : base(parentView, provider, target)
        {
            _parentView = parentView;
            _presenter = new VRCMergeAnimLayerWearableModuleEditorPresenter(this, parentView, (VRCMergeAnimLayerWearableModuleConfig)target);

            AnimLayerKeys = new string[] { };
            _selectedAnimLayerIndex = 0;
            _selectedPathMode = 0;
            _removeAnimatorAfterApply = true;
            _matchLayerWriteDefaults = true;
            _animatorObj = null;
        }

        public override void OnGUI()
        {
            Popup(t._("integrations.vrc.modules.mergeAnimLayer.editor.popup.layerToMerge"), ref _selectedAnimLayerIndex, AnimLayerKeys, ConfigChange);
            Popup(t._("integrations.vrc.modules.mergeAnimLayer.editor.popup.pathMode"), ref _selectedPathMode, new string[] { "Relative", "Absolute" }, ConfigChange);
            ToggleLeft(t._("integrations.vrc.modules.mergeAnimLayer.editor.toggle.removeAnimatorAfterApply"), ref _removeAnimatorAfterApply, ConfigChange);
            ToggleLeft(t._("integrations.vrc.modules.mergeAnimLayer.editor.toggle.matchExistingWriteDefaults"), ref _matchLayerWriteDefaults, ConfigChange);

            if (ShowNoTargetAvatarOrWearableHelpbox)
            {
                HelpBox(t._("integrations.vrc.modules.mergeAnimLayer.editor.helpbox.noTargetOrWearableSelected"), MessageType.Error);
            }
            else
            {
                if (_animatorObj == null)
                {
                    HelpBox(t._("integrations.vrc.modules.mergeAnimLayer.editor.helpbox.wearableRootWillBeUsedForFindAnimator"), MessageType.Info);
                }
                else
                {
                    HelpBox(t._("integrations.vrc.modules.mergeAnimLayer.editor.helpbox.objectWillBeUsedForFindAnimator"), MessageType.Info);
                    if (ShowAnimatorObjectPathNotFoundHelpbox)
                    {
                        HelpBox(t._("integrations.vrc.modules.mergeAnimLayer.editor.helpbox.objectPathNotFound"), MessageType.Error);
                    }
                    else if (ShowNotInWearableHelpbox)
                    {
                        HelpBox(t._("integrations.vrc.modules.mergeAnimLayer.editor.helpbox.notWithinWearable"), MessageType.Error);
                    }
                }

                if (ShowNoAnimatorHelpbox)
                {
                    HelpBox(t._("integrations.vrc.modules.mergeAnimLayer.editor.helpbox.noAnimatorAttached"), MessageType.Error);
                }
                GameObjectField(t._("integrations.vrc.modules.mergeAnimLayer.editor.gameObjectField.animatorLocation"), ref _animatorObj, true, ConfigChange);
            }
        }

        public override bool IsValid() => _presenter.IsValid();
    }
}
#endif
