using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace DirectEngine.Cameras
{
    public class DirectionalCamera : Camera
    {
        private Vector3 direction;
        private Vector3 up;
        /*public override Vector3 Position
        {
            get { return position; }
            set
            {
                if (position != value)
                {
                    needRecalculView = true;
                }
                position = value;
            }
        }*/
        public override Vector3 Forward { get { return Direction; } }
        public Vector3 Direction
        {
            get { return direction; }
            /*set
            {
                if (direction != value)
                {
                    needRecalculView = true;
                }
                direction = value;
                direction.Normalize();
            }*/
        }
        public Vector3 Up
        {
            get { return up; }
            set
            {
                if (up != value)
                {
                    needRecalculView = true;
                }
                up = value;
                up.Normalize();
            }
        }
        private float yaw;
        private float pitch;
        public float Yaw { get { return yaw; } }
        public float Pitch { get { return pitch; } }
        public float MinYaw { get; set; }
        public float MaxYaw { get; set; }
        public float MinPitch { get; set; }
        public float MaxPitch { get; set; }

        public DirectionalCamera(Vector3 position, Vector3 direction, Vector3 up, Viewport viewport, float fieldOfView = 0.4f * (float)Math.PI, float near = 0.01f, float far = 1000)
            : base(viewport, fieldOfView, near, far)
        {
            Position = position;
            //this.direction = direction;
            Up = up;
            MinYaw = float.NegativeInfinity;
            MaxYaw = float.PositiveInfinity;
            MinPitch = (float)(5 * -Math.PI / 12);
            MaxPitch = (float)(5 * Math.PI / 12);
        }

        public void RotViewXY(Vector2 angles)
        {
            yaw += angles.X;
            yaw = MathUtil.Clamp(yaw, MinYaw, MaxYaw);
            pitch += angles.Y;
            pitch = MathUtil.Clamp(pitch, MinPitch, MaxPitch);
            Quaternion q = Quaternion.RotationYawPitchRoll(yaw, pitch, 0);
            direction = Vector3.Transform(Vector3.UnitZ, q);
            needRecalculView = true;
        }

        public void MoveForward(float val)
        {
            Position += direction * val;
        }
        public void MoveLeft(float val)
        {
            Position += Vector3.Cross(direction, up) * val;
        }

        protected override void CalculViewMatrix()
        {
            viewMatrix = Matrix.LookAtLH(position, position + direction, up);
        }
    }
}
