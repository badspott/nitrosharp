﻿using System.Collections.Generic;
using System.Linq;
using System;
using System.Numerics;
using Veldrid;
using NitroSharp.Primitives;
using NitroSharp.Graphics.Objects;
using NitroSharp.Utilities;
using NitroSharp.Text;

namespace NitroSharp.Graphics
{
    internal sealed class RenderSystem : EntityProcessingSystem, IDisposable
    {
        private readonly Configuration _config;
        private readonly CommandList _cl;
        private readonly GraphicsDevice _gd;
        private readonly Canvas _canvas;
        private readonly EffectLibrary _effectLibrary;
        private readonly RenderContext _rc;

        private readonly SharedEffectProperties2D _sharedProps2D;
        private readonly SharedEffectProperties3D _sharedProps3D;
        private Cube _cube;

        public RenderSystem(GraphicsDevice graphicsDevice, FontService fontService, Configuration configuration)
        {
            _gd = graphicsDevice;
            _config = configuration;

            ResourceFactory factory = _gd.ResourceFactory;
            _cl = factory.CreateCommandList();
            _effectLibrary = new EffectLibrary(_gd);

            _sharedProps2D = new SharedEffectProperties2D(_gd);
            _sharedProps2D.Projection = Matrix4x4.CreateOrthographicOffCenter(
                0, DesignResolution.Width, DesignResolution.Height, 0, 0, -1);

            _sharedProps3D = new SharedEffectProperties3D(_gd);
            _sharedProps3D.View = Matrix4x4.CreateLookAt(Vector3.Zero, Vector3.UnitZ, Vector3.UnitY);

            _sharedProps3D.Projection = Matrix4x4.CreatePerspectiveFieldOfView(
                MathUtil.PI / 3.0f,
                DesignResolution.Width / DesignResolution.Height,
                0.1f,
                1000.0f);

            _canvas = new Canvas(graphicsDevice, _effectLibrary, _sharedProps2D);
            _rc = new RenderContext(_gd, factory, _cl, _canvas, _effectLibrary, _sharedProps2D, _sharedProps3D, fontService);
        }

        private SizeF DesignResolution => new SizeF(_config.WindowWidth, _config.WindowHeight);

        protected override void DeclareInterests(ISet<Type> interests)
        {
            interests.Add(typeof(Visual));
        }

        public override void OnRelevantEntityAdded(Entity entity)
        {
            entity.Visual.CreateDeviceObjects(_rc);
        }

        public override void OnRelevantEntityRemoved(Entity entity)
        {
            entity.Visual.Destroy(_rc);
        }

        public override void Update(float deltaMilliseconds)
        {
            _cl.Begin();

            _cl.SetFramebuffer(_gd.SwapchainFramebuffer);
            _cl.SetFullViewports();
            _cl.ClearColorTarget(0, RgbaFloat.Black);

            _cube?.Render(_rc);

            _canvas.Begin(_cl);
            base.Update(deltaMilliseconds);
            _canvas.End();

            _cl.End();

            _gd.SubmitCommands(_cl);
        }

        public void Present()
        {
            _gd.SwapBuffers();
        }

        public override IEnumerable<Entity> SortEntities(IEnumerable<Entity> entities)
        {
            return entities.OrderBy(x => x.GetComponent<Visual>().Priority).ThenBy(x => x.CreationTime);
        }

        public override void Process(Entity entity, float deltaMilliseconds)
        {
            var visual = entity.GetComponent<Visual>();
            if (visual.IsEnabled)
            {
                RenderItem(visual, Vector2.One);
            }
        }

        private void RenderItem(Visual visual, Vector2 scale)
        {
            if (visual is Cube cube)
            {
                _cube = cube;
                return;
            }

            var transform = visual.Entity.Transform.GetTransformMatrix();
            _canvas.SetTransform(transform);
            visual.Render(_rc);
        }

        public void Dispose()
        {
            _canvas.Dispose();
            _effectLibrary.Dispose();
            _sharedProps2D.Dispose();
            _sharedProps3D.Dispose();
            _cl.Dispose();
        }
    }
}
