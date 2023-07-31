using System;
using System.Collections.Generic;

namespace Chocopoi.DressingTools.UIBase.Views
{
    public interface IEditorView
    {
        event Action Load;
        event Action Unload;

        void OnEnable();
        void OnDisable();
        void OnGUI();
    }
}
