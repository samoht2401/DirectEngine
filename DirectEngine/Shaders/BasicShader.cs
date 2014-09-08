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
    public struct BasicLight
    {
        public Vector4 lightDir;
        public Vector4 lightColor;

        public BasicLight(Vector3 direction, Color color)
        {
            lightDir = new Vector4(direction.X, direction.Y, direction.Z, 0);
            lightColor = color.ToColor4();
        }
    }

    public struct BasicViewData
    {
        public Matrix viewProjMatrix;
        public Vector4 cameraPos;

        public BasicViewData(Matrix viewProj, Vector3 camPos)
        {
            viewProjMatrix = viewProj;
            cameraPos = new Vector4(camPos.X, camPos.Y, camPos.Z, 0);
        }
    }

    public class BasicShader : Shader
    {
        private Stack<bool> isInLinearModeStack;
        private SamplerState samplerLinear;
        private SamplerState samplerPoint;
        private Buffer clipBuffer;
        private Buffer lightBuffer;
        private Buffer worldMatrixBuffer;
        private Buffer viewDataBuffer;

        private Vector4 clip;
        public Vector4 ClipPlane
        {
            get { return clip; }
            set
            {
                clip = value;
                UpdateClip();
            }
        }

        private BasicLight light;
        public BasicLight Light { get { return light; } set { light = value; UpdateLight(); } }

        private BasicViewData view;
        public BasicViewData View { get { return view; } set { view = value; UpdateView(); } }

        public BasicShader(Game game)
            : base(game, "Content\\Shaders\\BasicEffect.fx",
            new InputElement[]
            {
                new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                new InputElement("TEXCOORD", 0, Format.R32G32_Float, Utilities.SizeOf<Vector3>(), 0),
                new InputElement("NORMAL", 0, Format.R32G32B32A32_Float, Utilities.SizeOf<Vector3>() + Utilities.SizeOf<Vector2>(), 0),
                new InputElement("TANGENT", 0, Format.R32G32B32A32_Float, Utilities.SizeOf<Vector3>() * 2 + Utilities.SizeOf<Vector2>(), 0)
            })
        {
            isInLinearModeStack = new Stack<bool>();
            ClipPlane = new Vector4(0, 1, 0, 100);
            Light = new BasicLight(-Vector3.UnitY, Color.White);
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

            clipBuffer = new Buffer(Game.Device, Utilities.SizeOf<Vector4>(), ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);
            lightBuffer = new Buffer(Game.Device, Utilities.SizeOf<BasicLight>(), ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);
            worldMatrixBuffer = new Buffer(Game.Device, Utilities.SizeOf<Matrix>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            viewDataBuffer = new Buffer(Game.Device, Utilities.SizeOf<BasicViewData>(), ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);
        }

        protected internal override void DeleteSampler()
        {
            Utilities.Dispose(ref samplerLinear);
            Utilities.Dispose(ref samplerPoint);

            base.DeleteSampler();
        }
        protected internal override void DeleteConstantBuffer()
        {
            Utilities.Dispose(ref clipBuffer);
            Utilities.Dispose(ref lightBuffer);
            Utilities.Dispose(ref worldMatrixBuffer);
            Utilities.Dispose(ref viewDataBuffer);

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

            Game.Context.VertexShader.SetConstantBuffer(0, clipBuffer);
            Game.Context.PixelShader.SetConstantBuffer(1, lightBuffer);
            Game.Context.VertexShader.SetConstantBuffer(2, worldMatrixBuffer);
            Game.Context.VertexShader.SetConstantBuffer(3, viewDataBuffer);
        }

        public override void SetViewProjMatrix(Matrix matrix)
        {
            base.SetViewProjMatrix(matrix);

            matrix.Transpose();
            BasicViewData data = View;
            data.viewProjMatrix = matrix;
            View = data;
        }

        public override void SetWorldMatrix(Matrix matrix)
        {
            base.SetViewProjMatrix(matrix);

            matrix.Transpose();
            Game.Context.UpdateSubresource(ref matrix, worldMatrixBuffer);
        }

        private void UpdateClip()
        {
            DataStream map;
            Game.Context.MapSubresource(clipBuffer, 0, MapMode.WriteDiscard, MapFlags.None, out map);
            map.Write(clip);
            Game.Context.UnmapSubresource(clipBuffer, 0);
        }
        private void UpdateLight()
        {
            DataStream map;
            Game.Context.MapSubresource(lightBuffer, 0, MapMode.WriteDiscard, MapFlags.None, out map);
            map.Write(light);
            Game.Context.UnmapSubresource(lightBuffer, 0);
        }
        private void UpdateView()
        {
            DataStream map;
            Game.Context.MapSubresource(viewDataBuffer, 0, MapMode.WriteDiscard, MapFlags.None, out map);
            map.Write(view);
            Game.Context.UnmapSubresource(viewDataBuffer, 0);
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
    }
}
