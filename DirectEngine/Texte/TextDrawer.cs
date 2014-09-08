using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace DirectEngine.Texte
{
    internal struct TextData : IEquatable<TextData>
    {
        public Font font;
        public string[] text;
        public RenderToTexture texture;

        public TextData(Font font, string[] text)
        {
            this.font = font;
            this.text = text;
            this.texture = null;
        }

        public bool Equals(TextData other)
        {
            if (font != other.font)
                return false;
            if (text.Length != other.text.Length)
                return false;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] != other.text[i])
                    return false;
            }
            return true;
        }
    }

    public static class TextDrawer
    {
        private static List<TextData> textDict = new List<TextData>();

        public static RenderToTexture GetTextTexture(Font font, string[] text)
        {
            TextData data = new TextData(font, text);

            // If the text already exist, find and return the right index
            IEnumerable<TextData> whereResult = textDict.Where(pair => pair.Equals(data));
            if (whereResult.Count() > 0)
                return whereResult.ElementAt(0).texture;

            // If the text doesn't exist yet, we create one
            Vector2 size = GetTextSize(font, text);
            RenderToTexture texture = new RenderToTexture(font.Game, (int)size.X, (int)size.Y);
            // Then we render the text char by char on quads
            RenderText(font, text, texture);

            // Add it to the list to not render it again
            data.texture = texture;
            textDict.Add(data);

            return texture;
        }

        public static Vector2 GetTextSize(Font font, string[] text)
        {
            // Find the height
            int Y = font.Height * text.Length;

            // Find the width
            int X = 0;
            foreach (string s in text)
            {
                int lineWidth = 0;
                for (int i = 0; i < s.Length; i++)
                    lineWidth += font.GetCharWidth(s[i]) + 1;
                if (lineWidth > X)
                    X = lineWidth;
            }

            return new Vector2(X, Y);
        }

        private static void RenderText(Font font, string[] text, RenderToTexture texture)
        {
            DeviceContext context = font.Game.Context;

            texture.SetAsRenderTarget();
            Color Alpha = Color.White;
            Alpha.A = 0;
            texture.ClearRenderTarget(Alpha);

            font.Game.PushShader(font.Game.FontShader);
            font.Game.BindTexture(font.TextureName);
            font.Game.SetBlendingState(false, null);

            List<int> indices = new List<int>();
            List<Vertex> vertices = new List<Vertex>();

            float currentY = 1;
            int indexOffset = 0;
            for (int l = 0; l < text.Length; l++)
            {
                float currentX = -1;
                string s = text[l];
                for (int m = 0; m < s.Length; m++)
                {
                    char c = s[m];

                    currentX += 2f / texture.Width;
                    float charWidth = (float)font.GetCharWidth(c) / texture.Width * 2;
                    float charHeight = (float)font.Height / texture.Height * 2;

                    if (c != ' ')
                    {
                        vertices.Add(new Vertex(currentX, currentY - charHeight, 0f, font.getMinU(c), 1));
                        vertices.Add(new Vertex(currentX, currentY, 0f, font.getMinU(c), 0));
                        vertices.Add(new Vertex(currentX + charWidth, currentY, 0f, font.getMaxU(c), 0));
                        vertices.Add(new Vertex(currentX + charWidth, currentY - charHeight, 0f, font.getMaxU(c), 1));
                        indices.Add(indexOffset);
                        indices.Add(1 + indexOffset);
                        indices.Add(2 + indexOffset);
                        indices.Add(0 + indexOffset);
                        indices.Add(2 + indexOffset);
                        indices.Add(3 + indexOffset);
                        indexOffset += 4;
                    }
                    currentX += charWidth;
                }
                currentY -= (float)font.Height / texture.Height * 2;
            }
            Buffer indexBuffer = Buffer.Create(font.Game.Device, BindFlags.IndexBuffer, indices.ToArray());
            Buffer vertexBuffer = Buffer.Create(font.Game.Device, BindFlags.VertexBuffer, vertices.ToArray());

            font.Game.Context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertexBuffer, Utilities.SizeOf<Vertex>(), 0));
            font.Game.Context.InputAssembler.SetIndexBuffer(indexBuffer, SharpDX.DXGI.Format.R32_SInt, 0);

            font.Game.Context.DrawIndexed(indices.Count, 0, 0);

            font.Game.PopShader();
            texture.UnsetAsRenderTarget();
        }
    }
}
