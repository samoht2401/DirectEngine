using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;
using Resource = SharpDX.Direct3D11.Resource;

namespace DirectEngine
{
    public class RenderToTexture : IDisposable
    {
        Game game;

        Texture2D texture;
        Texture2D depthStencilBuffer;
        RenderTargetView renderView;
        DepthStencilView depthStencilView;
        ShaderResourceView shaderView;

        public int Width { protected set; get; }
        public int Height { protected set; get; }

        public RenderToTexture(Game game, int width, int height, bool usePersoDepth = false)
        {
            this.game = game;
            Device device = game.Device;
            texture = new Texture2D(device, new Texture2DDescription()
                                    {
                                        Format = Format.R32G32B32A32_Float,
                                        ArraySize = 1,
                                        MipLevels = 1,
                                        Width = width,
                                        Height = height,
                                        SampleDescription = new SampleDescription(1, 0),
                                        Usage = ResourceUsage.Default,
                                        BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                                        CpuAccessFlags = CpuAccessFlags.None,
                                        OptionFlags = ResourceOptionFlags.None
                                    });
            renderView = new RenderTargetView(device, texture, new RenderTargetViewDescription()
                                                {
                                                    Format = texture.Description.Format,
                                                    Dimension = RenderTargetViewDimension.Texture2D
                                                });
            shaderView = new ShaderResourceView(device, texture);

            if (usePersoDepth)
            {
                depthStencilBuffer = new Texture2D(device, new Texture2DDescription()
                {
                    Format = Format.D32_Float_S8X24_UInt,
                    ArraySize = 1,
                    MipLevels = 1,
                    Width = width,
                    Height = height,
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = ResourceUsage.Default,
                    BindFlags = BindFlags.DepthStencil,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.None
                });
                depthStencilView = new DepthStencilView(device, depthStencilBuffer);
            }

            Width = width;
            Height = height;
        }

        public void Dispose()
        {
            shaderView.Dispose();
            renderView.Dispose();
            texture.Dispose();
            if (depthStencilView != null)
                depthStencilView.Dispose();
            if (depthStencilBuffer != null)
                depthStencilBuffer.Dispose();
        }

        public void SaveToFile(string path)
        {
            Texture2D.ToFile(game.Context, texture, ImageFileFormat.Png, path);
        }

        public void SetAsRenderTarget()
        {
            game.PushAsRenderTarget(renderView, depthStencilView);
            game.PushViewport(new Viewport(0, 0, Width, Height));
        }

        public void UnsetAsRenderTarget()
        {
            game.PopRenderTarget();
            game.PopViewport();
        }

        public void ClearRenderTarget(Color color)
        {
            game.Context.ClearRenderTargetView(renderView, color);
            if (depthStencilView != null)
                game.Context.ClearDepthStencilView(depthStencilView, DepthStencilClearFlags.Depth, 1, 0);
            else
                game.Context.ClearDepthStencilView(game.depthStencilView, DepthStencilClearFlags.Depth, 1, 0);
        }

        public ShaderResourceView GetShaderResourceView()
        {
            return shaderView;
        }
    }
}
