/*
 * File: DTBaseComponent.cs
 * Project: DressingTools
 * Created Date: Saturday, July 22nd 2023, 1:00:41 am
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

using UnityEngine;

namespace Chocopoi.DressingTools
{
    [DefaultExecutionOrder(-9999)]
#if VRC_SDK_VRCSDK3
    // Add IEditorOnly to suppress the VRCSDK builder error
    public abstract class DTBaseComponent : MonoBehaviour, VRC.SDKBase.IEditorOnly
#else
    public class DTBaseComponent : MonoBehaviour
#endif
    {
    }
}
