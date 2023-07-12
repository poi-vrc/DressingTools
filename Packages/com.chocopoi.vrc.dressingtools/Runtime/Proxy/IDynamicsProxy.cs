using System.Collections.Generic;
using UnityEngine;

namespace Chocopoi.DressingTools.Proxy
{
    public interface IDynamicsProxy
    {
        Component Component { get; }

        Transform Transform { get; }

        GameObject GameObject { get; }

        Transform RootTransform { get; set; }

        List<Transform> IgnoreTransforms { get; set; }
    }
}
