/*
 * File: ISerializer.cs
 * Project: DressingTools
 * Created Date: Saturday, Aug 22nd 2023, 16:35:11 pm
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
using Newtonsoft.Json.Linq;

namespace Chocopoi.DressingTools.Lib.Wearable.Serializers
{
    public interface ISerializer
    {
        JObject Serialize(object obj);
        void Deserialize(object obj, JObject jObject);
    }
}
