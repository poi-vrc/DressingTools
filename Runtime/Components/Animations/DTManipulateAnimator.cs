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

using UnityEditor.Animations;
using UnityEngine;

namespace Chocopoi.DressingTools.Components.Animations
{
    /// <summary>
    /// Generic manipulate animator component. It will search for the animator reference in the build and perform manipulation
    /// </summary>
    [AddComponentMenu("DressingTools/DT Manipulate Animator")]
    internal class DTManipulateAnimator : DTBaseComponent
    {
        public enum TargetTypes
        {
            AnimatorController = 0,
            Animator = 1,
            VRCAnimLayer = 101,
        }

        public enum SourceTypes
        {
            AnimatorController = 0,
            Animator = 1,
        }

        public enum PathModes
        {
            /// <summary>
            /// Relative path from animator or specified root
            /// </summary>
            Relative = 0,
            /// <summary>
            /// Absolute path from avatar root
            /// </summary>
            Absolute = 1
        }

        public enum ManipulateModes
        {
            /// <summary>
            /// Add layers and parameters from source to target
            /// </summary>
            Add = 0
        }

        public TargetTypes TargetType { get => m_TargetType; set => m_TargetType = value; }
#if DT_VRCSDK3A
        public VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType VRCTargetLayer { get => m_VRCTargetLayer; set => m_VRCTargetLayer = value; }
#endif
        public Animator TargetAnimator { get => m_TargetAnimator; set => m_TargetAnimator = value; }
        public AnimatorController TargetController { get => m_TargetController; set => m_TargetController = value; }
        public PathModes PathMode { get => m_PathMode; set => m_PathMode = value; }
        public ManipulateModes ManipulateMode { get => m_ManipulateMode; set => m_ManipulateMode = value; }
        public SourceTypes SourceType { get => m_SourceType; set => m_SourceType = value; }
        /// <summary>
        /// Relative root for relative path mode and source type animator controller.
        /// If null, this component will be used as relative root. For animator type, this is
        /// ignored and the animator root will be used.
        /// </summary>
        public Transform SourceRelativeRoot { get => m_SourceRelativeRoot; set => m_SourceRelativeRoot = value; }
        public AnimatorController SourceController { get => m_SourceController; set => m_SourceController = value; }
        public Animator SourceAnimator { get => m_SourceAnimator; set => m_SourceAnimator = value; }
        public bool RemoveSourceAnimator { get => m_RemoveSourceAnimator; set => m_RemoveSourceAnimator = value; }
        public bool MatchTargetWriteDefaults { get => m_MatchTargetWriteDefaults; set => m_MatchTargetWriteDefaults = value; }

        [SerializeField] private TargetTypes m_TargetType;
#if DT_VRCSDK3A
        [SerializeField] private VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType m_VRCTargetLayer;
#else
        [SerializeField] private int m_VRCTargetLayer;
#endif
        [SerializeField] private Animator m_TargetAnimator;
        [SerializeField] private AnimatorController m_TargetController;
        [SerializeField] private PathModes m_PathMode;
        [SerializeField] private ManipulateModes m_ManipulateMode;
        [SerializeField] private SourceTypes m_SourceType;
        [SerializeField] private Transform m_SourceRelativeRoot;
        [SerializeField] private AnimatorController m_SourceController;
        [SerializeField] private Animator m_SourceAnimator;
        [SerializeField] private bool m_RemoveSourceAnimator;
        [SerializeField] private bool m_MatchTargetWriteDefaults;

        public DTManipulateAnimator()
        {
            m_TargetType = TargetTypes.AnimatorController;
#if DT_VRCSDK3A
            m_VRCTargetLayer = VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.FX;
#endif
            m_TargetController = null;
            m_TargetAnimator = null;
            m_PathMode = PathModes.Relative;
            m_ManipulateMode = ManipulateModes.Add;
            m_SourceType = SourceTypes.Animator;
            m_SourceRelativeRoot = null;
            m_SourceController = null;
            m_SourceAnimator = null;
            m_RemoveSourceAnimator = true;
            m_MatchTargetWriteDefaults = true;
        }
    }
}
