using Chocopoi.DressingTools.Logging;
using UnityEngine;

namespace Chocopoi.DressingTools.Cabinet
{
    [AddComponentMenu("DressingTools/DT Cabinet")]
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

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
