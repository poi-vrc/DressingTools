#if VRC_SDK_VRCSDK3
using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Logging;
using Chocopoi.DressingTools.UI;
using UnityEditor;
using UnityEngine;
namespace Chocopoi.DressingTools.Integrations.VRChat
{
    internal class ApplyCabinetHook : IBuildDTCabinetHook
    {
        private DTReport _report;

        private DTCabinet _cabinet;

        public ApplyCabinetHook(DTReport report, DTCabinet cabinet)
        {
            _report = report;
            _cabinet = cabinet;
        }

        public bool OnPreprocessAvatar()
        {
            EditorUtility.DisplayProgressBar("DressingTools", "Applying cabinet...", 0);

            _cabinet.Apply(_report);

            return !_report.HasLogType(DTReportLogType.Error);
        }
    }
}
#endif
