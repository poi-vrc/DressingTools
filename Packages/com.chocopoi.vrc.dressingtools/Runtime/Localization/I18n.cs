using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Chocopoi.DressingTools.Localization
{
    public class I18n
    {
        private static readonly string DEFAULT_LOCALE = "en";

        private static I18n instance = null;

        public static I18n GetInstance()
        {
            return instance ?? (instance = new I18n());
        }

        private Dictionary<string, JObject> translations = null;

        private string selectedLocale = null;

        private I18n()
        {
            LoadTranslations();
        }

        public string[] GetAvailableLocales()
        {
            if (translations == null)
            {
                return new string[] { };
            }

            var keys = new string[translations.Keys.Count];
            translations.Keys.CopyTo(keys, 0);

            return keys;
        }

        public void LoadTranslations()
        {
            translations = new Dictionary<string, JObject>();

            var translationFileNames = Directory.GetFiles("Packages/com.chocopoi.vrc.dressingtools/Translations", "*.json");
            foreach (var translationFileName in translationFileNames)
            {
                try
                {
                    var reader = new StreamReader(translationFileName);
                    var json = reader.ReadToEnd();
                    reader.Close();
                    translations.Add(Path.GetFileNameWithoutExtension(translationFileName), JObject.Parse(json));
                }
                catch (IOException e)
                {
                    Debug.LogError(e);
                }
            }
        }

        public void SetLocale(string locale)
        {
            selectedLocale = locale;
        }

        public string _(string key, params object[] args)
        {
            return Translate(key, args);
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

            if ((value = TranslateByLocale(selectedLocale, key, args)) != null)
            {
                return value;
            }

            if ((value = TranslateByLocale(DEFAULT_LOCALE, key, args)) != null)
            {
                return value;
            }

            return fallback ?? string.Format("{0} ({1})", key, JoinArrayToString(args));
        }

        public string TranslateByLocale(string locale, string key, params object[] args)
        {
            if (locale != null && translations.ContainsKey(locale))
            {
                translations.TryGetValue(locale, out var t);

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

