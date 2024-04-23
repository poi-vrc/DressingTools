/*
 * Copyright (c) 2024 chocopoi
 * 
 * This file is part of DressingTools.
 * 
 * DressingTools is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 * 
 * DressingTools is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with DressingTools. If not, see <https://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;
using Chocopoi.DressingFramework;
using Chocopoi.DressingFramework.Extensibility.Sequencing;
using Chocopoi.DressingFramework.Localization;
using Chocopoi.DressingTools.Components;
using Chocopoi.DressingTools.Components.Modifiers;
using Chocopoi.DressingTools.Dresser;
using Chocopoi.DressingTools.Dresser.Standard;
using Chocopoi.DressingTools.Dresser.Tags;
using Chocopoi.DressingTools.Localization;
using UnityEngine;
using StdDresserAPIDynamicsOption = Chocopoi.DressingTools.Dresser.Standard.StandardDresserSettings.DynamicsOptions;
using StdDresserComponentDynamicsOption = Chocopoi.DressingTools.Components.Modifiers.DTArmatureMapping.AMDresserStandardConfig.DynamicsOptions;

namespace Chocopoi.DressingTools.Passes.Modifiers
{
    [ComponentPassFor(typeof(DTArmatureMapping))]
    internal class ArmatureMappingPass : ComponentPass
    {
        private const string LogLabel = "ArmatureMappingPass";
        private static readonly I18nTranslator t = I18n.ToolTranslator;

        public override BuildConstraint Constraint =>
            InvokeAtStage(BuildStage.Transpose)
                .BeforePass<ObjectMappingPass>()
                .BeforePass<IgnoreDynamicsPass>()
                .BeforePass<CopyDynamicsPass>()
                .Build();

        private static StdDresserAPIDynamicsOption StdDresserComponentToAPIDynamicsOption(StdDresserComponentDynamicsOption dynamicsOptions)
        {
            switch (dynamicsOptions)
            {
                case StdDresserComponentDynamicsOption.RemoveDynamicsAndUseParentConstraint:
                    return StdDresserAPIDynamicsOption.RemoveDynamicsAndUseParentConstraint;
                case StdDresserComponentDynamicsOption.KeepDynamicsAndUseParentConstraintIfNecessary:
                    return StdDresserAPIDynamicsOption.KeepDynamicsAndUseParentConstraintIfNecessary;
                case StdDresserComponentDynamicsOption.IgnoreTransform:
                    return StdDresserAPIDynamicsOption.IgnoreTransform;
                case StdDresserComponentDynamicsOption.CopyDynamics:
                    return StdDresserAPIDynamicsOption.CopyDynamics;
                case StdDresserComponentDynamicsOption.IgnoreAll:
                    return StdDresserAPIDynamicsOption.IgnoreAll;
                default:
                case StdDresserComponentDynamicsOption.Auto:
                    return StdDresserAPIDynamicsOption.Auto;
            }
        }

        private static List<DTObjectMapping.Mapping> ConvertToComponentObjectMapping(List<ObjectMapping> dresserObjectMappings)
        {
            var output = new List<DTObjectMapping.Mapping>();
            foreach (var dresserObjectMapping in dresserObjectMappings)
            {
                DTObjectMapping.Mapping.MappingType type;
                if (dresserObjectMapping.Type == ObjectMapping.MappingType.MoveToBone)
                {
                    type = DTObjectMapping.Mapping.MappingType.MoveToBone;
                }
                else if (dresserObjectMapping.Type == ObjectMapping.MappingType.ParentConstraint)
                {
                    type = DTObjectMapping.Mapping.MappingType.ParentConstraint;
                }
                else if (dresserObjectMapping.Type == ObjectMapping.MappingType.IgnoreTransform)
                {
                    type = DTObjectMapping.Mapping.MappingType.IgnoreTransform;
                }
                else
                {
                    Debug.LogError("[DressingTools] Unknown dresser mapping type detected: " + dresserObjectMapping.Type);
                    continue;
                }

                output.Add(new DTObjectMapping.Mapping()
                {
                    Type = type,
                    SourceTransform = dresserObjectMapping.SourceTransform,
                    TargetPath = dresserObjectMapping.TargetPath
                });
            }
            return output;
        }

        private static List<DTArmatureMapping.Tag> ConvertToComponentTags(List<ITag> dresserTags)
        {
            var output = new List<DTArmatureMapping.Tag>();
            foreach (var dresserTag in dresserTags)
            {
                var tag = new DTArmatureMapping.Tag()
                {
                    SourceTransform = dresserTag.SourceTransform
                };

                if (dresserTag is CopyDynamicsTag copyDynTag)
                {
                    tag.Type = DTArmatureMapping.Tag.TagType.CopyDynamics;
                    tag.TargetPath = copyDynTag.TargetPath;
                }

                output.Add(tag);
            }
            return output;
        }

        private static bool GenerateMappings(Context ctx, DTArmatureMapping armMapComp, out List<DTObjectMapping.Mapping> objectMappings, out List<DTArmatureMapping.Tag> tags)
        {
            objectMappings = null;
            tags = null;

            IDresser dresser;
            IDresserSettings settings;
            if (armMapComp.DresserType == DTArmatureMapping.DresserTypes.Standard)
            {
                dresser = new StandardDresser();
                var ds = new StandardDresserSettings()
                {
                    SourceArmature = armMapComp.SourceArmature,
                    TargetArmaturePath = armMapComp.TargetArmaturePath,
                    DynamicsOption = StdDresserComponentToAPIDynamicsOption(armMapComp.DresserStandardConfig.DynamicsOption)
                };
                settings = ds;
            }
            else
            {
                ctx.Report.LogError(LogLabel, $"Unknown dresser type {armMapComp.DresserType}, ignoring: {armMapComp.name}");
                return false;
            }

            dresser.Execute(ctx.Report, ctx.AvatarGameObject, settings, out var dresserObjectMappings, out var dresserTags);

            objectMappings = ConvertToComponentObjectMapping(dresserObjectMappings);
            tags = ConvertToComponentTags(dresserTags);
            return true;
        }

        private static void ApplyMappingsAndTags(DTArmatureMapping armMapComp, List<DTObjectMapping.Mapping> objectMappings, List<DTArmatureMapping.Tag> tags, List<DTBaseComponent> generatedComponents)
        {
            var objMapComp = armMapComp.gameObject.AddComponent<DTObjectMapping>();
            objMapComp.Mappings = objectMappings;
            objMapComp.GroupObjects = armMapComp.GroupBones;
            objMapComp.Prefix = armMapComp.Prefix;
            objMapComp.Suffix = armMapComp.Suffix;
            objMapComp.PreventDuplicateNames = armMapComp.PreventDuplicateNames;
            generatedComponents.Add(objMapComp);

            foreach (var tag in tags)
            {
                if (tag.SourceTransform == null)
                {
                    continue;
                }

                if (tag.Type == DTArmatureMapping.Tag.TagType.CopyDynamics)
                {
                    var comp = tag.SourceTransform.gameObject.AddComponent<DTCopyDynamics>();
                    comp.SourceSearchMode = DTCopyDynamics.DynamicsSearchMode.ControlRoot;
                    comp.SourcePath = tag.TargetPath;
                    generatedComponents.Add(comp);
                }
            }
        }

        public override bool Invoke(Context ctx, DTBaseComponent component, out List<DTBaseComponent> generatedComponents)
        {
            generatedComponents = new List<DTBaseComponent>();
            var armMapComp = (DTArmatureMapping)component;

            var targetArmature = ctx.AvatarGameObject.transform.Find(armMapComp.TargetArmaturePath);
            if (targetArmature == null)
            {
                ctx.Report.LogWarn(LogLabel, $"No target armature specified, ignoring: {component.name}");
                return true;
            }

            List<DTObjectMapping.Mapping> objectMappings;
            List<DTArmatureMapping.Tag> tags;
            if (armMapComp.Mode == DTArmatureMapping.MappingMode.Auto ||
                armMapComp.Mode == DTArmatureMapping.MappingMode.Override)
            {
                if (!GenerateMappings(ctx, armMapComp, out objectMappings, out tags))
                {
                    return false;
                }

                if (armMapComp.Mode == DTArmatureMapping.MappingMode.Override)
                {
                    DresserUtils.HandleObjectMappingOverrides(objectMappings, armMapComp.Mappings);
                    DresserUtils.HandleTagsOverrides(tags, armMapComp.Tags);
                }
            }
            else
            {
                objectMappings = armMapComp.Mappings;
                tags = armMapComp.Tags;
            }

            ApplyMappingsAndTags(armMapComp, objectMappings, tags, generatedComponents);
            return true;
        }
    }
}
