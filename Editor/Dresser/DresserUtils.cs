/*
 * Copyright (c) 2024 chocopoi
 * 
 * This file is part of DressingTools.
 * 
 * DressingTools is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 * 
 * DressingTools is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with DressingFramework. If not, see <https://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;
using System.IO;
using Chocopoi.DressingTools.Components.Modifiers;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Chocopoi.DressingTools.Dresser
{
    internal class DresserUtils
    {
        private const string BoneNameMappingsPath = "Packages/com.chocopoi.vrc.dressingtools/Resources/BoneNameMappings.json";
        private static List<List<string>> s_boneNameMappings = null;

        private static void LoadBoneNameMappings()
        {
            try
            {
                var reader = new StreamReader(BoneNameMappingsPath);
                var json = reader.ReadToEnd();
                reader.Close();
                var jObj = JObject.Parse(json);
                s_boneNameMappings = jObj["mappings"].ToObject<List<List<string>>>();
            }
            catch (IOException e)
            {
                Debug.LogError(e);
            }
        }

        private static string BeautifyBoneName(string str)
        {
            // trim the string
            var output = str.Trim();

            // check if there is a prefix
            if (output.StartsWith("("))
            {
                //find the first closing bracket
                var prefixBracketEnd = output.IndexOf(")");
                if (prefixBracketEnd != -1 && prefixBracketEnd != output.Length - 1) //remove it if there is
                {
                    output = output.Substring(prefixBracketEnd + 1).Trim();
                }
            }

            // check if there is a suffix
            if (output.EndsWith(")"))
            {
                //find the first closing bracket
                var suffixBracketStart = output.LastIndexOf("(");
                if (suffixBracketStart != -1 && suffixBracketStart != 0) //remove it if there is
                {
                    output = output.Substring(0, suffixBracketStart).Trim();
                }
            }

            return output;
        }

        public static Transform FindMatchingBone(Transform boneParent, string childName)
        {
            // load bone name mappings if needed
            if (s_boneNameMappings == null)
            {
                LoadBoneNameMappings();
            }

            childName = BeautifyBoneName(childName);

            var exactMatchBoneTransform = boneParent.Find(childName);
            if (exactMatchBoneTransform != null)
            {
                // exact match
                return exactMatchBoneTransform;
            }

            // try match it via the mapping list
            foreach (var boneNames in s_boneNameMappings)
            {
                if (boneNames.Contains(childName))
                {
                    foreach (var boneName in boneNames)
                    {
                        var remappedBoneTransform = boneParent.Find(boneName);
                        if (remappedBoneTransform != null)
                        {
                            // found alternative bone name
                            return remappedBoneTransform;
                        }
                    }
                }
            }

            // match failure
            return null;
        }

        public static void HandleObjectMappingOverrides(List<DTObjectMapping.Mapping> generatedMappings, List<DTObjectMapping.Mapping> overrideMappings)
        {
            var toAdd = new List<DTObjectMapping.Mapping>();
            foreach (var mappingOverride in overrideMappings)
            {
                var matched = false;

                foreach (var originalMapping in generatedMappings)
                {
                    // override on match
                    if (originalMapping.SourceTransform == mappingOverride.SourceTransform)
                    {
                        originalMapping.TargetPath = mappingOverride.TargetPath;
                        originalMapping.Type = mappingOverride.Type;
                        matched = true;
                        break;
                    }
                }

                if (!matched)
                {
                    // add mapping if not matched
                    toAdd.Add(mappingOverride);
                }
            }
            generatedMappings.AddRange(toAdd);
        }

        public static void HandleTagsOverrides(List<DTArmatureMapping.Tag> generatedTags, List<DTArmatureMapping.Tag> overrideTags)
        {
            var toAdd = new List<DTArmatureMapping.Tag>();
            foreach (var tagOverride in overrideTags)
            {
                var matched = false;

                foreach (var originalMapping in generatedTags)
                {
                    // override on match
                    if (originalMapping.SourceTransform == tagOverride.SourceTransform)
                    {
                        originalMapping.Type = tagOverride.Type;
                        if (tagOverride.Type == DTArmatureMapping.Tag.TagType.IgnoreTransform)
                        {
                            originalMapping.TargetPath = tagOverride.TargetPath;
                        }
                        else
                        if (tagOverride.Type == DTArmatureMapping.Tag.TagType.CopyDynamics)
                        {
                            originalMapping.TargetPath = tagOverride.TargetPath;
                        }
                        matched = true;
                        break;
                    }
                }

                if (!matched)
                {
                    // add mapping if not matched
                    toAdd.Add(tagOverride);
                }
            }
            generatedTags.AddRange(toAdd);
        }
    }
}
