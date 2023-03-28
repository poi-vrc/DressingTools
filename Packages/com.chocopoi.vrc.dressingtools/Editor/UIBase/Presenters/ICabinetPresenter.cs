using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Proxy;
using UnityEngine;

namespace Chocopoi.DressingTools.UIBase.Presenters
{
    internal interface ICabinetPresenter
    {
        DTCabinet[] GetCabinets();

        DTCabinet GetAvatarCabinet(GameObject avatar);
    }
}
