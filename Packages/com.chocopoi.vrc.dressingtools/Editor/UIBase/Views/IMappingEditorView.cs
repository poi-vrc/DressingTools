using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chocopoi.DressingTools.UIBase.Views
{
    internal class ViewBoneMapping
    {
        public bool isInvalid;
        public int mappingType;
        public string wearablePath;
        public GameObject wearableObject;
        public Action MappingChange;
        public Action RemoveMappingButtonClick;

        public ViewBoneMapping()
        {
            isInvalid = true;
            wearablePath = null;
            mappingType = 0;
            wearableObject = null;
        }
    }

    internal class ViewAvatarHierachyNode
    {
        public bool foldout;
        public Transform avatarObjectTransform;
        public List<ViewBoneMapping> wearableMappings;
        public List<ViewAvatarHierachyNode> childs;
        public Action AddMappingButtonClick;

        public ViewAvatarHierachyNode()
        {
            foldout = true;
            avatarObjectTransform = null;
            wearableMappings = new List<ViewBoneMapping>();
            childs = new List<ViewAvatarHierachyNode>();
        }
    }

    internal interface IMappingEditorView : IEditorView
    {
        event Action BoneMappingModeChange;
        event Action BoneMappingDisplayModeChange;

        GameObject TargetAvatar { get; set; }
        GameObject TargetWearable { get; set; }
        int SelectedBoneMappingMode { get; set; }
        int SelectedBoneMappingDisplayMode { get; set; }
        List<ViewAvatarHierachyNode> AvatarHierachyNodes { get; set; }
        bool ShowBoneMappingNotAvailableHelpbox { get; set; }
    }
}
