/*
 * Copyright (c) 2024 chocopoi
 * 
 * This file is part of DressingTools.
 * 
 * DressingTools is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 * 
 * DressingTools is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with DressingTools. If not, see <https://www.gnu.org/licenses/>.
 */

using Chocopoi.DressingFramework;
using Chocopoi.DressingFramework.Animations;
using Chocopoi.DressingFramework.Detail.DK.Logging;
using Chocopoi.DressingFramework.Extensibility.Sequencing;
using Chocopoi.DressingFramework.Logging;
using UnityEngine;

namespace Chocopoi.DressingTools.Passes
{
    /// <summary>
    /// DressingTools' internal component destructive apply context
    /// </summary>
    internal class ComponentApplyContext : Context
    {
        public override BuildRuntime CurrentRuntime => BuildRuntime.DK;
        public override object RuntimeContext => null;
        // TODO: asset container
        public override Object AssetContainer => throw new System.NotImplementedException();
        internal override Report Report => _report;

        private readonly DKReport _report;

        public ComponentApplyContext(GameObject avatarGameObject) : base(avatarGameObject)
        {
            _report = new DKReport();
            // TODO: for now, just add a dummy store here with no clips
            AddContextFeature(new AnimationStore(this));
        }

        public override void CreateAsset(Object obj, string name)
        {
            throw new System.NotImplementedException();
        }
    }
}
