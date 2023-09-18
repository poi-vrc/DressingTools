/*
 * File: I18n.cs
 * Project: DressingTools
 * Created Date: Sunday, July 23rd 2023, 11:08:58 pm
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

using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Chocopoi.DressingTools.Localization
{
    internal class I18n
    {
        public const string DefaultLocale = "en";

        private static I18n s_instance = null;
        public static I18n Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = new I18n();
                }
                return s_instance;
            }
        }

        private Dictionary<string, JObject> _translations = null;

        public string CurrentLocale { get; private set; }

        private I18n()
        {
            LoadTranslations();
        }

        public string[] GetAvailableLocales()
        {
            var keys = new string[_translations.Keys.Count];
            _translations.Keys.CopyTo(keys, 0);

            return keys;
        }

        public void LoadTranslations()
        {
            _translations = new Dictionary<string, JObject>();

            var translationFileNames = Directory.GetFiles("Packages/com.chocopoi.vrc.dressingtools/Translations", "*.json");
            foreach (var translationFileName in translationFileNames)
            {
                try
                {
                    var reader = new StreamReader(translationFileName);
                    var json = reader.ReadToEnd();
                    reader.Close();
                    _translations.Add(Path.GetFileNameWithoutExtension(translationFileName), JObject.Parse(json));
                }
                catch (IOException e)
                {
                    Debug.LogError(e);
                }
            }
        }

        public void SetLocale(string locale)
        {
            CurrentLocale = locale;
        }

        public string _(string key, params object[] args)
        {
            return Translate(key, args);
        }

        public void LocalizeElement(VisualElement elem, bool recursively = true)
        {
            if (elem is Foldout foldout)
            {
                // we process texts that starts with @
                var text = foldout.text;
                if (!string.IsNullOrEmpty(text) && text.StartsWith("@"))
                {
                    var i18nKey = text.Substring(1);
                    foldout.text = Translate(i18nKey);
                }
            }
            else if (elem is TextElement textElem)
            {
                // we process texts that starts with @
                var text = textElem.text;
                if (!string.IsNullOrEmpty(text) && text.StartsWith("@"))
                {
                    var i18nKey = text.Substring(1);
                    textElem.text = Translate(i18nKey);
                }
            }

            if (recursively)
            {
                foreach (var child in elem.Children())
                {
                    LocalizeElement(child, recursively);
                }
            }
        }

        public string Translate(string key, params object[] args)
        {
            return Translate(key, null, args);
        }

        private string JoinArrayToString(object[] arr)
        {
            string output = "";
            for (var i = 0; i < arr.Length; i++)
            {
                output += arr[i].ToString();
                if (i != arr.Length - 1)
                {
                    output += ", ";
                }
            }
            return output;
        }

        public string Translate(string key, string fallback = null, params object[] args)
        {
            string value;

            if ((value = TranslateByLocale(CurrentLocale, key, args)) != null)
            {
                return value;
            }

            if ((value = TranslateByLocale(DefaultLocale, key, args)) != null)
            {
                return value;
            }

            return fallback ?? string.Format("{0} ({1})", key, JoinArrayToString(args));
        }

        public string TranslateByLocale(string locale, string key, params object[] args)
        {
            if (locale != null && _translations.ContainsKey(locale))
            {
                _translations.TryGetValue(locale, out var t);

                var value = t?.Value<string>(key);

                if (value != null)
                {
                    return string.Format(value, args);
                }
            }

            return null;
        }
    }
}

