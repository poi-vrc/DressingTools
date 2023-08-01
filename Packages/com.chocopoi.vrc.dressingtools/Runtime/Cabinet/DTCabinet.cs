using Chocopoi.DressingTools.Logging;
using UnityEngine;

namespace Chocopoi.DressingTools.Cabinet
{
    [AddComponentMenu("DressingTools/DT Cabinet")]
    [DefaultExecutionOrder(-19999)]
    public class DTCabinet : DTBaseComponent
    {
        public GameObject avatarGameObject;

        public string avatarArmatureName;

        public DTCabinetWearable[] GetWearables()
        {
            return avatarGameObject.GetComponentsInChildren<DTCabinetWearable>();
        }

        public void Apply(DTReport report)
        {
            new DTCabinetApplier(report, this).Execute();
        }

        void Start()
        {
            // TODO: the report shouldn't put here
            Apply(new DTReport());
        }
    }
}
