﻿/*
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

namespace Chocopoi.DressingTools.OneConf.Wearable
{
    // sadly, json.net cannot serialize unity vector3,quaternion well
    // creating custom classes as a workaround
    [Serializable]
    internal class AvatarConfigVector3
    {
        public float x;
        public float y;
        public float z;

        public AvatarConfigVector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public AvatarConfigVector3(Vector3 vec)
        {
            x = vec.x;
            y = vec.y;
            z = vec.z;
        }

        // copy constructor
        public AvatarConfigVector3(AvatarConfigVector3 toCopy)
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
    internal class AvatarConfigQuaternion
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public AvatarConfigQuaternion(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public AvatarConfigQuaternion(Quaternion q)
        {
            x = q.x;
            y = q.y;
            z = q.z;
            w = q.w;
        }

        // copy constructor
        public AvatarConfigQuaternion(AvatarConfigQuaternion toCopy)
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

    /// <summary>
    /// Avatar configuration. Used for automatic fitting scale, and future features for identifying compatible wearables for a specific avatar.
    /// </summary>
    [Serializable]
    internal class AvatarConfig
    {
        /// <summary>
        /// GUIDs of compatible avatars (Reserved for future use)
        /// </summary>
        public List<string> guids;

        /// <summary>
        /// Name of avatar
        /// </summary>
        public string name;

        /// <summary>
        /// Avatar armature name. For reference only, might not be used for dressing since armature name is obtained from cabinet settings.
        /// </summary>
        public string armatureName;

        /// <summary>
        /// World position delta between avatar and wearable
        /// </summary>
        public AvatarConfigVector3 worldPosition;

        /// <summary>
        /// World rotation delta between avatar and wearable
        /// </summary>
        public AvatarConfigQuaternion worldRotation;

        /// <summary>
        /// Lossy avatar scale
        /// </summary>
        public AvatarConfigVector3 avatarLossyScale;

        /// <summary>
        /// Lossy wearable scale
        /// </summary>
        public AvatarConfigVector3 wearableLossyScale;

        /// <summary>
        /// Constructs a new avatar config
        /// </summary>
        public AvatarConfig()
        {
            guids = new List<string>();
            worldPosition = new AvatarConfigVector3(0, 0, 0);
            worldRotation = new AvatarConfigQuaternion(0, 0, 0, 0);
            avatarLossyScale = new AvatarConfigVector3(0, 0, 0);
            wearableLossyScale = new AvatarConfigVector3(0, 0, 0);
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="toCopy">Avatar config to copy</param>
        public AvatarConfig(AvatarConfig toCopy)
        {
            // deep copy
            guids = new List<string>(toCopy.guids);

            worldPosition = new AvatarConfigVector3(toCopy.worldPosition);
            worldRotation = new AvatarConfigQuaternion(toCopy.worldRotation);
            avatarLossyScale = new AvatarConfigVector3(toCopy.avatarLossyScale);
            wearableLossyScale = new AvatarConfigVector3(toCopy.wearableLossyScale);
        }
    }
}
