/*
 * File: WearableConfigUtility.cs
 * Project: DressingFramework
 * Created Date: Tuesday, Sep 26th 2023, 05:01:33 pm
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
using Chocopoi.DressingFramework;
using Chocopoi.DressingFramework.Serialization;
using Chocopoi.DressingTools.OneConf.Wearable;
using Newtonsoft.Json;
using UnityEngine;

namespace Chocopoi.DressingTools.OneConf.Serialization
{
    /// <summary>
    /// Utility functions for wearable config
    /// </summary>
    internal static class WearableConfigUtility
    {
        /// <summary>
        /// Attempts to deserialize wearable config JSON
        /// </summary>
        /// <param name="json">JSON string</param>
        /// <param name="config">Output wearable config</param>
        /// <returns>Success or not</returns>
        public static bool TryDeserialize(string json, out WearableConfig config)
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

        /// <summary>
        /// Serialize wearable config into JSON
        /// </summary>
        /// <param name="config">Wearable config</param>
        /// <returns>Serialized JSON string</returns>
        public static string Serialize(WearableConfig config) => JsonConvert.SerializeObject(config);

        /// <summary>
        /// Deserialize wearable config
        /// </summary>
        /// <param name="json">Serialized JSON string</param>
        /// <returns>Wearable config</returns>
        /// <exception cref="Exception">Exception during deserialization or incompatible config version</exception>
        public static WearableConfig Deserialize(string json)
        {
            // TODO: perform schema check
            var jObject = DKEditorUtils.ParseJson(json);

            var version = jObject["version"].ToObject<SerializationVersion>();
            if (version.Major > WearableConfig.CurrentConfigVersion.Major)
            {
                throw new Exception("Incompatible wearable config version: " + version.Major + " > " + WearableConfig.CurrentConfigVersion.Major);
            }

            var serializer = new JsonSerializer();
            serializer.Converters.Add(new WearableModuleConverter());

            return jObject.ToObject<WearableConfig>(serializer);
        }

        /// <summary>
        /// Clone the wearable config
        /// </summary>
        /// <param name="config">Wearable config</param>
        /// <returns>Cloned wearable config</returns>
        public static WearableConfig Clone(WearableConfig config)
        {
            // a tricky and easier way to copy
            return Deserialize(Serialize(config));
        }
    }
}
