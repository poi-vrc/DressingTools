using UnityEditor;

namespace Chocopoi.DressingTools.Dresser.Default
{
    public enum DTDefaultDresserDynamicsOption
    {
        RemoveDynamicsAndUseParentConstraint = 0,
        KeepDynamicsAndUseParentConstraintIfNecessary = 1,
        IgnoreTransform = 2,
        CopyDynamics = 3,
        IgnoreAll = 4,
    }

    public class DTDefaultDresserSettings : DTDresserSettings
    {
        public DTDefaultDresserDynamicsOption dynamicsOption;

        public DTDefaultDresserSettings()
        {
            // default settings
            dynamicsOption = DTDefaultDresserDynamicsOption.RemoveDynamicsAndUseParentConstraint;
        }

        private DTDefaultDresserDynamicsOption ConvertIntToDynamicsOption(int dynamicsOption)
        {
            switch (dynamicsOption)
            {
                case 1:
                    return DTDefaultDresserDynamicsOption.KeepDynamicsAndUseParentConstraintIfNecessary;
                case 2:
                    return DTDefaultDresserDynamicsOption.IgnoreTransform;
                case 3:
                    return DTDefaultDresserDynamicsOption.CopyDynamics;
                case 4:
                    return DTDefaultDresserDynamicsOption.IgnoreAll;
                default:
                case 0:
                    return DTDefaultDresserDynamicsOption.RemoveDynamicsAndUseParentConstraint;
            }
        }

        public override bool DrawEditorGUI()
        {
            var modified = base.DrawEditorGUI();

            // Dynamics Option
            var newDynamicsOption = ConvertIntToDynamicsOption(EditorGUILayout.Popup("Dynamics Option", (int)dynamicsOption, new string[] {
                        "Remove wearable dynamics and ParentConstraint",
                        "Keep wearable dynamics and ParentConstraint if needed",
                        "Remove wearable dynamics and IgnoreTransform",
                        "Copy avatar dynamics data to wearable",
                        "Ignore all dynamics"
                    }));

            modified |= dynamicsOption != newDynamicsOption;
            dynamicsOption = newDynamicsOption;

            return modified;
        }
    }
}
