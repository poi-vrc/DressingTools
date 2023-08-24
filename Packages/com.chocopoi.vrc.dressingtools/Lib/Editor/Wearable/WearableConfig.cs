/*
 * File: WearableConfig.cs
 * Project: DressingTools
 * Created Date: Saturday, July 29th 2023, 10:31:11 am
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
using Chocopoi.DressingTools.Lib.Serialization;
using Chocopoi.DressingTools.Lib.Wearable.Modules;
using Chocopoi.DressingTools.Lib.Wearable.Serializers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Chocopoi.DressingTools.Lib.Wearable
{
    public class WearableConfig : VersionedObject
    {
        public static readonly SerializationVersion CurrentConfigVersion = new SerializationVersion(1, 0, 0);
        private static readonly Dictionary<int, ISerializer> Serializers = new Dictionary<int, ISerializer>() {
            { 1, new WearableV1Serializer() },
        };

        public override SerializationVersion Version { get; set; }
        public WearableInfo Info { get; set; }
        public AvatarConfig AvatarConfig { get; set; }

        public List<WearableModule> Modules;

        public WearableConfig()
        {
            // initialize some fields
            Version = CurrentConfigVersion;
            var isoTimeStr = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
            Info = new WearableInfo
            {
                uuid = Guid.NewGuid().ToString(),
                createdTime = isoTimeStr,
                updatedTime = isoTimeStr
            };
            AvatarConfig = new AvatarConfig();
            Modules = new List<WearableModule>();
        }

        public override ISerializer GetSerializerByVersion(SerializationVersion version)
        {
            if (version == null)
            {
                return null;
            }
            return Serializers.ContainsKey(version.Major) ? Serializers[version.Major] : null;
        }

        public static WearableConfig Deserialize(string json)
        {
            // TODO: perform schema check
            var jObject = JObject.Parse(json);
            return Deserialize(jObject);
        }

        public static WearableConfig Deserialize(JObject jObject)
        {
            var config = new WearableConfig();
            config.DeserializeFrom(jObject);
            return config;
        }

        public WearableConfig Clone()
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
