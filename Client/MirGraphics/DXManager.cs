using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Client.MirControls;
using Client.MirScenes;
using SlimDX;
using SlimDX.Direct3D9;
using Blend = SlimDX.Direct3D9.Blend;

namespace Client.MirGraphics
{
    class DXManager
    {
        public static readonly ResourceRegistry<MImage> TextureList = new ResourceRegistry<MImage>(
            image => image.CleanTime, image => image.DisposeTexture(),
            image => (long)image.Width * image.Height * 4 * (image.HasMask ? 2 : 1));
        public static readonly ResourceRegistry<MirControl> ControlList = new ResourceRegistry<MirControl>(
            control => control.CleanTime, control => control.DisposeTexture());
        private static long _nextCleanTime;
        private static bool _cleanControlsNext;
        private static int _blendBatchDepth;
        private static bool _opacityOverride = true;

        public static Device Device;
        public static Sprite Sprite;
        public static Line Line;

        public static Surface CurrentSurface;
        public static Surface MainSurface;
        public static Surface OutputSurface;
        public static PresentationLayout Layout { get; private set; }
        private static SlimDX.Direct3D9.DisplayMode _desktopMode;
        private static Rectangle _desktopBounds;
        public static PresentParameters Parameters;
        public static bool DeviceLost;
        public static float Opacity = 1F;
        public static bool Blending;
        public static float BlendingRate;
        public static BlendMode BlendingMode;


        public static Texture RadarTexture;
        public static List<Texture> Lights = new List<Texture>();
        public static Texture PoisonDotBackground;

        public static Texture FloorTexture, LightTexture;
        public static Surface FloorSurface, LightSurface;

        public static PixelShader GrayScalePixelShader;
        public static PixelShader NormalPixelShader;
        public static PixelShader MagicPixelShader;

        public static bool GrayScale;

        public static Point[] LightSizes =
        {
            new Point(125,95),
            new Point(205,156),
            new Point(285,217),
            new Point(365,277),
            new Point(445,338),
            new Point(525,399),
            new Point(605,460),
            new Point(685,521),
            new Point(765,581),
            new Point(845,642),
            new Point(925,703)
        };

        public static void Create()
        {
            Parameters = new PresentParameters
            {
                BackBufferFormat = Format.X8R8G8B8,
                PresentFlags = PresentFlags.LockableBackBuffer,
                SwapEffect = SwapEffect.Discard,
                PresentationInterval = Settings.FPSCap ? PresentInterval.One : PresentInterval.Immediate,
                Windowed = Settings.Mode != DisplayMode.FullScreen,
            };


            Direct3D d3d = new Direct3D();
            ConfigurePresentation();

            Capabilities devCaps = d3d.GetDeviceCaps(0, DeviceType.Hardware);
            DeviceType devType = DeviceType.Reference;
            CreateFlags devFlags = CreateFlags.HardwareVertexProcessing;

            if (devCaps.VertexShaderVersion.Major >= 2 && devCaps.PixelShaderVersion.Major >= 2)
                devType = DeviceType.Hardware;

            if ((devCaps.DeviceCaps & DeviceCaps.HWTransformAndLight) != 0)
                devFlags = CreateFlags.HardwareVertexProcessing;


            if ((devCaps.DeviceCaps & DeviceCaps.PureDevice) != 0)
                devFlags |= CreateFlags.PureDevice;


            try
            {
                Device = new Device(d3d, d3d.Adapters.DefaultAdapter.Adapter, devType, Program.Form.Handle, devFlags, Parameters);
            }
            catch (Direct3D9Exception ex)
            {
                if (!TryWindowedFallback("Create", ex)) throw;
                ConfigurePresentation();
                Device = new Device(d3d, d3d.Adapters.DefaultAdapter.Adapter, devType, Program.Form.Handle, devFlags, Parameters);
            }

            Device.SetDialogBoxMode(true);

            // Shaders survive Reset and must also exist if the first canvas allocation needs a retry.
            LoadPixelsShaders();
            try
            {
                LoadTextures();
            }
            catch (Exception ex)
            {
                CleanUp();
                DeviceLost = true;
                NeedsReset = true;
                LogGraphicsDiagnostics("Initial display resources could not be created: " + ex);
                throw;
            }
            LogPresentationState("Create");
        }

        public static bool TryGetDesktopBounds(out Rectangle bounds)
        {
            bounds = _desktopBounds;
            // Do not mistake the active exclusive mode for the original desktop mode.
            if (Device != null && Parameters != null && !Parameters.Windowed && _desktopMode.Width > 0)
                return true;

            try
            {
                using (Direct3D d3d = new Direct3D())
                {
                    var adapter = d3d.Adapters.DefaultAdapter;
                    _desktopMode = adapter.CurrentDisplayMode;
                    if (_desktopMode.Width <= 0 || _desktopMode.Height <= 0) return false;

                    Screen screen = Screen.AllScreens.FirstOrDefault(s => s.DeviceName == adapter.Details.DeviceName)
                        ?? Screen.FromControl(Program.Form);
                    _desktopBounds = new Rectangle(screen.Bounds.Location, new Size(_desktopMode.Width, _desktopMode.Height));
                    bounds = _desktopBounds;
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogGraphicsDiagnostics("Desktop mode query failed: " + ex.GetType().Name + ": " + ex.Message);
                return false;
            }
        }

        private static void ConfigurePresentation()
        {
            Size outputSize = Program.Form == null ? Size.Empty : Program.Form.ClientSize;
            if (Settings.Mode == DisplayMode.FullScreen)
                outputSize = new Size(_desktopMode.Width, _desktopMode.Height);
            if (outputSize.Width <= 0 || outputSize.Height <= 0)
                throw new InvalidOperationException("The display output size is unavailable.");

            Parameters.Windowed = Settings.Mode != DisplayMode.FullScreen;
            Parameters.BackBufferWidth = outputSize.Width;
            Parameters.BackBufferHeight = outputSize.Height;
            Parameters.BackBufferFormat = Parameters.Windowed ? Format.X8R8G8B8 : _desktopMode.Format;
            Parameters.FullScreenRefreshRateInHertz = Parameters.Windowed ? 0 : _desktopMode.RefreshRate;
            Parameters.PresentationInterval = Settings.FPSCap ? PresentInterval.One : PresentInterval.Immediate;
        }

        private static bool TryWindowedFallback(string operation, Direct3D9Exception ex)
        {
            if (Settings.Mode == DisplayMode.Windowed || ex.ResultCode.Code == ResultCode.DeviceLost.Code ||
                ex.ResultCode.Code == ResultCode.DeviceNotReset.Code)
                return false;

            LogGraphicsDiagnostics(operation + " failed; falling back to windowed mode: " + ex);
            CMain.SetDisplayMode(DisplayMode.Windowed);
            return true;
        }

        public static void BeginFrame()
        {
            // Bind before Clear and Sprite.Begin so both use the logical target's dimensions.
            Device.SetRenderTarget(0, MainSurface);
            CurrentSurface = MainSurface;
            Device.Clear(ClearFlags.Target, Color.Black, 0, 0);
            Device.BeginScene();
            Sprite.Begin(SpriteFlags.AlphaBlend);
            ResetDrawingState();
        }

        private static void ResetDrawingState()
        {
            // Sprite.Begin establishes normal alpha blending. Keep the software cache in sync,
            // including after an interrupted nested draw or a recreated device.
            Opacity = 1F;
            _opacityOverride = true;
            Blending = false;
            BlendingRate = 1F;
            BlendingMode = BlendMode.NORMAL;
            GrayScale = false;
            _blendBatchDepth = 0;
            Device.PixelShader = null;
        }

        public static bool PresentFrame()
        {
            // Sprite.End and EndScene have already run; the copy does not use scene shaders.
            long started = GraphicsDiagnostics.Timestamp();
            try
            {
                if (MainSurface != OutputSurface)
                {
                    Device.SetRenderTarget(0, OutputSurface);
                    CurrentSurface = OutputSurface;
                    Device.Clear(ClearFlags.Target, Color.Black, 0, 0);
                    Device.StretchRectangle(MainSurface, new Rectangle(Point.Empty, Layout.LogicalSize),
                        OutputSurface, Layout.Viewport, TextureFilter.None);
                }

                Device.Present();
                return true;
            }
            catch (Direct3D9Exception ex)
            {
                if (!TryWindowedFallback("Present", ex)) throw;
                return false;
            }
            finally
            {
                GraphicsDiagnostics.Record(GraphicsMetric.Present, started);
            }
        }

        private static unsafe void LoadPixelsShaders()
        {
            var shaderNormalPath = Settings.ShadersPath + "normal.ps";
            var shaderGrayScalePath = Settings.ShadersPath + "grayscale.ps";
            var shaderMagicPath = Settings.ShadersPath + "magic.ps";

            if (System.IO.File.Exists(shaderNormalPath))
            {
                using (var gs = ShaderBytecode.AssembleFromFile(shaderNormalPath, ShaderFlags.None))
                    NormalPixelShader = new PixelShader(Device, gs);
            }
            if (System.IO.File.Exists(shaderGrayScalePath))
            {
                using (var gs = ShaderBytecode.AssembleFromFile(shaderGrayScalePath, ShaderFlags.None))
                    GrayScalePixelShader = new PixelShader(Device, gs);
            }
            if (System.IO.File.Exists(shaderMagicPath))
            {
                using (var gs = ShaderBytecode.AssembleFromFile(shaderMagicPath, ShaderFlags.None))
                    MagicPixelShader = new PixelShader(Device, gs);
            }
        }

        private static unsafe void LoadTextures()
        {
            Sprite = new Sprite(Device);
            Line = new Line(Device) { Width = 1F };
            ResetDrawingState();

            OutputSurface = Device.GetBackBuffer(0, 0);
            SurfaceDescription output = OutputSurface.Description;
            Layout = new PresentationLayout(new Size(Settings.ScreenWidth, Settings.ScreenHeight), new Size(output.Width, output.Height));
            MainSurface = Layout.LogicalSize == Layout.OutputSize ? OutputSurface
                : Surface.CreateRenderTarget(Device, Settings.ScreenWidth, Settings.ScreenHeight,
                    output.Format, MultisampleType.None, 0, false);
            Device.SetRenderTarget(0, MainSurface);
            CurrentSurface = MainSurface;
            Device.Clear(ClearFlags.Target, Color.Black, 0, 0);

            if (RadarTexture == null || RadarTexture.Disposed)
            {
                RadarTexture = new Texture(Device, 2, 2, 1, Usage.None, Format.A8R8G8B8, Pool.Managed);

                DataRectangle stream = RadarTexture.LockRectangle(0, LockFlags.Discard);
                using (Bitmap image = new Bitmap(2, 2, 8, PixelFormat.Format32bppArgb, stream.Data.DataPointer))
                using (Graphics graphics = Graphics.FromImage(image))
                    graphics.Clear(Color.White);
                RadarTexture.UnlockRectangle(0);
            }
            if (PoisonDotBackground == null || PoisonDotBackground.Disposed)
            {
                PoisonDotBackground = new Texture(Device, 5, 5, 1, Usage.None, Format.A8R8G8B8, Pool.Managed);

                DataRectangle stream = PoisonDotBackground.LockRectangle(0, LockFlags.Discard);
                using (Bitmap image = new Bitmap(5, 5, 20, PixelFormat.Format32bppArgb, stream.Data.DataPointer))
                using (Graphics graphics = Graphics.FromImage(image))
                    graphics.Clear(Color.White);
                PoisonDotBackground.UnlockRectangle(0);
            }
            CreateLights();
        }

        private unsafe static void CreateLights()
        {

            for (int i = Lights.Count - 1; i >= 0; i--)
                Lights[i].Dispose();

            Lights.Clear();

            for (int i = 1; i < LightSizes.Length; i++)
            {
                // int width = 125 + (57 *i);
                //int height = 110 + (57 * i);
                int width = LightSizes[i].X;
                int height = LightSizes[i].Y;

                Texture light = new Texture(Device, width, height, 1, Usage.None, Format.A8R8G8B8, Pool.Managed);

                DataRectangle stream = light.LockRectangle(0, LockFlags.Discard);
                using (Bitmap image = new Bitmap(width, height, width * 4, PixelFormat.Format32bppArgb, stream.Data.DataPointer))
                {
                    using (Graphics graphics = Graphics.FromImage(image))
                    {
                        using (GraphicsPath path = new GraphicsPath())
                        {
                            //path.AddEllipse(new Rectangle(0, 0, width, height));
                            //using (PathGradientBrush brush = new PathGradientBrush(path))
                            //{
                            //    graphics.Clear(Color.FromArgb(0, 0, 0, 0));
                            //    brush.SurroundColors = new[] { Color.FromArgb(0, 255, 255, 255) };
                            //    brush.CenterColor = Color.FromArgb(255, 255, 255, 255);
                            //    graphics.FillPath(brush, path);
                            //    graphics.Save();
                            //}

                            path.AddEllipse(new Rectangle(0, 0, width, height));
                            using (PathGradientBrush brush = new PathGradientBrush(path))
                            {
                                Color[] blendColours = { Color.White,
                                                     Color.FromArgb(255,210,210,210),
                                                     Color.FromArgb(255,160,160,160),
                                                     Color.FromArgb(255,70,70,70),
                                                     Color.FromArgb(255,40,40,40),
                                                     Color.FromArgb(0,0,0,0)};

                                float[] radiusPositions = { 0f, .20f, .40f, .60f, .80f, 1.0f };

                                ColorBlend colourBlend = new ColorBlend();
                                colourBlend.Colors = blendColours;
                                colourBlend.Positions = radiusPositions;

                                graphics.Clear(Color.FromArgb(0, 0, 0, 0));
                                brush.InterpolationColors = colourBlend;
                                brush.SurroundColors = blendColours;
                                brush.CenterColor = Color.White;
                                graphics.FillPath(brush, path);
                                graphics.Save();
                            }
                        }
                    }
                }

                light.UnlockRectangle(0);
                //light.Disposing += (o, e) => Lights.Remove(light);
                Lights.Add(light);
            }
        }

        public static void SetSurface(Surface surface)
        {
            if (CurrentSurface == surface)
                return;

            Flush();

            try
            {
                Device.SetRenderTarget(0, surface);
            }
            catch (Exception ex)
            {
                LogGraphicsDiagnostics(string.Format(
                    "SetRenderTarget failed: isMain={0} isFloor={1} isLight={2} isNull={3} isDisposed={4} error={5}",
                    surface == MainSurface, surface == FloorSurface, surface == LightSurface,
                    surface == null, surface != null && surface.Disposed, ex.Message));
                throw;
            }

            // Cache the surface only after D3D accepted it. If SetRenderTarget throws, keeping the
            // old value keeps the cache in sync with the device; updating it first makes every later
            // call skip SetRenderTarget because the target looks unchanged.
            CurrentSurface = surface;
            GraphicsDiagnostics.RecordStateChange();
        }
        public static void SetGrayscale(bool value)
        {
            GrayScale = value;

            if (value == true)
            {
                if (Device.PixelShader == GrayScalePixelShader) return;
                Flush();
                Device.PixelShader = GrayScalePixelShader;
                GraphicsDiagnostics.RecordStateChange();
            }
            else
            {
                if (Device.PixelShader == null) return;
                Flush();
                Device.PixelShader = null;
                GraphicsDiagnostics.RecordStateChange();
            }
        }

        public static void DrawOpaque(Texture texture, Rectangle? sourceRect, Vector3? position, Color4 color, float opacity)
        {
            color.Alpha = opacity;
            Draw(texture, sourceRect, position, color);
        }

        public static void Draw(Texture texture, Rectangle? sourceRect, Vector3? position, Color4 color)
        {
            Sprite.Draw(texture, sourceRect, Vector3.Zero, position, color);
            CMain.DPSCounter++;
            GraphicsDiagnostics.RecordDraw();
        }

        private static void Flush()
        {
            Sprite.Flush();
            GraphicsDiagnostics.RecordFlush();
        }

        private static bool NeedsReset;

        public static void AttemptReset()
        {
            try
            {
                Result result = DXManager.Device.TestCooperativeLevel();

                if (!NeedsReset)
                {
                    if (result.Code == ResultCode.DeviceLost.Code) return;

                    if (result.Code == ResultCode.Success.Code)
                    {
                        DXManager.DeviceLost = false;
                        return;
                    }

                    if (result.Code != ResultCode.DeviceNotReset.Code) return;

                    // Device lost but ready to reset: release default-pool resources once, then retry Reset each frame
                    TearDown();
                    return;
                }

                if (result.Code == ResultCode.DeviceLost.Code) return;

                TryReset();
            }
            catch
            {
            }
        }

        public static void ResetDevice()
        {
            if (DXManager.Device == null || DXManager.Parameters == null) return;

            TearDown();
        }

        private static void TearDown()
        {
            // Close a possibly open scene before the device is recreated, otherwise the leftover
            // BeginScene keeps failing after the Reset.
            SafeEndScene();

            try
            {
                DXManager.CleanUp();
            }
            catch
            {
            }

            DXManager.DeviceLost = true;
            NeedsReset = true;

            TryReset();
        }

        private static string _lastGraphicsLog;
        private static int _resetCount;

        /// <summary>
        /// Records transitions and failures without letting diagnostics interrupt the game.
        /// D3D display modes in this log are not measurements of the physical scanout signal.
        /// </summary>
        public static void LogGraphicsDiagnostics(string message)
        {
            if (message == _lastGraphicsLog) return;
            _lastGraphicsLog = message;

            try
            {
                System.IO.File.AppendAllText(@".\Graphics.log",
                    string.Format("[{0:yyyy-MM-dd HH:mm:ss}] {1}{2}", DateTime.Now, message, Environment.NewLine));
            }
            catch
            {
            }
        }

        /// <summary>
        /// Best-effort cleanup when a drawing exception skips Sprite.End/EndScene.
        /// Cleanup failures must not mask the original drawing or device error.
        /// </summary>
        public static void SafeEndScene()
        {
            try
            {
                if (Sprite != null) Sprite.End();
            }
            catch
            {
            }

            if (Device == null || Device.Disposed) return;

            try
            {
                Device.EndScene();
            }
            catch
            {
            }
        }

        private static bool TryReset()
        {
            if (!NeedsReset) return false;

            if (Settings.ScreenWidth <= 0 || Settings.ScreenHeight <= 0) return false;

            int clientWidth = 0, clientHeight = 0;

            if (Program.Form != null)
            {
                clientWidth = Program.Form.ClientSize.Width;
                clientHeight = Program.Form.ClientSize.Height;
            }

            try
            {
                ConfigurePresentation();
                DXManager.Device.Reset(DXManager.Parameters);
            }
            catch (Exception ex)
            {
                // DEVICELOST / DEVICENOTRESET: keep NeedsReset so the Idle loop retries on the next frame.
                // Sprites and other resources are null here, so RenderEnvironment does not draw.
                LogGraphicsDiagnostics(string.Format(
                    "Reset FAILED mode={0} windowed={1} requested={2}x{3} client={4}x{5} error={6}: {7}",
                    Settings.Mode, DXManager.Parameters.Windowed,
                    DXManager.Parameters.BackBufferWidth, DXManager.Parameters.BackBufferHeight,
                    clientWidth, clientHeight, ex.GetType().Name, ex.Message));
                if (ex is Direct3D9Exception d3dException && TryWindowedFallback("Reset", d3dException))
                    return !NeedsReset;
                return false;
            }

            // Keep the Create order. Discarding back-buffer contents is not object disposal.
            try
            {
                DXManager.Device.SetDialogBoxMode(true);
            }
            catch
            {
            }

            try
            {
                DXManager.LoadTextures();
            }
            catch (Exception ex)
            {
                CleanUp();
                LogGraphicsDiagnostics("Display resources could not be rebuilt: " + ex);
                if (ex is Direct3D9Exception d3dException && TryWindowedFallback("LoadTextures", d3dException))
                    return !NeedsReset;
                return false;
            }

            NeedsReset = false;
            DXManager.DeviceLost = false;

            CMain.InvalidateSceneTextures();

            SafeEndScene();
            _resetCount++;
            LogPresentationState("Reset OK");

            return true;
        }

        private static string _lastPresentationState;

        private static void LogPresentationState(string operation)
        {
            try
            {
                var displayMode = Device.GetDisplayMode(0);
                var clientSize = Program.Form == null ? Size.Empty : Program.Form.ClientSize;
                string state = string.Format(
                    "mode={0} windowed={1} logical={2}x{3} client={4}x{5} output={6}x{7} viewport={8} d3dMode={9}x{10}@{11}Hz",
                    Settings.Mode, Parameters.Windowed, Layout.LogicalSize.Width, Layout.LogicalSize.Height,
                    clientSize.Width, clientSize.Height, Layout.OutputSize.Width, Layout.OutputSize.Height,
                    Layout.Viewport, displayMode.Width, displayMode.Height, displayMode.RefreshRate);
                if (state == _lastPresentationState && _resetCount > 3 && _resetCount % 100 != 0) return;
                _lastPresentationState = state;
                LogGraphicsDiagnostics(operation + " #" + _resetCount + " " + state);
            }
            catch (Exception ex)
            {
                LogGraphicsDiagnostics("Display diagnostics failed: " + ex.GetType().Name + ": " + ex.Message);
            }
        }

        public static void AttemptRecovery()
        {
            SafeEndScene();
            try
            {
                Device.SetRenderTarget(0, MainSurface);
                CurrentSurface = MainSurface;
                MirScene.ActiveScene?.Redraw();
                CMain.InvalidateSceneTextures();
            }
            catch
            {
                ResetDevice();
            }
        }
        public static void SetOpacity(float opacity)
        {
            if (Opacity == opacity && _opacityOverride)
                return;

            Flush();
            ApplyOpacity(opacity);
            Opacity = opacity;
            _opacityOverride = true;
            GraphicsDiagnostics.RecordStateChange();
            Flush();
        }

        private static void ApplyOpacity(float opacity)
        {
            Device.SetRenderState(RenderState.AlphaBlendEnable, true);
            if (opacity >= 1 || opacity < 0)
            {
                Device.SetRenderState(RenderState.SourceBlend, SlimDX.Direct3D9.Blend.SourceAlpha);
                Device.SetRenderState(RenderState.DestinationBlend, SlimDX.Direct3D9.Blend.InverseSourceAlpha);
                Device.SetRenderState(RenderState.SourceBlendAlpha, Blend.One);
                Device.SetRenderState(RenderState.BlendFactor, Color.FromArgb(255, 255, 255, 255).ToArgb());
            }
            else
            {
                Device.SetRenderState(RenderState.SourceBlend, Blend.BlendFactor);
                Device.SetRenderState(RenderState.DestinationBlend, Blend.InverseBlendFactor);
                Device.SetRenderState(RenderState.SourceBlendAlpha, Blend.SourceAlpha);
                Device.SetRenderState(RenderState.BlendFactor, Color.FromArgb((byte)(255 * opacity), (byte)(255 * opacity), (byte)(255 * opacity), (byte)(255 * opacity)).ToArgb());
            }
        }
        public static void SetBlend(bool value, float rate = 1F, BlendMode mode = BlendMode.NORMAL)
        {
            if (value == Blending && BlendingRate == rate && BlendingMode == mode && _opacityOverride == !value) return;

            Flush();

            Sprite.End();

            if (value)
            {
                Sprite.Begin(SpriteFlags.DoNotSaveState);
                Device.SetRenderState(RenderState.AlphaBlendEnable, true);

                switch (mode)
                {
                    case BlendMode.INVLIGHT:
                        Device.SetRenderState(RenderState.BlendOperation, BlendOperation.Add);
                        Device.SetRenderState(RenderState.SourceBlend, Blend.BlendFactor);
                        Device.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceColor);
                        break;
                    default:
                        Device.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
                        Device.SetRenderState(RenderState.DestinationBlend, Blend.One);
                        break;
                }

                Device.SetRenderState(RenderState.BlendFactor, Color.FromArgb((byte)(255 * rate), (byte)(255 * rate),
                                                                (byte)(255 * rate), (byte)(255 * rate)).ToArgb());
            }
            else
            {
                Sprite.Begin(SpriteFlags.AlphaBlend);
                // Sprite.Begin resets GPU blend state, even if the software opacity is unchanged.
                ApplyOpacity(Opacity);
            }

            Device.SetRenderTarget(0, CurrentSurface);
            Blending = value;
            BlendingRate = rate;
            BlendingMode = mode;
            _opacityOverride = !value;
            GraphicsDiagnostics.RecordStateChange();
        }

        internal readonly struct BlendState
        {
            private readonly bool _enabled;
            private readonly float _rate;
            private readonly BlendMode _mode;
            private readonly float _opacity;
            private readonly bool _opacityOverride;

            public BlendState(bool enabled, float rate, BlendMode mode, float opacity, bool opacityOverride)
            {
                _enabled = enabled;
                _rate = rate;
                _mode = mode;
                _opacity = opacity;
                _opacityOverride = opacityOverride;
            }

            public void Restore()
            {
                SetBlend(_enabled, _rate, _mode);
                if (_opacityOverride) SetOpacity(_opacity);
                else Opacity = _opacity;
            }
        }

        internal static BlendState CaptureBlendState() => new BlendState(Blending, BlendingRate, BlendingMode, Opacity, _opacityOverride);
        internal static bool IsBlendBatchActive => _blendBatchDepth > 0;

        internal readonly struct BlendBatch : IDisposable
        {
            private readonly BlendState _state;
            private readonly int _previousDepth;

            public BlendBatch(BlendState state, int previousDepth)
            {
                _state = state;
                _previousDepth = previousDepth;
            }

            public void Restore() => _state.Restore();

            public void Dispose()
            {
                try { Restore(); }
                finally { _blendBatchDepth = _previousDepth; }
            }
        }

        internal static BlendBatch BeginBlendBatch()
        {
            var batch = new BlendBatch(CaptureBlendState(), _blendBatchDepth);
            _blendBatchDepth++;
            return batch;
        }

        public static void SetNormal(float blend, Color tintcolor)
        {
            if (Device.PixelShader == NormalPixelShader)
                return;

            Flush();
            Device.PixelShader = NormalPixelShader;
            Device.SetPixelShaderConstant(0, new Vector4[] { new Vector4(1.0F, 1.0F, 1.0F, blend) });
            Device.SetPixelShaderConstant(1, new Vector4[] { new Vector4(tintcolor.R / 255, tintcolor.G / 255, tintcolor.B / 255, 1.0F) });
            GraphicsDiagnostics.RecordStateChange();
            Flush();
        }

        public static void SetGrayscale(float blend, Color tintcolor)
        {
            if (Device.PixelShader == GrayScalePixelShader)
                return;

            Flush();
            Device.PixelShader = GrayScalePixelShader;
            Device.SetPixelShaderConstant(0, new Vector4[] { new Vector4(1.0F, 1.0F, 1.0F, blend) });
            Device.SetPixelShaderConstant(1, new Vector4[] { new Vector4(tintcolor.R / 255, tintcolor.G / 255, tintcolor.B / 255, 1.0F) });
            GraphicsDiagnostics.RecordStateChange();
            Flush();
        }

        public static void SetBlendMagic(float blend, Color tintcolor)
        {
            if (Device.PixelShader == MagicPixelShader || MagicPixelShader == null)
                return;

            Flush();
            Device.PixelShader = MagicPixelShader;
            Device.SetPixelShaderConstant(0, new Vector4[] { new Vector4(1.0F, 1.0F, 1.0F, blend) });
            Device.SetPixelShaderConstant(1, new Vector4[] { new Vector4(tintcolor.R / 255, tintcolor.G / 255, tintcolor.B / 255, 1.0F) });
            GraphicsDiagnostics.RecordStateChange();
            Flush();
        }

        public static void Clean()
        {
            long now = CMain.Time;
            if (now >= _nextCleanTime)
            {
                _nextCleanTime = now + 1000;
                TextureList.StartSweep();
                ControlList.StartSweep();
            }
            if (!TextureList.HasPendingSweep && !ControlList.HasPendingSweep) return;

            long started = System.Diagnostics.Stopwatch.GetTimestamp();
            int released = 0;
            // The time limit is soft: a single driver disposal cannot be interrupted.
            try
            {
                for (int inspected = 0; inspected < 64 && released < 16; inspected++)
                {
                    bool texturesPending = TextureList.HasPendingSweep;
                    bool controlsPending = ControlList.HasPendingSweep;
                    if (!texturesPending && !controlsPending) break;
                    bool cleanControl = controlsPending && (_cleanControlsNext || !texturesPending);
                    if (cleanControl ? ControlList.CleanOne(now) : TextureList.CleanOne(now)) released++;
                    _cleanControlsNext = !cleanControl;
                    if (System.Diagnostics.Stopwatch.GetElapsedTime(started).TotalMilliseconds >= 1) break;
                }
            }
            finally
            {
                GraphicsDiagnostics.Record(GraphicsMetric.Cleanup, started);
            }
        }


        private static void CleanUp()
        {
            if (Sprite != null)
            {
                if (!Sprite.Disposed)
                {
                    Sprite.Dispose();
                }

                Sprite = null;
            }

            if (Line != null)
            {
                if (!Line.Disposed)
                {
                    Line.Dispose();
                }

                Line = null;
            }

            // CurrentSurface borrows a canvas or scene-cache surface; it owns no extra wrapper.
            CurrentSurface = null;

            if (MainSurface != null)
            {
                if (!MainSurface.Disposed)
                {
                    MainSurface.Dispose();
                }

                MainSurface = null;
            }

            if (OutputSurface != null)
            {
                if (!OutputSurface.Disposed)
                    OutputSurface.Dispose();
                OutputSurface = null;
            }
            Layout = default;

            if (PoisonDotBackground != null)
            {
                if (!PoisonDotBackground.Disposed)
                {
                    PoisonDotBackground.Dispose();
                }

                PoisonDotBackground = null;
            }

            if (RadarTexture != null)
            {
                if (!RadarTexture.Disposed)
                {
                    RadarTexture.Dispose();
                }

                RadarTexture = null;
            }

            if (FloorTexture != null)
            {
                if (!FloorTexture.Disposed)
                {
                    FloorTexture.Dispose();
                }

                DXManager.FloorTexture = null;

                if (DXManager.FloorSurface != null && !DXManager.FloorSurface.Disposed)
                {
                    DXManager.FloorSurface.Dispose();
                }

                DXManager.FloorSurface = null;
            }

            if (LightTexture != null)
            {
                if (!LightTexture.Disposed)
                    LightTexture.Dispose();

                DXManager.LightTexture = null;

                if (DXManager.LightSurface != null && !DXManager.LightSurface.Disposed)
                {
                    DXManager.LightSurface.Dispose();
                }

                DXManager.LightSurface = null;
            }

            if (Lights != null)
            {
                for (int i = 0; i < Lights.Count; i++)
                {
                    if (!Lights[i].Disposed)
                        Lights[i].Dispose();
                }
                Lights.Clear();
            }

            for (int i = TextureList.Count - 1; i >= 0; i--)
            {
                MImage m = TextureList[i];

                if (m == null) continue;

                m.DisposeTexture();
            }
            TextureList.Clear();


            for (int i = ControlList.Count - 1; i >= 0; i--)
            {
                MirControl c = ControlList[i];

                if (c == null) continue;

                c.DisposeTexture();
            }
            ControlList.Clear();
        }

        public static void Dispose()
        {
            try
            {
                CleanUp();

                Device?.Direct3D?.Dispose();
                Device?.Dispose();

                NormalPixelShader?.Dispose();
                GrayScalePixelShader?.Dispose();
                MagicPixelShader?.Dispose();
            }
            finally
            {
                GraphicsDiagnostics.Stop();
            }
        }
    }
}
