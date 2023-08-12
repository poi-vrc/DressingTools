﻿/*
 * File: DTAvatarConfig.cs
 * Project: DressingTools
 * Created Date: Tuesday, August 1st 2023, 12:37:10 am
 * Author: chocopoi (poi@chocopoi.com)
 * -----
 * Copyright (c) 2023 chocopoi
 * 
 * This file is part of DressingTools.
 * 
 * DressingTools is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 * 
 * DressingTools is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with DressingTools. If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chocopoi.DressingTools.Wearable
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

        // copy constructor
        public DTAvatarConfigVector3(DTAvatarConfigVector3 toCopy)
        {
            x = toCopy.x;
            y = toCopy.y;
            z = toCopy.z;
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

        // copy constructor
        public DTAvatarConfigQuaternion(DTAvatarConfigQuaternion toCopy)
        {
            x = toCopy.x;
            y = toCopy.y;
            z = toCopy.z;
            w = toCopy.w;
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
        public List<string> guids;
        public string name;
        public string armatureName;
        public DTAvatarConfigVector3 worldPosition;
        public DTAvatarConfigQuaternion worldRotation;
        public DTAvatarConfigVector3 avatarLossyScale;
        public DTAvatarConfigVector3 wearableLossyScale;

        public DTAvatarConfig()
        {
            guids = new List<string>();
            worldPosition = new DTAvatarConfigVector3(0, 0, 0);
            worldRotation = new DTAvatarConfigQuaternion(0, 0, 0, 0);
            avatarLossyScale = new DTAvatarConfigVector3(0, 0, 0);
            wearableLossyScale = new DTAvatarConfigVector3(0, 0, 0);
        }

        // copy constructor
        public DTAvatarConfig(DTAvatarConfig toCopy)
        {
            // deep copy
            guids = new List<string>(toCopy.guids);

            worldPosition = new DTAvatarConfigVector3(toCopy.worldPosition);
            worldRotation = new DTAvatarConfigQuaternion(toCopy.worldRotation);
            avatarLossyScale = new DTAvatarConfigVector3(toCopy.avatarLossyScale);
            wearableLossyScale = new DTAvatarConfigVector3(toCopy.wearableLossyScale);
        }
    }
}
