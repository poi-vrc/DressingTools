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

using System.Collections.Generic;
using UnityEngine;

namespace Chocopoi.DressingTools.Dynamics
{
    /// <summary>
    /// Dynamics proxy interface
    /// </summary>
    internal interface IDynamics
    {
        /// <summary>
        /// The dynamics component
        /// </summary>
        Component Component { get; set; }

        /// <summary>
        /// The transform that the component is located
        /// </summary>
        Transform Transform { get; }

        /// <summary>
        /// The GameObject that the component is located
        /// </summary>
        GameObject GameObject { get; }

        /// <summary>
        /// A list of root transforms that the dynamics is controlling
        /// </summary>
        ICollection<Transform> RootTransforms { get; set; }

        /// <summary>
        /// Dynamics ignore transforms
        /// </summary>
        ICollection<Transform> IgnoreTransforms { get; set; }
    }
}
