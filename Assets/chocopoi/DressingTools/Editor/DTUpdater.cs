using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using UnityEngine;
using UnityEditor;

namespace Chocopoi.DressingTools
{
    public class DTUpdater
    {
        public class ManifestInfo
        {
            public int version;
            public int compat_version;
        }

        public class ManifestBranch
        {
            public string name;
            public string version;
            public string updated_at;
            public string github_url;
            public string booth_url;
        }

        public class Manifest
        {
            public ManifestInfo info;
            public string default_branch;
            public ManifestBranch[] branches;
        }

        public class ParsedVersion
        {
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

        public static ParsedVersion GetCurrentVersion()
        {
            if (currentVersion != null)
            {
                return currentVersion;
            }

            StreamReader reader = new StreamReader("Assets/chocopoi/DressingTools/version.txt");
            string str = reader.ReadToEnd();
            reader.Close();
            return currentVersion = ParseVersionString(str);
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
                    WebRequest request = WebRequest.Create(ManifestJsonUrl);
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
                Debug.LogWarning("Branch \"" + currentVersion.branch + "\" not found in manifest. Using default branch \"" + manifest.default_branch + "\" instead");
                branch = GetManifestBranch(manifest.default_branch);

                if (branch == null)
                {
                    throw new Exception("Default branch provided by manifest \"" + manifest.default_branch + "\" not found!");
                }
            }

            return branch;
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

            ManifestBranch branch = GetBranchLatestVersion(currentVersion.branch);
            ParsedVersion remoteVersion = ParseVersionString(branch.version);

            if (remoteVersion == null)
            {
                throw new Exception("Error parsing remote version string \"" + branch.version + "\"!");
            }

            return CompareVersions(remoteVersion, currentVersion) > 0;
        }

        public static ParsedVersion ParseVersionString(string str)
        {
            ParsedVersion pv = new ParsedVersion();

            //find the first hyphen first
            int hyphenIndex = str.IndexOf('-');

            if (hyphenIndex == -1)
            {
                //previous versions does not have branches, skipping them
                pv.branch = null;
            } else
            {
                pv.branch = str.Substring(hyphenIndex + 1);
            }

            //split the version part
            pv.version = str.Substring(0, hyphenIndex);
            string[] strs = pv.version.Split('.');

            if (strs.Length != 3)
            {
                Debug.LogError("[DressingTools] Version string \"" + str + "\" is invalid and does not have exact 3 numbers in version part.");
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
            //compare major
            if (a.versionNumbers[0] > b.versionNumbers[0])
            {
                return 1;
            } else if (a.versionNumbers[0] < b.versionNumbers[0])
            {
                return -1;
            }

            //compare minor
            if (a.versionNumbers[1] > b.versionNumbers[1])
            {
                return 1;
            }
            else if (a.versionNumbers[1] < b.versionNumbers[1])
            {
                return -1;
            }

            //compare patch
            if (a.versionNumbers[2] > b.versionNumbers[2])
            {
                return 1;
            }
            else if (a.versionNumbers[2] < b.versionNumbers[2])
            {
                return -1;
            }

            //same
            return 0;
        }
    }
}
