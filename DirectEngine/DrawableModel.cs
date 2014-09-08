using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SharpDX;

namespace DirectEngine
{
    public class DrawableModel : DrawableObj
    {
        public DrawableModel(Game game, string textureName, string normalMapName, bool useTransparency, string path)
            : base(game,
            new Vertex[] { },
                new int[] { }, textureName, normalMapName, useTransparency)
        {
            Load(path, true);
        }

        struct VertexInfo
        {
            Vector3 pos;
            Vector2 texCoord;
            Vector3 normal;

            public VertexInfo(Vector3 pos, Vector2 texCoord, Vector3 normal)
            {
                this.pos = pos;
                this.texCoord = texCoord;
                this.normal = normal;
            }
        }

        protected void Load(string path, bool isRightHanded = false)
        {

            //std::wifstream fileIn (filename.c_str());	//Open file
            string meshMatLib;					//String to hold our obj material library filename

            //Arrays to store our model's information
            List<int> indexTab = new List<int>();
            List<Vector3> vertPosTab = new List<Vector3>();
            List<Vector2> vertTexCoordTab = new List<Vector2>();
            List<Vector3> vertNormTab = new List<Vector3>();
            List<Vector3> vertTangTab = new List<Vector3>();
            List<string> meshMatTab = new List<string>();

            //Vertex definition indices
            List<int> faceVertexIndex = new List<int>();
            List<Vector4> vertexIndexedTab = new List<Vector4>();

            //Make sure we have a default if no tex coords or normals are defined
            bool hasTexCoord = false;
            bool hasNorm = false;

            int vIndex = 0;
            List<int> subsetIndexStart = new List<int>();
            int subsetCount = 0;

            System.Globalization.NumberFormatInfo formatInfo = new System.Globalization.NumberFormatInfo();
            formatInfo.NumberDecimalSeparator = ".";

            using (StreamReader reader = new StreamReader(path))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    switch (line[0])
                    {
                        case '#': { break; }
                        case 'v':	//Get Vertex Descriptions
                            {
                                switch (line[1])
                                {
                                    case ' ': //v - vert position
                                        {
                                            float vx, vy, vz;
                                            string[] splitResult = line.Substring(2).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                            vx = float.Parse(splitResult[0], formatInfo);
                                            vy = float.Parse(splitResult[1], formatInfo);
                                            vz = float.Parse(splitResult[2], formatInfo);
                                            if (isRightHanded)
                                                vertPosTab.Add(new Vector3(vx, vy, -vz));
                                            else
                                                vertPosTab.Add(new Vector3(vx, vy, vz));
                                            break;
                                        }
                                    case 't': //vt - vert tex coords
                                        {
                                            float vtx, vty;
                                            string[] splitResult = line.Substring(3).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                            vtx = float.Parse(splitResult[0], formatInfo);
                                            vty = float.Parse(splitResult[1], formatInfo);
                                            if (isRightHanded)
                                                vertTexCoordTab.Add(new Vector2(vtx, 1 - vty));
                                            else
                                                vertTexCoordTab.Add(new Vector2(vtx, vty));
                                            hasTexCoord = true;
                                            break;
                                        }
                                    case 'n': //vn - vert normal
                                        {
                                            float vnx, vny, vnz;
                                            string[] splitResult = line.Substring(3).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                            vnx = float.Parse(splitResult[0], formatInfo);
                                            vny = float.Parse(splitResult[1], formatInfo);
                                            vnz = float.Parse(splitResult[2], formatInfo);
                                            if (isRightHanded)
                                                vertNormTab.Add(new Vector3(vnx, vny, -vnz));
                                            else
                                                vertNormTab.Add(new Vector3(vnx, vny, vnz));
                                            hasNorm = true;
                                            break;
                                        }
                                }
                                break;
                            }
                        //New group (Subset)
                        case 'g': //g - defines a group
                            {
                                if (line[1] == ' ')
                                {
                                    subsetIndexStart.Add(vIndex);
                                    subsetCount++;
                                }
                                break;
                            }
                        case 'f': //f - defines the faces
                            {
                                if (line[1] == ' ')
                                {
                                    string[] vertDef = line.Substring(2).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                    int triangleCount = vertDef.Length - 2;

                                    Vector4 vertex = Vector4.Zero;
                                    List<int> vertexIndex = new List<int>();

                                    Vector4[] faceVertex = new Vector4[3];
                                    for (int i = 0; i < vertDef.Length; i++)
                                    {
                                        if (vertDef[i].Contains("//")) // Position//Normal
                                        {
                                            string[] splited = vertDef[i].Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                                            vertex.X = int.Parse(splited[0], formatInfo) - 1;
                                            vertex.Y = 0;
                                            vertex.Z = int.Parse(splited[1], formatInfo) - 1;
                                        }
                                        else if (vertDef[i].Contains("/")) // Position/TexCoord or Position/TexCoord/Normal
                                        {
                                            string[] splited = vertDef[i].Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                                            if (splited.Length == 2)
                                            {
                                                vertex.X = int.Parse(splited[0], formatInfo) - 1;
                                                vertex.Y = int.Parse(splited[1], formatInfo) - 1;
                                                vertex.Z = 0;
                                            }
                                            else
                                            {
                                                vertex.X = int.Parse(splited[0], formatInfo) - 1;
                                                vertex.Y = int.Parse(splited[1], formatInfo) - 1;
                                                vertex.Z = int.Parse(splited[2], formatInfo) - 1;
                                            }
                                        }
                                        else // Position
                                        {
                                            vertex.X = int.Parse(vertDef[i], formatInfo) - 1;
                                            vertex.Y = 0;
                                            vertex.Z = 0;
                                        }

                                        faceVertex[i % 3] = vertex;

                                        if (i % 3 == 2) // Une nouvelle face est à former
                                        {
                                            // Calcul Tangent
                                            Vector3 pos1 = vertPosTab[(int)faceVertex[0].X];
                                            Vector3 pos2 = vertPosTab[(int)faceVertex[1].X];
                                            Vector3 pos3 = vertPosTab[(int)faceVertex[2].X];

                                            Vector2 tu1 = vertTexCoordTab[(int)faceVertex[0].Y];
                                            Vector2 tu2 = vertTexCoordTab[(int)faceVertex[1].Y];
                                            Vector2 tu3 = vertTexCoordTab[(int)faceVertex[2].Y];

                                            // Calculate the two vectors for this face.
                                            Vector3 vec1 = pos2 - pos1;
                                            Vector3 vec2 = pos3 - pos1;

                                            // Calculate the tu and tv texture space vectors.
                                            Vector2 tvec1 = tu2 - tu1;
                                            Vector2 tvec2 = tu3 - tu1;

                                            // Calculate the denominator of the tangent equation.
                                            float multBy = 1.0f / (tvec1.X * tvec2.Y - tvec2.X * tvec1.Y);

                                            // Calculate the cross products and multiply by the coefficient to get the tangent.
                                            Vector3 tangent = (tvec2.Y * vec1 - tvec1.Y * vec2) * multBy;
                                            tangent.Normalize();

                                            int index = -1;
                                            if (vertTangTab.Contains(tangent))
                                                index = vertTangTab.IndexOf(tangent);
                                            else
                                            {
                                                vertTangTab.Add(tangent);
                                                index = vertTangTab.Count - 1;
                                            }

                                            faceVertex[0].W = index;
                                            faceVertex[1].W = index;
                                            faceVertex[2].W = index;

                                            for (int j = 0; j < 3; j++)
                                            {
                                                index = -1;
                                                if (vertexIndexedTab.Contains(faceVertex[j]))
                                                    index = vertexIndexedTab.IndexOf(faceVertex[j]);
                                                else
                                                {
                                                    vertexIndexedTab.Add(faceVertex[j]);
                                                    index = vertexIndexedTab.Count - 1;
                                                }
                                                vertexIndex.Add(index);
                                            }
                                        }                                        
                                    }

                                    for (int i = 0; i < triangleCount; i++)
                                    {
                                        faceVertexIndex.Add(vertexIndex[i]);
                                        faceVertexIndex.Add(vertexIndex[2 + i]);
                                        faceVertexIndex.Add(vertexIndex[1 + i]);
                                    }
                                }
                                break;
                            }
                        case 'm': //mtllib - material library filename
                            {
                                if (line.StartsWith("mtllib"))
                                {
                                    meshMatLib = line.Split(' ').Last();
                                }
                                break;
                            }
                        case 'u': //usemtl - which material to use
                            {
                                if (line.StartsWith("usemtl"))
                                {
                                    meshMatTab.Add(line.Split(' ').Last());
                                }
                                break;
                            }
                        default:
                            break;
                    }
                }
                if (!hasNorm)
                    vertNormTab.Add(Vector3.Zero);
                if (!hasTexCoord)
                    vertTexCoordTab.Add(Vector2.Zero);

                reader.Close();
            }
            List<Vertex> vertices = new List<Vertex>();
            List<int> indices = new List<int>();

            //Create our vertices using the information we got 
            //from the file and store them in a vector
            foreach (Vector4 vertexIndexed in vertexIndexedTab)
                vertices.Add(new Vertex(vertPosTab[(int)vertexIndexed.X], vertTexCoordTab[(int)vertexIndexed.Y], vertNormTab[(int)vertexIndexed.Z], vertTangTab[(int)vertexIndexed.W]));

            foreach (int i in faceVertexIndex)
                indices.Add(i);

            SetVertexBuffer(vertices.ToArray());
            SetIndexBuffer(indices.ToArray());
        }
    }
}
