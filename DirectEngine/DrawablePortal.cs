using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using DirectEngine.Rooms;

namespace DirectEngine
{
    public class DrawablePortal : DrawablePlane
    {

        public DrawablePortal(Game game, Portal portal)
            : base(game, null, null, false)
        {
            Vector4 size = portal.Size;
            float width = size.Z - size.X;
            float height = size.W - size.Y;
            Position = portal.Room1Position + new Vector3(size.X + width / 2, size.Y + height / 2, 0);
            float angleOfRot = (float)(Math.PI - Math.Acos(Vector3.Dot(Vector3.UnitZ, portal.Room1Normal)));
            Rotation = //Quaternion.RotationAxis(Vector3.UnitY, angleOfRot);
            Portal.GetRotationToAlign2To1(Vector3.UnitZ, portal.Room1Normal);
            Scale = new Vector3(width, height, 1);
        }

        /*protected override void Draw_Drawing()
        {
            //game.PushShader();
            base.Draw_Drawing();
            //game.PopShader();
        }*/
    }
}
