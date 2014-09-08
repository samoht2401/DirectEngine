using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using DirectEngine.Logic;

namespace DirectEngine.Rooms
{
    public class Room : DrawableObj, ICompositeDrawableObj
    {
        private Dictionary<string, DrawableObj> objDict;
        private Dictionary<string, ILogicObj> logicDict;
        private Dictionary<string, Portal> portalsDict;
        private List<BoundingBox> boundBoxList;

        public string Name { get; internal set; }

        // Override original to make sure nothing change the worldmatrix wich may only be change by other rooms
        public override Vector3 Position { get { return WorldMatrix.TranslationVector; } set { } }
        public override Vector4 Position4 { get { return new Vector4(WorldMatrix.TranslationVector, 1); } set { } }
        public override Vector3 Scale { get { return Vector3.One; } set { } }
        public override Quaternion Rotation { get { return Quaternion.Zero; } set { } }
        public override Matrix WorldMatrix { get { return worldMatrix; } }

        public Room(Game game, params BoundingBox[] bounds)
            : base(game)
        {
            objDict = new Dictionary<string, DrawableObj>();
            logicDict = new Dictionary<string, ILogicObj>();
            portalsDict = new Dictionary<string, Portal>();
            boundBoxList = new List<BoundingBox>();
            boundBoxList.AddRange(bounds);
        }

        public void AddObj(string name, DrawableObj obj)
        {
            objDict.Add(name, obj);
        }
        public void AddLogic(string name, ILogicObj obj)
        {
            logicDict.Add(name, obj);
        }
        public void AddPortal(Portal portal)
        {
            portalsDict.Add(portal.Name, portal);
            //objDict.Add("portal_" + (portalsDict.Count - 1), new DrawablePortal(game, portal, size));
        }

        public bool RemoveObj(string name)
        {
            return objDict.Remove(name);
        }
        public bool RemoveLogic(string name)
        {
            return objDict.Remove(name);
        }
        /*public bool RemovePortal(Room room)
        {
            objDict.Remove("portal_"+portalsDict.)
                portalsDict.Remove(room);
            return result&& objDict.
        }*/

        public List<Portal> GetPortals()
        {
            return new List<Portal>(portalsDict.Values);
        }

        internal void SetWorldMatrix(Matrix mat)
        {
            worldMatrix = mat;
        }

        public override void Draw(Matrix worldModif)
        {
            //ViewProj = viewProj;
            Draw_WorldMatrixCalculation();
            Matrix childWorldModif = worldModif * WorldMatrix;

            List<DrawableObj> list = new List<DrawableObj>(objDict.Values);
            list.Sort();
            foreach (DrawableObj toDraw in list)
                toDraw.Draw(childWorldModif);
        }

        public List<DrawableObj> GetDrawablePart()
        {
            return objDict.Values.ToList();
        }
    }
}
