/*
 * File: ApplyContext.cs
 * Project: DressingTools
 * Created Date: Saturday, Aug 22th 2023, 11:25:01 am
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
using System.Linq;
using Chocopoi.DressingTools.Lib.Animations;
using Chocopoi.DressingTools.Lib.Cabinet;
using Chocopoi.DressingTools.Lib.Logging;
using Chocopoi.DressingTools.Lib.Proxy;
using Chocopoi.DressingTools.Lib.Wearable;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.Lib
{
    public class ApplyCabinetContext
    {
        public const string GeneratedAssetsFolderName = "_DTGeneratedAssets";
        public const string GeneratedAssetsPath = "Assets/" + GeneratedAssetsFolderName;

        private const string AssetNamePrefix = "cpDT_";

        public DTReport report;
        public CabinetConfig cabinetConfig;
        public GameObject avatarGameObject;
        public List<IDynamicsProxy> avatarDynamics;
        public Dictionary<DTWearable, ApplyWearableContext> wearableContexts;
        public IPathRemapper pathRemapper;
        public IAnimationStore animationStore;

        private readonly string _randomString;

        public ApplyCabinetContext()
        {
            _randomString = DTLibRuntimeUtils.RandomString(8);
        }

        public string MakeUniqueName(string name)
        {
            return $"{avatarGameObject.name}_{_randomString}_{name}";
        }

        public string MakeUniqueAssetPath(string name)
        {
            return $"{GeneratedAssetsPath}/{AssetNamePrefix}{MakeUniqueName(name)}";
        }

        public void CreateUniqueAsset(Object obj, string name)
        {
            AssetDatabase.CreateAsset(obj, MakeUniqueAssetPath(name));
        }
    }

    public class ApplyWearableContext
    {
        public WearableConfig wearableConfig;
        public GameObject wearableGameObject;
        public List<IDynamicsProxy> wearableDynamics;

        private readonly string _randomString;

        public ApplyWearableContext()
        {
            _randomString = DTLibRuntimeUtils.RandomString(8);
        }

        public string MakeUniqueName(string name)
        {
            return $"{wearableGameObject.name}_{_randomString}_{name}";
        }

        public string MakeUniqueAssetPath(ApplyCabinetContext cabCtx, string name)
        {
            return cabCtx.MakeUniqueAssetPath(MakeUniqueName(name));
        }

        public void CreateUniqueAsset(ApplyCabinetContext cabCtx, Object obj, string name)
        {
            cabCtx.CreateUniqueAsset(obj, MakeUniqueName(name));
        }
    }
}
