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

using System;
using System.Collections.Generic;
using Chocopoi.DressingFramework;
using Chocopoi.DressingFramework.Extensibility.Sequencing;
using Chocopoi.DressingFramework.Localization;
using Chocopoi.DressingTools.Components;
using Chocopoi.DressingTools.Components.Modifiers;
using Chocopoi.DressingTools.Localization;

namespace Chocopoi.DressingTools.Passes.Modifiers
{
    [ComponentPassFor(typeof(DTMoveRoot))]
    internal class MoveRootPass : ComponentPass
    {
        public class MessageCode
        {
            public const string AvatarPathNotFound = "passes.moveRoot.msgCode.error.destinationPathNotFound";
        }
        private const string LogLabel = "MoveRootPass";
        private static readonly I18nTranslator t = I18n.ToolTranslator;

        public override BuildConstraint Constraint =>
            InvokeAtStage(BuildStage.Transpose)
                // need to let it group first, or the search will be wrong if we move something
                .AfterPass<GroupDynamicsPass>()
                .BeforePass<GroupDynamicsModifyAnimPass>()
                .Build();

        public override bool Invoke(Context ctx, DTBaseComponent component, out List<DTBaseComponent> generatedComponents)
        {
            generatedComponents = new List<DTBaseComponent>();
            var moveRoot = (DTMoveRoot)component;
            if (string.IsNullOrEmpty(moveRoot.DestinationPath))
            {
                // move to avatar root
                component.transform.SetParent(ctx.AvatarGameObject.transform);
                return true;
            }

            // find avatar object
            var avatarObj = ctx.AvatarGameObject.transform.Find(moveRoot.DestinationPath);

            if (avatarObj == null)
            {
                ctx.Report.LogErrorLocalized(t, LogLabel, MessageCode.AvatarPathNotFound);
                return false;
            }

            // set to parent
            component.transform.SetParent(avatarObj);

            return true;
        }
    }
}
