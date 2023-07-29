using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chocopoi.DressingTools.Wearable
{
    [Serializable]
    public class DTWearableInfo
    {
        public string uuid;
        public string name;
        public string author;
        public string description;
        public string createdTime;
        public string updatedTime;

        public DTWearableInfo() { }

        // copy constructor
        public DTWearableInfo(DTWearableInfo toCopy)
        {
            uuid = toCopy.uuid;
            name = toCopy.name;
            author = toCopy.author;
            description = toCopy.description;
            createdTime = toCopy.createdTime;
            updatedTime = toCopy.updatedTime;
        }
    }
}
