using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace DirectEngine.Cameras
{
    public abstract class Camera
    {
        protected bool needRecalculView;
        protected Matrix viewMatrix;
        public Matrix ViewMatrix
        {
            get
            {
                if (needRecalculView)
                {
                    CalculViewMatrix();
                    needRecalculView = false;
                    needRecalculViewProj = true;
                }
                return viewMatrix;
            }
        }

        protected Viewport viewport;
        protected float fieldOfView;
        protected float near;
        protected float far;
        public Viewport Viewport { get { return viewport; } set { if (viewport != value) needRecalculProjection = true; viewport = value; } }
        public float FieldOfView { get { return fieldOfView; } set { if (fieldOfView != value)needRecalculProjection = true; fieldOfView = value; } }
        public float Near { get { return near; } set { if (near != value)needRecalculProjection = true; near = value; } }
        public float Far { get { return far; } set { if (far != value)needRecalculProjection = true; far = value; } }

        protected Vector3 prevPosition;
        protected Vector3 position;
        public virtual Vector3 PrevPosition
        {
            get { return prevPosition; }
        }
        public virtual Vector3 Position
        {
            get { return position; }
            set { if (position != value) needRecalculView = true; /*prevPosition = position;*/ position = value; }
        }
        public virtual Ray DisplacementRay { get { Vector3 dir = position - prevPosition; dir.Normalize(); return new Ray(prevPosition, dir); } }
        public virtual float DisplacementDistance { get { return (position - prevPosition).Length(); } }
        protected Vector3 forward;
        public virtual Vector3 Forward { get { return forward; } }

        private bool needRecalculProjection;
        protected Matrix projectionMatrix;
        public Matrix ProjectionMatrix
        {
            get
            {
                if (needRecalculProjection)
                {
                    CalculProjectionMatrix();
                    needRecalculProjection = false;
                    needRecalculViewProj = true;
                }
                return projectionMatrix;
            }
        }

        protected bool needRecalculViewProj;
        protected Matrix viewProjMatrix;
        public Matrix ViewProjMatrix
        {
            get
            {
                if (needRecalculViewProj || needRecalculProjection || needRecalculView)
                {
                    //Matrix m = ProjectionMatrix;
                    viewProjMatrix = ViewMatrix * ProjectionMatrix;
                    needRecalculViewProj = false;
                    frustrum = new BoundingFrustum(viewProjMatrix);
                }
                return viewProjMatrix;
            }
        }

        protected BoundingFrustum frustrum;
        public BoundingFrustum Frustrum
        {
            get { return frustrum; }
        }

        public Camera(Viewport viewport, float fieldOfView = 0.4f * (float)Math.PI, float near = 0.1f, float far = 1000)
        {
            needRecalculView = true;
            viewMatrix = ViewMatrix;

            this.viewport = viewport;
            this.fieldOfView = fieldOfView;
            this.near = near;
            this.far = far;
            needRecalculProjection = true;
            projectionMatrix = ProjectionMatrix;

            needRecalculViewProj = true;
            viewProjMatrix = ViewProjMatrix;
        }

        public void Update()
        {
            prevPosition = position;
        }

        protected abstract void CalculViewMatrix();
        protected virtual void CalculProjectionMatrix()
        {
            projectionMatrix = Matrix.PerspectiveFovLH(fieldOfView, viewport.AspectRatio, near, far);
        }
    }
}
