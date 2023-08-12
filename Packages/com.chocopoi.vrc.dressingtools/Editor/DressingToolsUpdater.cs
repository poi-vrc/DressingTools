/*
 * File: DressingToolsUpdater.cs
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
using System.IO;
using System.Net;
using System.Threading;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools
{
    public class DressingToolsUpdater
    {
        [Serializable]
        public class ManifestInfo
        {
            public int version;
            public int compat_version;
        }

        [Serializable]
        public class ManifestBranch
        {
            public string name;
            public string version;
            public string updated_at;
            public string github_url;
            public string booth_url;
        }

        [Serializable]
        public class Manifest
        {
            public ManifestInfo info;
            public string default_branch;
            public ManifestBranch[] branches;
        }

        public class ParsedVersion
        {
            public string fullVersionString;
            public string version;
            public int[] versionNumbers;
        }

        private const int SupportedManifestVersion = 1;

        private const string ManifestJsonUrl = "https://poi-vrc.github.io/DressingTools/updater_manifest.json";

        private static Manifest s_manifest = null;

        private static ParsedVersion s_currentVersion = null;

        private static bool s_checkingUpdate = false;

        private static DateTime s_lastUpdateCheckTime = DateTime.MinValue;

        private static bool s_lastUpdateCheckErrored = false;

        private static bool s_branchNotFoundWarningSent = false;

        public static ParsedVersion GetCurrentVersion()
        {
            if (s_currentVersion != null)
            {
                return s_currentVersion;
            }

            try
            {
                var reader = new StreamReader("Packages/com.chocopoi.vrc.dressingtools/package.json");
                var str = reader.ReadToEnd();
                reader.Close();

                var packageJson = JObject.Parse(str);

                if (!packageJson.ContainsKey("version"))
                {
                    Debug.LogError("[DressingTools] Error: package.json does not contain version key!");
                    return null;
                }

                return s_currentVersion = ParseVersionString(packageJson.Value<string>("version"));
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return null;
            }
        }

        public static bool IsUpdateChecked()
        {
            return s_manifest != null || (DateTime.Now - s_lastUpdateCheckTime).TotalMinutes <= 5;
        }

        public static bool IsLastUpdateCheckErrored()
        {
            return s_lastUpdateCheckErrored;
        }

        public static void FetchOnlineVersion(Action<Manifest> callback)
        {
            if (s_checkingUpdate)
            {
                return;
            }
            s_checkingUpdate = true;

            new Thread(() =>
            {
                try
                {
                    var request = WebRequest.Create(ManifestJsonUrl + "?" + DateTimeOffset.Now.ToUnixTimeSeconds());
                    var response = request.GetResponse();

                    var reader = new StreamReader(response.GetResponseStream());
                    var json = reader.ReadToEnd();

                    s_manifest = JsonUtility.FromJson<Manifest>(json);

                    if (s_manifest.info.compat_version > SupportedManifestVersion)
                    {
                        s_manifest = null;
                        Debug.LogError("[DressingTools] This DressingTools update checker is too old and does not support the online updater manifest. Please download the latest version to check updates.");
                        EditorUtility.DisplayDialog("DressingTools", "This DressingTools update checker is too old and does not support the online updater manifest. Please download the latest version to check updates.", "OK");
                        s_lastUpdateCheckErrored = true;
                    }

                    callback?.Invoke(s_manifest);
                }
                catch (Exception e)
                {
                    Debug.LogError("[DressingTools] Check update failed: " + e.Message);
                    s_lastUpdateCheckErrored = true;
                }

                s_lastUpdateCheckTime = DateTime.Now;
                s_checkingUpdate = false;
            }).Start();
        }

        public static ManifestBranch GetManifestBranch(string branchName)
        {
            if (s_manifest == null)
            {
                throw new Exception("Online updater manifest has not been loaded.");
            }

            for (var i = 0; i < s_manifest.branches.Length; i++)
            {
                if (s_manifest.branches[i].name == branchName)
                {
                    return s_manifest.branches[i];
                }
            }

            return null;
        }

        public static ManifestBranch GetBranchLatestVersion(string branchName)
        {
            var branch = GetManifestBranch(branchName);

            if (branch == null)
            {
                if (!s_branchNotFoundWarningSent)
                {
                    s_branchNotFoundWarningSent = true;
                    Debug.LogWarning("Branch \"" + branchName + "\" not found in manifest. Using default branch \"" + s_manifest.default_branch + "\" instead");
                }

                branch = GetManifestBranch(s_manifest.default_branch);

                if (branch == null)
                {
                    throw new Exception("Default branch provided by manifest \"" + s_manifest.default_branch + "\" not found!");
                }
            }

            return branch;
        }

        public static string GetDefaultBranchName()
        {
            if (s_manifest == null)
            {
                throw new Exception("Online updater manifest has not been loaded.");
            }

            return s_manifest.default_branch;
        }

        public static string[] GetAvailableBranches()
        {
            if (s_manifest == null)
            {
                throw new Exception("Online updater manifest has not been loaded.");
            }

            var strs = new string[s_manifest.branches.Length];
            for (var i = 0; i < strs.Length; i++)
            {
                strs[i] = s_manifest.branches[i].name;
            }

            return strs;
        }

        public static bool IsUpdateAvailable()
        {
            if (s_currentVersion == null || s_manifest == null)
            {
                throw new Exception("Current version or online updater manifest has not been loaded.");
            }

            var preferences = PreferencesUtility.GetPreferences();

            var branch = GetBranchLatestVersion(preferences.app.updateBranch);
            var remoteVersion = ParseVersionString(branch.version);

            if (remoteVersion == null)
            {
                throw new Exception("Error parsing remote version string \"" + branch.version + "\"!");
            }

            //return CompareVersions(remoteVersion, currentVersion) > 0;
            return !remoteVersion.version.Equals(s_currentVersion.version);
        }

        public static ParsedVersion ParseVersionString(string str)
        {
            var pv = new ParsedVersion();

            pv.fullVersionString = str;

            //find the first hyphen first, we ignore the content after hyphen since v2
            var hyphenIndex = str.IndexOf('-');

            if (hyphenIndex != -1)
            {
                //split the version part
                pv.version = str.Substring(0, hyphenIndex);
            }
            else
            {
                pv.version = str;
            }

            var strs = pv.version.Split('.');

            if (strs.Length < 3)
            {
                Debug.LogError("[DressingTools] Version string \"" + str + "\" is invalid and has less than 3 numbers in version part.");
                return null;
            }

            pv.versionNumbers = new int[strs.Length];
            for (var i = 0; i < strs.Length; i++)
            {
                pv.versionNumbers[i] = int.Parse(strs[i]);
            }

            return pv;
        }

        public static int CompareVersions(ParsedVersion a, ParsedVersion b)
        {
            if (a.versionNumbers.Length > b.versionNumbers.Length)
            {
                return 1;
            }
            else if (a.versionNumbers.Length < b.versionNumbers.Length)
            {
                return -1;
            }

            for (var i = 0; i < a.versionNumbers.Length; i++)
            {
                if (a.versionNumbers[i] > b.versionNumbers[i])
                {
                    return 1;
                }
                else if (a.versionNumbers[i] < b.versionNumbers[i])
                {
                    return -1;
                }
            }

            //same
            return 0;
        }
    }
}
