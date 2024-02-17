/*
 * Copyright (c) 2023 chocopoi
 * 
 * This file is part of DressingTools.
 * 
 * DressingTools is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 * 
 * DressingTools is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with DressingFramework. If not, see <https://www.gnu.org/licenses/>.
 */

using Chocopoi.DressingFramework;
using Chocopoi.DressingFramework.Extensibility.Sequencing;

namespace Chocopoi.DressingTools.OneConf
{
    internal abstract class WearablePass : BuildPass
    {
        /// <summary>
        /// Invoke this hook
        /// </summary>
        /// <param name="cabCtx">Apply cabinet context</param>
        /// <param name="wearCtx">Apply wearable context</param>
        /// <param name="isPreview">Whether this is a preview apply</param>
        /// <returns>Return false to stop continuing execution</returns>
        public abstract bool Invoke(CabinetContext cabCtx, WearableContext wearCtx);

        public override bool Invoke(Context ctx)
        {
            var cabCtx = ctx.Extra<CabinetContext>();
            foreach (var wearCtx in cabCtx.wearableContexts.Values)
            {
                if (!Invoke(cabCtx, wearCtx))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
