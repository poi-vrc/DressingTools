using System.Collections.Generic;
using Chocopoi.DressingTools.Cabinet;

namespace Chocopoi.DressingTools.Dresser
{
    internal class DTDefaultDresser : IDTDresser
    {
        public DTBoneMapping[] Execute(DTDresserSettings settings, out DTDresserReport report)
        {
            report = new DTDresserReport();
            var boneMappings = new List<DTBoneMapping>();

            // TODO

            return boneMappings.ToArray();
        }
    }
}
