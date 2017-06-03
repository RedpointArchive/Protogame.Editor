using Protogame;

namespace Protogame.Editor.Layout
{
    public interface IDockableContainer : IContainer
    {
        bool Visible { get; }
    }
}