using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chocopoi.DressingTools.Cabinet
{
    [Serializable]
    public class DTCabinetWearable : DTWearableConfig
    {
        public GameObject wearableGameObject;
        public List<GameObject> appliedObjects;
        public string serializedJson;

        public DTCabinetWearable(DTWearableConfig toCopy) : base(toCopy)
        {
        }
    }
}
