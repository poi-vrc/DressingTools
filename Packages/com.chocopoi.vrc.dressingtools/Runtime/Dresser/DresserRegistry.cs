using System.Collections.Generic;
using System.Linq;

namespace Chocopoi.DressingTools.Dresser
{
    public class DresserRegistry
    {
        private static readonly List<IDTDresser> dressers = new List<IDTDresser>()
        {
            new DTDefaultDresser()
        };

        public static IDTDresser GetDresserByTypeName(string name)
        {
            foreach (var dresser in dressers)
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
            string[] dresserKeys = new string[dressers.Count];
            for (var i = 0; i < dressers.Count; i++)
            {
                dresserKeys[i] = dressers[i].FriendlyName;
            }
            return dresserKeys;
        }

        public static IDTDresser GetDresserByIndex(int index) => dressers[index];

        public static int GetDresserKeyIndexByTypeName(string name)
        {
            for (var i = 0; i < dressers.Count; i++)
            {
                var type = dressers[i].GetType();
                if (name == type.FullName || name == type.Name)
                {
                    return i;
                }
            }
            return -1;
        }

        public static IDTDresser GetDresserByName(string name)
        {
            return dressers.FirstOrDefault(d => d.FriendlyName == name);
        }
    }
}
