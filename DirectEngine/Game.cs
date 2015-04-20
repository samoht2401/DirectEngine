using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using SharpDX.DirectInput;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using Resource = SharpDX.Direct3D11.Resource;
using DirectEngine.Texte;
using DirectEngine.Shaders;
using DirectEngine.Input;
using DirectEngine.Cameras;

namespace DirectEngine
{
    public abstract class Game : IDisposable
    {
        public Font BaseFont;

        RenderForm form;
        Device device;
        SwapChainDescription swapChainDesc;
        SwapChain swapChain;
        DeviceContext context;
        Factory factory;
        Texture2D backBuffer;
        public DepthStencilView depthStencilView;
        Texture2D depthStencilBuffer;
        RenderTargetView renderView;

        BlendState blendStateTransparency;
        DepthStencilState withZBuffer;
        DepthStencilState withoutZBuffer;
        RasterizerState CCWcullMode;
        RasterizerState CWcullMode;

        //Texture2D texture;

        public System.Diagnostics.Stopwatch clock;

        Dictionary<string, DrawableObj> drawableObj;

        public RenderForm Form { get { return form; } }
        public Device Device { get { return device; } }
        public DeviceContext Context { get { return context; } }

        BasicShader basicShader;
        public BasicShader BasicShader { get { return basicShader; } }
        _2DShader _2dShader;
        public _2DShader _2DShader { get { return _2dShader; } }
        FontShader fontShader;
        public FontShader FontShader { get { return fontShader; } }
        PortalShader portalShader;
        public PortalShader PortalShader { get { return portalShader; } }

        public Viewport Viewport { get; protected set; }
        public Camera Camera { get; protected set; }

        public Game(string shaderFile)
        {
            SharpDX.Direct3D.PixHelper.AllowProfiling(true);

            form = new RenderForm("SharpDX - MiniTri Direct3D 11 Sample");

            // SwapChain description
            swapChainDesc = new SwapChainDescription()
            {
                BufferCount = 1,
                ModeDescription =
                    new ModeDescription(form.ClientSize.Width, form.ClientSize.Height,
                                        new Rational(60, 1), Format.R8G8B8A8_UNorm),
                IsWindowed = true,
                OutputHandle = form.Handle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            };

            // Create Device and SwapChain
            Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.None, swapChainDesc, out device, out swapChain);
            context = device.ImmediateContext;

            // Ignore all windows events
            factory = swapChain.GetParent<Factory>();
            factory.MakeWindowAssociation(form.Handle, WindowAssociationFlags.IgnoreAll);

            // New RenderTargetView from the backbuffer
            backBuffer = Texture2D.FromSwapChain<Texture2D>(swapChain, 0);
            renderView = new RenderTargetView(device, backBuffer);

            // Depth buffer creation
            depthStencilBuffer = new Texture2D(device, new Texture2DDescription()
            {
                Format = Format.D32_Float_S8X24_UInt,
                ArraySize = 1,
                MipLevels = 1,
                Width = form.ClientSize.Width,
                Height = form.ClientSize.Height,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            });
            depthStencilView = new DepthStencilView(device, depthStencilBuffer);

            // Create default shaders
            basicShader = new BasicShader(this);
            LinkBasicShader();
            _2dShader = new _2DShader(this);
            fontShader = new FontShader(this);
            portalShader = new PortalShader(this);

            // Prepare All the stages
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            Viewport = new Viewport(0, 0, form.ClientSize.Width, form.ClientSize.Height, 0.0f, 1.0f);
            context.Rasterizer.SetViewport(Viewport);
            context.OutputMerger.SetTargets(depthStencilView, renderView);

            // Blending
            RenderTargetBlendDescription renderTargetBlendDesc = new RenderTargetBlendDescription()
            {
                IsBlendEnabled = true,
                SourceBlend = BlendOption.SourceColor,
                DestinationBlend = BlendOption.InverseBlendFactor,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.Zero,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            };
            BlendStateDescription blendDesc = new BlendStateDescription() { AlphaToCoverageEnable = true };
            blendDesc.RenderTarget[0] = renderTargetBlendDesc;
            blendStateTransparency = new BlendState(device, blendDesc);

            // ClockWise and CounterClockWise States
            RasterizerStateDescription rasterizerDesc = new RasterizerStateDescription()
            {
                FillMode = FillMode.Solid,
                CullMode = CullMode.Back,
                IsFrontCounterClockwise = true
            };
            CCWcullMode = new RasterizerState(device, rasterizerDesc);
            rasterizerDesc.IsFrontCounterClockwise = false;
            CWcullMode = new RasterizerState(device, rasterizerDesc);

            // ZBuffer
            DepthStencilStateDescription zBufferDesc = new DepthStencilStateDescription()
            {
                IsDepthEnabled = true,
                DepthWriteMask = DepthWriteMask.All,
                DepthComparison = Comparison.Less,
            };
            withZBuffer = new DepthStencilState(device, zBufferDesc);
            zBufferDesc.IsDepthEnabled = false;
            withoutZBuffer = new DepthStencilState(device, zBufferDesc);

            form.Resize += form_Resize;

            InputManager.Initialize();

            drawableObj = new Dictionary<string, DrawableObj>();

            Camera = new TargetCamera(new Vector3(0, 0, -2f), new Vector3(0, 0, 4), Vector3.Up, Viewport);

            clock = new System.Diagnostics.Stopwatch();
            clock.Start();

            BaseFont = Font.LoadFont(this, "base");
        }
        bool needToResize = false;
        void form_Resize(object sender, EventArgs e) { needToResize = true; }

        public void Dispose()
        {
            foreach (DrawableObj toDispose in drawableObj.Values)
                toDispose.Dispose();

            InputManager.Unitialize();

            Utilities.Dispose(ref basicShader);
            Utilities.Dispose(ref renderView);
            Utilities.Dispose(ref depthStencilView);
            Utilities.Dispose(ref depthStencilBuffer);
            Utilities.Dispose(ref backBuffer);
            context.ClearState();
            context.Flush();
            Utilities.Dispose(ref device);
            Utilities.Dispose(ref context);
            Utilities.Dispose(ref swapChain);
            Utilities.Dispose(ref factory);
            Utilities.Dispose(ref blendStateTransparency);
            Utilities.Dispose(ref CCWcullMode);
            Utilities.Dispose(ref CWcullMode);
        }

        public void Run()
        {
            TimeSpan Elapsed, TotalTime;
            Stopwatch totalStopWatch = new Stopwatch();
            Stopwatch stopWatch = new Stopwatch();
            totalStopWatch.Start();
            stopWatch.Start();
            // Main loop
            RenderLoop.Run(form, () =>
            {
                TotalTime = totalStopWatch.Elapsed;
                Elapsed = stopWatch.Elapsed;
                stopWatch.Restart();
                if (needToResize)
                    Resize();
                Draw(Elapsed, TotalTime);
                Update(Elapsed, TotalTime);
            });
        }
        public void Quit()
        {
            form.Close();
        }

        public virtual void Update(TimeSpan elapsed, TimeSpan totalElapsed)
        {
            InputManager.UpdateState();
            Camera.Update();
        }
        public virtual void Draw(TimeSpan elapsed, TimeSpan totalElapsed)
        {
            SetViewProjMatrix(Camera.ViewProjMatrix);
        }
        public virtual void Resize()
        {
            needToResize = false;
            Viewport = new Viewport(0, 0, form.ClientSize.Width, form.ClientSize.Height, 0.0f, 1.0f);

            // Dispose all previous allocated resources
            Utilities.Dispose(ref backBuffer);
            Utilities.Dispose(ref renderView);
            Utilities.Dispose(ref depthStencilBuffer);
            Utilities.Dispose(ref depthStencilView);

            // Resize the backbuffer
            swapChain.ResizeBuffers(swapChainDesc.BufferCount, form.ClientSize.Width, form.ClientSize.Height, Format.Unknown, SwapChainFlags.None);

            // Get the backbuffer from the swapchain
            backBuffer = Texture2D.FromSwapChain<Texture2D>(swapChain, 0);

            // Renderview on the backbuffer
            renderView = new RenderTargetView(device, backBuffer);

            // Create the depth buffer
            depthStencilBuffer = new Texture2D(device, new Texture2DDescription()
            {
                Format = Format.D32_Float_S8X24_UInt,
                ArraySize = 1,
                MipLevels = 1,
                Width = form.ClientSize.Width,
                Height = form.ClientSize.Height,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            });

            // Create the depth buffer view
            depthStencilView = new DepthStencilView(device, depthStencilBuffer);

            // Setup targets and viewport for rendering
            context.Rasterizer.SetViewport(Viewport);
            context.OutputMerger.SetTargets(depthStencilView, renderView);
            _2DShader.ScreenSize = new Vector2(Viewport.Width, Viewport.Height);
            PortalShader.ScreenSize = new Vector2(Viewport.Width, Viewport.Height);
            Camera.Viewport = Viewport;
        }

        public void RegisterDrawableObj(string name, DrawableObj obj)
        {
            drawableObj.Add(name, obj);
        }
        public DrawableObj GetDrawableObj(string name)
        {
            if (drawableObj.ContainsKey(name))
                return drawableObj[name];
            return null;
        }

        private bool currentBlendingStateUseTransparency = false;
        private Color4? currentBlendFactor = null;
        public void SetBlendingState(bool useTransparency, Color4? blendFactor)
        {
            if (useTransparency == currentBlendingStateUseTransparency && (currentBlendFactor == blendFactor || !useTransparency))
                return;
            // Opaque
            if (!useTransparency)
                context.OutputMerger.SetBlendState(null, null, 0xffffffff);
            //Transparent
            else
                context.OutputMerger.SetBlendState(blendStateTransparency, blendFactor, 0xffffffff);
            currentBlendingStateUseTransparency = useTransparency;
        }
        private bool currentCullingStateCCW = true;
        public void SetCullingDirection(bool counterClockWise)
        {
            if (counterClockWise == currentCullingStateCCW)
                return;
            // ClockWise
            if (!counterClockWise)
                context.Rasterizer.State = CWcullMode;
            //CounterClockWise
            else
                context.Rasterizer.State = CCWcullMode;
            currentCullingStateCCW = counterClockWise;
        }

        private bool currentDepthStateZBuffer = true;
        public void SetZBufferUsageState(bool useZBuffer)
        {
            if (useZBuffer == currentDepthStateZBuffer)
                return;
            // Opaque
            if (useZBuffer)
                context.OutputMerger.SetDepthStencilState(withZBuffer);
            //Transparent
            else
                context.OutputMerger.SetDepthStencilState(withoutZBuffer);
            currentDepthStateZBuffer = useZBuffer;
        }

        private Dictionary<string, Texture2D> textureDict = new Dictionary<string, Texture2D>();
        public void LoadTexture2D(string name)
        {
            if (textureDict.ContainsKey(name))
                return;
            Texture2D result = Texture2D.FromFile<Texture2D>(device, name);
            textureDict.Add(name, result);
        }
        public bool IsTexture2D(string name)
        {
            return textureDict.ContainsKey(name);
        }
        public Vector2 GetTexture2DSize(string name)
        {
            if (!IsTexture2D(name))
                return Vector2.Zero;
            Texture2D texture = textureDict[name];
            return new Vector2(texture.Description.Width, texture.Description.Height);
        }
        private Dictionary<string, ShaderResourceView> textureViewDict = new Dictionary<string, ShaderResourceView>();
        private string[] currentBindedTexture = new string[32];
        public void BindTexture(string name, int slot = 0)
        {
            if (currentBindedTexture[slot] == name)
                return;
            if (textureViewDict.ContainsKey(name))
                BindTexture(textureViewDict[name], name, slot);
            else
            {
                if (!textureDict.ContainsKey(name))
                    LoadTexture2D(name);
                ShaderResourceView view = new ShaderResourceView(device, textureDict[name]);
                textureViewDict.Add(name, view);
                BindTexture(view, name, slot);
            }
        }
        public void BindTexture(RenderToTexture texture, int slot = 0)
        {
            BindTexture(texture.GetShaderResourceView(), texture.ToString(), slot);
        }
        protected void BindTexture(ShaderResourceView view, string name, int slot = 0)
        {
            context.PixelShader.SetShaderResource(slot, view);
            currentBindedTexture[slot] = name;
        }
        public void SaveTextureToFile(string name, string path)
        {
            Texture2D.ToFile(Context, textureDict[name], ImageFileFormat.Png, path);
        }

        public void SetScreenAsRenderTarget()
        {
            //context.OutputMerger.SetTargets(depthStencilView, renderView);
            PushAsRenderTarget(renderView, depthStencilView);
        }
        private Stack<KeyValuePair<RenderTargetView, DepthStencilView>> renderViewsStack = new Stack<KeyValuePair<RenderTargetView, DepthStencilView>>();
        public void PushAsRenderTarget(RenderTargetView renderTargetView, DepthStencilView depthStencilView = null)
        {
            if (renderTargetView == null && depthStencilView == null)
                return;
            RenderTargetView render = renderTargetView;
            DepthStencilView depth = depthStencilView;
            if (renderViewsStack.Count > 0)
            {
                if (render == null)
                    render = renderViewsStack.Peek().Key;
                if (depth == null)
                    depth = renderViewsStack.Peek().Value;
            }
            else
            {
                if (render == null)
                    render = this.renderView;
                if (depth == null)
                    depth = this.depthStencilView;
            }
            renderViewsStack.Push(new KeyValuePair<RenderTargetView, DepthStencilView>(render, depth));
            context.OutputMerger.SetTargets(depth, render);
        }
        public void PopRenderTarget()
        {
            if (renderViewsStack.Count > 0)
                renderViewsStack.Pop();
            if (renderViewsStack.Count > 0)
                context.OutputMerger.SetTargets(renderViewsStack.Peek().Value, renderViewsStack.Peek().Key);
            else
                context.OutputMerger.SetTargets(depthStencilView, renderView);
        }
        private Stack<Viewport> viewportStack = new Stack<Viewport>();
        public void PushViewport(Viewport viewport)
        {
            /*if (viewportStack.Count > 0 && viewportStack.Peek() == viewport)
                return;*/
            viewportStack.Push(viewport);
            context.Rasterizer.SetViewport(viewport);
            _2DShader.ScreenSize = new Vector2(viewport.Width, viewport.Height);
            PortalShader.ScreenSize = new Vector2(viewport.Width, viewport.Height);
        }
        public void PopViewport()
        {
            if (viewportStack.Count > 0)
                viewportStack.Pop();
            if (viewportStack.Count > 0)
            {
                Viewport view = viewportStack.Peek();
                context.Rasterizer.SetViewport(view);
                _2DShader.ScreenSize = new Vector2(view.Width, view.Height);
                PortalShader.ScreenSize = new Vector2(view.Width, view.Height);
            }
            else
            {
                context.Rasterizer.SetViewport(Viewport);
                _2DShader.ScreenSize = new Vector2(Viewport.Width, Viewport.Height);
                PortalShader.ScreenSize = new Vector2(Viewport.Width, Viewport.Height);
            }
        }

        public void LinkBasicShader()
        {
            PushShader(basicShader);
        }
        private Stack<Shader> shaderStack = new Stack<Shader>();
        public void PushShader(Shader shader)
        {
            if (shaderStack.Count > 0 && shader == shaderStack.Peek())
                return;

            shaderStack.Push(shader);
            shader.LinkShader();
            shader.SetViewProjMatrix(viewProjMat);
        }
        public void PopShader()
        {
            shaderStack.Pop();
            if (shaderStack.Count > 0)
            {
                shaderStack.Peek().LinkShader();
                shaderStack.Peek().SetViewProjMatrix(viewProjMat);
            }
            else
                LinkBasicShader();
        }

        private Matrix viewProjMat = Matrix.Identity;
        public virtual void SetViewProjMatrix(Matrix matrix)
        {
            viewProjMat = matrix;
            if (shaderStack.Count > 0)
                shaderStack.Peek().SetViewProjMatrix(matrix);
            else
                BasicShader.SetViewProjMatrix(matrix);
        }
        public virtual void SetWorldMatrix(Matrix matrix)
        {
            if (shaderStack.Count > 0)
                shaderStack.Peek().SetWorldMatrix(matrix);
            else
                BasicShader.SetWorldMatrix(matrix);
        }

        protected void ClearScreen(Color color, RenderTargetView renderView_ = null, DepthStencilView depthView = null)
        {
            if (depthView == null)
                context.ClearDepthStencilView(depthStencilView, DepthStencilClearFlags.Depth, 1.0f, 0);
            else
                context.ClearDepthStencilView(depthView, DepthStencilClearFlags.Depth, 1.0f, 0);
            if (renderView == null)
                context.ClearRenderTargetView(this.renderView, color);
            else
                context.ClearRenderTargetView(renderView, color);
        }
        public void ClearDepthBuffer(DepthStencilView depthView)
        {
            context.ClearDepthStencilView(depthView, DepthStencilClearFlags.Depth, 1.0f, 0);
        }
        protected void RenderRegisteredObj()
        {
            List<DrawableObj> list = new List<DrawableObj>(drawableObj.Values);
            list.Sort();
            foreach (DrawableObj toDraw in list)
                toDraw.Draw(Camera.ViewProjMatrix);
        }
        protected void Present()
        {
            //Texture2D.ToFile(context, backBuffer, ImageFileFormat.Png, "screen.png");
            swapChain.Present(0, PresentFlags.None);
        }
    }
}
