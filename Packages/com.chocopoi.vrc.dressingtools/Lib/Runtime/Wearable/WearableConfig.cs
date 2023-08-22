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
using Chocopoi.DressingTools.Lib.Wearable.Modules;
using Chocopoi.DressingTools.Lib.Wearable.Serializers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SemanticVersioning;

namespace Chocopoi.DressingTools.Lib.Wearable
{
    public class WearableConfigMigrator
    {
        public static bool Migrate(string inputJson, out string outputJson)
        {
            // TODO: do migration if necessary
            outputJson = inputJson;
            return false;
        }
    }

    public class WearableConfigVersion
    {
        public int Major { get; private set; }
        public int Minor { get; private set; }
        public int Patch { get; private set; }
        public string Extra { get; private set; }

        public WearableConfigVersion(int major, int minor, int patch)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
            Extra = null;
        }

        public WearableConfigVersion(int major, int minor, int patch, string extra)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
            Extra = extra;
        }

        public WearableConfigVersion(string str)
        {
            var hyphenIndex = str.IndexOf('-');
            string versionStr = null;
            if (hyphenIndex != -1)
            {
                Extra = str.Substring(hyphenIndex + 1);
                versionStr = str.Substring(0, hyphenIndex);
            }
            else
            {
                versionStr = str;
            }

            if (versionStr.Length == 0)
            {
                throw new ArgumentException("Version part is empty");
            }

            var splits = versionStr.Split('.');
            if (splits.Length < 2)
            {
                throw new ArgumentException("Version string is not in major, minor or optionally patch format");
            }

            if (!int.TryParse(splits[0], out var major))
            {
                throw new ArgumentException("Could not parse major: " + splits[0]);
            }
            Major = major;

            if (!int.TryParse(splits[1], out var minor))
            {
                throw new ArgumentException("Could not parse minor: " + splits[1]);
            }
            Minor = minor;

            if (splits.Length > 2)
            {
                if (!int.TryParse(splits[2], out var patch))
                {
                    throw new ArgumentException("Could not parse patch: " + splits[2]);
                }
                Patch = patch;
            }
            else
            {
                Patch = 0;
            }
        }

        public override string ToString()
        {
            var output = string.Format("{0}.{1}.{2}", Major, Minor, Patch);
            if (Extra != null)
            {
                output += "-" + Extra;
            }
            return output;
        }
    }

    public class WearableConfig
    {
        public static readonly WearableConfigVersion CurrentConfigVersion = new WearableConfigVersion(1, 0, 0);
        private static readonly Dictionary<int, IWearableConfigSerializer> Serializers = new Dictionary<int, IWearableConfigSerializer>() {
            { 1, new Version1Serializer() },
        };

        public WearableConfigVersion Version { get; set; }
        public WearableInfo Info { get; set; }
        public AvatarConfig AvatarConfig { get; set; }
        public List<WearableModule> Modules;

        public WearableConfig()
        {
            // initialize some fields
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

        public string Serialize()
        {
            var serializer = Serializers[CurrentConfigVersion.Major];
            return serializer.Serialize(this);
        }

        public static WearableConfig Deserialize(string json)
        {
            JObject jObject = JObject.Parse(json);
            if (!jObject.ContainsKey("version"))
            {
                throw new JsonException("Config does not contain version");
            }
            var configVersion = new WearableConfigVersion(jObject["version"].Value<string>());

            if (!Serializers.ContainsKey(configVersion.Major))
            {
                throw new Exception("Incompatible config version: " + configVersion.Major);
            }

            var serializer = Serializers[configVersion.Major];
            return serializer.Deserialize(json);
        }

        public WearableConfig Clone()
        {
            // a tricky and easier way to copy 
            return Deserialize(Serialize());
        }

        public override string ToString()
        {
            return Serialize();
        }
    }
}
