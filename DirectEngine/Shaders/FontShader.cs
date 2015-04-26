using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace DirectEngine.Shaders
{
    public class FontShader : Shader
    {
        private SamplerState sampler;

        public FontShader(Game game)
            : base(game, "Content\\Shaders\\FontShader.fx",
            new InputElement[]
            {
                new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                new InputElement("TEXCOORD", 0, Format.R32G32_Float, Utilities.SizeOf<Vector3>(), 0),
                new InputElement("NORMAL", 0, Format.R32G32B32A32_Float, Utilities.SizeOf<Vector3>() + Utilities.SizeOf<Vector2>(), 0),
                new InputElement("TANGENT", 0, Format.R32G32B32A32_Float, Utilities.SizeOf<Vector3>() * 2 + Utilities.SizeOf<Vector2>(), 0)
            }) { }

        protected internal override void CreateSampler()
        {
            base.CreateSampler();

            sampler = new SamplerState(Game.Device, new SamplerStateDescription()
            {
                Filter = Filter.MinMagMipPoint,
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

        protected internal override void DeleteSampler()
        {
            Utilities.Dispose(ref sampler);

            base.DeleteSampler();
        }

        protected internal override void LinkConstantBuffer()
        {
            base.LinkConstantBuffer();

            //Game.Context.VertexShader.SetConstantBuffer(0, null);
        }

        protected internal override void LinkSampler()
        {
            base.LinkSampler();

            Game.Context.PixelShader.SetSampler(0, sampler);
        }
    }
}
