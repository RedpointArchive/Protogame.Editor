using System;

namespace ProtogameUIStylingTest
{
    public class MenuEntry
    {
        public MenuEntry(string path, bool enabled, int order, Action<MenuEntry> handler, object userdata)
        {
            Path = path;
            Enabled = enabled;
            Order = order;
            Handler = handler;
            Userdata = userdata;
        }

        public string Path { get; set; }

        public bool Enabled { get; set; }

        public int Order { get; set; }

        public Action<MenuEntry> Handler { get; set; }

        public object Userdata { get; set; }

        public Func<MenuEntry, bool> DynamicEnabledHandler { get; set; }

        public Func<MenuEntry, string> DynamicTextHandler { get; set; }
    }
}
