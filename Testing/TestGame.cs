using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;
using DirectEngine;
using DirectEngine.Texte;
using DirectEngine.Rooms;
using DirectEngine.Input;
using DirectEngine.Cameras;
using DirectEngine.Shaders;

namespace Testing
{
    public class TestGame : Game
    {
        RoomManager roomManager;
        Room room1;
        Room room2;
        Room room3;
        DrawableCube first;
        DrawableCube second;
        DrawablePlane ground;
        DrawableModel model1;
        DrawableModel model2;
        DrawableModel monkeyModel;
        RenderToTexture renderTexture;
        RenderToTexture textTexture;
        Portal portalTo2;
        Font font;
        DirectionalCamera cam;

        public TestGame()
            : base("BaseEffect.fx")
        {
            roomManager = new RoomManager(this);
            first = new DrawableCube(this, "supernova.jpg", null, true);
            second = new DrawableCube(this, "supernova.jpg", null, true);
            first.Position = new Vector3(4, 2, 0);
            model1 = new DrawableModel(this, "supernova.jpg", null, false, "room.obj");
            model2 = new DrawableModel(this, "Test.png", null, false, "room.obj");
            monkeyModel = new DrawableModel(this, "wood.jpg", "wood_n.jpg", false, "monkey.obj");
            monkeyModel.Position = new Vector3(4, 2, 0);
            room1 = new Room(this);
            room1.AddObj("model", model1);
            //room1.AddObj("first", first);
            //room1.AddObj("second", second);
            room2 = new Room(this);
            room2.AddObj("model", model2);
            //room2.AddObj("first", first);
            room2.AddObj("monkey", monkeyModel);
            room3 = new Room(this);
            room3.AddObj("model", model1);
            roomManager.AddRoom("room1", room1);
            roomManager.AddRoom("room2", room2);
            roomManager.AddRoom("room3", room3);
            //room2.AddObj("first", first);
            //RegisterDrawableObj("first", first);
            //RegisterDrawableObj("second", second);
            renderTexture = new RenderToTexture(this, Viewport.Width, Viewport.Height);
            font = Font.LoadFont(this, "base");
            textTexture = TextDrawer.GetTextTexture(font, new string[] { "Salut !", "Hello", "Como te llama ?", "Tu ne sais pas, qu'elle tristesse" });
            BasicShader.PushIsInLinearMode(true);

            Vector4 size = new Vector4(-1, 0, 1, 4);
            portalTo2 = new Portal(new Vector3(-2.005f, 0, 0), new Vector3(1, 0, 0), new Vector3(9.795f, 0, 0), new Vector3(1, 0, 0), "room2", size, "1");
            Portal portalTo22 = new Portal(new Vector3(2.005f, 0, 0), new Vector3(-1, 0, 0), new Vector3(-9.795f, 0, 0), new Vector3(-1, 0, 0), "room2", size, "2");
            Portal portalTo23 = new Portal(new Vector3(0, 0, -2.005f), new Vector3(0, 0, 1), new Vector3(0, 0, 9.795f), new Vector3(0, 0, 1), "room2", size, "3");
            Portal portalTo24 = new Portal(new Vector3(0, 0, 2.005f), new Vector3(0, 0, -1), new Vector3(0, 0, -9.795f), new Vector3(0, 0, -1), "room2", size, "4");
            room1.AddPortal(portalTo2);
            room1.AddPortal(portalTo22);
            room1.AddPortal(portalTo23);
            room1.AddPortal(portalTo24);
            room2.AddPortal(portalTo2.GetOppositePortal("room1"));
            room2.AddPortal(portalTo22.GetOppositePortal("room1"));
            room2.AddPortal(portalTo23.GetOppositePortal("room1"));
            room2.AddPortal(portalTo24.GetOppositePortal("room1"));

            //Portal portal2To3 = new Portal(new Vector3(2.01f, 0, 0), new Vector3(-1, 0, 0), new Vector3(9.7f, 0, 0), new Vector3(1, 0, 0), "room3", size);
            //room2.AddPortal(portal2To3);

            ground = new DrawablePlane(this, "supernova.jpg", null, false);
            ground.Rotation = Quaternion.RotationAxis(Vector3.UnitX, (float)Math.PI / 2);
            ground.Scale = Vector3.One * 50;

            cam = new DirectionalCamera(new Vector3(0, 0, -2), new Vector3(0, 0, 1), new Vector3(0, 1, 0), Viewport);
            Camera = cam;
            Camera.Position = Vector3.UnitZ * 6;
        }

        Vector2 lightAngle = Vector2.Zero;
        public override void Update(TimeSpan elapsed, TimeSpan totalElapsed)
        {
            base.Update(elapsed, totalElapsed);
            float elap = (float)elapsed.TotalMilliseconds;
            float e = (float)totalElapsed.TotalMilliseconds / 1000;

            //second.Position4 = Vector4.Transform(new Vector4(0, 0, 0, 1), Matrix.Translation(4, 0, 0) * Matrix.RotationY((float)Math.Pow(Math.Cos(e * 0.2f), 3) * (float)Math.PI + (float)Math.PI / 2f)) + new Vector4(0, 0, 4, 1);
            first.Rotation = Quaternion.RotationYawPitchRoll(0.1f * e, 0.2f * e, 0.4f * e);
            //second.Rotation = Quaternion.RotationYawPitchRoll(0.4f * e, 0.2f * e, 0.1f * e);
            //first.BlendFactor = new Color4((float)Math.Cos(e * 0.5f) * 0.5f + 0.5f, (float)Math.Cos(e) * 0.5f + 0.5f, (float)Math.Cos(e * 0.25f) * 0.5f + 0.5f, (float)Math.Cos(e * 2f) * 0.5f + 0.5f);

            float rot = (float)Math.Cos(e) * (float)Math.PI / 4 - (float)Math.PI / 4;
            Vector3 norm = Vector3.Zero;
            norm.X = (float)Math.Cos(rot);
            norm.Z = (float)Math.Sin(rot);
            //portalTo2.Room1Normal = norm;

            Keyboard keyboard = InputManager.Keyboard;
            Mouse mouse = InputManager.Mouse;
            //cam.RotViewXY(new Vector2(0.01f * -elap * 0.01f, 0 * -elap * 0.01f));
            cam.RotViewXY(new Vector2(mouse.GetOffsetX() * 0.01f, mouse.GetOffsetY() * 0.01f));
            if (keyboard.IsPressed(Key.W))
                cam.MoveForward(elap * 0.01f);
            if (keyboard.IsPressed(Key.S))
                cam.MoveForward(-elap * 0.01f);
            if (keyboard.IsPressed(Key.A))
                cam.MoveLeft(elap * 0.01f);
            if (keyboard.IsPressed(Key.D))
                cam.MoveLeft(-elap * 0.01f);
            Vector3 yAjust = cam.Position;
            yAjust.Y = 2;
            cam.Position = yAjust;

            roomManager.UpdatePointOfView(Camera);

            if (keyboard.IsPressed(Key.Up))
                lightAngle.Y += elap * 0.01f;
            if (keyboard.IsPressed(Key.Down))
                lightAngle.Y -= elap * 0.01f;
            if (keyboard.IsPressed(Key.Left))
                lightAngle.X += elap * 0.01f;
            if (keyboard.IsPressed(Key.Right))
                lightAngle.X -= elap * 0.01f;

            Quaternion q = Quaternion.RotationYawPitchRoll(lightAngle.X, lightAngle.Y, 0);
            BasicShader.Light = new BasicLight(q.Axis, Color.White);

            /*if (keyboard.IsPressed(Key.W))
                Camera.Zoom(elap * 0.01f);
            if (keyboard.IsPressed(Key.S))
                Camera.Zoom(-elap * 0.01f);
            if (keyboard.IsPressed(Key.A))
                Camera.GoLeft(-elap * 0.0025f);
            if (keyboard.IsPressed(Key.D))
            Camera.GoLeft(elap * 0.0025f);
            if (keyboard.IsPressed(Key.Space))
                Camera.GoUp(elap * 0.0025f);
            if (keyboard.IsPressed(Key.LeftShift))
                Camera.GoUp(-elap * 0.0025f);
            if (keyboard.IsPressed(Key.Q))
                Camera.Roll(elap * 0.0025f);
            if (keyboard.IsPressed(Key.E))
                Camera.Roll(-elap * 0.0025f);*/

            if (keyboard.IsPressed(Key.Escape))
                Quit();

        }

        float rot = 0;
        float[] fpsTab = new float[500];
        RenderToTexture fpsText;
        int fpsIndex = 500;
        public override void Draw(TimeSpan elapsed, TimeSpan totalElapsed)
        {
            base.Draw(elapsed, totalElapsed);

            ClearScreen(Color.IndianRed);
            //SetZBufferUsageState(false);
            //renderTexture.ClearRenderTarget(Context, depthStencilView, Color.White);
            //renderTexture.SetAsRenderTarget(this);
            //RenderRegisteredObj();
            //ground.Draw(Camera.ViewProjMatrix);
            //model1.Draw();
            roomManager.MakeRender();
            //List<DrawableObj> list = new List<DrawableObj>();
            //list.Add(room1);
            //list.Add(room2);
            //list.Sort();
            //room1.TransformLinkedRoom();
            //foreach (DrawableObj toDraw in list)
            //toDraw.Draw(Camera.ViewProjMatrix);
            //renderTexture.UnsetAsRenderTarget(this);
            //SetScreenAsRenderTarget();
            //ClearScreen(Color.AliceBlue);
            /*BindTexture(renderTexture.GetShaderResourceView(), "shader");
            DrawableCube cube = new DrawableCube(this, null, false);
            //cube.BlendFactor = Color.Cyan;
            cube.Position = new Vector3(0, 0, 4);
            cube.Scale = new Vector3(2, 2, 2);
            rot += (float)(elapsed.TotalMilliseconds / 5000);
            cube.Rotation += Quaternion.RotationAxis(Vector3.Up, rot);
            cube.Draw(Camera.ViewProjMatrix);*/

            PushShader(_2DShader);

            SetZBufferUsageState(false);
            if (fpsIndex >= 500)
            {
                float moyenne = 0;
                for (int i = 0; i < 500; i++)
                    moyenne += fpsTab[i];
                moyenne /= 500;
                fpsText = TextDrawer.GetTextTexture(font, new string[] { "FPS : " + (moyenne).ToString("F0") });
                fpsIndex = 0;
            }
            else
            {
                fpsTab[fpsIndex] = (float)(1000 / elapsed.TotalMilliseconds);
                fpsIndex++;
            }
            BindTexture(fpsText.GetShaderResourceView(), "fps");
            DrawablePlane plane = new DrawablePlane(this, null, null, false);
            //plane.Position = new Vector3(-Viewport.Width + 500, Viewport.Height - 100, 0);
            plane.Use2DPosModif = true;
            plane.Position = new Vector3(0, 0, 0);
            plane.Scale = new Vector3(fpsText.Width * 3, fpsText.Height * 3, 1);
            plane.Draw(Matrix.Identity);//Matrix.OrthoLH(Viewport.Width, Viewport.Height, 0, 1));
            //SetZBufferUsageState(true);

            RenderToTexture mouse = TextDrawer.GetTextTexture(font, new string[]{"X : " + InputManager.Mouse.GetX(),
                                                                                 "Y : " + InputManager.Mouse.GetY(),
                                                                                 "Z : " + InputManager.Mouse.GetOffsetX(),
                                                                                 "Xoff : " + InputManager.Mouse.GetOffsetX(),
                                                                                 "Yoff : " + InputManager.Mouse.GetOffsetY(),});
            BindTexture(mouse.GetShaderResourceView(), "mouse");
            plane = new DrawablePlane(this, null, null, false);

            plane.Use2DPosModif = true;
            plane.Position = new Vector3(0, -100, 0);
            plane.Scale = new Vector3(mouse.Width * 3, mouse.Height * 3, 1);
            plane.Draw();

            SetZBufferUsageState(true);


            PopShader();

            Present();

            SharpDX.Direct3D.PixHelper.EndEvent();
        }

        public override void Resize()
        {
            base.Resize();

            Utilities.Dispose(ref renderTexture);
            renderTexture = new RenderToTexture(this, Viewport.Width / 2, Viewport.Width / 2);
        }
    }
}
