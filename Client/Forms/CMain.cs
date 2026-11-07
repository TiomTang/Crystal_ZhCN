using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Security;
using Client.MirControls;
using Client.MirGraphics;
using Client.MirNetwork;
using Client.MirObjects;
using Client.MirScenes;
using Client.MirSounds;
using SlimDX.Direct3D9;
using SlimDX.Windows;
using Font = System.Drawing.Font;

namespace Client
{
    public partial class CMain : RenderForm
    {
        public static MirControl DebugBaseLabel, HintBaseLabel;
        public static MirLabel DebugTextLabel, HintTextLabel, ScreenshotTextLabel;
        public static Graphics Graphics;
        public static Point MPoint;
        private static bool _applyingDisplayMode;
        private static bool _outputSizeChanged;
        private static bool _pointerInViewport;
        private static MouseButtons _pressedMouseButtons;

        public readonly static Stopwatch Timer = Stopwatch.StartNew();
        public readonly static DateTime StartTime = DateTime.UtcNow;
        public static long Time;
        public static DateTime Now { get { return StartTime.AddMilliseconds(Time); } }
        public static readonly Random Random = new Random();

        public static string DebugText = "";

        private static long _fpsTime;
        private static int _fps;
        private static long _drawTime;
        public static int FPS;
        public static int DPS;
        public static int DPSCounter;

        public static long PingTime;
        public static long NextPing = 10000;

        public static bool Shift, Alt, Ctrl, Tilde, SpellTargetLock;
        public static double BytesSent, BytesReceived;

        public static KeyBindSettings InputKeys = new KeyBindSettings();

        public CMain()
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            MouseClick += CMain_MouseClick;
            MouseDown += CMain_MouseDown;
            MouseUp += CMain_MouseUp;
            MouseMove += CMain_MouseMove;
            MouseDoubleClick += CMain_MouseDoubleClick;
            KeyPress += CMain_KeyPress;
            KeyDown += CMain_KeyDown;
            KeyUp += CMain_KeyUp;
            Deactivate += CMain_Deactivate;
            MouseWheel += CMain_MouseWheel;
            ClientSizeChanged += CMain_ClientSizeChanged;
            MouseLeave += CMain_MouseLeave;
            MouseCaptureChanged += CMain_MouseCaptureChanged;


            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.Selectable, true);
            FormBorderStyle = Settings.Mode == DisplayMode.Windowed && !Settings.Borderless ? FormBorderStyle.FixedDialog : FormBorderStyle.None;

            Graphics = CreateGraphics();
            Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            Graphics.CompositingQuality = CompositingQuality.HighQuality;
            Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            Graphics.TextContrast = 0;
        }

        private void CMain_Load(object sender, EventArgs e)
        {
            this.Text = GameLanguage.Get("Legend of Mir 2");
            try
            {
                ApplyDisplayMode();

                LoadMouseCursors();
                SetMouseCursor(MouseCursor.Default);

                SlimDX.Configuration.EnableObjectTracking = true;

                DXManager.Create();
                SoundManager.Create();
            }
            catch (Exception ex)
            {
                SaveError(ex.ToString());
            }
        }

        private static void Application_Idle(object sender, EventArgs e)
        {
            try
            {
                while (AppStillIdle)
                {
                    UpdateTime();
                    UpdateEnviroment();

                    if (IsDrawTime())
                    {
                        RenderEnvironment();
                        UpdateFrameTime();
                    }
                }

            }
            catch (Exception ex)
            {
                SaveError(ex.ToString());
            }
        }

        private static void CMain_Deactivate(object sender, EventArgs e)
        {
            CancelMouseInput();
            Shift = false;
            Alt = false;
            Ctrl = false;
            Tilde = false;
            SpellTargetLock = false;
        }

        public static void CMain_KeyDown(object sender, KeyEventArgs e)
        {
            Shift = e.Shift;
            Alt = e.Alt;
            Ctrl = e.Control;

            if (!String.IsNullOrEmpty(InputKeys.GetKey(KeybindOptions.TargetSpellLockOn)))
            {
                SpellTargetLock = e.KeyCode == (Keys)Enum.Parse(typeof(Keys), InputKeys.GetKey(KeybindOptions.TargetSpellLockOn), true);
            }
            else
            {
                SpellTargetLock = false;
            }


            if (e.KeyCode == Keys.Oem8)
                CMain.Tilde = true;

            try
            {
                if (e.Alt && e.KeyCode == Keys.Enter)
                {
                    ToggleDisplayMode();
                    return;
                }

                if (MirScene.ActiveScene != null)
                    MirScene.ActiveScene.OnKeyDown(e);

            }
            catch (Exception ex)
            {
                SaveError(ex.ToString());
            }
        }
        public static void CMain_MouseMove(object sender, MouseEventArgs e)
        {
            Rectangle viewport = GetPresentationLayout().GetClientViewport(Program.Form.ClientSize);
            if (Settings.Mode == DisplayMode.FullScreen || Settings.MouseClip)
                Cursor.Clip = viewport.IsEmpty ? Rectangle.Empty : Program.Form.RectangleToScreen(viewport);
            else if (!Cursor.Clip.IsEmpty)
                Cursor.Clip = Rectangle.Empty;

            Point clientPoint = Program.Form.PointToClient(Cursor.Position);
            e = ScaleMouseArgs(new MouseEventArgs(e.Button, e.Clicks, clientPoint.X, clientPoint.Y, e.Delta), false);

            try
            {
                if (!_pointerInViewport && MirControl.ActiveControl != null && MirControl.ActiveControl.Moving)
                    MirControl.ActiveControl.OnMouseMove(e);
                else if (_pointerInViewport && MirScene.ActiveScene != null)
                    MirScene.ActiveScene.OnMouseMove(e);
            }
            catch (Exception ex)
            {
                SaveError(ex.ToString());
            }
        }
        public static void CMain_KeyUp(object sender, KeyEventArgs e)
        {
            Shift = e.Shift;
            Alt = e.Alt;
            Ctrl = e.Control;

            if (!String.IsNullOrEmpty(InputKeys.GetKey(KeybindOptions.TargetSpellLockOn)))
            {
                SpellTargetLock = e.KeyCode == (Keys)Enum.Parse(typeof(Keys), InputKeys.GetKey(KeybindOptions.TargetSpellLockOn), true);
            }
            else
            {
                SpellTargetLock = false;
            }

            if (e.KeyCode == Keys.Oem8)
                CMain.Tilde = false;

            foreach (KeyBind KeyCheck in CMain.InputKeys.Keylist)
            {
                if (KeyCheck.function != KeybindOptions.Screenshot) continue;
                if (KeyCheck.Key != e.KeyCode)
                    continue;
                if ((KeyCheck.RequireAlt != 2) && (KeyCheck.RequireAlt != (Alt ? 1 : 0)))
                    continue;
                if ((KeyCheck.RequireShift != 2) && (KeyCheck.RequireShift != (Shift ? 1 : 0)))
                    continue;
                if ((KeyCheck.RequireCtrl != 2) && (KeyCheck.RequireCtrl != (Ctrl ? 1 : 0)))
                    continue;
                if ((KeyCheck.RequireTilde != 2) && (KeyCheck.RequireTilde != (Tilde ? 1 : 0)))
                    continue;
                Program.Form.CreateScreenShot();
                break;

            }
            try
            {
                if (MirScene.ActiveScene != null)
                    MirScene.ActiveScene.OnKeyUp(e);
            }
            catch (Exception ex)
            {
                SaveError(ex.ToString());
            }
        }
        public static void CMain_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (MirScene.ActiveScene != null)
                    MirScene.ActiveScene.OnKeyPress(e);
            }
            catch (Exception ex)
            {
                SaveError(ex.ToString());
            }
        }
        public static void CMain_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            e = ScaleMouseArgs(e);
            if (!_pointerInViewport || (_pressedMouseButtons & e.Button) == MouseButtons.None) return;
            try
            {
                if (MirScene.ActiveScene != null)
                    MirScene.ActiveScene.OnMouseClick(e);
            }
            catch (Exception ex)
            {
                SaveError(ex.ToString());
            }
        }
        public static void CMain_MouseUp(object sender, MouseEventArgs e)
        {
            MapControl.MapButtons &= ~e.Button;
            if (e.Button != MouseButtons.Right || !Settings.NewMove)
                GameScene.CanRun = false;

            e = ScaleMouseArgs(e);
            bool pressed = (_pressedMouseButtons & e.Button) != MouseButtons.None;
            _pressedMouseButtons &= ~e.Button;
            if (!pressed) return;

            try
            {
                // Releasing in a black bar must still end an in-game drag or pressed button.
                if (!_pointerInViewport && MirControl.ActiveControl != null)
                    MirControl.CancelMouseInput(e);
                else if (MirScene.ActiveScene != null)
                    MirScene.ActiveScene.OnMouseUp(e);
            }
            catch (Exception ex)
            {
                SaveError(ex.ToString());
            }
        }
        public static void CMain_MouseDown(object sender, MouseEventArgs e)
        {
            e = ScaleMouseArgs(e);
            if (!_pointerInViewport) return;
            _pressedMouseButtons |= e.Button;

            if (Program.Form.ActiveControl is TextBox)
            {
                MirTextBox textBox = Program.Form.ActiveControl.Tag as MirTextBox;

                if (textBox != null && textBox.CanLoseFocus)
                    Program.Form.ActiveControl = null;
            }

            if (e.Button == MouseButtons.Right && (GameScene.SelectedCell != null || GameScene.PickedUpGold))
            {
                GameScene.SelectedCell = null;
                GameScene.PickedUpGold = false;
                return;
            }

            try
            {
                if (MirScene.ActiveScene != null)
                    MirScene.ActiveScene.OnMouseDown(e);
            }
            catch (Exception ex)
            {
                SaveError(ex.ToString());
            }
        }
        public static void CMain_MouseClick(object sender, MouseEventArgs e)
        {
            e = ScaleMouseArgs(e);
            // WinForms raises MouseClick before MouseUp, while the accepted press is still active.
            if (!_pointerInViewport || (_pressedMouseButtons & e.Button) == MouseButtons.None) return;
            try
            {
                if (MirScene.ActiveScene != null)
                    MirScene.ActiveScene.OnMouseClick(e);
            }
            catch (Exception ex)
            {
                SaveError(ex.ToString());
            }
        }
        public static void CMain_MouseWheel(object sender, MouseEventArgs e)
        {
            e = ScaleMouseArgs(e);
            if (!_pointerInViewport) return;
            try
            {
                if (MirScene.ActiveScene != null)
                    MirScene.ActiveScene.OnMouseWheel(e);
            }
            catch (Exception ex)
            {
                SaveError(ex.ToString());
            }
        }

        private static void UpdateTime()
        {
            Time = Timer.ElapsedMilliseconds;
        }

        private static void UpdateFrameTime()
        {
            if (Time >= _fpsTime)
            {
                _fpsTime = Time + 1000;
                FPS = _fps;
                _fps = 0;

                DPS = DPSCounter;
                DPSCounter = 0;
            }
            else
                _fps++;
        }

        private static bool IsDrawTime()
        {
            int targetUpdates = 1000 / Math.Max(1, Settings.MaxFPS);

            if (Time >= _drawTime)
            {
                _drawTime = Time + targetUpdates;
                return true;
            }
            return false;
        }

        private static void UpdateEnviroment()
        {
            Network.Process();

            if (MirScene.ActiveScene != null)
                MirScene.ActiveScene.Process();

            for (int i = 0; i < MirAnimatedControl.Animations.Count; i++)
                MirAnimatedControl.Animations[i].UpdateOffSet();

            for (int i = 0; i < MirAnimatedButton.Animations.Count; i++)
                MirAnimatedButton.Animations[i].UpdateOffSet();

            CreateHintLabel();

            if (Settings.DebugMode)
            {
                CreateDebugLabel();
            }
        }

        private static void RenderEnvironment()
        {
            long renderStarted = GraphicsDiagnostics.Timestamp();
            try
            {
                if (_outputSizeChanged)
                {
                    _outputSizeChanged = false;
                    CancelMouseInput();
                    DXManager.ResetDevice();
                }

                DXManager.Clean(); // One bounded maintenance slice, including while the device is lost.
                if (DXManager.DeviceLost)
                {
                    DXManager.AttemptReset();
                    Thread.Sleep(1);
                    return;
                }

                DXManager.BeginFrame();

                long sceneStarted = GraphicsDiagnostics.Timestamp();
                try
                {
                    if (MirScene.ActiveScene != null)
                        MirScene.ActiveScene.Draw();

                    DXManager.Sprite.End();
                    DXManager.Device.EndScene();
                }
                finally
                {
                    GraphicsDiagnostics.Record(GraphicsMetric.Scene, sceneStarted);
                }
                if (DXManager.PresentFrame())
                    GraphicsDiagnostics.RecordFrame(GraphicsDiagnostics.Timestamp());
            }
            catch (Direct3D9Exception ex)
            {
                // A drawing exception may skip the matching Sprite.End/EndScene calls.
                DXManager.SafeEndScene();

                DXManager.DeviceLost = true;
                SaveError(ex.ToString());
            }
            catch (Exception ex)
            {
                SaveError(ex.ToString());

                DXManager.AttemptRecovery();
            }
            finally
            {
                GraphicsDiagnostics.Record(GraphicsMetric.Render, renderStarted);
                if (GraphicsDiagnostics.Enabled)
                {
                    GraphicsDiagnostics.UpdateResources(DXManager.TextureList.Count, DXManager.ControlList.Count,
                        DXManager.TextureList.EstimatedBytes);
                    GraphicsDiagnostics.PublishIfDue();
                }
            }
        }

        private static void CreateDebugLabel()
        {
            string text;

            if (MirControl.MouseControl != null)
            {
                text = string.Format("FPS: {0}", FPS);

                text += string.Format(", DPS: {0}", DPS);

                text += string.Format(", Time: {0:HH:mm:ss UTC}", Now);

                if (MirControl.MouseControl is MapControl)
                    text += string.Format(", Co Ords: {0}", MapControl.MapLocation);

                if (MirControl.MouseControl is MirImageControl)
                    text += string.Format(", Control: {0}", MirControl.MouseControl.GetType().Name);

                if (MirScene.ActiveScene is GameScene)
                    text += string.Format(", Objects: {0}", MapControl.Objects.Count);

                if (MirScene.ActiveScene is GameScene && !string.IsNullOrEmpty(DebugText))
                    text += string.Format(", Debug: {0}", DebugText);

                if (MirObjects.MapObject.MouseObject != null)
                {
                    text += string.Format(", Target: {0}", MirObjects.MapObject.MouseObject.Name);
                }
                else
                {
                    text += string.Format(", Target: none");
                }
            }
            else
            {
                text = string.Format("FPS: {0}", FPS);
            }

            text += string.Format(", Ping: {0}", PingTime);

            text += string.Format(", Sent: {0}, Received: {1}", Functions.ConvertByteSize(BytesSent), Functions.ConvertByteSize(BytesReceived));

            text += string.Format(", TLC: {0}", DXManager.TextureList.Count(x => x.TextureValid));
            text += string.Format(", CLC: {0}", DXManager.ControlList.Count(x => x.IsDisposed == false));

            if (Settings.Mode != DisplayMode.Windowed)
            {
                if (DebugBaseLabel == null || DebugBaseLabel.IsDisposed)
                {
                    DebugBaseLabel = new MirControl
                    {
                        BackColour = Color.FromArgb(50, 50, 50),
                        Border = true,
                        BorderColour = Color.Black,
                        DrawControlTexture = true,
                        Location = new Point(5, 5),
                        NotControl = true,
                        Opacity = 0.5F
                    };
                }

                if (DebugTextLabel == null || DebugTextLabel.IsDisposed)
                {
                    DebugTextLabel = new MirLabel
                    {
                        AutoSize = true,
                        BackColour = Color.Transparent,
                        ForeColour = Color.White,
                        Parent = DebugBaseLabel,
                    };

                    DebugTextLabel.SizeChanged += (o, e) => DebugBaseLabel.Size = DebugTextLabel.Size;
                }

                DebugTextLabel.Text = text;
            }
            else
            {
                if (DebugBaseLabel != null && DebugBaseLabel.IsDisposed == false)
                {
                    DebugBaseLabel.Dispose();
                    DebugBaseLabel = null;
                }
                if (DebugTextLabel != null && DebugTextLabel.IsDisposed == false)
                {
                    DebugTextLabel.Dispose();
                    DebugTextLabel = null;
                }

                Program.Form.Text = $"{GameLanguage.Get("Legend of Mir 2")} - {text}";
            }
        }

        private static void CreateHintLabel()
        {
            if (HintBaseLabel == null || HintBaseLabel.IsDisposed)
            {
                HintBaseLabel = new MirControl
                {
                    BackColour = Color.FromArgb(255, 0, 0, 0),
                    Border = true,
                    DrawControlTexture = true,
                    BorderColour = Color.FromArgb(255, 144, 144, 0),
                    ForeColour = Color.Yellow,
                    Parent = MirScene.ActiveScene,
                    NotControl = true,
                    Opacity = 0.5F
                };
            }


            if (HintTextLabel == null || HintTextLabel.IsDisposed)
            {
                HintTextLabel = new MirLabel
                {
                    AutoSize = true,
                    BackColour = Color.Transparent,
                    ForeColour = Color.Yellow,
                    Parent = HintBaseLabel,
                };

                HintTextLabel.SizeChanged += (o, e) => HintBaseLabel.Size = HintTextLabel.Size;
            }

            if (MirControl.MouseControl == null || string.IsNullOrEmpty(MirControl.MouseControl.Hint))
            {
                HintBaseLabel.Visible = false;
                return;
            }

            HintBaseLabel.Visible = true;

            HintTextLabel.Text = MirControl.MouseControl.Hint;

            Point point = MPoint.Add(-HintTextLabel.Size.Width, 20);

            if (point.X + HintBaseLabel.Size.Width >= Settings.ScreenWidth)
                point.X = Settings.ScreenWidth - HintBaseLabel.Size.Width - 1;
            if (point.Y + HintBaseLabel.Size.Height >= Settings.ScreenHeight)
                point.Y = Settings.ScreenHeight - HintBaseLabel.Size.Height - 1;

            if (point.X < 0)
                point.X = 0;
            if (point.Y < 0)
                point.Y = 0;

            HintBaseLabel.Location = point;
        }

        private static void ToggleDisplayMode()
        {
            switch (Settings.Mode)
            {
                case DisplayMode.Windowed:
                    Settings.Mode = DisplayMode.Maximized;
                    break;
                case DisplayMode.Maximized:
                    Settings.Mode = DisplayMode.FullScreen;
                    break;
                default:
                    Settings.Mode = DisplayMode.Windowed;
                    break;
            }

            ApplyDisplayMode();
        }

        public static void SetDisplayMode(DisplayMode mode)
        {
            if (Settings.Mode == mode) return;

            Settings.Mode = mode;
            ApplyDisplayMode();
        }

        /// <summary>
        /// Places the output window without changing the game's logical canvas or field of view.
        /// </summary>
        private static void ApplyDisplayMode()
        {
            // Program.Form is CMain and CenterToScreen is protected, so a Form-typed reference cannot call it
            CMain form = Program.Form;
            if (form == null || _applyingDisplayMode) return;

            _applyingDisplayMode = true;
            try
            {
                Rectangle bounds = Screen.FromControl(form).Bounds;
                if (Settings.Mode == DisplayMode.FullScreen && !DXManager.TryGetDesktopBounds(out bounds))
                {
                    Settings.Mode = DisplayMode.Windowed;
                    DXManager.LogGraphicsDiagnostics("Fullscreen desktop mode unavailable; using windowed mode.");
                }

                form.FormBorderStyle = Settings.Mode == DisplayMode.Windowed && !Settings.Borderless
                    ? FormBorderStyle.FixedDialog : FormBorderStyle.None;
                form.TopMost = Settings.Mode == DisplayMode.FullScreen;
                form.WindowState = FormWindowState.Normal;
                if (Settings.Mode == DisplayMode.Windowed)
                {
                    form.ClientSize = new Size(Settings.ScreenWidth, Settings.ScreenHeight);
                    form.CenterToScreen();
                }
                else
                    form.Bounds = bounds;
                CancelMouseInput();
            }
            finally
            {
                _applyingDisplayMode = false;
            }

            _outputSizeChanged = false;
            DXManager.ResetDevice();
        }

        private static void CMain_ClientSizeChanged(object sender, EventArgs e)
        {
            if (_applyingDisplayMode || Program.Form == null || DXManager.Device == null || DXManager.DeviceLost ||
                DXManager.Parameters == null || !DXManager.Parameters.Windowed ||
                Program.Form.WindowState == FormWindowState.Minimized) return;
            if (Program.Form.ClientSize.Width > 0 && Program.Form.ClientSize.Height > 0)
                // WinForms can still be inside SetClientSizeCore. Defer until sizing is complete,
                // otherwise a synchronous fallback window is overwritten by the outer resize.
                _outputSizeChanged = true;
        }

        private static void CMain_MouseLeave(object sender, EventArgs e)
        {
            if (_pressedMouseButtons != MouseButtons.None) return;
            _pointerInViewport = false;
            MPoint = new Point(-1, -1);
            InvalidateMouseTarget();
        }

        private static void CMain_MouseCaptureChanged(object sender, EventArgs e)
        {
            if (!Program.Form.Capture && _pressedMouseButtons != MouseButtons.None)
                CancelMouseInput();
        }

        private static void InvalidateMouseTarget()
        {
            MapControl.MapButtons = MouseButtons.None;
            MapControl.MouseLocation = new Point(-1, -1);
            MapObject.MouseObjectID = 0;
            MapObject.MouseObject = null;
            try
            {
                MirControl.ClearMouseHover();
            }
            catch (Exception ex)
            {
                SaveError(ex.ToString());
            }
        }

        private static void CancelMouseInput()
        {
            MouseButtons buttons = _pressedMouseButtons;
            _pressedMouseButtons = MouseButtons.None;
            _pointerInViewport = false;
            MPoint = new Point(-1, -1);
            MapControl.MapButtons = MouseButtons.None;
            Cursor.Clip = Rectangle.Empty;
            try
            {
                MirControl.CancelMouseInput(new MouseEventArgs(buttons, 0, -1, -1, 0));
            }
            catch (Exception ex)
            {
                SaveError(ex.ToString());
            }
            InvalidateMouseTarget();
        }

        /// <summary>
        /// Converts client pixels through the same viewport used by presentation.
        /// Black bars and points outside the client area are not game coordinates.
        /// </summary>
        public static Point ToGamePoint(Point clientPoint)
        {
            Size size = Program.Form == null ? Size.Empty : Program.Form.ClientSize;
            return GetPresentationLayout().ToGamePoint(clientPoint, size);
        }

        private static PresentationLayout GetPresentationLayout()
        {
            return DXManager.Layout.IsValid ? DXManager.Layout : new PresentationLayout(
                new Size(Settings.ScreenWidth, Settings.ScreenHeight), Program.Form == null ? Size.Empty : Program.Form.ClientSize);
        }

        private static MouseEventArgs ScaleMouseArgs(MouseEventArgs e, bool refreshTarget = true)
        {
            Point point = ToGamePoint(e.Location);
            _pointerInViewport = point.X >= 0 && point.Y >= 0;
            if (!_pointerInViewport && MirControl.ActiveControl != null && MirControl.ActiveControl.Moving)
                point = GetPresentationLayout().ToGamePoint(e.Location, Program.Form.ClientSize, true);
            MPoint = point;

            MouseEventArgs scaled = new MouseEventArgs(e.Button, e.Clicks, point.X, point.Y, e.Delta);
            if (!_pointerInViewport)
                InvalidateMouseTarget();
            else if (refreshTarget)
            {
                try
                {
                    // Down, wheel and click can arrive without a preceding move after a resize.
                    // Refresh the cached control and map hit at this event's logical position.
                    MirScene.ActiveScene?.OnMouseMove(new MouseEventArgs(e.Button, 0, point.X, point.Y, 0));
                }
                catch (Exception ex)
                {
                    SaveError(ex.ToString());
                }
            }
            return scaled;
        }

        public static void InvalidateSceneTextures()
        {
            if (GameScene.Scene == null || MirScene.ActiveScene != GameScene.Scene) return;

            GameScene.Scene.MapControl.FloorValid = false;
            GameScene.Scene.TextureValid = false;
        }

        public void CreateScreenShot()
        {
            string text = string.Format("[{0} Server {1}] {2} {3:hh\\:mm\\:ss}",
                Settings.P_ServerName.Length > 0 ? Settings.P_ServerName : "Crystal",
                MapControl.User != null ? MapControl.User.Name : "",
                Now.ToShortDateString(),
                Now.TimeOfDay);

            Surface backbuffer = DXManager.MainSurface;

            using (var stream = Surface.ToStream(backbuffer, ImageFileFormat.Png))
            {
                Bitmap image = new Bitmap(stream);

                using (Graphics graphics = Graphics.FromImage(image))
                {
                    StringFormat sf = new StringFormat
                    {
                        LineAlignment = StringAlignment.Center,
                        Alignment = StringAlignment.Center
                    };

                    graphics.DrawString(text, new Font(Settings.FontName, 9F), Brushes.Black, new Point((Settings.ScreenWidth / 2) + 3, 10), sf);
                    graphics.DrawString(text, new Font(Settings.FontName, 9F), Brushes.Black, new Point((Settings.ScreenWidth / 2) + 4, 9), sf);
                    graphics.DrawString(text, new Font(Settings.FontName, 9F), Brushes.Black, new Point((Settings.ScreenWidth / 2) + 5, 10), sf);
                    graphics.DrawString(text, new Font(Settings.FontName, 9F), Brushes.Black, new Point((Settings.ScreenWidth / 2) + 4, 11), sf);
                    graphics.DrawString(text, new Font(Settings.FontName, 9F), Brushes.White, new Point((Settings.ScreenWidth / 2) + 4, 10), sf);//SandyBrown               

                    string path = Path.Combine(Application.StartupPath, @"Screenshots\");
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);

                    int count = Directory.GetFiles(path, "*.png").Length;

                    image.Save(Path.Combine(path, string.Format("Image {0}.png", count)), ImageFormat.Png);
                }
            }
        }

        public static void SaveError(string ex)
        {
            try
            {
                if (Settings.RemainingErrorLogs-- > 0)
                {
                    File.AppendAllText(@".\Error.txt",
                                       string.Format("[{0}] {1}{2}", Now, ex, Environment.NewLine));
                }
            }
            catch
            {
            }
        }

        public static void SetResolution(int width, int height)
        {
            if (Settings.ScreenWidth == width && Settings.ScreenHeight == height) return;

            Settings.ScreenWidth = width;
            Settings.ScreenHeight = height;

            try
            {
                if (DXManager.Device != null && !DXManager.DeviceLost)
                {
                    DXManager.Device.SetRenderTarget(0, DXManager.OutputSurface);
                    DXManager.CurrentSurface = DXManager.OutputSurface;
                    DXManager.Device.Clear(ClearFlags.Target, Color.Black, 0, 0);
                    DXManager.Device.Present();
                }
            }
            catch
            {
            }

            ApplyDisplayMode();
        }

        #region ScreenCapture

        //private Bitmap CaptureScreen()
        //{
            
        //}

        #endregion

        #region Idle Check
        private static bool AppStillIdle
        {
            get
            {
                PeekMsg msg;
                return !PeekMessage(out msg, IntPtr.Zero, 0, 0, 0);
            }
        }

        [SuppressUnmanagedCodeSecurity]
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        private static extern bool PeekMessage(out PeekMsg msg, IntPtr hWnd, uint messageFilterMin,
                                               uint messageFilterMax, uint flags);

        [StructLayout(LayoutKind.Sequential)]
        private struct PeekMsg
        {
            private readonly IntPtr hWnd;
            private readonly Message msg;
            private readonly IntPtr wParam;
            private readonly IntPtr lParam;
            private readonly uint time;
            private readonly Point p;
        }
        #endregion

        private void CMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (CMain.Time < GameScene.LogTime && !Settings.UseTestConfig && !GameScene.Observing)
            {
                GameScene.Scene.ChatDialog.ReceiveChat(GameLanguage.Get("Cannot leave game for {0} seconds", (GameScene.LogTime - CMain.Time) / 1000), ChatType.System);
                e.Cancel = true;
            }
            else
            {
                Settings.Save();

                DXManager.Dispose();
                SoundManager.Dispose();
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x007E && Settings.Mode == DisplayMode.Maximized && !_applyingDisplayMode) // WM_DISPLAYCHANGE
            {
                base.WndProc(ref m);
                ApplyDisplayMode();
                return;
            }

            if (m.Msg == 0x0112) // WM_SYSCOMMAND
            {
                if (m.WParam.ToInt32() == 0xF100) // SC_KEYMENU
                {
                    m.Result = IntPtr.Zero;
                    return;
                }
                else if (m.WParam.ToInt32() == 0xF030) // SC_MAXIMISE
                {
                    SetDisplayMode(DisplayMode.Maximized);
                    return;
                }
            }

            base.WndProc(ref m);
        }


        public static Cursor[] Cursors;
        public static MouseCursor CurrentCursor = MouseCursor.None;
        public static void SetMouseCursor(MouseCursor cursor)
        {
            if (!Settings.UseMouseCursors) return;

            if (CurrentCursor != cursor)
            {
                CurrentCursor = cursor;
                Program.Form.Cursor = Cursors[(byte)cursor];
            }
        }

        private static void LoadMouseCursors()
        {
            Cursors = new Cursor[8];

            Cursors[(int)MouseCursor.None] = Program.Form.Cursor;

            string path = $"{Settings.MouseCursorPath}Cursor_Default.CUR";
            if (File.Exists(path))
                Cursors[(int)MouseCursor.Default] = LoadCustomCursor(path);

            path = $"{Settings.MouseCursorPath}Cursor_Normal_Atk.CUR";
            if (File.Exists(path))
                Cursors[(int)MouseCursor.Attack] = LoadCustomCursor(path);

            path = $"{Settings.MouseCursorPath}Cursor_Compulsion_Atk.CUR";
            if (File.Exists(path))
                Cursors[(int)MouseCursor.AttackRed] = LoadCustomCursor(path);

            path = $"{Settings.MouseCursorPath}Cursor_Npc.CUR";
            if (File.Exists(path))
                Cursors[(int)MouseCursor.NPCTalk] = LoadCustomCursor(path);

            path = $"{Settings.MouseCursorPath}Cursor_TextPrompt.CUR";
            if (File.Exists(path))
                Cursors[(int)MouseCursor.TextPrompt] = LoadCustomCursor(path);

            path = $"{Settings.MouseCursorPath}Cursor_Trash.CUR";
            if (File.Exists(path))
                Cursors[(int)MouseCursor.Trash] = LoadCustomCursor(path);

            path = $"{Settings.MouseCursorPath}Cursor_Upgrade.CUR";
            if (File.Exists(path))
                Cursors[(int)MouseCursor.Upgrade] = LoadCustomCursor(path);
        }

        public static Cursor LoadCustomCursor(string path)
        {
            IntPtr hCurs = LoadCursorFromFile(path);
            if (hCurs == IntPtr.Zero) throw new Win32Exception();
            var curs = new Cursor(hCurs);
            // Note: force the cursor to own the handle so it gets released properly
            //var fi = typeof(Cursor).GetField("ownHandle", BindingFlags.NonPublic | BindingFlags.Instance);
            //fi.SetValue(curs, true);
            return curs;
        }

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr LoadCursorFromFile(string path);
    }
}
