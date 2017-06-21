using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protogame.Editor.Toolbar
{
    public interface IToolbarProvider
    {
        GenericToolbarEntry[] GetToolbarItems();
    }
}
