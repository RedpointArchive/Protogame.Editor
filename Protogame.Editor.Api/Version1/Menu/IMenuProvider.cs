using System.Collections.Generic;

namespace Protogame.Editor.Api.Version1.Menu
{
    public interface IMenuProvider
    {
        IEnumerable<MenuEntry> GetMenuItems();
    }
}
