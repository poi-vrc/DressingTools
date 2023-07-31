using System;
using System.Collections.Generic;
using Chocopoi.DressingTools.UI.Presenters.Modules;
using Chocopoi.DressingTools.UIBase.Views;
using Chocopoi.DressingTools.Wearable;
using Chocopoi.DressingTools.Wearable.Modules;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Views.Modules
{
    [CustomModuleEditor(typeof(BlendshapeSyncModule))]
    internal class BlendshapeSyncModuleEditor : ModuleEditor, IBlendshapeSyncModuleEditorView
    {
        private static Localization.I18n t = Localization.I18n.GetInstance();

        public event Action AddBlendshapeSyncButtonClick;
        public bool ShowCannotRenderWithoutTargetAvatarAndWearableHelpBox { get; set; }
        public List<BlendshapeSyncData> BlendshapeSyncs { get; set; }

        private BlendshapeSyncModuleEditorPresenter presenter_;

        public BlendshapeSyncModuleEditor(IWearableConfigView configView, DTWearableModuleBase target) : base(configView, target)
        {
            presenter_ = new BlendshapeSyncModuleEditorPresenter(this, configView, (BlendshapeSyncModule)target);

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
