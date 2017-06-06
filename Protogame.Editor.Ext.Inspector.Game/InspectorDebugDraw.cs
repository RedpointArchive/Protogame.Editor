using Jitter;
using Jitter.LinearMath;
using Microsoft.Xna.Framework;

namespace Protogame.Editor.Ext.Inspector.Game
{
    public class InspectorDebugDraw : IDebugDrawer
    {
        private readonly IRenderContext _renderContext;
        private readonly IDebugRenderer _debugRenderer;

        public InspectorDebugDraw(IRenderContext renderContext, IDebugRenderer debugRenderer)
        {
            _renderContext = renderContext;
            _debugRenderer = debugRenderer;
        }

        public void DrawLine(JVector start, JVector end)
        {
            
        }

        public void DrawPoint(JVector pos)
        {
            
        }

        public void DrawTriangle(JVector pos1, JVector pos2, JVector pos3)
        {
            _debugRenderer.RenderDebugTriangle(
                _renderContext,
                pos1.ToXNAVector(),
                pos2.ToXNAVector(),
                pos3.ToXNAVector(),
                Color.Yellow,
                Color.Yellow,
                Color.Yellow);
        }
    }
}
