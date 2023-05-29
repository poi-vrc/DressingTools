using UnityEngine;

namespace Chocopoi.DressingTools.Dresser.Default
{
    public enum DTDefaultDresserDynamicsOption
    {
        RemoveDynamicsAndParentConstraint = 0
    }

    public class DTDefaultDresserSettings: DTDresserSettings
    {
        public bool removeExistingPrefixSuffix;

        public DTDefaultDresserDynamicsOption dynamicsOption;
    }
}
