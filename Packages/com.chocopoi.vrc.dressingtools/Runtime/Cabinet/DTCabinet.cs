using System.Collections.Generic;
using Chocopoi.DressingTools.Applier;
using Chocopoi.DressingTools.Applier.Default;
using Chocopoi.DressingTools.Logging;
using Newtonsoft.Json;
using UnityEngine;

namespace Chocopoi.DressingTools.Cabinet
{
    public enum DTCabinetApplierMode
    {
        LateApply = 0,
        ApplyImmediately = 1
    }

    [AddComponentMenu("DressingTools/DT Cabinet")]
    public class DTCabinet : DTBaseComponent
    {
        private static readonly Dictionary<string, IDTApplier> appliers = new Dictionary<string, IDTApplier>
        {
            { "Default", new DTDefaultApplier() }
        };

        public GameObject avatarGameObject;

        public string avatarArmatureName;

        public string applierName;

        public string serializedApplierSettings;

        public static string[] GetApplierKeys()
        {
            string[] applierKeys = new string[appliers.Count];
            appliers.Keys.CopyTo(applierKeys, 0);
            return applierKeys;
        }

        public static IDTApplier GetApplierByKey(string key)
        {
            if (appliers.TryGetValue(key, out var applier))
            {
                return applier;
            }
            return null;
        }

        public static IDTApplier GetApplierByTypeName(string typeName)
        {
            foreach (var applier in appliers.Values)
            {
                var type = applier.GetType();
                if (type.Name == typeName || type.FullName == typeName)
                {
                    return applier;
                }
            }
            return null;
        }

        public static string GetApplierKeyByTypeName(string typeName)
        {
            foreach (var applier in appliers)
            {
                var type = applier.Value.GetType();
                if (type.FullName == typeName || type.Name == typeName)
                {
                    return applier.Key;
                }
            }
            return null;
        }

        public DTCabinetWearable[] GetWearables()
        {
            return avatarGameObject.GetComponentsInChildren<DTCabinetWearable>();
        }

        public DTReport Apply()
        {
            var applier = GetApplierByTypeName(applierName);
            var settings = applier.DeserializeSettings(serializedApplierSettings);
            return applier.ApplyCabinet(settings, this);
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
