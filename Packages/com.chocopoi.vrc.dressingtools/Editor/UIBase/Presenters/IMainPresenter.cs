using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Wearable;
using UnityEngine;

namespace Chocopoi.DressingTools.UIBase.Presenters
{
    internal interface IMainPresenter
    {
        void StartDressingWizard();
        void AddToCabinet(DTCabinet cabinet, DTWearableConfig config, GameObject wearableGameObject);
    }
}
