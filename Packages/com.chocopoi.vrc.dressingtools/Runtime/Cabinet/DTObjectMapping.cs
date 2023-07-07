using System;
using UnityEngine;

namespace Chocopoi.DressingTools.Cabinet
{
    [Serializable]
    public class DTObjectMapping
    {
        public string avatarObjectPath;
        public string wearableObjectPath;

        public bool Equals(DTObjectMapping x)
        {
            return avatarObjectPath == x.avatarObjectPath && wearableObjectPath == x.wearableObjectPath;
        }

        public override string ToString()
        {
            return string.Format("\"{0}\" -> \"{1}\"", wearableObjectPath, avatarObjectPath);
        }
    }
}
