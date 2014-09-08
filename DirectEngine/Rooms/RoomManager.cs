using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using DirectEngine.Cameras;

namespace DirectEngine.Rooms
{
    public class RoomManager
    {
        private Game game;
        private Dictionary<string, Room> roomsDict;
        private Dictionary<Portal, DrawablePortal> drawPortalDict;
        private Room roomIn;

        public RoomManager(Game game)
        {
            this.game = game;
            roomsDict = new Dictionary<string, Room>();
            drawPortalDict = new Dictionary<Portal, DrawablePortal>();
        }

        public void AddRoom(string name, Room room)
        {
            room.Name = name;
            roomsDict.Add(name, room);
            if (roomIn == null)
                roomIn = room;
        }

        public Room GetRoom(string name)
        {
            return roomsDict[name];
        }

        public void SetRoomIn(string name)
        {
            roomIn = roomsDict[name];
        }

        private bool haveJustTp = false;
        public void UpdatePointOfView(Camera pointOfView)
        {
            bool noCollision = true;
            Matrix trans = roomIn.WorldMatrix;
            trans.Invert();
            Ray ray = new Ray(Vector3.TransformCoordinate(pointOfView.DisplacementRay.Position, trans), Vector3.TransformNormal(pointOfView.DisplacementRay.Direction, trans));
            if (ray.Direction != Vector3.Zero)
            {
                foreach (Portal portal in roomIn.GetPortals())
                {
                    float dist;
                    if (portal.IntersectRay(ray, out dist))
                    {
                        if (dist <= pointOfView.DisplacementDistance + MathUtil.ZeroTolerance)
                        {
                            if (!haveJustTp)
                            {
                                Matrix oldRoomInWMat = roomIn.WorldMatrix;
                                roomIn = GetRoom(portal.Room2Name);
                                roomIn.SetWorldMatrix(portal.GetTransRoom2RealtiveToRoom1() * oldRoomInWMat);
                                haveJustTp = true;
                                break;
                            }
                        }
                        noCollision = false;
                    }
                }
            }
            if (noCollision)
                haveJustTp = false;
        }

        public void MakeRender()
        {
            if (roomIn == null)
                return;

            Vector4 oldClip = game.BasicShader.ClipPlane;

            /* Matrix invertRoomInWorld = roomIn.WorldMatrix;
             invertRoomInWorld.Invert();
             Vector3 positionInRoomIn = Vector3.TransformCoordinate(game.Camera.Position, invertRoomInWorld);
             Vector3 directionInRoomIn = Vector3.TransformCoordinate(game.Camera.Forward, invertRoomInWorld);*/

            //Dictionary<Portal, Room> toRenderRoom = new Dictionary<Portal, Room>();
            foreach (Portal portal in roomIn.GetPortals())
            {
                if (!portal.IntersectFrustrum(game.Camera.Frustrum, game.Camera.Position, roomIn.WorldMatrix))
                    continue;

                Room linkedTo = GetRoom(portal.Room2Name);
                linkedTo.SetWorldMatrix(portal.GetTransRoom2RealtiveToRoom1() * roomIn.WorldMatrix);
                //toRenderRoom.Add(portal, linkedTo);

                DrawablePortal drawPortal;
                if (drawPortalDict.ContainsKey(portal))
                    drawPortal = drawPortalDict[portal];
                else
                {
                    drawPortal = new DrawablePortal(game, portal);
                    drawPortalDict.Add(portal, drawPortal);
                }

                // Make the clipPlane
                Vector3 posTrans = Vector3.TransformCoordinate(portal.Room1Position, roomIn.WorldMatrix);
                Vector3 normTrans = Vector3.TransformNormal(portal.Room1Normal, roomIn.WorldMatrix);
                normTrans.Normalize();
                float dot = Vector3.Dot(new Vector3(normTrans.X, normTrans.Y, normTrans.Z), new Vector3(posTrans.X, posTrans.Y, posTrans.Z));
                Vector4 clipPlane1 = new Vector4(normTrans, -dot);

                RenderToTexture renderTo = new RenderToTexture(game, game.Viewport.Width, game.Viewport.Height, true);
                renderTo.ClearRenderTarget(Color.Black);
                renderTo.SetAsRenderTarget();

                // Récurance
                // Dictionary<Portal, Room> toRenderRoom2 = new Dictionary<Portal, Room>();
                foreach (Portal portal2 in linkedTo.GetPortals())
                {
                    if (!portal2.IntersectFrustrum(game.Camera.Frustrum, game.Camera.Position, linkedTo.WorldMatrix))
                        continue;

                    Room linkedTo2 = GetRoom(portal2.Room2Name);
                    Matrix oldLinkedTo2World = linkedTo2.WorldMatrix;
                    linkedTo2.SetWorldMatrix(portal2.GetTransRoom2RealtiveToRoom1() * linkedTo.WorldMatrix);
                    //toRenderRoom2.Add(portal2, linkedTo2);

                    DrawablePortal drawPortal2;
                    if (drawPortalDict.ContainsKey(portal2))
                        drawPortal2 = drawPortalDict[portal2];
                    else
                    {
                        drawPortal2 = new DrawablePortal(game, portal2);
                        drawPortalDict.Add(portal2, drawPortal2);
                    }

                    RenderToTexture renderTo2 = new RenderToTexture(game, game.Viewport.Width, game.Viewport.Height, true);
                    renderTo2.ClearRenderTarget(Color.Black);
                    renderTo2.SetAsRenderTarget();

                    // Make the clipPlane
                    Vector3 posTrans2 = Vector3.TransformCoordinate(portal2.Room1Position, linkedTo.WorldMatrix);
                    Vector3 normTrans2 = Vector3.TransformNormal(portal2.Room1Normal, linkedTo.WorldMatrix);
                    normTrans2.Normalize();
                    float dot2 = Vector3.Dot(new Vector3(normTrans2.X, normTrans2.Y, normTrans2.Z), new Vector3(posTrans2.X, posTrans2.Y, posTrans2.Z));

                    game.BasicShader.ClipPlane = new Vector4(normTrans2, -dot2);
                    game.PortalShader.ClipPlane = clipPlane1;

                    linkedTo2.Draw();
                    renderTo2.UnsetAsRenderTarget();

                    if (linkedTo2.Name == roomIn.Name)
                        linkedTo2.SetWorldMatrix(oldLinkedTo2World);
                    game.BindTexture(renderTo2);
                    game.PushShader(game.PortalShader);
                    drawPortal2.Draw(linkedTo.WorldMatrix);
                    game.PopShader();

                    renderTo2.Dispose();
                }// Fin récurance


                game.BasicShader.ClipPlane = new Vector4(normTrans, -dot);
                game.PortalShader.ClipPlane = new Vector4(0, 1, 0, 1);
                linkedTo.Draw();
                renderTo.UnsetAsRenderTarget();

                if (Input.InputManager.Keyboard.HasJustBeenPressed(Input.Key.Y))
                    renderTo.SaveToFile("portal2.png");

                game.BindTexture(renderTo);
                game.PushShader(game.PortalShader);
                drawPortal.Draw(roomIn.WorldMatrix);
                game.PopShader();

                renderTo.Dispose();
            }

            game.BasicShader.ClipPlane = oldClip;

            roomIn.Draw();
        }
    }
}
