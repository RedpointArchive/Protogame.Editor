using Protogame;
using System;

namespace Protogame.Editor.Api.Version1.Menu
{
    [Serializable]
    public class MenuEntry
    {
        public MenuEntry(string path, bool enabled, int order, MenuClickHandler handler, object userdata)
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

        public MenuClickHandler Handler { get; set; }

        public object Userdata { get; set; }

        public MenuDynamicEnabledHandler DynamicEnabledHandler { get; set; }

        public MenuDynamicTextHandler DynamicTextHandler { get; set; }
    }

    public delegate void MenuClickHandler(MenuEntry menuEntry);

    public delegate bool MenuDynamicEnabledHandler(MenuEntry menuEntry);

    public delegate string MenuDynamicTextHandler(MenuEntry menuEntry);
}
