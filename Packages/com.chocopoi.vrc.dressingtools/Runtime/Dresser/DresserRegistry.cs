using System.Collections.Generic;

namespace Chocopoi.DressingTools.Dresser
{
    public class DresserRegistry
    {
        private static readonly Dictionary<string, IDTDresser> dressers = new Dictionary<string, IDTDresser>()
        {
            { "Default", new DTDefaultDresser() }
        };

        public static IDTDresser GetDresserByTypeName(string name)
        {
            foreach (var dresser in dressers.Values)
            {
                var type = dresser.GetType();
                if (name == type.FullName || name == type.Name)
                {
                    return dresser;
                }
            }
            return null;
        }

        public static string[] GetAvailableDresserKeys()
        {
            string[] dresserKeys = new string[dressers.Keys.Count];
            dressers.Keys.CopyTo(dresserKeys, 0);
            return dresserKeys;
        }

        public static IDTDresser GetDresserByName(string name)
        {
            dressers.TryGetValue(name, out var dresser);
            return dresser;
        }
    }
}
