using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Logging;
using UnityEngine;

namespace Chocopoi.DressingTools.Integrations.VRChat
{
    internal interface IBuildDTCabinetHook
    {
        bool OnPreprocessAvatar();
    }
}
