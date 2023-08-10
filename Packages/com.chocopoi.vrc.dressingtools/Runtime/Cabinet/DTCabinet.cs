using System;
using Chocopoi.DressingTools.Logging;
using UnityEngine;

namespace Chocopoi.DressingTools.Cabinet
{
    [AddComponentMenu("DressingTools/DT Cabinet")]
    [DefaultExecutionOrder(-19999)]
    public class DTCabinet : DTBaseComponent
    {
        private const string LogLabel = "DTCabinet";

        public GameObject avatarGameObject;

        public string avatarArmatureName;

        public DTCabinetWearable[] GetWearables()
        {
            return avatarGameObject.GetComponentsInChildren<DTCabinetWearable>();
        }

        public void Apply(DTReport report)
        {
            try
            {
                new DTCabinetApplier(report, this).Execute();
            }
            catch (Exception ex)
            {
                report.LogExceptionLocalized(LogLabel, ex, "cabinet.apply.msgCode.hasException");
            }
        }

        void Start()
        {
            // TODO: the report shouldn't put here
            Apply(new DTReport());
        }
    }
}
