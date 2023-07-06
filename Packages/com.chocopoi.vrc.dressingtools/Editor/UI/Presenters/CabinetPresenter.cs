using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Proxy;
using Chocopoi.DressingTools.UIBase.Presenters;
using Chocopoi.DressingTools.UIBase.Views;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Presenters
{
    internal class CabinetPresenter : ICabinetPresenter
    {
        public CabinetPresenter(ICabinetSubView cabinetSubView)
        {

        }

        public DTCabinet[] GetCabinets()
        {
            return Object.FindObjectsOfType<DTCabinet>();
        }

        public DTCabinet GetAvatarCabinet(GameObject avatar)
        {
            // Find DTContainer in the scene, if not, create one
            var container = Object.FindObjectOfType<DTContainer>();

            if (container == null)
            {
                var gameObject = new GameObject("DressingTools");
                container = gameObject.AddComponent<DTContainer>();
            }

            // Find cabinets in the container
            DTCabinet[] cabinets = container.gameObject.GetComponentsInChildren<DTCabinet>();

            foreach (var cabinet in cabinets)
            {
                if (cabinet.avatarGameObject == avatar)
                {
                    return cabinet;
                }
            }

            // create new cabinet if not exist
            var cabinetGameObject = new GameObject("Cabinet_" + avatar.name);
            cabinetGameObject.transform.SetParent(container.transform);

            var newCabinet = cabinetGameObject.AddComponent<DTCabinet>();

            // TODO: read default config, scan for armature names?
            newCabinet.avatarGameObject = avatar;
            newCabinet.avatarArmatureName = "Armature";

            return newCabinet;
        }

        // TODO: Implement cabinet migration from v1
    }
}
