using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools
{
    [Serializable]
    public class Preferences
    {
        public class App
        {
            public int selectedLanguage;
            public string updateBranch;
        }

        public int version;
        public App app;
    }
}
