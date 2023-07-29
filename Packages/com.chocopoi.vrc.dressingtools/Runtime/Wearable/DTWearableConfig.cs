using System;
using System.Collections.Generic;
using System.Globalization;
using Chocopoi.DressingTools.Wearable.Modules;
using Newtonsoft.Json;
using UnityEngine;

namespace Chocopoi.DressingTools.Wearable
{
    // serialization is handled by newtonsoft json
    public class DTWearableConfig
    {
        public const int CurrentConfigVersion = 1;

        public int configVersion;
        public DTWearableInfo info;
        public DTAvatarConfig targetAvatarConfig;

        [JsonConverter(typeof(DTWearableModuleBaseConverter))]
        public DTWearableModuleBase[] modules;

        public DTWearableConfig()
        {
            // initialize some fields
            var isoTimeStr = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
            info = new DTWearableInfo
            {
                uuid = Guid.NewGuid().ToString(),
                createdTime = isoTimeStr,
                updatedTime = isoTimeStr
            };
            targetAvatarConfig = new DTAvatarConfig();
            modules = new DTWearableModuleBase[] {
                new ArmatureMappingModule()
            };
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

        public static DTWearableConfig Deserialize(string json)
        {
            return JsonConvert.DeserializeObject<DTWearableConfig>(json);
        }

        public DTWearableConfig Clone()
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
