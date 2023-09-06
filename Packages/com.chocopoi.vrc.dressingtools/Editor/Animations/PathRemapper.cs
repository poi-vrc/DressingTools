/*
 * File: PathRemapper.cs
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

using System.Collections.Generic;
using Chocopoi.AvatarLib.Animations;
using Chocopoi.DressingTools.Lib.Animations;
using UnityEngine;

namespace Chocopoi.DressingTools.Animations
{
    internal class PathRemapper : IPathRemapper
    {
        private Dictionary<GameObject, List<string>> _originalPaths;
        private bool _invalidMappingsCache;
        private Dictionary<string, string> _mappingsCache;
        private bool _invalidNonContainerBoneMappingsCache;
        private Dictionary<string, string> _nonContainerBoneMappingsCache;
        private List<GameObject> _taggedContainerBones;
        private GameObject _avatarRoot;

        public PathRemapper(GameObject avatarRoot)
        {
            _avatarRoot = avatarRoot;
            _originalPaths = new Dictionary<GameObject, List<string>>();
            _invalidMappingsCache = true;
            _invalidNonContainerBoneMappingsCache = true;
            _mappingsCache = new Dictionary<string, string>();
            _nonContainerBoneMappingsCache = new Dictionary<string, string>();
            _taggedContainerBones = new List<GameObject>();

            GenerateAllOriginalPaths(avatarRoot);
        }

        private void GenerateAllOriginalPaths(GameObject root)
        {
            _originalPaths.Clear();
            var transforms = root.GetComponentsInChildren<Transform>(true);
            foreach (var trans in transforms)
            {
                if (trans == root.transform) continue;
                var path = AnimationUtils.GetRelativePath(trans, root.transform);
                _originalPaths[trans.gameObject] = new List<string>() {
                    path
                };
            }
        }

        public void InvalidateCache()
        {
            _invalidMappingsCache = true;
            _invalidNonContainerBoneMappingsCache = true;
            _mappingsCache.Clear();
            _nonContainerBoneMappingsCache.Clear();
        }

        public void TagContainerBone(GameObject gameObject)
        {
            if (!_taggedContainerBones.Contains(gameObject))
            {
                _taggedContainerBones.Add(gameObject);
                InvalidateCache();
            }
        }

        public string Remap(string originalPath, bool avoidContainerBones = false)
        {
            // generate mappings or return cache
            var remappings = GenerateMappings(avoidContainerBones);

            // attempts to find a remapped path
            if (remappings.TryGetValue(originalPath, out var remappedPath))
            {
                return remappedPath;
            }

            // no remaps
            return originalPath;
        }

        private Dictionary<string, string> GenerateMappings(bool avoidContainerBones)
        {
            Dictionary<string, string> mappings = avoidContainerBones ? _nonContainerBoneMappingsCache : _mappingsCache;
            bool invalidCache = avoidContainerBones ? _invalidNonContainerBoneMappingsCache : _invalidMappingsCache;

            // return cache if possible
            if (!invalidCache && mappings != null)
            {
                _invalidMappingsCache = true;
                _invalidNonContainerBoneMappingsCache = true;
                return mappings;
            }

            foreach (KeyValuePair<GameObject, List<string>> pair in _originalPaths)
            {
                var go = pair.Key;

                // traverse up until no container bones are found
                if (avoidContainerBones)
                {
                    var p = go.transform;
                    while (p != null && _taggedContainerBones.Contains(p.gameObject))
                    {
                        p = p.parent;
                    }

                    if (p == null || _taggedContainerBones.Contains(p.gameObject))
                    {
                        throw new System.Exception("Unable to generate mappings with root being tagged as container bone");
                    }
                }

                var originalPaths = pair.Value;

                // regenerate relative path
                foreach (var originalPath in originalPaths)
                {
                    mappings[originalPath] = AnimationUtils.GetRelativePath(go.transform, _avatarRoot.transform);
                }
            }

            if (avoidContainerBones)
            {
                _invalidNonContainerBoneMappingsCache = false;
            }
            else
            {
                _invalidMappingsCache = false;
            }
            return mappings;
        }
    }
}
