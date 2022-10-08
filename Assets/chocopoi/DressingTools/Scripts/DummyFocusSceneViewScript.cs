using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyFocusSceneViewScript : MonoBehaviour
{
    void Start()
    {
#if UNITY_EDITOR
        UnityEditor.SceneView.FocusWindowIfItsOpen(typeof(UnityEditor.SceneView));
#endif
    }

    void Update()
    {

    }
}
