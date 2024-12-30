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
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Chocopoi.DressingTools
{
    internal class UpdateChecker
    {
        public class ParsedVersion
        {
            public static readonly ParsedVersion Zero = new("0.0.0");

            public readonly string fullString;
            public readonly string version;
            public readonly string extra;
            public readonly int[] versionNumbers;

            public ParsedVersion(string str)
            {
                fullString = str;

                //find the first hyphen first, we ignore the content after hyphen since v2
                var hyphenIndex = str.IndexOf('-');

                if (hyphenIndex != -1)
                {
                    //split the version part
                    version = str[..hyphenIndex];
                    extra = str[(hyphenIndex + 1)..];
                }
                else
                {
                    version = str;
                    extra = null;
                }

                var strs = version.Split('.');

                versionNumbers = new int[] { 0, 0, 0 };
                var len = Math.Min(versionNumbers.Length, strs.Length);
                for (var i = 0; i < len; i++)
                {
                    versionNumbers[i] = int.Parse(strs[i]);
                }
            }

            public int Compare(ParsedVersion b)
            {
                if (versionNumbers.Length > b.versionNumbers.Length)
                {
                    return 1;
                }
                else if (versionNumbers.Length < b.versionNumbers.Length)
                {
                    return -1;
                }

                for (var i = 0; i < versionNumbers.Length; i++)
                {
                    if (versionNumbers[i] > b.versionNumbers[i])
                    {
                        return 1;
                    }
                    else if (versionNumbers[i] < b.versionNumbers[i])
                    {
                        return -1;
                    }
                }

                //same
                return 0;
            }
        }

        private const string VpmJsonUrl = "https://vpm.chocopoi.com/index.json";
        private const string PackageName = "com.chocopoi.vrc.dressingtools";
        private const string PackageJsonPath = "Packages/com.chocopoi.vrc.dressingtools/package.json";
        private const int UpdateCheckPeriodMinutes = 15;

        private static ParsedVersion s_currentVersion = null;
        private static ParsedVersion s_latestVersion = null;
        private static DateTime s_lastUpdateCheckTime = DateTime.MinValue;

        public static ParsedVersion CurrentVersion => s_currentVersion ??= GetLocalPackageJsonVersion();

        public static ParsedVersion LatestVersion => (DateTime.Now - s_lastUpdateCheckTime).TotalMinutes > UpdateCheckPeriodMinutes ? (s_latestVersion = GetOnlineLatestVersion()) : s_latestVersion;

        private static ParsedVersion GetOnlineLatestVersion()
        {
            s_lastUpdateCheckTime = DateTime.Now;
            try
            {
                var request = WebRequest.Create(VpmJsonUrl + "?" + DateTimeOffset.Now.ToUnixTimeSeconds());
                var response = request.GetResponse();

                var reader = new StreamReader(response.GetResponseStream());
                var jsonStr = reader.ReadToEnd();

                var vpmJson = JObject.Parse(jsonStr);

                if (!vpmJson.ContainsKey("packages"))
                {
                    Debug.LogWarning("[DressingTools] VPM json does not contain property \"packages\"");
                    return CurrentVersion;
                }

                var packages = vpmJson.Value<JObject>("packages");

                if (!packages.ContainsKey(PackageName))
                {
                    Debug.LogWarning("[DressingTools] VPM json does not contain package \"" + PackageName + "\"");
                    return CurrentVersion;
                }

                var package = packages.Value<JObject>(PackageName);
                if (!package.ContainsKey("versions"))
                {
                    Debug.LogWarning("[DressingTools] VPM json package \"" + PackageName + "\" does not contain property \"versions\"");
                    return CurrentVersion;
                }

                // loop through all versions to find the max
                var latestVersion = CurrentVersion;
                var packageVersions = package.Value<JObject>("versions");
                foreach (var version in packageVersions)
                {
                    var pv = new ParsedVersion(version.Key);

                    if (pv.extra != null)
                    {
                        // TODO: ignore those with extra for now (pre-release packages)
                        continue;
                    }

                    if (pv.Compare(latestVersion) > 0)
                    {
                        latestVersion = pv;
                    }
                }

                return latestVersion;
            }
            catch (Exception e)
            {
                Debug.LogWarning(e.StackTrace);
                Debug.LogWarning("[DressingTools] Check update from VPM json failed: " + e.Message);
                return CurrentVersion;
            }
        }

        private static ParsedVersion GetLocalPackageJsonVersion()
        {
            try
            {
                var reader = new StreamReader(PackageJsonPath);
                var str = reader.ReadToEnd();
                reader.Close();

                var packageJson = JObject.Parse(str);

                if (!packageJson.ContainsKey("version"))
                {
                    Debug.LogWarning("[DressingTools] Error: package.json does not contain version key!");
                    return ParsedVersion.Zero;
                }

                return new ParsedVersion(packageJson.Value<string>("version"));
            }
            catch (Exception e)
            {
                Debug.LogWarning(e.StackTrace);
                Debug.LogWarning("[DressingTools] Error: Unable to parse package json: " + e.Message);
                return ParsedVersion.Zero;
            }
        }

        public static bool IsUpdateChecked()
        {
            return (DateTime.Now - s_lastUpdateCheckTime).TotalMinutes <= UpdateCheckPeriodMinutes;
        }

        public static bool IsUpdateAvailable()
        {
            return LatestVersion.Compare(CurrentVersion) > 0;
        }

        public static void InvalidateVersionCheckCache()
        {
            s_lastUpdateCheckTime = DateTime.MinValue;
        }
    }
}
