using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
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
            public string full_version_string;
            public string version;
            public int[] versionNumbers;
            public string branch;
        }

        private static readonly int SupportedManifestVersion = 1;

        private static readonly string ManifestJsonUrl = "https://poi-vrc.github.io/DressingTools/updater_manifest.json";

        private static Manifest manifest = null;

        private static ParsedVersion currentVersion = null;

        private static bool checkingUpdate = false;

        private static DateTime lastUpdateCheckTime = DateTime.MinValue;

        private static bool lastUpdateCheckErrored = false;

        private static bool branchNotFoundWarningSent = false;

        public static ParsedVersion GetCurrentVersion()
        {
            if (currentVersion != null)
            {
                return currentVersion;
            }

            try
            {
                StreamReader reader = new StreamReader("Packages/com.chocopoi.vrc.dressingtools/version.txt");
                string str = reader.ReadToEnd();
                // remove newline characters and trim string
                str = str.Trim().Replace("\n", "").Replace("\r", "");
                reader.Close();
                return currentVersion = ParseVersionString(str);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return null;
            }
        }

        public static bool IsUpdateChecked()
        {
            return manifest != null || (DateTime.Now - lastUpdateCheckTime).TotalMinutes <= 5;
        }

        public static bool IsLastUpdateCheckErrored()
        {
            return lastUpdateCheckErrored;
        }

        public static void FetchOnlineVersion(Action<Manifest> callback)
        {
            if (checkingUpdate)
            {
                return;
            }
            checkingUpdate = true;

            new Thread(() =>
            {
                try
                {
                    WebRequest request = WebRequest.Create(ManifestJsonUrl + "?" + DateTimeOffset.Now.ToUnixTimeSeconds());
                    WebResponse response = request.GetResponse();

                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    string json = reader.ReadToEnd();

                    manifest = JsonUtility.FromJson<Manifest>(json);

                    if (manifest.info.compat_version > SupportedManifestVersion)
                    {
                        manifest = null;
                        Debug.LogError("[DressingTools] This DressingTools update checker is too old and does not support the online updater manifest. Please download the latest version to check updates.");
                        EditorUtility.DisplayDialog("DressingTools", "This DressingTools update checker is too old and does not support the online updater manifest. Please download the latest version to check updates.", "OK");
                        lastUpdateCheckErrored = true;
                    }

                    callback?.Invoke(manifest);
                }
                catch (Exception e)
                {
                    Debug.LogError("[DressingTools] Check update failed: " + e.Message);
                    lastUpdateCheckErrored = true;
                }

                lastUpdateCheckTime = DateTime.Now;
                checkingUpdate = false;
            }).Start();
        }

        public static ManifestBranch GetManifestBranch(string branchName)
        {
            if (manifest == null)
            {
                throw new Exception("Online updater manifest has not been loaded.");
            }

            for (int i = 0; i < manifest.branches.Length; i++)
            {
                if (manifest.branches[i].name == branchName)
                {
                    return manifest.branches[i];
                }
            }

            return null;
        }

        public static ManifestBranch GetBranchLatestVersion(string branchName)
        {
            ManifestBranch branch = GetManifestBranch(branchName);

            if (branch == null)
            {
                if (!branchNotFoundWarningSent)
                {
                    branchNotFoundWarningSent = true;
                    Debug.LogWarning("Branch \"" + currentVersion.branch + "\" not found in manifest. Using default branch \"" + manifest.default_branch + "\" instead");
                }

                branch = GetManifestBranch(manifest.default_branch);

                if (branch == null)
                {
                    throw new Exception("Default branch provided by manifest \"" + manifest.default_branch + "\" not found!");
                }
            }

            return branch;
        }

        public static string GetDefaultBranchName()
        {
            if (manifest == null)
            {
                throw new Exception("Online updater manifest has not been loaded.");
            }

            return manifest.default_branch;
        }

        public static string[] GetAvailableBranches()
        {
            if (manifest == null)
            {
                throw new Exception("Online updater manifest has not been loaded.");
            }

            string[] strs = new string[manifest.branches.Length];
            for (int i = 0; i < strs.Length; i++)
            {
                strs[i] = manifest.branches[i].name;
            }

            return strs;
        }

        public static bool IsUpdateAvailable()
        {
            if (currentVersion == null || manifest == null)
            {
                throw new Exception("Current version or online updater manifest has not been loaded.");
            }

            if (currentVersion.branch == null || currentVersion.branch == "")
            {
                Debug.LogWarning("Current version branch is empty, which is unexpected. Defaults to tell user there is an update anyways.");
                return true;
            }

            Preferences.Json preferences = Preferences.GetPreferences();

            ManifestBranch branch = GetBranchLatestVersion(preferences.app.update_branch);
            ParsedVersion remoteVersion = ParseVersionString(branch.version);

            if (remoteVersion == null)
            {
                throw new Exception("Error parsing remote version string \"" + branch.version + "\"!");
            }

            return !currentVersion.branch.Equals(remoteVersion.branch) || CompareVersions(remoteVersion, currentVersion) > 0;
        }

        public static ParsedVersion ParseVersionString(string str)
        {
            ParsedVersion pv = new ParsedVersion();

            pv.full_version_string = str;

            //find the first hyphen first
            int hyphenIndex = str.IndexOf('-');

            if (hyphenIndex == -1)
            {
                //previous versions does not have branches, skipping them
                pv.branch = null;
            }
            else
            {
                pv.branch = str.Substring(hyphenIndex + 1);
            }

            //split the version part
            pv.version = str.Substring(0, hyphenIndex);
            string[] strs = pv.version.Split('.');

            if (strs.Length < 3)
            {
                Debug.LogError("[DressingTools] Version string \"" + str + "\" is invalid and has less than 3 numbers in version part.");
                return null;
            }

            pv.versionNumbers = new int[strs.Length];
            for (int i = 0; i < strs.Length; i++)
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

            for (int i = 0; i < a.versionNumbers.Length; i++)
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
