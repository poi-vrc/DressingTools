using System;
using Chocopoi.DressingTools.Wearable;
using UnityEngine;

namespace Chocopoi.DressingTools.UIBase.Views
{
    internal interface IWearableConfigViewParent
    {
        event Action TargetAvatarOrWearableChange;
        GameObject TargetAvatar { get; set; }
        GameObject TargetWearable { get; set; }
        DTWearableConfig Config { get; set; }
    }
}
