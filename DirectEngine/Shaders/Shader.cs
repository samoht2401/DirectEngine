using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace DirectEngine.Shaders
{
    public struct MapData
    {
        public float Width;
        public float Height;
        public float InvW;
        public float InvH;

        public MapData(float width, float height)
        {
            Width = width;
            Height = height;
            InvW = 1.0f / Width;
            InvH = 1.0f / Height;
        }
    }

    public abstract class Shader : IDisposable
    {
        public Game Game { protected set; get; }

        private VertexShader vertexShader;
        private PixelShader pixelShader;
        private InputLayout layout;

        public Shader(Game game, string path, InputElement[] layoutElems)
        {
            Game = game;

            // Initialize Shader
            // Compile Vertex and Pixel shaders
            CompilationResult vertexShaderByteCode = ShaderBytecode.CompileFromFile(path, "VS", "vs_4_0", ShaderFlags.None, EffectFlags.None);
            vertexShader = new VertexShader(Game.Device, vertexShaderByteCode);
            CompilationResult pixelShaderByteCode = ShaderBytecode.CompileFromFile(path, "PS", "ps_4_0", ShaderFlags.None, EffectFlags.None);
            pixelShader = new PixelShader(Game.Device, pixelShaderByteCode);

            // Layout from VertexShader input signature
            layout = new InputLayout(Game.Device, ShaderSignature.GetInputSignature(vertexShaderByteCode), layoutElems);

            // Create Constant Buffer
            CreateConstantBuffer();

            // Create Sampler
            CreateSampler();

            // Free compilation result
            vertexShaderByteCode.Dispose();
            pixelShaderByteCode.Dispose();
        }
        protected internal virtual void CreateConstantBuffer() { }
        protected internal virtual void CreateSampler() { }

        public void Dispose()
        {
            DeleteSampler();
            DeleteConstantBuffer();
            Utilities.Dispose(ref layout);
            Utilities.Dispose(ref pixelShader);
            Utilities.Dispose(ref vertexShader);
        }
        protected internal virtual void DeleteConstantBuffer() { }
        protected internal virtual void DeleteSampler() { }

        internal void LinkShader()
        {
            Game.Context.InputAssembler.InputLayout = layout;
            LinkConstantBuffer();
            Game.Context.VertexShader.Set(vertexShader);
            Game.Context.PixelShader.Set(pixelShader);
            LinkSampler();
        }
        protected internal virtual void LinkConstantBuffer() { }
        protected internal virtual void LinkSampler() { }

        public virtual void SetViewProjMatrix(Matrix matrix) { }
        public virtual void SetWorldMatrix(Matrix matrix) { }
    }
}
