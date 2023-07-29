using UnityEngine;

namespace Chocopoi.DressingTools.Integrations.VRChat
{
    internal interface IBuildDTCabinetHook
    {
        bool OnPreprocessAvatar(GameObject avatarGameObject);
    }
}
