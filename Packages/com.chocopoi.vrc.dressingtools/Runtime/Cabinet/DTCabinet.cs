using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chocopoi.DressingTools.Cabinet
{
    public enum DTCabinetApplierMode
    {
        LateApplyFullyIsolated = 0,
        LateApplyEmbedScripts = 1,
        ApplyImmediately = 2
    }

    public class DTCabinet : MonoBehaviour
    {
        public GameObject avatarGameObject;

        public string avatarArmatureName;

        public DTCabinetApplierMode applierMode;

        public string applierName;

        public string serializedApplierSettings;

        public List<DTCabinetWearable> wearables = new List<DTCabinetWearable>();

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
