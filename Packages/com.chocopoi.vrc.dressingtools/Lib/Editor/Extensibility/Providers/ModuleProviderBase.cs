/*
 * File: ModuleProviderBase.cs
 * Project: DressingTools
 * Created Date: Saturday, Aug 24th 2023, 15:31:11 am
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

using Chocopoi.DressingTools.Lib.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Chocopoi.DressingTools.Lib.Extensibility.Providers
{
    public interface IModuleConfig { }

    public class UnknownModuleConfig : IModuleConfig
    {
        public string RawJson { get; private set; }
        public UnknownModuleConfig(string rawJson)
        {
            RawJson = rawJson;
        }
    }

    public abstract class ModuleProviderBase
    {
        public abstract string ModuleIdentifier { get; }
        public abstract string FriendlyName { get; }
        public abstract int CallOrder { get; }
        public abstract bool AllowMultiple { get; }

        public abstract IModuleConfig DeserializeModuleConfig(JObject jObject);
        public virtual string SerializeModuleConfig(IModuleConfig moduleConfig) => JsonConvert.SerializeObject(moduleConfig);
        public abstract IModuleConfig NewModuleConfig();
    }
}
