using System;

namespace Chocopoi.DressingTools.UI.Modules
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
