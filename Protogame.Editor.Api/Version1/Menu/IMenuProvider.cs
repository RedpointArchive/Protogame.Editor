using System.Collections.Generic;

namespace Protogame.Editor.Api.Version1.Menu
{
    public interface IMenuProvider
    {
        MenuEntry[] GetMenuItems();
    }
}
