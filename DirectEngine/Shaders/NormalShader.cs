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
    public struct NormalMatrixData
    {
        public Matrix viewProjMatrix;
        public Matrix worldMatrix;

        public NormalMatrixData(Matrix viewProj, Matrix world)
        {
            viewProjMatrix = viewProj;
            worldMatrix = world;
        }
    }

    public class NormalShader : Shader
    {
        private Buffer clipBuffer;
        private Buffer matricesBuffer;

        private Vector4 clip;
        public Vector4 ClipPlane { get { return clip; } set { clip = value; UpdateClip(); } }

        private NormalMatrixData matrices;
        public NormalMatrixData Matrices { get { return matrices; } set { matrices = value; UpdateMatrices(); } }

        public NormalShader(Game game)
            : base(game, "Content\\Shaders\\NormalEffect.fx",
            new InputElement[]
            {
                new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                new InputElement("NORMAL", 0, Format.R32G32B32A32_Float, Utilities.SizeOf<Vector3>() + Utilities.SizeOf<Vector2>(), 0),
            })
        {
            ClipPlane = new Vector4(0, 1, 0, 100);
            Matrices = new NormalMatrixData(Matrix.Identity, Matrix.Identity);
        }

        protected internal override void CreateConstantBuffer()
        {
            base.CreateConstantBuffer();

            clipBuffer = new Buffer(Game.Device, Utilities.SizeOf<Vector4>(), ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);
            matricesBuffer = new Buffer(Game.Device, Utilities.SizeOf<NormalMatrixData>(), ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);
        }

        protected internal override void DeleteConstantBuffer()
        {
            Utilities.Dispose(ref clipBuffer);
            Utilities.Dispose(ref matricesBuffer);

            base.DeleteConstantBuffer();
        }

        protected internal override void LinkConstantBuffer()
        {
            base.LinkConstantBuffer();

            Game.Context.VertexShader.SetConstantBuffer(0, clipBuffer);
            Game.Context.VertexShader.SetConstantBuffer(1, matricesBuffer);
        }

        public override void SetViewProjMatrix(Matrix matrix)
        {
            base.SetViewProjMatrix(matrix);

            matrix.Transpose();
            NormalMatrixData data = Matrices;
            data.viewProjMatrix = matrix;
            Matrices = data;
        }

        public override void SetWorldMatrix(Matrix matrix)
        {
            base.SetWorldMatrix(matrix);

            matrix.Transpose();
            NormalMatrixData data = Matrices;
            data.worldMatrix = matrix;
            Matrices = data;
        }

        private void UpdateClip()
        {
            DataStream map;
            Game.Context.MapSubresource(clipBuffer, 0, MapMode.WriteDiscard, MapFlags.None, out map);
            map.Write(clip);
            Game.Context.UnmapSubresource(clipBuffer, 0);
        }
        private void UpdateMatrices()
        {
            DataStream map;
            Game.Context.MapSubresource(matricesBuffer, 0, MapMode.WriteDiscard, MapFlags.None, out map);
            map.Write(matrices);
            Game.Context.UnmapSubresource(matricesBuffer, 0);
        }
    }
}
