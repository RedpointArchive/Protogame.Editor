using System;
using Jitter.Dynamics;
using Microsoft.Xna.Framework;
using Protogame.Editor.Api.Game.Version1;
using Protogame.Editor.Api.Version1;
using Protogame.Editor.Ext.Inspector.Game;

[assembly: Extension(typeof(InspectorGameHostExtension))]

namespace Protogame.Editor.Ext.Inspector.Game
{    
    public class InspectorGameHostExtension : IGameHostExtension
    {
        private readonly IPhysicsEngine _physicsEngine;
        private readonly IDebugRenderer _debugRenderer;
        private readonly InspectorRenderPass _inspectorRenderPass;
        private readonly IConsoleHandle _consoleHandle;
        private WeakReference<RigidBody> _rigidBodyRef;
        private Ray _castingRay;

        public InspectorGameHostExtension(
            IPhysicsEngine physicsEngine,
            IDebugRenderer debugRenderer,
            InspectorRenderPass inspectorRenderPass,
            IConsoleHandle consoleHandle)
        {
            _physicsEngine = physicsEngine;
            _debugRenderer = debugRenderer;
            _inspectorRenderPass = inspectorRenderPass;
            _consoleHandle = consoleHandle;
            _rigidBodyRef = new WeakReference<RigidBody>(null);
        }

        public void Update(IGameContext gameContext, IUpdateContext updateContext)
        {
        }

        public void Render(IGameContext gameContext, IRenderContext renderContext)
        {
            if (renderContext.IsFirstRenderPass())
            {
                renderContext.AppendTransientRenderPass(_inspectorRenderPass);
            }
            
            if (renderContext.IsCurrentRenderPass<I3DRenderPass>())
            {
                _castingRay = gameContext.MouseRay;
                _inspectorRenderPass.CaptureMatricesFromCurrentRenderPass(renderContext);
            }

            if (renderContext.IsCurrentRenderPass<InspectorRenderPass>())
            {
                RigidBody rigidBody = null;
                object owner = null;
                Vector3 normal = default(Vector3);
                float distance = 0;
                if (_physicsEngine.Raycast(_castingRay, EliminateObjectsTooClose, ref rigidBody, ref owner, ref normal, ref distance))
                {
                    _rigidBodyRef.SetTarget(rigidBody);
                }
                else
                {
                    _rigidBodyRef.SetTarget(null);
                }

                if (_rigidBodyRef.TryGetTarget(out rigidBody) && rigidBody != null)
                {
                    rigidBody.EnableDebugDraw = true;
                    rigidBody.DebugDraw(new InspectorDebugDraw(renderContext, _debugRenderer));
                }
            }
        }

        private bool EliminateObjectsTooClose(RigidBody body, object owner, Vector3 normal, Ray originalRay, float distance)
        {
            // Only accept objects that are at least 2 units away.
            return distance > 2;
        }
    }
}
