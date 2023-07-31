using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chocopoi.DressingTools.UIBase.Views
{
    public struct WearablePreview
    {
        public string name;
        public Action RemoveButtonClick;
    }

    internal interface ICabinetSubView : IEditorView
    {
        event Action CreateCabinetButtonClick;
        event Action AddWearableButtonClick;

        bool ShowCreateCabinetWizard { get; set; }
        bool ShowCabinetWearables { get; set; }
        int SelectedCabinetIndex { get; set; }
        string[] AvailableCabinetSelections { get; set; }
        GameObject CabinetAvatarGameObject { get; set; }
        string CabinetAvatarArmatureName { get; set; }
        List<WearablePreview> WearablePreviews { get; set; }
        GameObject SelectedCreateCabinetGameObject { get; }
    }
}
