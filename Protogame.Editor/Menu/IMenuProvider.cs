using System.Collections.Generic;

namespace Protogame.Editor.Menu
{
    public interface IMenuProvider
    {
        IEnumerable<MenuEntry> GetMenuItems();
    }
}
