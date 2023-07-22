using UnityEngine;

namespace Chocopoi.DressingTools.Integrations.VRC
{
    internal interface IBuildDTCabinetHook
    {
        bool OnPreprocessAvatar(GameObject avatarGameObject);
    }
}
