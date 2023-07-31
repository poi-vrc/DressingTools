using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chocopoi.DressingTools.UIBase.Views
{
    internal class BlendshapeSyncData
    {
        public bool isAvatarGameObjectInvalid;

        public GameObject avatarGameObject;
        public string[] avatarAvailableBlendshapeNames;
        public int avatarSelectedBlendshapeIndex;
        public float avatarBlendshapeValue;

        public bool isWearableGameObjectInvalid;

        public GameObject wearableGameObject;
        public string[] wearableAvailableBlendshapeNames;
        public int wearableSelectedBlendshapeIndex;
        public float wearableBlendshapeValue;

        public bool inverted;

        public Action avatarGameObjectFieldChangeEvent;
        public Action avatarBlendshapeNameChangeEvent;

        public Action wearableGameObjectFieldChangeEvent;
        public Action wearableBlendshapeNameChangeEvent;

        public Action invertedToggleChangeEvent;

        public Action removeButtonClickEvent;

        public BlendshapeSyncData()
        {
            isAvatarGameObjectInvalid = true;

            avatarGameObject = null;
            avatarAvailableBlendshapeNames = new string[] { "---" };
            avatarSelectedBlendshapeIndex = 0;
            avatarBlendshapeValue = 0;

            isWearableGameObjectInvalid = true;

            wearableGameObject = null;
            wearableAvailableBlendshapeNames = new string[] { "---" };
            wearableSelectedBlendshapeIndex = 0;
            wearableBlendshapeValue = 0;
        }
    }

    internal interface IBlendshapeSyncModuleEditorView : IEditorView
    {
        event Action AddBlendshapeSyncButtonClick;
        bool ShowCannotRenderWithoutTargetAvatarAndWearableHelpBox { get; set; }
        List<BlendshapeSyncData> BlendshapeSyncs { get; set; }
    }
}
