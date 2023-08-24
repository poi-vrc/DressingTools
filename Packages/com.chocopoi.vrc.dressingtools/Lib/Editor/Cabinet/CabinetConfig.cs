/*
 * File: CabinetConfig.cs
 * Project: DressingTools
 * Created Date: Saturday, Aug 24th 2023, 05:08:11 pm
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
using System.Globalization;
using System.Text.RegularExpressions;
using Chocopoi.DressingTools.Lib.Cabinet.Modules;
using Chocopoi.DressingTools.Lib.Cabinet.Serializers;
using Chocopoi.DressingTools.Lib.Serialization;
using Chocopoi.DressingTools.Lib.Wearable.Modules;
using Chocopoi.DressingTools.Lib.Wearable.Serializers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Chocopoi.DressingTools.Lib.Cabinet
{
    public class CabinetConfig : VersionedObject
    {
        public static readonly SerializationVersion CurrentConfigVersion = new SerializationVersion(1, 0, 0);
        private static readonly Dictionary<int, ISerializer> Serializers = new Dictionary<int, ISerializer>() {
            { 1, new CabinetV1Serializer() },
        };

        public override SerializationVersion Version { get; set; }

        public string AvatarArmatureName { get; set; }
        public bool GroupDynamics { get; set; }
        public bool GroupDynamicsSeparateGameObjects { get; set; }
        public bool AnimationWriteDefaults { get; set; }

        public List<CabinetModule> Modules;

        public CabinetConfig()
        {
            Version = CurrentConfigVersion;
            AvatarArmatureName = "Armature";
            GroupDynamics = true;
            GroupDynamicsSeparateGameObjects = true;
            AnimationWriteDefaults = true;
            Modules = new List<CabinetModule>();
        }

        public override ISerializer GetSerializerByVersion(SerializationVersion version)
        {
            if (version == null)
            {
                return null;
            }
            return Serializers.ContainsKey(version.Major) ? Serializers[version.Major] : null;
        }

        public static CabinetConfig Deserialize(string json)
        {
            // TODO: perform schema check
            var jObject = JObject.Parse(json);
            return Deserialize(jObject);
        }

        public static bool TryDeserialize(string json, out CabinetConfig config)
        {
            try
            {
                config = Deserialize(json);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                config = null;
                return false;
            }
        }

        public static CabinetConfig Deserialize(JObject jObject)
        {
            var config = new CabinetConfig();
            config.DeserializeFrom(jObject);
            return config;
        }

        public CabinetConfig Clone()
        {
            // a tricky and easier way to copy
            return Deserialize(Serialize());
        }

        public override string ToString()
        {
            return Serialize().ToString(Formatting.None);
        }
    }
}
