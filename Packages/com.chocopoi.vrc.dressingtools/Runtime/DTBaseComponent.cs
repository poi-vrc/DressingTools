using UnityEngine;

namespace Chocopoi.DressingTools
{
    [DefaultExecutionOrder(-9999)]
#if VRC_SDK_VRCSDK3
    // Add IEditorOnly to suppress the VRCSDK builder error
    public abstract class DTBaseComponent : MonoBehaviour, VRC.SDKBase.IEditorOnly
#else
    public class DTBaseComponent : MonoBehaviour
#endif
    {
    }
}
