namespace ProtogameUIStylingTest
{
    using System.Collections.Generic;

    using Microsoft.Xna.Framework;

    using Protoinject;

    using Protogame;

    public class ProtogameUIStylingTestWorld : IWorld, IHasCanvases
    {
        private readonly NuiRenderer _nuiRenderer;
        private Canvas _canvas;
        private ISkinLayout _skinLayout;
        private ISkinDelegator _skinDelegator;

        public ProtogameUIStylingTestWorld(
            INode worldNode,
            IHierarchy hierarchy,
            ISkinLayout skinLayout,
            ISkinDelegator skinDelegator)
        {
            _skinLayout = skinLayout;
            _skinDelegator = skinDelegator;

            SetupCanvas();

            var entity = new CanvasEntity(_skinLayout, _skinDelegator);
            entity.Canvas = _canvas;
            hierarchy.AddChildNode(worldNode, hierarchy.CreateNodeForObject(entity));
        }

        private void SetupCanvas()
        {
            _canvas = new Canvas();

            var relativeContainer = new RelativeContainer();
            relativeContainer.AddChild(new Button {Text = "Add Skybox"}, new Rectangle(20, 20, 120, 18));
            relativeContainer.AddChild(new Button { Text = "Set Component" }, new Rectangle(20, 40, 120, 18));

            var checkBoxContainer = new HorizontalContainer();
            var checkBox = new CheckBox();
            checkBoxContainer.AddChild(checkBox, "16");
            checkBoxContainer.AddChild(new Label { Text = "My Option 1", AttachTo = checkBox }, "*");
            relativeContainer.AddChild(checkBoxContainer, new Rectangle(20, 60, 120, 16));

            _canvas.SetChild(relativeContainer);
        }

        public void Dispose()
        {
        }

        public void RenderAbove(IGameContext gameContext, IRenderContext renderContext)
        {
        }

        public void RenderBelow(IGameContext gameContext, IRenderContext renderContext)
        {
            if (renderContext.IsFirstRenderPass())
            {
                gameContext.Graphics.GraphicsDevice.Clear(new Color(162, 162, 162, 255));
            }
        }

        public void Update(IGameContext gameContext, IUpdateContext updateContext)
        {
        }

        public IEnumerable<KeyValuePair<Canvas, Rectangle>> Canvases { get; }
        public bool CanvasesEnabled { get; }
    }
}
