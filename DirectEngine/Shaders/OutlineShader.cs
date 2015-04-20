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
    public class OutlineShader : Shader
    {
        private Buffer mapDataBuffer;

        private MapData mapData;
        public MapData MapData { get { return mapData; } set { mapData = value; UpdateMapSize(); } }

        private SamplerState samplerLinear;

        public OutlineShader(Game game)
            : base(game, "Content\\Shaders\\OutlineEffect.fx",
            new InputElement[]
            {
                new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                new InputElement("TEXCOORD", 0, Format.R32G32_Float, Utilities.SizeOf<Vector3>(), 0)
            })
        {
            mapData = new MapData(1, 1);
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
        }
        protected internal override void CreateConstantBuffer()
        {
            base.CreateConstantBuffer();

            mapDataBuffer = new Buffer(Game.Device, Utilities.SizeOf<Vector4>(), ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);
        }

        protected internal override void DeleteSampler()
        {
            Utilities.Dispose(ref samplerLinear);

            base.DeleteSampler();
        }
        protected internal override void DeleteConstantBuffer()
        {
            Utilities.Dispose(ref mapDataBuffer);

            base.DeleteConstantBuffer();
        }

        protected internal override void LinkSampler()
        {
            base.LinkSampler();

            Game.Context.PixelShader.SetSampler(0, samplerLinear);
        }
        protected internal override void LinkConstantBuffer()
        {
            base.LinkConstantBuffer();

            Game.Context.PixelShader.SetConstantBuffer(0, mapDataBuffer);
        }

        private void UpdateMapSize()
        {
            DataStream map;
            Game.Context.MapSubresource(mapDataBuffer, 0, MapMode.WriteDiscard, MapFlags.None, out map);
            map.Write(new Vector4(mapData.Width, mapData.Height, mapData.InvW, mapData.InvH));
            Game.Context.UnmapSubresource(mapDataBuffer, 0);
        }
    }
}
