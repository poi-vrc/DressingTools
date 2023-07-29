#if VRC_SDK_VRCSDK3
using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Logging;
using UnityEditor;
using UnityEngine;
namespace Chocopoi.DressingTools.Integrations.VRChat
{
    internal class ApplyCabinetHook : IBuildDTCabinetHook
    {
        private DTCabinet cabinet;

        public ApplyCabinetHook(DTCabinet cabinet)
        {
            this.cabinet = cabinet;
        }

        public bool OnPreprocessAvatar(GameObject avatarGameObject)
        {
            EditorUtility.DisplayProgressBar("DressingTools", "Applying cabinet...", 0);

            var report = new DTReport();

            cabinet.Apply(report);

            // TODO: show report frame if have errors
            if (report.HasLogType(DTReportLogType.Error))
            {
                EditorUtility.DisplayDialog("DressingTools", "Error occured when applying cabinet, aborting", "OK");
                return false;
            }

            return true;
        }
    }
}
#endif
