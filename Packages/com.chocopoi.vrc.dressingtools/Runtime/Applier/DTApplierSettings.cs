using Newtonsoft.Json;
using UnityEngine;

namespace Chocopoi.DressingTools.Applier
{
    public class DTApplierSettings
    {
        public bool removeExistingPrefixSuffix;

        public bool groupBones;

        public bool groupRootObjects;

        public bool groupDynamics;

        public virtual bool DrawEditorGUI()
        {
            return false;
        }
    }
}
