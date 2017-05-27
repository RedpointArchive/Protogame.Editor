using System.Collections.Generic;

namespace ProtogameUIStylingTest
{
    public interface IMenuProvider
    {
        IEnumerable<MenuEntry> GetMenuItems();
    }
}
