/*
 * File: Preferences.cs
 * Project: DressingTools
 * Created Date: Saturday, July 22nd 2023, 12:36:56 am
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
using Chocopoi.DressingTools.Lib.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Chocopoi.DressingTools
{
    [Serializable]
    internal class Preferences
    {
        public static readonly SerializationVersion CurrentConfigVersion = new SerializationVersion(1, 0, 0);

        public class App
        {
            public string selectedLanguage;
            public string updateBranch;

            public App()
            {
                ResetToDefaults();
            }

            public void ResetToDefaults()
            {
                selectedLanguage = Localization.I18n.DefaultLocale;
                updateBranch = "v2";
            }
        }

        public class Cabinet
        {
            public string defaultArmatureName;
            public bool defaultGroupDynamics;
            public bool defaultGroupDynamicsSeparateDynamics;
            public bool defaultAnimationWriteDefaults;

            public Cabinet()
            {
                ResetToDefaults();
            }

            public void ResetToDefaults()
            {
                defaultArmatureName = "Armature";
                defaultGroupDynamics = true;
                defaultGroupDynamicsSeparateDynamics = true;
                defaultAnimationWriteDefaults = true;
            }
        }

        public SerializationVersion version;
        public App app;
        public Cabinet cabinet;

        public Preferences()
        {
            version = CurrentConfigVersion;
            app = new App();
            cabinet = new Cabinet();
        }

        public void ResetToDefaults()
        {
            app.ResetToDefaults();
            cabinet.ResetToDefaults();
        }

        public string Serialize()
        {
            version = CurrentConfigVersion;
            return JsonConvert.SerializeObject(this);
        }
    }
}
