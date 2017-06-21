namespace Protogame.Editor.Api.Game.Version1
{
    public interface IGameHostExtension
    {
        void Update(IGameContext gameContext, IUpdateContext updateContext);

        void Render(IGameContext gameContext, IRenderContext renderContext);
    }
}
