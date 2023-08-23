/*
 * File: WearableConfigVersionedObject.cs
 * Project: DressingTools
 * Created Date: Saturday, Aug 23th 2023, 09:39:11 am
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
    public abstract class WearableConfigVersionedObject
    {
        public abstract WearableConfigVersion Version { get; set; }

        public abstract ISerializer GetSerializerByVersion(WearableConfigVersion version);

        public virtual void DeserializeFrom(JObject jObject)
        {
            // TODO: do schema check

            var moduleVersion = jObject.ContainsKey("version") ?
                new WearableConfigVersion(jObject["version"].Value<string>()) :
                null;
            var serializer = GetSerializerByVersion(moduleVersion);

            if (serializer == null)
            {
                throw new Exception("Incompatible object version for deserialization");
            }

            serializer.DeserializeFrom(this, jObject);
        }

        public virtual JObject Serialize()
        {
            var serializer = GetSerializerByVersion(Version);

            if (serializer == null)
            {
                throw new Exception("Incompatible object version for serialization");
            }

            return serializer.SerializeFrom(this);
        }
    }
}
