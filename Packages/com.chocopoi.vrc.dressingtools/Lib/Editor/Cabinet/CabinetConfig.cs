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
using Chocopoi.DressingTools.Lib.Cabinet.Modules;
using Chocopoi.DressingTools.Lib.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Chocopoi.DressingTools.Lib.Cabinet
{
    public class CabinetConfig
    {
        public static readonly SerializationVersion CurrentConfigVersion = new SerializationVersion(1, 0, 0);

        public SerializationVersion version;
        public string avatarArmatureName;
        public bool groupDynamics;
        public bool groupDynamicsSeparateGameObjects;
        public bool animationWriteDefaults;

        public List<CabinetModule> modules;

        public CabinetConfig()
        {
            version = CurrentConfigVersion;
            avatarArmatureName = "Armature";
            groupDynamics = true;
            groupDynamicsSeparateGameObjects = true;
            animationWriteDefaults = true;
            modules = new List<CabinetModule>();
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

        public string Serialize() => JsonConvert.SerializeObject(this);

        public static CabinetConfig Deserialize(string json)
        {
            // TODO: perform schema check
            var jObject = JObject.Parse(json);

            var version = jObject["version"].ToObject<SerializationVersion>();
            if (version.Major > CurrentConfigVersion.Major)
            {
                throw new Exception("Incompatbile cabinet config version: " + version.Major + " > " + CurrentConfigVersion.Major);
            }

            return jObject.ToObject<CabinetConfig>();
        }

        public CabinetConfig Clone()
        {
            // a tricky and easier way to copy
            return Deserialize(Serialize());
        }
    }
}
