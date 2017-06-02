namespace Protogame.Editor
{
    public class EditorHotKeyBinder : StaticEventBinder<IGameContext>
    {
        public override void Configure()
        {
            Priority = 150;

            Bind<KeyPressEvent>(x => x.Key == Microsoft.Xna.Framework.Input.Keys.OemTilde).ToListener<EditorHotKeyListener>();
        }
    }
}
