using System;

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
