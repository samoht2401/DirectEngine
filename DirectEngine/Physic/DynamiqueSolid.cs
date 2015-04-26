using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using DirectEngine.Logic;

namespace DirectEngine.Physic
{
    public class DynamiqueSolid : ILogicObj // Are sphere
    {
        private const float airFriction = 0.1f;

        public PhysicManager Manager { get; protected set; }

        private float radius;
        public float Radius { get { return radius; } set { radius = value; } }
        public float FrictionCoefficient { get; set; }
        private Vector3 position;
        public Vector3 Position { get { return position; } }
        private Vector3 velocity;
        public Vector3 Velocity { get { return velocity; } set { velocity = value; } }
        private Dictionary<string, Vector3> accelerations;

        public DynamiqueSolid(Vector3 initialPos, float radius, Vector3 velocity, float friction = 10f)
        {
            position = initialPos;
            Radius = radius;
            Velocity = velocity;
            accelerations = new Dictionary<String, Vector3>();
            FrictionCoefficient = friction;
        }

        internal void LinkToManager(PhysicManager manager)
        {
            Manager = manager;
        }
        internal void UnLinkManager()
        {
            Manager = null;
        }

        public void SetAcceleration(string key, Vector3 acceleration)
        {
            if (acceleration == Vector3.Zero && accelerations.ContainsKey(key))
                accelerations.Remove(key);
            else
                accelerations[key] = acceleration;
        }

        public void Update(TimeSpan elapsed, TimeSpan totalTime)
        {
            if (Manager == null)
                return;
            float e = (float)elapsed.TotalSeconds;
/*
            // Compute air friction acceleration
            //Vector3 airAcc = -Velocity;

            // Compute accelerations
            Vector3 acceleration = Vector3.Zero;
            foreach (Vector3 acc in accelerations.Values)
                acceleration += acc;

            // Compute new velocity
            if (Velocity != Vector3.Zero)
                Velocity = (1 - e * FrictionCoefficient * airFriction / Velocity.Length()) * Velocity + e * acceleration;
            else
                Velocity += e * acceleration;

            // Compute collision
            CollisionResult collision = Manager.ComputeCollision(Position, e * Velocity, Radius);
            // Correct velocity
            Vector3 remainingVelocity = Velocity - collision.ActualMouvement / e;
            foreach (SolidPlane plane in collision.CollidedWith)
            {
                remainingVelocity -= Vector3.Dot(remainingVelocity, plane.Normal) * plane.Normal;
                if (remainingVelocity != Vector3.Zero)
                    remainingVelocity *= 1 - e * plane.FrictionCoefficient * FrictionCoefficient / remainingVelocity.Length();
            }
            Velocity = collision.ActualMouvement / e + remainingVelocity;*/

            Velocity = Vector3.Zero;
            foreach (Vector3 acc in accelerations.Values)
                Velocity += acc;

            // Compute collision
            CollisionResult collision = Manager.ComputeCollision(Position, e * Velocity, Radius);
            Velocity = collision.ActualMouvement / e ;

            // Mouve
            position = Position + e * Velocity;

            //System.Console.WriteLine(Velocity.ToString());
        }
    }
}
