using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chocopoi.DressingTools.UIBase.Views
{
    internal interface IMoveRootModuleEditorView : IEditorView
    {
        event Action TargetAvatarOrWearableChange;
        event Action MoveToGameObjectFieldChange;
        bool ShowSelectAvatarFirstHelpBox { get; set; }
        bool IsGameObjectInvalid { get; set; }
        GameObject MoveToGameObject { get; set; }
    }
}
