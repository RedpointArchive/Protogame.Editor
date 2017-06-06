using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Protogame.Editor.Ext.Inspector.Game
{
    public class InspectorRenderPass : IDebugRenderPass
    {
        private RasterizerState _oldRasterizerState;

        private RasterizerState _debugRasterizerState;

        private DepthStencilState _oldDepthState;

        private DepthStencilState _debugDepthState;

        private IAssetReference<UberEffectAsset> _basicEffect;
        private List<IDebugLayer> _enabledLayers;

        public InspectorRenderPass(IAssetManager assetManager)
        {
            _basicEffect = assetManager.Get<UberEffectAsset>("effect.BuiltinSurface");
            Lines = new List<VertexPositionNormalColor>();
            Triangles = new List<VertexPositionNormalColor>();
            _enabledLayers = new List<IDebugLayer> { new InspectorDebugLayer() };
        }

        public bool IsPostProcessingPass => false;
        public bool SkipWorldRenderBelow => true;
        public bool SkipWorldRenderAbove => true;
        public bool SkipEntityRender => true;
        public bool SkipEngineHookRender => false;
        public string EffectTechniqueName => RenderPipelineTechniqueName.Forward;

        public List<VertexPositionNormalColor> Lines { get; }

        public List<VertexPositionNormalColor> Triangles { get; }

        public Matrix View { get; set; }
        public Matrix Projection { get; set; }

        public void CaptureMatricesFromCurrentRenderPass(IRenderContext renderContext)
        {
            View = renderContext.View;
            Projection = renderContext.Projection;
        }

        public void BeginRenderPass(IGameContext gameContext, IRenderContext renderContext, IRenderPass previousPass, RenderTarget2D postProcessingSource)
        {
            if (!_basicEffect.IsReady)
            {
                return;
            }
            
            _oldRasterizerState = renderContext.GraphicsDevice.RasterizerState;
            _oldDepthState = renderContext.GraphicsDevice.DepthStencilState;

            if (_debugRasterizerState == null)
            {
                _debugRasterizerState = new RasterizerState()
                {
                    CullMode = CullMode.None,
                    FillMode = FillMode.WireFrame,
                    DepthBias = _oldRasterizerState.DepthBias,
                    DepthClipEnable = true,
                    MultiSampleAntiAlias = false,
                    Name = "InspectorRasterizerState",
                    ScissorTestEnable = false,
                    SlopeScaleDepthBias = _oldRasterizerState.SlopeScaleDepthBias
                };
            }

            if (_debugDepthState == null)
            {
                _debugDepthState = new DepthStencilState()
                {
                    DepthBufferEnable = false,
                    DepthBufferWriteEnable = false
                };
            }

            renderContext.GraphicsDevice.RasterizerState = _debugRasterizerState;
            renderContext.GraphicsDevice.DepthStencilState = _debugDepthState;

            Lines.Clear();
            Triangles.Clear();
        }

        public void EndRenderPass(IGameContext gameContext, IRenderContext renderContext, IRenderPass nextPass)
        {
            if (!_basicEffect.IsReady)
            {
                return;
            }
            
            var world = renderContext.World;
            var view = renderContext.View;
            var projection = renderContext.Projection;
            renderContext.View = View;
            renderContext.Projection = Projection;
            renderContext.World = Matrix.Identity;

            _basicEffect.Asset.Effects["Color"].NativeEffect.Parameters["World"].SetValue(renderContext.World);
            _basicEffect.Asset.Effects["Color"].NativeEffect.Parameters["View"].SetValue(renderContext.View);
            _basicEffect.Asset.Effects["Color"].NativeEffect.Parameters["Projection"].SetValue(renderContext.Projection);

            foreach (var pass in _basicEffect.Asset.Effects["Color"].NativeEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                if (Lines.Count > 0)
                {
                    // Batch render all lines.
                    var lines = Lines.ToArray();
                    for (var i = 0; i < lines.Length; i += 1000)
                    {
                        renderContext.GraphicsDevice.DrawUserPrimitives(
                            PrimitiveType.LineList,
                            lines,
                            i,
                            lines.Length - i > 1000 ? 500 : (lines.Length - i) / 2);
                    }
                }

                if (Triangles.Count > 0)
                {
                    // Batch render all triangles.
                    var triangles = Triangles.ToArray();
                    for (var i = 0; i < triangles.Length; i += 1500)
                    {
                        renderContext.GraphicsDevice.DrawUserPrimitives(
                            PrimitiveType.TriangleList,
                            triangles,
                            i,
                            triangles.Length - i > 1500 ? 500 : (triangles.Length - i) / 3);
                    }
                }
            }

            renderContext.World = world;
            renderContext.View = view;
            renderContext.Projection = projection;

            renderContext.GraphicsDevice.RasterizerState = _oldRasterizerState;
            renderContext.GraphicsDevice.DepthStencilState = _oldDepthState;
        }

        public string Name { get; set; }

        public List<IDebugLayer> EnabledLayers => _enabledLayers;
    }
}