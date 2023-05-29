using UnityEngine;

namespace Chocopoi.DressingTools.Dresser.Default
{
    public enum DTDefaultDresserDynamicsOption
    {
        RemoveDynamicsAndUseParentConstraint = 0,
        KeepDynamicsAndUseParentConstraintIfNecessary = 1,
        IgnoreTransform = 2,
        CopyDynamics = 3,
        IgnoreAll = 4,
    }

    public class DTDefaultDresserSettings : DTDresserSettings
    {
        public bool removeExistingPrefixSuffix;

        public DTDefaultDresserDynamicsOption dynamicsOption;
    }
}
