using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace DirectEngine.Cameras
{
    public class TargetCamera : Camera
    {
        private Vector3 target;
        private Vector3 up;
        private Vector3 forward;
        private Vector3 right;
        public override Vector3 Position
        {
            get { return position; }
            set
            {
                if (position != value)
                {
                    UpdateRightUpForward();
                    needRecalculView = true;
                }
                position = value;
            }
        }
        public Vector3 Target
        {
            get { return target; }
            set
            {
                if (target != value)
                {
                    UpdateRightUpForward();
                    needRecalculView = true;
                }
                target = value;
            }
        }
        public Vector3 Up
        {
            get { return up; }
            set
            {
                if (up != value)
                {
                    UpdateRightUpForward();
                    needRecalculView = true;
                }
                up = value;
                up.Normalize();
            }
        }
        public bool LockUpVector { get; set; }
        public Vector3 Forward { get { return forward; } }
        public Vector3 Right { get { return right; } }

        public void Zoom(float zoom)
        {
            Vector3 oldForward = Target - Position;
            oldForward.Normalize();
            Vector3 newPos = Position + oldForward * zoom;
            Vector3 futurForward = Target - newPos;
            float k = 0;
            if (futurForward.X != 0)
                k = oldForward.X / futurForward.X;
            else if (futurForward.Y != 0)
                k = oldForward.Y / futurForward.Y;
            else if (futurForward.Z != 0)
                k = oldForward.Z / futurForward.Z;
            if (k <= 0)
                newPos = Target - oldForward * 0.001f;
            Position = newPos;
        }
        public void GoLeft(float val, bool isAngle = true)
        {
            if (!isAngle)
                Position += Right * val;
            else
            {
                Vector4 v = Vector3.Transform(Position, Matrix.Translation(-Target) * Matrix.RotationAxis(Up, val) * Matrix.Translation(Target));
                Position = new Vector3(v.X, v.Y, v.Z);
            }
        }
        public void GoUp(float val, bool isAngle = true)
        {
            if (!isAngle)
                Position += Up * val;
            else
            {
                Vector4 v = Vector3.Transform(Position, Matrix.Translation(-Target) * Matrix.RotationAxis(Right, val) * Matrix.Translation(Target));
                Position = new Vector3(v.X, v.Y, v.Z);
            }
        }
        public void Roll(float angle)
        {
            Vector4 v = Vector3.Transform(Up, Matrix.RotationAxis(Forward, angle));
            Up = new Vector3(v.X, v.Y, v.Z);
            //right = Vector3.Cross(Up, Forward);
        }

        public TargetCamera(Vector3 position, Vector3 target, Vector3 up, Viewport viewport, float fieldOfView = 0.4f * (float)Math.PI, float near = 0.1f, float far = 1000)
            : base(viewport, fieldOfView, near, far)
        {
            this.position = position;
            this.target = target;
            this.up = up;
            LockUpVector = false;
            UpdateRightUpForward();
        }

        protected override void CalculViewMatrix()
        {
            viewMatrix = Matrix.LookAtLH(position, target, up);
        }

        protected void UpdateRightUpForward()
        {
            forward = target - position;
            forward.Normalize();
            right = Vector3.Cross(up, forward);
            right.Normalize();
            if (!LockUpVector)
                up = Vector3.Cross(forward, right);
        }
    }
}
