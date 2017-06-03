using System;
using Protogame;

namespace Protogame.Editor.Layout
{
    public class SingleTabbedContainer : SingleContainer, ITabbableContainer
    {
        public SingleTabbedContainer()
        {
            Visible = true;
            Enabled = true;
        }

        public string Title { get; set; }
        public IAssetReference<TextureAsset> Icon { get; set; }
        public virtual bool Visible { get; set; }
        public virtual bool Enabled { get; set; }
    }
}
