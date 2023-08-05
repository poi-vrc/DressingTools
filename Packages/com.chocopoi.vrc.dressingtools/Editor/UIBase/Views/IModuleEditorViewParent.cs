using System;
using System.Collections.Generic;
using Chocopoi.DressingTools.UI.Views.Modules;
using Chocopoi.DressingTools.Wearable;
using UnityEngine;

namespace Chocopoi.DressingTools.UIBase.Views
{
    internal interface IModuleEditorViewParent : IEditorView
    {
        event Action TargetAvatarOrWearableChange;
        GameObject TargetAvatar { get; }
        GameObject TargetWearable { get; }
    }
}
