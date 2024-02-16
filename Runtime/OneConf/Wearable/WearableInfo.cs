/*
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
using System.Globalization;

namespace Chocopoi.DressingTools.OneConf.Wearable
{
    /// <summary>
    /// Wearable meta information
    /// </summary>
    internal class WearableInfo
    {
        /// <summary>
        /// UUID of this configuration
        /// </summary>
        public string uuid;

        /// <summary>
        /// Name of wearable
        /// </summary>
        public string name;

        /// <summary>
        /// Author of this configuration
        /// </summary>
        public string author;

        /// <summary>
        /// Description of this configuration
        /// </summary>
        public string description;

        /// <summary>
        /// Configuration created time in ISO8601 standard
        /// </summary>
        public string createdTime;

        /// <summary>
        /// Configuration updated time in ISO8601 standard
        /// </summary>
        public string updatedTime;

        /// <summary>
        /// Thumbnail PNG encoded in Base64 string. The thumbnail can be transparent.
        /// </summary>
        public string thumbnail;

        /// <summary>
        /// Constructs a new wearable info
        /// </summary>
        public WearableInfo() { }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="toCopy">Info to copy</param>
        public WearableInfo(WearableInfo toCopy)
        {
            uuid = toCopy.uuid;
            name = toCopy.name;
            author = toCopy.author;
            description = toCopy.description;
            createdTime = toCopy.createdTime;
            updatedTime = toCopy.updatedTime;
            thumbnail = null;
        }

        /// <summary>
        /// Refresh updated time
        /// </summary>
        public void RefreshUpdatedTime()
        {
            updatedTime = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
        }
    }
}
