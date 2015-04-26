using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;
using MapFlags = SharpDX.Direct3D11.MapFlags;

namespace DirectEngine.Shaders
{
    public class _2DShader : Shader
    {
        private struct _2dData
        {
            Matrix world;
            Vector4 screenSize;

            public _2dData(Matrix w, Vector2 size)
            {
                world = w;
                screenSize = new Vector4(size, 0, 1);
            }
        }

        private Stack<bool> isInLinearModeStack;
        private SamplerState samplerLinear;
        private SamplerState samplerPoint;
        private Buffer databuffer;
        private Matrix world;

        private Vector2 size;
        public Vector2 ScreenSize
        {
            get { return size; }
            set
            {
                size = value;
                UpdateSize();
            }
        }

        public _2DShader(Game game)
            : base(game, "Content\\Shaders\\2DShader.fx",
            new InputElement[]
            {
                new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                new InputElement("TEXCOORD", 0, Format.R32G32_Float, Utilities.SizeOf<Vector3>(), 0),
                new InputElement("NORMAL", 0, Format.R32G32B32A32_Float, Utilities.SizeOf<Vector3>() + Utilities.SizeOf<Vector2>(), 0),
                new InputElement("TANGENT", 0, Format.R32G32B32A32_Float, Utilities.SizeOf<Vector3>() * 2 + Utilities.SizeOf<Vector2>(), 0)
            })
        {
            isInLinearModeStack = new Stack<bool>();
        }

        protected internal override void CreateSampler()
        {
            base.CreateSampler();

            samplerLinear = new SamplerState(Game.Device, new SamplerStateDescription()
            {
                Filter = Filter.MinMagMipLinear,
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                BorderColor = Color.Black,
                ComparisonFunction = Comparison.Never,
                MaximumAnisotropy = 16,
                MipLodBias = 0,
                MinimumLod = 0,
                MaximumLod = 16,
            });
            samplerPoint = new SamplerState(Game.Device, new SamplerStateDescription()
            {
                Filter = Filter.MinMagMipPoint,
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                BorderColor = Color.White,
                ComparisonFunction = Comparison.Never,
                MaximumAnisotropy = 16,
                MipLodBias = 0,
                MinimumLod = 0,
                MaximumLod = 16,
            });
        }
        protected internal override void CreateConstantBuffer()
        {
            base.CreateConstantBuffer();

            databuffer = new Buffer(Game.Device, Utilities.SizeOf<_2dData>(), ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);
        }

        protected internal override void DeleteSampler()
        {
            Utilities.Dispose(ref samplerLinear);
            Utilities.Dispose(ref samplerPoint);

            base.DeleteSampler();
        }
        protected internal override void DeleteConstantBuffer()
        {
            Utilities.Dispose(ref databuffer);

            base.DeleteConstantBuffer();
        }

        protected internal override void LinkSampler()
        {
            base.LinkSampler();

            if (isInLinearModeStack.Count > 0 && isInLinearModeStack.Peek())
                Game.Context.PixelShader.SetSampler(0, samplerLinear);
            else
                Game.Context.PixelShader.SetSampler(0, samplerPoint);
        }
        protected internal override void LinkConstantBuffer()
        {
            base.LinkConstantBuffer();

            Game.Context.VertexShader.SetConstantBuffer(0, databuffer);
        }

        private void UpdateSize()
        {
            _2dData data = new _2dData(world, size);
            DataStream map;
            Game.Context.MapSubresource(databuffer, 0, MapMode.WriteDiscard, MapFlags.None, out map);
            map.Write(data);
            Game.Context.UnmapSubresource(databuffer, 0);
        }

        public void PushIsInLinearMode(bool isLinear)
        {
            bool old = !isLinear;
            if (isInLinearModeStack.Count > 0)
                old = isInLinearModeStack.Peek();

            isInLinearModeStack.Push(isLinear);

            if (old != isLinear)
                LinkSampler();
        }
        public void PushIsInLinearMode()
        {
            if (isInLinearModeStack.Count > 0)
            {
                bool old = isInLinearModeStack.Pop();
                if (isInLinearModeStack.Count <= 0 || isInLinearModeStack.Peek() != old)
                    LinkSampler();
            }
        }

        public override void SetWorldMatrix(Matrix matrix)
        {
            base.SetWorldMatrix(matrix);

            world = matrix;
            world.Transpose();
            UpdateSize();
        }
    }
}
