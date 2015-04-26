using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using DirectEngine.Logic;

namespace DirectEngine.Physic
{
    public struct CollisionResult
    {
        public Vector3 ActualMouvement;
        public List<SolidPlane> CollidedWith;
    }

    public class PhysicManager : ILogicObj
    {
        private List<SolidPlane> planes;
        private Dictionary<string, DynamiqueSolid> dynamiqueSolids;

        public PhysicManager()
        {
            planes = new List<SolidPlane>();
            dynamiqueSolids = new Dictionary<string, DynamiqueSolid>();
        }

        public void AddSolidPlane(SolidPlane plane)
        {
            planes.Add(plane);
        }

        public void LinkDynamiqueSolid(string key, DynamiqueSolid solid)
        {
            solid.LinkToManager(this);
            dynamiqueSolids.Add(key, solid);
        }
        public void UnlinkDynamiqueSolid(string key)
        {
            DynamiqueSolid solid = dynamiqueSolids[key];
            if (solid != null)
                solid.UnLinkManager();
            dynamiqueSolids.Remove(key);
        }
        public void UnlinkDynamiqueSolid(DynamiqueSolid solid)
        {
            solid.UnLinkManager();
            dynamiqueSolids.Remove(dynamiqueSolids.First(x => x.Value == solid).Key);
        }

        public CollisionResult ComputeCollision(Vector3 position, Vector3 mouvement, float radius = 0.0f)
        {
            CollisionResult result;
            result.ActualMouvement = mouvement;
            result.CollidedWith = new List<SolidPlane>();
            int i = 0;
            foreach (SolidPlane plane in planes)
            {
                Vector3 newMouv = plane.TestAgainst(position, result.ActualMouvement, radius);
                if (newMouv != result.ActualMouvement)
                {
                    i++;
                    if (i > 1)
                        i = 0;
                    result.ActualMouvement = newMouv;
                    result.CollidedWith.Add(plane);
                }
            }
            return result;
        }

        public void Update(TimeSpan elapsed, TimeSpan totalTime)
        {
            foreach(DynamiqueSolid solid in dynamiqueSolids.Values)
            {
                solid.Update(elapsed, totalTime);;
            }
        }
    }
}
