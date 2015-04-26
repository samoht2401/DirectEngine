using System;
using System.Collections.Generic;
using System.Text;
using SharpDX;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace DirectEngine
{
    public struct Vertex
    {
        public Vector3 Position;
        public Vector2 TexCoord;
        public Vector3 Normal;
        public Vector3 Tangent;

        public Vertex(Vector3 pos, Vector2 texCoord, Vector3 normal, Vector3 tangent)
        {
            Position = pos;
            TexCoord = texCoord;
            Normal = normal;
            Tangent = tangent;
        }
        public Vertex(float x, float y, float z, float u, float v)
        {
            Position = new Vector3(x, y, z);
            TexCoord = new Vector2(u, v);
            Normal = Vector3.Zero;
            Tangent = Vector3.Zero;
        }
    }

    public class DrawableObj : IDisposable, IComparable
    {
        protected Game game;
        protected string textureName;
        protected string normalMapName;
        protected List<Vertex> vertices;
        protected Buffer vertexBuffer;
        protected Buffer indexBuffer;
        protected int indexCount;
        protected bool useTransparency;

        protected Matrix ViewProj;
        private bool needWorldMatrixUpdate;
        protected Matrix worldMatrix;
        protected Matrix worldModifMatrix;

        private bool use2DPosModif;
        public bool Use2DPosModif { get { return use2DPosModif; } set { if (use2DPosModif != value)needWorldMatrixUpdate = true; use2DPosModif = value; } }
        private Vector3 position;
        public virtual Vector3 Position { get { return position; } set { if (position != value)needWorldMatrixUpdate = true; position = value; } }
        public virtual Vector4 Position4 { get { return new Vector4(position, 1); } set { Vector3 val = new Vector3(value.X, value.Y, value.Z); if (position != val)needWorldMatrixUpdate = true; position = val; } }
        private Vector3 scale;
        public virtual Vector3 Scale { get { return scale; } set { if (scale != value)needWorldMatrixUpdate = true; scale = value; } }
        private Quaternion rotation;
        public virtual Quaternion Rotation { get { return rotation; } set { if (rotation != value)needWorldMatrixUpdate = true; rotation = value; } }
        public virtual Matrix WorldMatrix { get { if (needWorldMatrixUpdate) Draw_WorldMatrixCalculation(); return worldMatrix; } }

        public Color4? BlendFactor;

        public DrawableObj(Game game, Vertex[] vertex = null, int[] index = null, string textureName = null, string normalMapName = null, bool useTransparency = false)
        {
            this.game = game;
            if (vertex != null)
                SetVertexBuffer(vertex);
            if (index != null)
                SetIndexBuffer(index);
            needWorldMatrixUpdate = false;
            worldMatrix = Matrix.Identity;
            worldModifMatrix = Matrix.Identity;
            this.textureName = textureName;
            this.normalMapName = normalMapName;
            this.useTransparency = useTransparency;
            Use2DPosModif = false;
            Position = Vector3.Zero;
            Scale = Vector3.One;
            Rotation = Quaternion.Zero;
            BlendFactor = new Color4(0.5f, 0.5f, 0.5f, 0.5f);
        }

        public void Dispose()
        {
            if (vertexBuffer != null)
                vertexBuffer.Dispose();
            if (indexBuffer != null)
                indexBuffer.Dispose();
        }

        public void SetVertexBuffer(Vertex[] vertex)
        {
            if (vertex.Length > 0)
            {
                vertices = new List<Vertex>();
                vertices.AddRange(vertex);
                vertexBuffer = Buffer.Create(game.Device, BindFlags.VertexBuffer, vertex);
            }
        }

        public void SetIndexBuffer(int[] index)
        {
            if (index.Length > 0)
            {
                indexBuffer = Buffer.Create(game.Device, BindFlags.IndexBuffer, index);
                indexCount = index.Length;
            }
        }

        public void SetTexture(string textureName)
        {
            this.textureName = textureName;
        }

        public virtual void Draw()
        {
            Draw(Matrix.Identity);
        }
        public virtual void Draw(Matrix worldModif)
        {
            //ViewProj = viewProj;
            worldModifMatrix = worldModif;

            // Geometrie
            Draw_GeometrieUpload();

            // Matrices
            Draw_WorldMatrixCalculation();
            Draw_ShaderInputSetter();

            // Texture
            Draw_TextureBinding();

            // Transparency
            Draw_BlendingSetting();

            // Draw
            Draw_Drawing();
        }

        protected virtual void Draw_GeometrieUpload()
        {
            game.Context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertexBuffer, Utilities.SizeOf<Vertex>(), 0));
        }
        protected virtual void Draw_WorldMatrixCalculation()
        {
            if (needWorldMatrixUpdate)
            {
                if (Use2DPosModif)
                    worldMatrix = Matrix.Scaling(Scale) * Matrix.RotationQuaternion(Rotation) * Matrix.Translation(Position/*+ new Vector3(-game.Viewport.Width + Scale.X, game.Viewport.Height - Scale.Y, 0)*/);
                else
                    worldMatrix = Matrix.Scaling(Scale) * Matrix.RotationQuaternion(Rotation) * Matrix.Translation(Position);
                needWorldMatrixUpdate = false;
            }
        }
        protected virtual void Draw_ShaderInputSetter()
        {
            game.SetWorldMatrix(WorldMatrix * worldModifMatrix);
        }
        protected virtual void Draw_TextureBinding()
        {
            if (!String.IsNullOrEmpty(textureName))
                game.BindTexture(textureName);
            if (!String.IsNullOrEmpty(normalMapName))
                game.BindTexture(normalMapName, 1);
        }
        protected virtual void Draw_BlendingSetting()
        {
            game.SetBlendingState(useTransparency, BlendFactor);
        }
        protected virtual void Draw_Drawing()
        {
            // Draw the back faces first for transparent obj
            if (useTransparency)
            {
                game.SetCullingDirection(true);
                if (indexBuffer != null)
                {
                    game.Context.InputAssembler.SetIndexBuffer(indexBuffer, SharpDX.DXGI.Format.R32_SInt, 0);
                    game.Context.DrawIndexed(indexCount, 0, 0);
                }
                else
                    game.Context.Draw(vertices.Count, 0);
                game.SetCullingDirection(false);
            }

            // Draw with or without index
            if (indexBuffer != null)
            {
                game.Context.InputAssembler.SetIndexBuffer(indexBuffer, SharpDX.DXGI.Format.R32_SInt, 0);
                game.Context.DrawIndexed(indexCount, 0, 0);
            }
            else
                game.Context.Draw(vertices.Count, 0);
        }

        public int CompareTo(object obj)
        {
            if (obj is DrawableObj)
                return CompareTo((DrawableObj)obj);
            return 0;
        }

        public int CompareTo(DrawableObj other)
        {
            // First compare on transparency : Draw transparent obj last
            if (useTransparency && !other.useTransparency)
                return 1;
            if (!useTransparency && other.useTransparency)
                return -1;
            // Second compare on the cam - obj distance : from the farthest to the nearest
            Vector3 toCam = game.Camera.Position - Position;
            Vector3 otherToCam = game.Camera.Position - other.Position;
            return (int)(otherToCam.LengthSquared() - toCam.LengthSquared());
        }
    }
}
