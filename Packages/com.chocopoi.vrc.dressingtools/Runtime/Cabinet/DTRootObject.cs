using System;
using UnityEngine;

namespace Chocopoi.DressingTools.Cabinet
{
    [Serializable]
    public class DTCabinetRootObject : DTRootObject
    {
        public GameObject objectReference;
    }

    public class DTRootObject
    {
        public string objectPath;
    }
}
