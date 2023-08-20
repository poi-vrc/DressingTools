/*
 * File: DTWearableConfig.cs
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
using Newtonsoft.Json;

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

    // serialization is handled by newtonsoft json
    public class WearableConfig
    {
        public const int CurrentConfigVersion = 1;

        public int configVersion;
        public WearableInfo info;
        public AvatarConfig targetAvatarConfig;

        [JsonConverter(typeof(WearableModuleConverter))]
        public List<WearableModuleBase> modules;

        public WearableConfig()
        {
            // initialize some fields
            var isoTimeStr = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
            info = new WearableInfo
            {
                uuid = Guid.NewGuid().ToString(),
                createdTime = isoTimeStr,
                updatedTime = isoTimeStr
            };
            targetAvatarConfig = new AvatarConfig();
            modules = new List<WearableModuleBase>();
        }

        public bool HasUnknownModules()
        {
            foreach (var module in modules)
            {
                if (module is UnknownModule)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsConfigVersionCompatible()
        {
            return configVersion <= CurrentConfigVersion;
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static WearableConfig Deserialize(string json)
        {
            return JsonConvert.DeserializeObject<WearableConfig>(json);
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
