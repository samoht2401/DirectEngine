using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace DirectEngine.Physic
{
    public class SolidPlane
    {
        private Vector3 center;
        public Vector3 Center { get { return center; } set { center = value; needMinMaxCal = true; } }
        private float width;
        public float Width { get { return width; } set { width = value; needMinMaxCal = true; } }
        private float height;
        public float Height { get { return height; } set { height = value; needMinMaxCal = true; } }
        private Vector3 up;
        public Vector3 Up { get { return up; } protected set { up = Vector3.Normalize(value); needLeftCal = true; } }
        private Vector3 normal;
        public Vector3 Normal { get { return normal; } protected set { normal = Vector3.Normalize(value); needLeftCal = true; } }
        private bool needLeftCal = true;
        private Vector3 left = Vector3.Zero;
        public Vector3 Left { get { if (needLeftCal) CalculateLeft(); return left; } set { left = Vector3.Normalize(value); } }
        private bool needMinMaxCal = true;
        private Vector3 min;
        public Vector3 Min { get { if (needMinMaxCal) CalculateMinMax(); return min; } set { min = value; } }
        private Vector3 max;
        public Vector3 Max { get { if (needMinMaxCal) CalculateMinMax(); return max; } set { max = value; } }

        public float FrictionCoefficient { get; set; }

        public SolidPlane(Vector3 center, Vector3 normal, Vector3 up, float width, float height, float friction = 0.8f)
        {
            Center = center;
            Normal = normal;
            Up = up;
            Width = width;
            Height = height;
            FrictionCoefficient = friction;
        }

        private void CalculateLeft()
        {
            Left = Vector3.Cross(Normal, Up);
            needMinMaxCal = true;
            needLeftCal = false;
        }
        private void CalculateMinMax()
        {
            float halfW = Width / 2f;
            float halfH = Height / 2f;
            Min = Center - halfW * Left - halfH * Up;
            Max = Center + halfW * Left + halfH * Up;
            needMinMaxCal = false;
        }

        public Vector3 TestAgainst(Vector3 initialPosition, Vector3 mouvement, float radius = 0.0f)
        {
            Vector3 NewCenter = Center + radius * Normal;
            Vector3 NewMin = Min + radius * Normal - radius * Up - radius * Left;
            Vector3 NewMax = Max + radius * Normal + radius * Up + radius * Left;

            Vector3 mouvN = Vector3.Normalize(mouvement);

            float dot_nm = Vector3.Dot(Normal, mouvN);
            float dot_np = Vector3.Dot(Normal, NewCenter - initialPosition);

            // If the mouvement going away, we ignore it (so plane collision is direction sentitive)
            if (dot_nm >= 0 || dot_np > 0)
                return mouvement;

            // Compute the fraction of the mouvement doable
            float fractionM = dot_np * dot_nm / mouvement.Length();
            // The whole mouvement won't touch the plane.
            if (fractionM > 1)
                return mouvement;

            // Compute intersection point
            Vector3 intersection = initialPosition + fractionM * mouvement;
            // Is it on the plane limits?
            Vector3 interMin = NewMin - intersection;
            Vector3 interMax = NewMax - intersection;
            float dot1 = Vector3.Dot(Left, interMin);
            float dot2 = Vector3.Dot(Up, interMin);
            float dot3 = Vector3.Dot(Left, interMax);
            float dot4 = Vector3.Dot(Up, interMax);
            if (dot1 > 0 || dot2 > 0 ||
                dot3 < 0 || dot4 < 0)
                return mouvement; // Intersection is not in plane boundary

            // Intersection is happenning
            Vector3 allowedMouv = intersection - initialPosition;
            Vector3 allowedMouvTowardPlane = Vector3.Dot(Normal, allowedMouv) * Normal;
            Vector3 mouvementSnapOnPlane = mouvement - Vector3.Dot(Normal, mouvement) * Normal;

            return mouvementSnapOnPlane + allowedMouvTowardPlane;
        }
    }
}
