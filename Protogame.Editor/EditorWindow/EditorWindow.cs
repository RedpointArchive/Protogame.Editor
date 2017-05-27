using Protogame.Editor.Layout;

namespace Protogame.Editor.EditorWindow
{
    public class EditorWindow : SingleTabbedContainer
    {
        public override bool Focused
        {
            get
            {
                return base.Focused;
            }
            set
            {
                base.Focused = value;

                if (value)
                {
                    OnFocus();
                }
            }
        }

        public virtual void OnFocus()
        {
        }
    }
}
