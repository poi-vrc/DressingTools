/*
 * Copyright (c) 2023 chocopoi
 * 
 * This file is part of DressingFramework.
 * 
 * DressingFramework is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 * 
 * DressingFramework is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with DressingFramework. If not, see <https://www.gnu.org/licenses/>.
 */

using Chocopoi.DressingFramework.Context;
using Chocopoi.DressingFramework.Extensibility.Plugin;
using Chocopoi.DressingFramework.Extensibility.Sequencing;
using Chocopoi.DressingTools.Animations;

namespace Chocopoi.DressingTools.Cabinet.Hooks
{
    internal class DKContextExtensionCabinetHook : CabinetHookBase
    {
        public override string FriendlyName => "DK Cabinet Context Extension";
        public override CabinetApplyConstraint Constraint =>
            ApplyAtStage(CabinetApplyStage.Pre, CabinetHookStageRunOrder.Before)
                .Build();

        public override bool Invoke(ApplyCabinetContext cabCtx)
        {
            var dkCabCtx = cabCtx.Extra<DKCabinetContext>();
            dkCabCtx.avatarDynamics = DTEditorUtils.ScanDynamics(cabCtx.avatarGameObject, true);
            dkCabCtx.animationStore = new AnimationStore(cabCtx);
            dkCabCtx.pathRemapper = new PathRemapper(cabCtx.avatarGameObject);

            foreach (var wearCtx in cabCtx.wearableContexts.Values)
            {
                var dkWearCtx = wearCtx.Extra<DKWearableContext>();
                dkWearCtx.wearableDynamics = DTEditorUtils.ScanDynamics(wearCtx.wearableGameObject, false);
            }
            return true;
        }
    }
}
