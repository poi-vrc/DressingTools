using UnityEngine;

namespace Chocopoi.DressingTools.UIBase.Views
{
    internal interface IMainView : IEditorView
    {
        int SelectedTab { get; set; }

        void ForceUpdateCabinetSubView();
        void StartSetupWizard(GameObject targetAvatar, GameObject targetWearable = null);
    }
}
