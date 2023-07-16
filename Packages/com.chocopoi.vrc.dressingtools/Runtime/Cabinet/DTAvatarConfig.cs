using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chocopoi.DressingTools.Cabinet
{
    // sadly, json.net cannot serialize unity vector3,quaternion well
    // creating custom classes as a workaround
    [Serializable]
    public class DTAvatarConfigVector3
    {
        public float x;
        public float y;
        public float z;

        public DTAvatarConfigVector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public DTAvatarConfigVector3(Vector3 vec)
        {
            x = vec.x;
            y = vec.y;
            z = vec.z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }

        public override string ToString()
        {
            return string.Format("{0:0.000}, {1:0.000}, {2:0.000}", x, y, z);
        }
    }

    [Serializable]
    public class DTAvatarConfigQuaternion
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public DTAvatarConfigQuaternion(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public DTAvatarConfigQuaternion(Quaternion q)
        {
            x = q.x;
            y = q.y;
            z = q.z;
            w = q.w;
        }

        public Quaternion ToQuaternion()
        {
            return new Quaternion(x, y, z, w);
        }

        public override string ToString()
        {
            return string.Format("{0:0.000}, {1:0.000}, {2:0.000}, {3:0.000}", x, y, z, w);
        }
    }

    [Serializable]
    public class DTAvatarConfig
    {
        public string[] guids;
        public string name;
        public string armatureName;
        public DTAvatarConfigVector3 worldPosition;
        public DTAvatarConfigQuaternion worldRotation;
        public DTAvatarConfigVector3 avatarLossyScale;
        public DTAvatarConfigVector3 wearableLossyScale;

        public DTAvatarConfig()
        {
            guids = new string[0];
            worldPosition = new DTAvatarConfigVector3(0, 0, 0);
            worldRotation = new DTAvatarConfigQuaternion(0, 0, 0, 0);
            avatarLossyScale = new DTAvatarConfigVector3(0, 0, 0);
            wearableLossyScale = new DTAvatarConfigVector3(0, 0, 0);
        }
    }
}
