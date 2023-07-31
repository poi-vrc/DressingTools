using System;

namespace Chocopoi.DressingTools.UI.Views.Modules
{
    public class CustomModuleEditor : Attribute
    {
        public Type ModuleType { get; private set; }

        public CustomModuleEditor(Type moduleType)
        {
            ModuleType = moduleType;
        }
    }
}
