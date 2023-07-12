using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Logging;
using Newtonsoft.Json;

namespace Chocopoi.DressingTools.Applier.Default
{
    public class DTDefaultApplier : IDTApplier
    {
        public DTReport ApplyCabinet(DTApplierSettings settings, DTCabinet cabinet)
        {
            throw new System.NotImplementedException();
        }

        public DTApplierSettings DeserializeSettings(string serializedJson)
        {
            return JsonConvert.DeserializeObject<DTDefaultApplierSettings>(serializedJson);
        }
    }
}
