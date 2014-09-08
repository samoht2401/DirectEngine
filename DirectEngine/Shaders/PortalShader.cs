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
    public class PortalShader : Shader
    {
        private struct PortalData
        {
            Vector4 screenSize;

            public PortalData(Vector2 size)
            {
                screenSize = new Vector4(size, 0, 1);
            }
        }

        private Stack<bool> isInLinearModeStack;
        private SamplerState samplerLinear;
        private SamplerState samplerPoint;
        private Buffer clipbuffer;
        private Buffer databuffer;
        private Buffer WorldMatrixBuffer;
        private Buffer ViewProjMatrixBuffer;

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

        private Vector4 clip;
        public Vector4 ClipPlane
        {
            get { return clip; }
            set
            {
                Vector4 val = value;

                clip = value;
                UpdateClip();
            }
        }

        public PortalShader(Game game)
            : base(game, "Content\\Shaders\\PortalShader.fx",
            new InputElement[]
            {
                new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                new InputElement("NORMAL", 0, Format.R32G32B32A32_Float, Utilities.SizeOf<Vector4>(), 0),
                new InputElement("TEXCOORD", 0, Format.R32G32_Float, Utilities.SizeOf<Vector4>() * 2, 0)
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

            clipbuffer = new Buffer(Game.Device, Utilities.SizeOf<Vector4>(), ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);
            databuffer = new Buffer(Game.Device, Utilities.SizeOf<PortalData>(), ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);
            WorldMatrixBuffer = new Buffer(Game.Device, Utilities.SizeOf<Matrix>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            ViewProjMatrixBuffer = new Buffer(Game.Device, Utilities.SizeOf<Matrix>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
        }

        protected internal override void DeleteSampler()
        {
            Utilities.Dispose(ref samplerLinear);
            Utilities.Dispose(ref samplerPoint);

            base.DeleteSampler();
        }
        protected internal override void DeleteConstantBuffer()
        {
            Utilities.Dispose(ref clipbuffer);
            Utilities.Dispose(ref databuffer);
            Utilities.Dispose(ref WorldMatrixBuffer);
            Utilities.Dispose(ref ViewProjMatrixBuffer);

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

            Game.Context.VertexShader.SetConstantBuffer(0, clipbuffer);
            Game.Context.PixelShader.SetConstantBuffer(1, databuffer);
            Game.Context.VertexShader.SetConstantBuffer(2, WorldMatrixBuffer);
            Game.Context.VertexShader.SetConstantBuffer(3, ViewProjMatrixBuffer);
        }

        private void UpdateSize()
        {
            PortalData data = new PortalData(size);
            DataStream map;
            Game.Context.MapSubresource(databuffer, 0, MapMode.WriteDiscard, MapFlags.None, out map);
            map.Write(data);
            Game.Context.UnmapSubresource(databuffer, 0);
        }
        private void UpdateClip()
        {
            DataStream map;
            Game.Context.MapSubresource(clipbuffer, 0, MapMode.WriteDiscard, MapFlags.None, out map);
            map.Write(clip);
            Game.Context.UnmapSubresource(clipbuffer, 0);
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
        public void PopIsInLinearMode()
        {
            if (isInLinearModeStack.Count > 0)
            {
                bool old = isInLinearModeStack.Pop();
                if (isInLinearModeStack.Count <= 0 || isInLinearModeStack.Peek() != old)
                    LinkSampler();
            }
        }

        public override void SetViewProjMatrix(Matrix matrix)
        {
            base.SetViewProjMatrix(matrix);

            matrix.Transpose();
            Game.Context.UpdateSubresource(ref matrix, ViewProjMatrixBuffer);
        }

        public override void SetWorldMatrix(Matrix matrix)
        {
            base.SetWorldMatrix(matrix);

            matrix.Transpose();
            Game.Context.UpdateSubresource(ref matrix, WorldMatrixBuffer);
        }

        /*private struct PortalData
        {
            Vector4 dimMult;
            Vector4 offset;

            public PortalData(Vector2 dimMult, Vector4 offset)
            {
                this.dimMult = new Vector4(dimMult, 0, 1);
                this.offset = offset;
            }
        }

        private Stack<bool> isInLinearModeStack;
        private SamplerState samplerLinear;
        private SamplerState samplerPoint;
        private Buffer databuffer;
        private Buffer wvpMatrixBuffer;

        Vector2 dimensionMultiplier;
        Vector4 tex_offset;

        public Vector2 DimMultiplier
        {
            get { return dimensionMultiplier; }
            set
            {
                dimensionMultiplier = value;
                UpdateData();
            }
        }

        public PortalShader(Game game)
            : base(game, "Content\\Shaders\\PortalShader.fx",
            new InputElement[]
            {
                new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                new InputElement("TEXCOORD", 0, Format.R32G32_Float, Utilities.SizeOf<Vector4>(), 0)
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

            databuffer = new Buffer(Game.Device, Utilities.SizeOf<PortalData>(), ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);
            wvpMatrixBuffer = new Buffer(Game.Device, Utilities.SizeOf<Matrix>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
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
            Utilities.Dispose(ref wvpMatrixBuffer);

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
            Game.Context.VertexShader.SetConstantBuffer(0, wvpMatrixBuffer);
        }

        private void UpdateData()
        {
            PortalData data = new PortalData(dimensionMultiplier, tex_offset);
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

        public override void SetMainMatrix(Matrix matrix)
        {
            base.SetMainMatrix(matrix);

            matrix.Transpose();
            Game.Context.UpdateSubresource(ref matrix, wvpMatrixBuffer);
        }*/
    }
}
