using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace DirectEngine.Rooms
{
    public class Portal
    {
        public Vector3 Room1Position { get; set; }
        private Vector3 normal1;
        public Vector3 Room1Normal { get { return normal1; } set { normal1 = value; normal1.Normalize(); } }

        public Vector3 Room2Position { get; set; }
        private Vector3 normal2;
        public Vector3 Room2Normal { get { return normal2; } set { normal2 = value; normal2.Normalize(); } }
        public string Room2Name { get; protected set; }

        private bool needUpdateMatrix;
        private Matrix matrix2To1;

        public Vector4 Size { get; protected set; }
        public string Name { get; protected set; }

        public Portal(Vector3 pos1, Vector3 norm1, Vector3 pos2, Vector3 norm2, string room2Name, Vector4 size, string name)
        {
            Room1Position = pos1;
            norm1.Y = 0;
            if (!norm1.IsNormalized)
                norm1.Normalize();
            Room1Normal = norm1;
            Room2Position = pos2;
            norm2.Y = 0;
            if (!norm2.IsNormalized)
                norm2.Normalize();
            Room2Normal = norm2;
            Room2Name = room2Name;

            needUpdateMatrix = true;
            matrix2To1 = Matrix.Identity;

            Size = size;
            Name = name;
        }

        public Matrix GetTransRoom2RealtiveToRoom1()
        {
            if (needUpdateMatrix)
            {
                // Rotation
                // First we need to make sure the portal are opposite, so that they normal are opposite.
                Matrix rotMatrix = Matrix.RotationQuaternion(GetRotationToOppos2To1(Room1Normal, Room2Normal));
                // Translation
                // We now need to find the translation vector to make the to side of the portal touching
                // First we transform P2 with the rotation
                Vector4 v = Vector3.Transform(Room2Position, rotMatrix);
                Vector3 P2Trans = new Vector3(v.X, v.Y, v.Z);
                // Then we calcul the translation vector wich is the difference bewteen the to point
                Vector3 transVec = Room1Position - P2Trans;
                // And we build the matrix
                Matrix transMatrix = Matrix.Translation(transVec);

                // The result is the combination of those matrix
                matrix2To1 = rotMatrix * transMatrix;
                needUpdateMatrix = false;
            }
            return matrix2To1;
        }

        public Portal GetOppositePortal(string originalRoomName)
        {
            return new Portal(Room2Position, Room2Normal, Room1Position, Room1Normal, originalRoomName, Size, "OppositeOf" + Name);
        }

        public bool IntersectRay(Ray ray, out float distance)
        {
            float width = Size.Z - Size.X;
            float height = Size.W - Size.Y;
            Vector3 position = Room1Position + new Vector3(Size.X + width / 2, Size.Y + height / 2, 0);

            Quaternion rotation = GetRotationToAlign2To1(Vector3.UnitZ, Room1Normal);

            Vector3 scale = new Vector3(width, height, 1);
            Matrix trans = Matrix.Scaling(scale) * Matrix.RotationQuaternion(rotation) * Matrix.Translation(position);

            Vector3 v1 = Vector3.TransformCoordinate(new Vector3(-1, -1, 0), trans);
            Vector3 v2 = Vector3.TransformCoordinate(new Vector3(-1, 1, 0), trans);
            Vector3 v3 = Vector3.TransformCoordinate(new Vector3(1, 1, 0), trans);
            Vector3 v4 = Vector3.TransformCoordinate(new Vector3(1, -1, 0), trans);

            return ray.Intersects(ref v1, ref v2, ref v3, out distance) || ray.Intersects(ref v1, ref v3, ref v4, out distance) ||
                ray.Intersects(ref v1, ref v3, ref v2, out distance) || ray.Intersects(ref v1, ref v4, ref v3, out distance);
        }

        public bool IntersectFrustrum(BoundingFrustum frustrum, Vector3 camPos, Matrix transformation)
        {
            float width = Size.Z - Size.X;
            float height = Size.W - Size.Y;
            Vector3 position = Room1Position + new Vector3(Size.X + width / 2, Size.Y + height / 2, 0);

            Quaternion rotation = GetRotationToAlign2To1(Vector3.UnitZ, Room1Normal);

            Vector3 scale = new Vector3(width, height, 1);
            Matrix trans = Matrix.Scaling(scale) * Matrix.RotationQuaternion(rotation) * Matrix.Translation(position) * transformation;

            Vector3 v1 = Vector3.TransformCoordinate(new Vector3(-1, -1, 0), trans);
            Vector3 v2 = Vector3.TransformCoordinate(new Vector3(-1, 1, 0), trans);
            Vector3 v3 = Vector3.TransformCoordinate(new Vector3(1, 1, 0), trans);
            Vector3 v4 = Vector3.TransformCoordinate(new Vector3(1, -1, 0), trans);

            float minX = Math.Min(Math.Min(v1.X, v2.X), Math.Min(v3.X, v4.X));
            float minY = Math.Min(Math.Min(v1.Y, v2.Y), Math.Min(v3.Y, v4.Y));
            float minZ = Math.Min(Math.Min(v1.Z, v2.Z), Math.Min(v3.Z, v4.Z));
            float maxX = Math.Max(Math.Max(v1.X, v2.X), Math.Max(v3.X, v4.X));
            float maxY = Math.Max(Math.Max(v1.Y, v2.Y), Math.Max(v3.Y, v4.Y));
            float maxZ = Math.Max(Math.Max(v1.Z, v2.Z), Math.Max(v3.Z, v4.Z));

            if(Input.InputManager.Keyboard.IsPressed(Input.Key.T))
                for(int i = 0;i<1000;i++)
                { }

            BoundingBox box = new BoundingBox(new Vector3(minX, minY, minZ), new Vector3(maxX, maxY, maxZ));
            if (!frustrum.Intersects(ref box))
                return false;

            /*Vector3 leftLim = Vector3.Cross(frustrum.Left.Normal, Vector3.UnitY);
            Vector3 rightLim = Vector3.Cross(frustrum.Right.Normal, Vector3.UnitY);

            bool leftPositif = Vector3.Dot(leftLim, Vector3.TransformNormal(Room1Normal, transformation)) > 0;
            bool rightPositif = Vector3.Dot(rightLim, Vector3.TransformNormal(Room1Normal, transformation)) > 0;

            return !(leftPositif && rightPositif); // If one is negatif return true*/
            Vector3 toPlane = Vector3.TransformCoordinate(Room1Position, transformation) - camPos;

            if (Vector3.Dot(toPlane, Vector3.TransformNormal(Room1Normal, transformation)) < 0)
                return false;
            return true;

            //return true;
        }

        public static Quaternion GetRotationToAlign2To1(Vector3 v1, Vector3 v2)
        {
            Quaternion result = Quaternion.Identity;
            v1.Normalize();
            v2.Normalize();
            if (v1 == v2) // No rotation need
                return result;

            float dot = Vector3.Dot(v1, v2);
            if (dot == 0) // If the normals are perpendiculair we use the cross prod
            {
                result = Quaternion.RotationAxis(Vector3.Cross(v1, v2), (float)Math.PI / 2);
            }
            else if (dot != 1)
            {
                // Then we find the angle, it equal to PI (flat angle) minus the already existing angle
                float angleOfRot = (float)(Math.Acos(dot));
                // We can build the rotation matrix
                result = Quaternion.RotationAxis(Vector3.UnitY, angleOfRot);
            }
            return result;
        }

        public static Quaternion GetRotationToOppos2To1(Vector3 v1, Vector3 v2)
        {
            Quaternion result = Quaternion.Identity;
            v1.Normalize();
            v2.Normalize();
            if (v1 == -v2) // No rotation need
                return result;

            float dot = Vector3.Dot(v1, v2);
            if (dot == 0) // If the normals are perpendiculair we use the cross prod
            {
                result = Quaternion.RotationAxis(Vector3.Cross(v1, v2), (float)(Math.PI - Math.PI / 2));
            }
            else if (dot != -1)
            {
                // They we find the angle, it equal to PI (flat angle) minus the already existing angle
                float angleOfRot = (float)(Math.PI - Math.Acos(dot));
                // We can build the rotation matrix
                result = Quaternion.RotationAxis(Vector3.UnitY, angleOfRot);
            }
            return result;
        }
    }
}
