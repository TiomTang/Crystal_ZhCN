using Client.MirControls;
using Client.MirGraphics;
using Client.MirGraphics.Particles;
using Client.MirNetwork;
using Client.MirObjects;
using Client.MirScenes.Dialogs;
using Client.MirSounds;
using SlimDX;
using SlimDX.Direct3D9;
using C = ClientPackets;
using Effect = Client.MirObjects.Effect;

namespace Client.MirScenes
{
    public sealed class MapControl : MirControl
    {
        public static UserObject User
        {
            get { return MapObject.User; }
            set { MapObject.User = value; }
        }

        public static UserHeroObject Hero
        {
            get { return MapObject.Hero; }
            set { MapObject.Hero = value; }
        }

        public static Dictionary<uint, MapObject> Objects = new Dictionary<uint, MapObject>();
        public static List<MapObject> ObjectsList = new List<MapObject>();

        public const int CellWidth = 48;
        public const int CellHeight = 32;

        public static int OffSetX;
        public static int OffSetY;

        public static int ViewRangeX;
        public static int ViewRangeY;

        private bool _autoPath;

        public bool AutoPath
        {
            get { return _autoPath; }
            set
            {
                if (_autoPath == value) return;
                _autoPath = value;

                if (!_autoPath)
                    CurrentPath = null;
            }
        }

        public PathFinder PathFinder;
        public List<Node> CurrentPath = null;

        public static Point MapLocation
        {
            get { return GameScene.User == null ? Point.Empty : new Point(MouseLocation.X / CellWidth - OffSetX, MouseLocation.Y / CellHeight - OffSetY).Add(GameScene.User.CurrentLocation); }
        }

        public static Point ToMouseLocation(Point p)
        {
            return new Point((p.X - MapObject.User.Movement.X + OffSetX) * CellWidth, (p.Y - MapObject.User.Movement.Y + OffSetY) * CellHeight).Add(MapObject.User.OffSetMove);
        }

        public static MouseButtons MapButtons;
        public static Point MouseLocation;
        public static long InputDelay;

        private static long nextAction;

        public static long NextAction
        {
            get { return nextAction; }
            set
            {
                if (GameScene.Observing) return;
                nextAction = value;
            }
        }

        public CellInfo[,] M2CellInfo;
        public List<Door> Doors = new List<Door>();
        public int Width, Height;

        public int Index;
        public string FileName = String.Empty;
        public string Title = String.Empty;
        public ushort MiniMap, BigMap, Music, SetMusic;
        public LightSetting Lights;
        public bool Lightning, Fire;
        public byte MapDarkLight;
        public long LightningTime, FireTime;
        public WeatherSetting Weather = WeatherSetting.None;
        public bool FloorValid, LightsValid;

        public long OutputDelay;

        private static bool _awakeningAction;

        public static bool AwakeningAction
        {
            get { return _awakeningAction; }
            set
            {
                if (_awakeningAction == value) return;
                _awakeningAction = value;
            }
        }

        private static bool _autoRun;

        public static bool AutoRun
        {
            get { return _autoRun; }
            set
            {
                if (_autoRun == value) return;
                _autoRun = value;
                if (GameScene.Scene != null)
                    GameScene.Scene.ChatDialog.ReceiveChat(value ? GameLanguage.ClientTextMap[nameof(ClientTextKeys.AutoRunOn)] : GameLanguage.ClientTextMap[nameof(ClientTextKeys.AutoRunOff)],
                        ChatType.Hint);
            }
        }

        public static bool AutoHit;

        public int AnimationCount;

        public static List<Effect> Effects = new List<Effect>();

        public MapControl()
        {
            MapButtons = MouseButtons.None;

            OffSetX = Settings.ScreenWidth / 2 / CellWidth;
            OffSetY = Settings.ScreenHeight / 2 / CellHeight - 1;

            ViewRangeX = OffSetX + 6;
            ViewRangeY = OffSetY + 6;

            Size = new Size(Settings.ScreenWidth, Settings.ScreenHeight);
            DrawControlTexture = true;
            BackColour = Color.Black;

            MouseDown += OnMouseDown;
            MouseMove += (o, e) => MouseLocation = e.Location;
            Click += OnMouseClick;
        }

        public void ResetMap()
        {
            GameScene.Scene.NPCDialog.Hide();

            MapObject.MouseObjectID = 0;
            MapObject.TargetObjectID = 0;
            MapObject.MagicObjectID = 0;

            if (M2CellInfo != null)
                for (var i = ObjectsList.Count - 1; i >= 0; i--)
                    ObjectsList[i]?.Remove();

            Objects.Clear();
            ObjectsList.Clear();
            Effects.Clear();
            Doors.Clear();

            if (User != null)
            {
                Objects[User.ObjectID] = User;
                ObjectsList.Add(User);
            }
        }

        public void LoadMap()
        {
            ResetMap();

            MapObject.MouseObjectID = 0;
            MapObject.TargetObjectID = 0;
            MapObject.MagicObjectID = 0;

            MapReader Map = new MapReader(FileName);
            M2CellInfo = Map.MapCells;
            Width = Map.Width;
            Height = Map.Height;

            PathFinder = new PathFinder(this);

            try
            {
                if (SetMusic != Music)
                {
                    SoundManager.Music?.Dispose();
                    SoundManager.PlayMusic(Music, true);
                }
            }
            catch (Exception)
            {
                // Do nothing. index was not valid.
            }

            SetMusic = Music;
            SoundList.Music = Music;

            UpdateWeather();
        }


        public void Process()
        {
            Processdoors();
            User.Process();
            for (int i = ObjectsList.Count - 1; i >= 0; i--)
            {
                if (ObjectsList[i] == User) continue;
                ObjectsList[i].Process();
            }

            for (int i = Effects.Count - 1; i >= 0; i--)
                Effects[i].Process();

            if (MapObject.TargetObject != null && MapObject.TargetObject is MonsterObject && MapObject.TargetObject.AI == 64)
                MapObject.TargetObjectID = 0;
            if (MapObject.MagicObject != null && MapObject.MagicObject is MonsterObject && MapObject.MagicObject.AI == 64)
                MapObject.MagicObjectID = 0;

            CheckInput();


            MapObject bestmouseobject = null;
            Point mouseCell = MapLocation;
            
            for (int y = mouseCell.Y + 1; y >= mouseCell.Y - 1; y--)
            {
                if (y >= Height || y < 0) continue;

                for (int x = mouseCell.X + 1; x >= mouseCell.X - 1; x--)
                {
                    if (x >= Width || x < 0) continue;

                    CellInfo cell = M2CellInfo[x, y];
                    if (cell.CellObjects == null) continue;

                    for (int i = cell.CellObjects.Count - 1; i >= 0; i--)
                    {
                        MapObject ob = cell.CellObjects[i];
                        if (ob == MapObject.User || !ob.MouseOver(CMain.MPoint)) continue;

                        if (MapObject.MouseObject != ob)
                        {
                            if (ob.Dead)
                            {
                                if (!Settings.TargetDead && GameScene.TargetDeadTime <= CMain.Time) continue;
                                bestmouseobject = ob;
                            }

                            MapObject.MouseObjectID = ob.ObjectID;
                            Redraw();
                        }

                        if (bestmouseobject != null && MapObject.MouseObject == null)
                        {
                            MapObject.MouseObjectID = bestmouseobject.ObjectID;
                            Redraw();
                        }

                        return; 
                    }
                }
            }

            if (MapObject.MouseObject != null)
            {
                MapObject.MouseObjectID = 0;
                Redraw();
            }
        }

        public static MapObject GetObject(uint targetID)
        {
            Objects.TryGetValue(targetID, out var ob);
            return ob;
        }

        public override void Draw()
        {
            //Do nothing.
        }

        protected override void CreateTexture()
        {
            if (User == null) return;

            if (!FloorValid)
                DrawFloor();


            if (Size != TextureSize)
                DisposeTexture();

            if (ControlTexture == null || ControlTexture.Disposed)
            {
                DXManager.ControlList.Add(this);
                ControlTexture = new Texture(DXManager.Device, Size.Width, Size.Height, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default);
                TextureSize = Size;
            }

            Surface oldSurface = DXManager.CurrentSurface;
            Surface surface = ControlTexture.GetSurfaceLevel(0);
            DXManager.SetSurface(surface);
            DXManager.Device.Clear(ClearFlags.Target, BackColour, 0, 0);

            DrawBackground();

            if (FloorValid)
            {
                DXManager.Draw(DXManager.FloorTexture, new Rectangle(0, 0, Settings.ScreenWidth, Settings.ScreenHeight), Vector3.Zero, Color.White);
            }

            DrawObjects();

            //render weather
            foreach (ParticleEngine engine in GameScene.Scene.ParticleEngines)
            {
                engine.Draw();
            }

            //Render Death, 

            LightSetting setting = Lights == LightSetting.Normal ? GameScene.Scene.Lights : Lights;

            if (setting != LightSetting.Day || GameScene.User.Poison.HasFlag(PoisonType.Blindness))
            {
                DrawLights(setting);
            }

            if (Settings.DropView || GameScene.DropViewTime > CMain.Time)
            {
                foreach (var ob in Objects.Values.OfType<ItemObject>())
                {
                    if (!ob.MouseOver(MouseLocation))
                        ob.DrawName();
                }
            }

            if (MapObject.MouseObject != null && !(MapObject.MouseObject is ItemObject))
                MapObject.MouseObject.DrawName();

            int offSet = 0;

            if (Settings.DisplayBodyName)
            {
                foreach (var ob in Objects.Values.OfType<MonsterObject>())
                {
                    if (ob.MouseOver(MouseLocation))
                        ob.DrawName();
                }
            }

            foreach (var ob in Objects.Values.OfType<ItemObject>())
            {
                if (ob.MouseOver(MouseLocation))
                {
                    ob.DrawName(offSet);
                    offSet -= ob.NameLabel.Size.Height + (ob.NameLabel.Border ? 1 : 0);
                }
            }

            if (MapObject.User.MouseOver(MouseLocation))
                MapObject.User.DrawName();

            DXManager.SetSurface(oldSurface);
            surface.Dispose();
            TextureValid = true;
        }

        protected internal override void DrawControl()
        {
            if (!DrawControlTexture)
                return;

            if (!TextureValid)
                CreateTexture();

            if (ControlTexture == null || ControlTexture.Disposed)
                return;

            float oldOpacity = DXManager.Opacity;

            if (MapObject.User.Dead) DXManager.SetGrayscale(true);

            DXManager.DrawOpaque(ControlTexture, new Rectangle(0, 0, Settings.ScreenWidth, Settings.ScreenHeight), Vector3.Zero, Color.White, Opacity);

            if (MapObject.User.Dead) DXManager.SetGrayscale(false);

            CleanTime = CMain.Time + Settings.CleanDelay;
        }

        private void DrawFloor()
        {
            if (DXManager.FloorTexture == null || DXManager.FloorTexture.Disposed)
            {
                DXManager.FloorTexture = new Texture(DXManager.Device, Settings.ScreenWidth, Settings.ScreenHeight, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default);
                DXManager.FloorSurface = DXManager.FloorTexture.GetSurfaceLevel(0);
            }

            Surface oldSurface = DXManager.CurrentSurface;
            DXManager.SetSurface(DXManager.FloorSurface);
            DXManager.Device.Clear(ClearFlags.Target, Color.Empty, 0, 0);

            // 预计算范围
            int startX = User.Movement.X - ViewRangeX;
            int endX = User.Movement.X + ViewRangeX;
            int startY = User.Movement.Y - ViewRangeY;
            int endY = User.Movement.Y + ViewRangeY;
            int endYExtended = endY + 5;

            // 缓存坐标计算结果
            int[] drawXCache = new int[endX - startX + 1];
            for (int xi = startX; xi <= endX; xi++)
                drawXCache[xi - startX] = (xi - User.Movement.X + OffSetX) * CellWidth - OffSetX + User.OffSetMove.X;

            int[] drawYCache = new int[endYExtended - startY + 1];
            for (int yi = startY; yi <= endYExtended; yi++)
                drawYCache[yi - startY] = (yi - User.Movement.Y + OffSetY) * CellHeight + User.OffSetMove.Y;

            // 一次循环绘制三层
            for (int y = startY; y <= endYExtended; y++)
            {
                if (y <= 0) continue;
                if (y >= Height) break;

                int drawY = drawYCache[y - startY];

                for (int x = startX; x <= endX; x++)
                {
                    if (x < 0) continue;
                    if (x >= Width) break;

                    int drawX = drawXCache[x - startX];
                    var cell = M2CellInfo[x, y];

                    // Back 层（偶数行列且 y <= endY）
                    if (y % 2 == 0 && x % 2 == 0 && y <= endY)
                    {
                        if (cell.BackImage != 0 && cell.BackIndex != -1)
                        {
                            int index = (cell.BackImage & 0x1FFFFFFF) - 1;
                            var lib = Libraries.MapLibs[cell.BackIndex];
                            lib.Draw(index, drawX, drawY);
                        }
                    }

                    // Middle 层
                    int midIndex = cell.MiddleImage - 1;
                    if (midIndex >= 0 && cell.MiddleIndex != -1)
                    {
                        var lib = Libraries.MapLibs[cell.MiddleIndex];
                        Size s = lib.GetSize(midIndex);
                        if ((s.Width == CellWidth && s.Height == CellHeight) ||
                            (s.Width == CellWidth * 2 && s.Height == CellHeight * 2))
                        {
                            lib.Draw(midIndex, drawX, drawY);
                        }
                    }

                    // Front 层
                    int frontIndex = (cell.FrontImage & 0x7FFF) - 1;
                    if (frontIndex != -1)
                    {
                        int fileIndex = cell.FrontIndex;
                        if (fileIndex != -1 && fileIndex != 200)
                        {
                            var lib = Libraries.MapLibs[fileIndex];
                            Size s = lib.GetSize(frontIndex);

                            // 门动画处理
                            if (cell.DoorIndex > 0)
                            {
                                Door doorInfo = GetDoor(cell.DoorIndex);
                                if (doorInfo == null)
                                {
                                    doorInfo = new Door() { index = cell.DoorIndex, DoorState = 0, ImageIndex = 0, LastTick = CMain.Time };
                                    Doors.Add(doorInfo);
                                }
                                else if (doorInfo.DoorState != 0)
                                {
                                    frontIndex += (doorInfo.ImageIndex + 1) * cell.DoorOffset;
                                }
                            }

                            if (frontIndex >= 0 &&
                                ((s.Width == CellWidth && s.Height == CellHeight) ||
                                 (s.Width == CellWidth * 2 && s.Height == CellHeight * 2)))
                            {
                                lib.Draw(frontIndex, drawX, drawY);
                            }
                        }
                    }
                }
            }

            DXManager.SetSurface(oldSurface);
            FloorValid = true;
        }

        private void DrawBackground()
        {
            string cleanFilename = FileName.Replace(Settings.MapPath, "");

            if (cleanFilename.StartsWith("ID1") || cleanFilename.StartsWith("ID2"))
            {
                Libraries.Background.Draw(10, 0, 0); //mountains
            }
            else if (cleanFilename.StartsWith("ID3_013"))
            {
                Libraries.Background.Draw(22, 0, 0); //desert
            }
            else if (cleanFilename.StartsWith("ID3_015"))
            {
                Libraries.Background.Draw(23, 0, 0); //greatwall
            }
            else if (cleanFilename.StartsWith("ID3_023") || cleanFilename.StartsWith("ID3_025"))
            {
                Libraries.Background.Draw(21, 0, 0); //village entrance
            }
        }

        private void DrawObjects()
        {
            if (Settings.Effect)
            {
                for (int i = Effects.Count - 1; i >= 0; i--)
                {
                    if (!Effects[i].DrawBehind) continue;
                    Effects[i].Draw();
                }
            }

            for (int y = User.Movement.Y - ViewRangeY; y <= User.Movement.Y + ViewRangeY + 25; y++)
            {
                if (y <= 0) continue;
                if (y >= Height) break;
                for (int x = User.Movement.X - ViewRangeX; x <= User.Movement.X + ViewRangeX; x++)
                {
                    if (x < 0) continue;
                    if (x >= Width) break;
                    M2CellInfo[x, y].DrawDeadObjects();
                }
            }

            for (int y = User.Movement.Y - ViewRangeY; y <= User.Movement.Y + ViewRangeY + 25; y++)
            {
                if (y <= 0) continue;
                if (y >= Height) break;
                int drawY = (y - User.Movement.Y + OffSetY + 1) * CellHeight + User.OffSetMove.Y;

                for (int x = User.Movement.X - ViewRangeX; x <= User.Movement.X + ViewRangeX; x++)
                {
                    if (x < 0) continue;
                    if (x >= Width) break;
                    int drawX = (x - User.Movement.X + OffSetX) * CellWidth - OffSetX + User.OffSetMove.X;
                    int index;
                    byte animation;
                    bool blend;
                    Size s;

                    #region Draw shanda's tile animation layer

                    index = M2CellInfo[x, y].TileAnimationImage;
                    animation = M2CellInfo[x, y].TileAnimationFrames;
                    if ((index > 0) & (animation > 0))
                    {
                        index--;
                        int animationoffset = M2CellInfo[x, y].TileAnimationOffset ^ 0x2000;
                        index += animationoffset * (AnimationCount % animation);
                        Libraries.MapLibs[190].DrawUp(index, drawX, drawY);
                    }

                    #endregion

                    #region Draw mir3 middle layer

                    if ((M2CellInfo[x, y].MiddleIndex >= 0) &&
                        (M2CellInfo[x, y].MiddleIndex != -1)) //M2P '> 199' changed to '>= 0' to include mir2 libraries. Fixes middle layer tile strips draw. Also changed in 'DrawFloor' above.
                    {
                        index = M2CellInfo[x, y].MiddleImage - 1;
                        if (index > 0)
                        {
                            animation = M2CellInfo[x, y].MiddleAnimationFrame;
                            blend = false;
                            if ((animation > 0) && (animation < 255))
                            {
                                if ((animation & 0x0f) > 0)
                                {
                                    blend = true;
                                    animation &= 0x0f;
                                }

                                if (animation > 0)
                                {
                                    byte animationTick = M2CellInfo[x, y].MiddleAnimationTick;
                                    index += (AnimationCount % (animation + (animation * animationTick))) / (1 + animationTick);

                                    if (blend && (animation == 10 || animation == 8)) //diamond mines, abyss blends
                                    {
                                        Libraries.MapLibs[M2CellInfo[x, y].MiddleIndex].DrawUpBlend(index, new Point(drawX, drawY));
                                    }
                                    else
                                    {
                                        Libraries.MapLibs[M2CellInfo[x, y].MiddleIndex].DrawUp(index, drawX, drawY);
                                    }
                                }
                            }

                            s = Libraries.MapLibs[M2CellInfo[x, y].MiddleIndex].GetSize(index);
                            if ((s.Width != CellWidth || s.Height != CellHeight) && (s.Width != (CellWidth * 2) || s.Height != (CellHeight * 2)) && !blend)
                            {
                                Libraries.MapLibs[M2CellInfo[x, y].MiddleIndex].DrawUp(index, drawX, drawY);
                            }
                        }
                    }

                    #endregion

                    #region Draw front layer

                    index = (M2CellInfo[x, y].FrontImage & 0x7FFF) - 1;

                    if (index < 0) continue;

                    int fileIndex = M2CellInfo[x, y].FrontIndex;
                    if (fileIndex == -1) continue;
                    animation = M2CellInfo[x, y].FrontAnimationFrame;

                    if ((animation & 0x80) > 0)
                    {
                        blend = true;
                        animation &= 0x7F;
                    }
                    else
                        blend = false;


                    if (animation > 0)
                    {
                        byte animationTick = M2CellInfo[x, y].FrontAnimationTick;
                        index += (AnimationCount % (animation + (animation * animationTick))) / (1 + animationTick);
                    }


                    if (M2CellInfo[x, y].DoorIndex > 0)
                    {
                        Door DoorInfo = GetDoor(M2CellInfo[x, y].DoorIndex);
                        if (DoorInfo == null)
                        {
                            DoorInfo = new Door() { index = M2CellInfo[x, y].DoorIndex, DoorState = 0, ImageIndex = 0, LastTick = CMain.Time };
                            Doors.Add(DoorInfo);
                        }
                        else
                        {
                            if (DoorInfo.DoorState != 0)
                            {
                                index += (DoorInfo.ImageIndex + 1) *
                                         M2CellInfo[x, y].DoorOffset; //'bad' code if you want to use animation but it's gonna depend on the animation > has to be custom designed for the animtion
                            }
                        }
                    }

                    s = Libraries.MapLibs[fileIndex].GetSize(index);
                    if (s.Width == CellWidth && s.Height == CellHeight && animation == 0) continue;
                    if ((s.Width == CellWidth * 2) && (s.Height == CellHeight * 2) && (animation == 0)) continue;

                    if (blend)
                    {
                        if (fileIndex == 14 || fileIndex == 27 || (fileIndex > 99 & fileIndex < 199))
                            Libraries.MapLibs[fileIndex].DrawBlend(index, new Point(drawX, drawY - (3 * CellHeight)), Color.White, true);
                        else
                            Libraries.MapLibs[fileIndex].DrawBlend(index, new Point(drawX, drawY - s.Height), Color.White, (index >= 2723 && index <= 2732));
                    }
                    else
                    {
                        if (fileIndex == 28 && Libraries.MapLibs[fileIndex].GetOffSet(index) != Point.Empty)
                            Libraries.MapLibs[fileIndex].Draw(index, new Point(drawX, drawY - CellHeight), Color.White, true);
                        else
                            Libraries.MapLibs[fileIndex].Draw(index, drawX, drawY - s.Height);
                    }

                    #endregion
                }

                for (int x = User.Movement.X - ViewRangeX; x <= User.Movement.X + ViewRangeX; x++)
                {
                    if (x < 0) continue;
                    if (x >= Width) break;
                    M2CellInfo[x, y].DrawObjects();
                }
            }

            DXManager.Sprite.Flush();
            float oldOpacity = DXManager.Opacity;
            DXManager.SetOpacity(0.4F);

            //MapObject.User.DrawMount();

            MapObject.User.DrawBody();

            if ((MapObject.User.Direction == MirDirection.Up) ||
                (MapObject.User.Direction == MirDirection.UpLeft) ||
                (MapObject.User.Direction == MirDirection.UpRight) ||
                (MapObject.User.Direction == MirDirection.Right) ||
                (MapObject.User.Direction == MirDirection.Left))
            {
                MapObject.User.DrawHead();
                MapObject.User.DrawWings();
            }
            else
            {
                MapObject.User.DrawWings();
                MapObject.User.DrawHead();
            }

            DXManager.SetOpacity(oldOpacity);

            if (Settings.HighlightTarget)
            {
                if (MapObject.MouseObject != null && !MapObject.MouseObject.Dead && MapObject.MouseObject != MapObject.TargetObject && MapObject.MouseObject.Blend)
                    MapObject.MouseObject.DrawBlend();

                if (MapObject.TargetObject != null)
                    MapObject.TargetObject.DrawBlend();
            }

            if (Settings.Effect)
            {
                for (int i = Effects.Count - 1; i >= 0; i--)
                {
                    if (Effects[i].DrawBehind) continue;
                    Effects[i].Draw();
                }
            }

            foreach (var ob in Objects.Values)
            {
                ob.DrawEffects(Settings.Effect);

                if (Settings.NameView && !(ob is ItemObject) && !ob.Dead)
                    ob.DrawName();

                ob.DrawChat();
                //ob.DrawHealth();
                ob.DrawPoison();
                ob.DrawDamages();
            }

            foreach (var ob in Objects.Values)
            {
                ob.DrawHealth();
            }
        }

        private Color GetBlindLight(Color light)
        {
            if (MapObject.User.BlindTime <= CMain.Time && MapObject.User.BlindCount < 25)
            {
                MapObject.User.BlindTime = CMain.Time + 100;
                MapObject.User.BlindCount++;
            }

            int count = MapObject.User.BlindCount;
            light = Color.FromArgb(255, Math.Max(20, light.R - (count * 10)), Math.Max(20, light.G - (count * 10)), Math.Max(20, light.B - (count * 10)));

            return light;
        }

        private void DrawLights(LightSetting setting)
        {
            if (DXManager.Lights == null || DXManager.Lights.Count == 0) return;

            if (DXManager.LightTexture == null || DXManager.LightTexture.Disposed)
            {
                DXManager.LightTexture = new Texture(DXManager.Device, Settings.ScreenWidth, Settings.ScreenHeight, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default);
                DXManager.LightSurface = DXManager.LightTexture.GetSurfaceLevel(0);
            }

            Surface oldSurface = DXManager.CurrentSurface;
            DXManager.SetSurface(DXManager.LightSurface);

            #region Night Lights

            Color darkness;

            switch (setting)
            {
                case LightSetting.Night:
                {
                    switch (MapDarkLight)
                    {
                        case 1:
                            darkness = Color.FromArgb(255, 20, 20, 20);
                            break;
                        case 2:
                            darkness = Color.LightSlateGray;
                            break;
                        case 3:
                            darkness = Color.SkyBlue;
                            break;
                        case 4:
                            darkness = Color.Goldenrod;
                            break;
                        default:
                            darkness = Color.Black;
                            break;
                    }
                }
                    break;
                case LightSetting.Evening:
                case LightSetting.Dawn:
                    darkness = Color.FromArgb(255, 50, 50, 50);
                    break;
                default:
                case LightSetting.Day:
                    darkness = Color.FromArgb(255, 255, 255, 255);
                    break;
            }

            if (MapObject.User.Poison.HasFlag(PoisonType.Blindness))
            {
                darkness = GetBlindLight(darkness);
            }

            DXManager.Device.Clear(ClearFlags.Target, darkness, 0, 0);

            #endregion

            int light;
            Point p;
            DXManager.SetBlend(true);
            DXManager.Device.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
            DXManager.Device.SetRenderState(RenderState.DestinationBlend, Blend.One);

            #region Object Lights (Player/Mob/NPC)

            // 优化：只处理视野范围内的光源对象
            // 计算最大光照范围（DXManager.Lights.Count - 1 通常是最大光照范围）
            int maxLightRange = DXManager.Lights.Count - 1 + 3; // 额外3格缓冲

            foreach (var ob in Objects.Values)
            {
                if (ob.Light > 0 && (!ob.Dead || ob == MapObject.User || ob.Race == ObjectType.Spell))
                {
                    // 空间剔除：只处理视野范围内的光源
                    int dx = Math.Abs(ob.CurrentLocation.X - MapObject.User.Movement.X);
                    int dy = Math.Abs(ob.CurrentLocation.Y - MapObject.User.Movement.Y);

                    if (dx > ViewRangeX + maxLightRange || dy > ViewRangeY + maxLightRange)
                        continue; // 跳过视野外的光源

                    light = ob.Light;

                    int lightRange = light % 15;
                    if (lightRange >= DXManager.Lights.Count)
                        lightRange = DXManager.Lights.Count - 1;

                    p = ob.DrawLocation;

                    Color lightColour = ob.LightColour;

                    if (ob.Race == ObjectType.Player)
                    {
                        switch (light / 15)
                        {
                            case 0: //no light source
                                lightColour = Color.FromArgb(255, 60, 60, 60);
                                break;
                            case 1:
                                lightColour = Color.FromArgb(255, 120, 120, 120);
                                break;
                            case 2: //Candle
                                lightColour = Color.FromArgb(255, 180, 180, 180);
                                break;
                            case 3: //Torch
                                lightColour = Color.FromArgb(255, 240, 240, 240);
                                break;
                            default: //Peddler Torch
                                lightColour = Color.FromArgb(255, 255, 255, 255);
                                break;
                        }
                    }
                    else if (ob.Race == ObjectType.Merchant)
                    {
                        lightColour = Color.FromArgb(255, 120, 120, 120);
                    }

                    if (MapObject.User.Poison.HasFlag(PoisonType.Blindness))
                    {
                        lightColour = GetBlindLight(lightColour);
                    }

                    if (DXManager.Lights[lightRange] != null && !DXManager.Lights[lightRange].Disposed)
                    {
                        p.Offset(-(DXManager.LightSizes[lightRange].X / 2) - (CellWidth / 2), -(DXManager.LightSizes[lightRange].Y / 2) - (CellHeight / 2) - 5);
                        DXManager.Draw(DXManager.Lights[lightRange], null, new Vector3((float)p.X, (float)p.Y, 0.0F), lightColour);
                    }
                }

                #region Object Effect Lights

                if (!Settings.Effect) continue;
                for (int e = 0; e < ob.Effects.Count; e++)
                {
                    Effect effect = ob.Effects[e];
                    if (!effect.Blend || CMain.Time < effect.Start || (!(effect is Missile) && effect.Light < ob.Light)) continue;

                    light = effect.Light;

                    p = effect.DrawLocation;

                    var lightColour = effect.LightColour;

                    if (MapObject.User.Poison.HasFlag(PoisonType.Blindness))
                    {
                        lightColour = GetBlindLight(lightColour);
                    }

                    if (DXManager.Lights[light] != null && !DXManager.Lights[light].Disposed)
                    {
                        p.Offset(-(DXManager.LightSizes[light].X / 2) - (CellWidth / 2), -(DXManager.LightSizes[light].Y / 2) - (CellHeight / 2) - 5);
                        DXManager.Draw(DXManager.Lights[light], null, new Vector3((float)p.X, (float)p.Y, 0.0F), lightColour);
                    }
                }

                #endregion
            }

            #endregion

            #region Map Effect Lights

            if (Settings.Effect)
            {
                for (int e = 0; e < Effects.Count; e++)
                {
                    Effect effect = Effects[e];
                    if (!effect.Blend || CMain.Time < effect.Start) continue;

                    light = effect.Light;
                    if (light == 0) continue;

                    p = effect.DrawLocation;

                    var lightColour = Color.White;

                    if (MapObject.User.Poison.HasFlag(PoisonType.Blindness))
                    {
                        lightColour = GetBlindLight(lightColour);
                    }

                    if (DXManager.Lights[light] != null && !DXManager.Lights[light].Disposed)
                    {
                        p.Offset(-(DXManager.LightSizes[light].X / 2) - (CellWidth / 2), -(DXManager.LightSizes[light].Y / 2) - (CellHeight / 2) - 5);
                        DXManager.Draw(DXManager.Lights[light], null, new Vector3((float)p.X, (float)p.Y, 0.0F), lightColour);
                    }
                }
            }

            #endregion

            #region Map Lights

            // 优化：缩小地图光源扫描范围，根据实际视野范围计算
            // 原来是 ±24，现在根据 ViewRange 动态计算
            int mapLightExtraRange = maxLightRange; // 使用和对象光源相同的缓冲范围

            for (int y = MapObject.User.Movement.Y - ViewRangeY - mapLightExtraRange; y <= MapObject.User.Movement.Y + ViewRangeY + mapLightExtraRange; y++)
            {
                if (y < 0) continue;
                if (y >= Height) break;
                for (int x = MapObject.User.Movement.X - ViewRangeX - mapLightExtraRange; x < MapObject.User.Movement.X + ViewRangeX + mapLightExtraRange; x++)
                {
                    if (x < 0) continue;
                    if (x >= Width) break;
                    int imageIndex = (M2CellInfo[x, y].FrontImage & 0x7FFF) - 1;
                    if (imageIndex == -1) continue;
                    int fileIndex = M2CellInfo[x, y].FrontIndex;
                    if (fileIndex == -1) continue;
                    if (M2CellInfo[x, y].Light <= 0 || M2CellInfo[x, y].Light >= 10) continue;
                    if (M2CellInfo[x, y].Light == 0) continue;

                    Color lightIntensity;

                    light = (M2CellInfo[x, y].Light % 10) * 3;

                    switch (M2CellInfo[x, y].Light / 10)
                    {
                        case 1:
                            lightIntensity = Color.FromArgb(255, 255, 255, 255);
                            break;
                        case 2:
                            lightIntensity = Color.FromArgb(255, 120, 180, 255);
                            break;
                        case 3:
                            lightIntensity = Color.FromArgb(255, 255, 180, 120);
                            break;
                        case 4:
                            lightIntensity = Color.FromArgb(255, 22, 160, 5);
                            break;
                        default:
                            lightIntensity = Color.FromArgb(255, 255, 255, 255);
                            break;
                    }

                    if (MapObject.User.Poison.HasFlag(PoisonType.Blindness))
                    {
                        lightIntensity = GetBlindLight(lightIntensity);
                    }

                    p = new Point(
                        (x + OffSetX - MapObject.User.Movement.X) * CellWidth + MapObject.User.OffSetMove.X,
                        (y + OffSetY - MapObject.User.Movement.Y) * CellHeight + MapObject.User.OffSetMove.Y + 32);


                    if (M2CellInfo[x, y].FrontAnimationFrame > 0)
                        p.Offset(Libraries.MapLibs[fileIndex].GetOffSet(imageIndex));

                    if (light >= DXManager.Lights.Count)
                        light = DXManager.Lights.Count - 1;

                    if (DXManager.Lights[light] != null && !DXManager.Lights[light].Disposed)
                    {
                        p.Offset(-(DXManager.LightSizes[light].X / 2) - (CellWidth / 2) + 10, -(DXManager.LightSizes[light].Y / 2) - (CellHeight / 2) - 5);
                        DXManager.Draw(DXManager.Lights[light], null, new Vector3((float)p.X, (float)p.Y, 0.0F), lightIntensity);
                    }
                }
            }

            #endregion

            DXManager.SetBlend(false);
            DXManager.SetSurface(oldSurface);

            DXManager.Device.SetRenderState(RenderState.SourceBlend, Blend.Zero);
            DXManager.Device.SetRenderState(RenderState.DestinationBlend, Blend.SourceColor);

            DXManager.Draw(DXManager.LightTexture, new Rectangle(0, 0, Settings.ScreenWidth, Settings.ScreenHeight), Vector3.Zero, Color.White);

            DXManager.Sprite.End();
            DXManager.Sprite.Begin(SpriteFlags.AlphaBlend);
        }

        private static void OnMouseClick(object sender, EventArgs e)
        {
            if (!(e is MouseEventArgs me)) return;

            if (AwakeningAction == true) return;
            switch (me.Button)
            {
                case MouseButtons.Left:
                {
                    AutoRun = false;
                    GameScene.Scene.MapControl.AutoPath = false;
                    if (MapObject.MouseObject == null) return;
                    NPCObject npc = MapObject.MouseObject as NPCObject;
                    if (npc != null)
                    {
                        if (npc.ObjectID == GameScene.NPCID &&
                            (CMain.Time <= GameScene.NPCTime || GameScene.Scene.NPCDialog.Visible))
                        {
                            return;
                        }

                        //GameScene.Scene.NPCDialog.Hide();

                        GameScene.NPCTime = CMain.Time + 5000;
                        GameScene.NPCID = npc.ObjectID;
                        Network.Enqueue(new C.CallNPC { ObjectID = npc.ObjectID, Key = "[@Main]" });
                    }
                }
                    break;
                case MouseButtons.Right:
                {
                    AutoRun = false;
                    if (MapObject.MouseObject == null)
                    {
                        if (Settings.NewMove && MapLocation != MapObject.User.CurrentLocation && GameScene.Scene.MapControl.EmptyCell(MapLocation))
                        {
                            var path = GameScene.Scene.MapControl.PathFinder.FindPath(MapObject.User.CurrentLocation, MapLocation, 20);

                            if (path != null && path.Count > 0)
                            {
                                GameScene.Scene.MapControl.CurrentPath = path;
                                GameScene.Scene.MapControl.AutoPath = true;
                                var offset = MouseLocation.Subtract(ToMouseLocation(MapLocation));
                                Effects.Add(new Effect(Libraries.Magic3, 500, 10, 600, MapLocation) { DrawOffset = offset.Subtract(8, 15) });
                            }
                        }

                        return;
                    }

                    if (CMain.Ctrl)
                    {
                        HeroObject hero = MapObject.MouseObject as HeroObject;

                        if (hero != null &&
                            hero.ObjectID != (Hero is null ? 0 : Hero.ObjectID) &&
                            CMain.Time >= GameScene.InspectTime)
                        {
                            GameScene.InspectTime = CMain.Time + 500;
                            InspectDialog.InspectID = hero.ObjectID;
                            Network.Enqueue(new C.Inspect { ObjectID = hero.ObjectID, Hero = true });
                            return;
                        }

                        PlayerObject player = MapObject.MouseObject as PlayerObject;

                        if (player != null &&
                            player != User &&
                            CMain.Time >= GameScene.InspectTime)
                        {
                            GameScene.InspectTime = CMain.Time + 500;
                            InspectDialog.InspectID = player.ObjectID;
                            Network.Enqueue(new C.Inspect { ObjectID = player.ObjectID });
                            return;
                        }
                    }
                }
                    break;
                case MouseButtons.Middle:
                    AutoRun = !AutoRun;
                    break;
            }
        }

        private static void OnMouseDown(object sender, MouseEventArgs e)
        {
            MapButtons |= e.Button;
            if (e.Button != MouseButtons.Right || !Settings.NewMove)
                GameScene.CanRun = false;

            if (AwakeningAction == true) return;

            if (e.Button != MouseButtons.Left) return;

            if (GameScene.SelectedCell != null)
            {
                if (GameScene.SelectedCell.GridType != MirGridType.Inventory && GameScene.SelectedCell.GridType != MirGridType.HeroInventory)
                {
                    GameScene.SelectedCell = null;
                    return;
                }

                MirItemCell cell = GameScene.SelectedCell;
                if (cell.Item.Info.Bind.HasFlag(BindMode.DontDrop))
                {
                    MirMessageBox messageBox = new MirMessageBox(string.Format(GameLanguage.ClientTextMap[nameof(ClientTextKeys.YouCannotDrop)], cell.Item.FriendlyName), MirMessageBoxButtons.OK);
                    messageBox.Show();
                    GameScene.SelectedCell = null;
                    return;
                }

                if (cell.Item.Count == 1)
                {
                    MirMessageBox messageBox = new MirMessageBox(string.Format(GameLanguage.ClientTextMap[nameof(ClientTextKeys.DropTip)], cell.Item.FriendlyName), MirMessageBoxButtons.YesNo);

                    messageBox.YesButton.Click += (o, a) =>
                    {
                        Network.Enqueue(new C.DropItem
                        {
                            UniqueID = cell.Item.UniqueID,
                            Count = 1,
                            HeroInventory = cell.GridType == MirGridType.HeroInventory
                        });

                        cell.Locked = true;
                    };
                    messageBox.Show();
                }
                else
                {
                    MirAmountBox amountBox = new MirAmountBox(GameLanguage.ClientTextMap[nameof(ClientTextKeys.DropAmount)], cell.Item.Info.Image, cell.Item.Count);

                    amountBox.OKButton.Click += (o, a) =>
                    {
                        if (amountBox.Amount <= 0) return;
                        Network.Enqueue(new C.DropItem
                        {
                            UniqueID = cell.Item.UniqueID,
                            Count = (ushort)amountBox.Amount,
                            HeroInventory = cell.GridType == MirGridType.HeroInventory
                        });

                        cell.Locked = true;
                    };

                    amountBox.Show();
                }

                GameScene.SelectedCell = null;

                return;
            }

            if (GameScene.PickedUpGold)
            {
                MirAmountBox amountBox = new MirAmountBox(GameLanguage.ClientTextMap[nameof(ClientTextKeys.DropAmount)], 116, GameScene.Gold);

                amountBox.OKButton.Click += (o, a) =>
                {
                    if (amountBox.Amount > 0)
                    {
                        Network.Enqueue(new C.DropGold { Amount = amountBox.Amount });
                    }
                };

                amountBox.Show();
                GameScene.PickedUpGold = false;
            }

            if (MapObject.MouseObject != null && !MapObject.MouseObject.Dead && !(MapObject.MouseObject is ItemObject) &&
                !(MapObject.MouseObject is NPCObject) && !(MapObject.MouseObject is MonsterObject && MapObject.MouseObject.AI == 64)
                && !(MapObject.MouseObject is MonsterObject && MapObject.MouseObject.AI == 70))
            {
                MapObject.TargetObjectID = MapObject.MouseObject.ObjectID;
                if (MapObject.MouseObject is MonsterObject && MapObject.MouseObject.AI != 6)
                    MapObject.MagicObjectID = MapObject.TargetObject.ObjectID;
            }
            else
                MapObject.TargetObjectID = 0;
        }

        private void CheckInput()
        {
            if (AwakeningAction == true) return;

            if ((MouseControl == this) && (MapButtons != MouseButtons.None)) AutoHit = false; //mouse actions stop mining even when frozen!
            if (!CanRideAttack()) AutoHit = false;

            if (CMain.Time < InputDelay || User.Poison.HasFlag(PoisonType.Paralysis) || User.Poison.HasFlag(PoisonType.LRParalysis) || User.Poison.HasFlag(PoisonType.Frozen) || User.Fishing) return;

            if (User.NextMagic != null && !User.RidingMount)
            {
                UseMagic(User.NextMagic, User);
                return;
            }

            if (CMain.Time < User.BlizzardStopTime || CMain.Time < User.ReincarnationStopTime) return;

            if (MapObject.TargetObject != null && !MapObject.TargetObject.Dead)
            {
                if (((MapObject.TargetObject.Name.EndsWith(")") || MapObject.TargetObject is PlayerObject) && CMain.Shift) ||
                    (!MapObject.TargetObject.Name.EndsWith(")") && MapObject.TargetObject is MonsterObject))
                {
                    GameScene.LogTime = CMain.Time + Globals.LogDelay;

                    if (User.Class == MirClass.Archer && User.HasClassWeapon && !User.RidingMount && !User.Fishing) //ArcherTest - non aggressive targets (player / pets)
                    {
                        if (Functions.InRange(MapObject.TargetObject.CurrentLocation, User.CurrentLocation, Globals.MaxAttackRange))
                        {
                            if (CMain.Time > GameScene.AttackTime)
                            {
                                User.QueuedAction = new QueuedAction
                                {
                                    Action = MirAction.AttackRange1,
                                    Direction = Functions.DirectionFromPoint(User.CurrentLocation, MapObject.TargetObject.CurrentLocation),
                                    Location = User.CurrentLocation,
                                    Params = new List<object>()
                                };
                                User.QueuedAction.Params.Add(MapObject.TargetObject != null ? MapObject.TargetObject.ObjectID : (uint)0);
                                User.QueuedAction.Params.Add(MapObject.TargetObject.CurrentLocation);

                                // MapObject.TargetObject = null; //stop constant attack when close up
                            }
                        }
                        else
                        {
                            if (CMain.Time >= OutputDelay)
                            {
                                OutputDelay = CMain.Time + 1000;
                                GameScene.Scene.OutputMessage(GameLanguage.ClientTextMap[nameof(ClientTextKeys.TargetTooFar)]);
                            }
                        }
                        //  return;
                    }

                    else if (Functions.InRange(MapObject.TargetObject.CurrentLocation, User.CurrentLocation, 1))
                    {
                        if (CMain.Time > GameScene.AttackTime && CanRideAttack() && !User.Poison.HasFlag(PoisonType.Dazed))
                        {
                            User.QueuedAction = new QueuedAction
                                { Action = MirAction.Attack1, Direction = Functions.DirectionFromPoint(User.CurrentLocation, MapObject.TargetObject.CurrentLocation), Location = User.CurrentLocation };
                            return;
                        }
                    }
                }
            }

            if (AutoHit && !User.RidingMount)
            {
                if (CMain.Time > GameScene.AttackTime)
                {
                    User.QueuedAction = new QueuedAction { Action = MirAction.Mine, Direction = User.Direction, Location = User.CurrentLocation };
                    return;
                }
            }


            MirDirection direction;
            if (MouseControl == this)
            {
                direction = MouseDirection();
                if (AutoRun)
                {
                    if (GameScene.CanRun && CanRun(direction) && CMain.Time > GameScene.NextRunTime && User.HP >= 10 && (!User.Sneaking || (User.Sneaking && User.Sprint))) //slow remove
                    {
                        int distance = User.RidingMount || User.Sprint && !User.Sneaking ? 3 : 2;
                        bool fail = false;
                        for (int i = 1; i <= distance; i++)
                        {
                            if (!CheckDoorOpen(Functions.PointMove(User.CurrentLocation, direction, i)))
                                fail = true;
                        }

                        if (!fail)
                        {
                            User.QueuedAction = new QueuedAction { Action = MirAction.Running, Direction = direction, Location = Functions.PointMove(User.CurrentLocation, direction, distance) };
                            return;
                        }
                    }

                    if ((CanWalk(direction, out direction)) && (CheckDoorOpen(Functions.PointMove(User.CurrentLocation, direction, 1))))
                    {
                        User.QueuedAction = new QueuedAction { Action = MirAction.Walking, Direction = direction, Location = Functions.PointMove(User.CurrentLocation, direction, 1) };
                        return;
                    }

                    if (direction != User.Direction)
                    {
                        User.QueuedAction = new QueuedAction { Action = MirAction.Standing, Direction = direction, Location = User.CurrentLocation };
                        return;
                    }

                    return;
                }

                switch (MapButtons)
                {
                    case MouseButtons.Left:
                        if (MapObject.MouseObject is NPCObject || (MapObject.MouseObject is PlayerObject && MapObject.MouseObject != User)) break;
                        if (MapObject.MouseObject is MonsterObject && MapObject.MouseObject.AI == 70) break;

                        if (CMain.Alt && !User.RidingMount)
                        {
                            User.QueuedAction = new QueuedAction { Action = MirAction.Harvest, Direction = direction, Location = User.CurrentLocation };
                            return;
                        }

                        if (CMain.Shift)
                        {
                            if (CMain.Time > GameScene.AttackTime && CanRideAttack()) //ArcherTest - shift click
                            {
                                MapObject target = null;
                                if (MapObject.MouseObject is MonsterObject || MapObject.MouseObject is PlayerObject) target = MapObject.MouseObject;

                                if (User.Class == MirClass.Archer && User.HasClassWeapon && !User.RidingMount && !User.Poison.HasFlag(PoisonType.Dazed))
                                {
                                    if (target != null)
                                    {
                                        if (!Functions.InRange(MapObject.MouseObject.CurrentLocation, User.CurrentLocation, Globals.MaxAttackRange))
                                        {
                                            if (CMain.Time >= OutputDelay)
                                            {
                                                OutputDelay = CMain.Time + 1000;
                                                GameScene.Scene.OutputMessage(GameLanguage.ClientTextMap[nameof(ClientTextKeys.TargetTooFar)]);
                                            }

                                            return;
                                        }
                                    }

                                    User.QueuedAction = new QueuedAction
                                        { Action = MirAction.AttackRange1, Direction = MouseDirection(), Location = User.CurrentLocation, Params = new List<object>() };
                                    User.QueuedAction.Params.Add(target != null ? target.ObjectID : (uint)0);
                                    User.QueuedAction.Params.Add(Functions.PointMove(User.CurrentLocation, MouseDirection(), 9));
                                    return;
                                }

                                //stops double slash from being used without empty hand or assassin weapon (otherwise bugs on second swing)
                                if (GameScene.User.DoubleSlash && (!User.HasClassWeapon && User.Weapon > -1)) return;
                                if (User.Poison.HasFlag(PoisonType.Dazed)) return;

                                User.QueuedAction = new QueuedAction { Action = MirAction.Attack1, Direction = direction, Location = User.CurrentLocation };
                            }

                            return;
                        }

                        if (MapObject.MouseObject is MonsterObject && User.Class == MirClass.Archer && MapObject.TargetObject != null && !MapObject.TargetObject.Dead && User.HasClassWeapon &&
                            !User.RidingMount) //ArcherTest - range attack
                        {
                            if (Functions.InRange(MapObject.MouseObject.CurrentLocation, User.CurrentLocation, Globals.MaxAttackRange))
                            {
                                if (CMain.Time > GameScene.AttackTime)
                                {
                                    User.QueuedAction = new QueuedAction { Action = MirAction.AttackRange1, Direction = direction, Location = User.CurrentLocation, Params = new List<object>() };
                                    User.QueuedAction.Params.Add(MapObject.TargetObject.ObjectID);
                                    User.QueuedAction.Params.Add(MapObject.TargetObject.CurrentLocation);
                                }
                            }
                            else
                            {
                                if (CMain.Time >= OutputDelay)
                                {
                                    OutputDelay = CMain.Time + 1000;
                                    GameScene.Scene.OutputMessage(GameLanguage.ClientTextMap[nameof(ClientTextKeys.TargetTooFar)]);
                                }
                            }

                            return;
                        }

                        if (MapLocation == User.CurrentLocation)
                        {
                            if (CMain.Time > GameScene.PickUpTime)
                            {
                                GameScene.PickUpTime = CMain.Time + 200;
                                Network.Enqueue(new C.PickUp());
                            }

                            return;
                        }

                        //mine
                        if (!ValidPoint(Functions.PointMove(User.CurrentLocation, direction, 1)))
                        {
                            if ((MapObject.User.Equipment[(int)EquipmentSlot.Weapon] != null) && (MapObject.User.Equipment[(int)EquipmentSlot.Weapon].Info.CanMine))
                            {
                                if (direction != User.Direction)
                                {
                                    User.QueuedAction = new QueuedAction { Action = MirAction.Standing, Direction = direction, Location = User.CurrentLocation };
                                    return;
                                }

                                AutoHit = true;
                                return;
                            }
                        }

                        if ((CanWalk(direction, out direction)) && (CheckDoorOpen(Functions.PointMove(User.CurrentLocation, direction, 1))))
                        {
                            User.QueuedAction = new QueuedAction { Action = MirAction.Walking, Direction = direction, Location = Functions.PointMove(User.CurrentLocation, direction, 1) };
                            return;
                        }

                        if (direction != User.Direction)
                        {
                            User.QueuedAction = new QueuedAction { Action = MirAction.Standing, Direction = direction, Location = User.CurrentLocation };
                            return;
                        }

                        if (CanFish(direction))
                        {
                            User.FishingTime = CMain.Time;
                            Network.Enqueue(new C.FishingCast { CastOut = true });
                            return;
                        }

                        break;
                    case MouseButtons.Right:
                        if (MapObject.MouseObject is PlayerObject && MapObject.MouseObject != User && CMain.Ctrl) break;
                        if (Settings.NewMove) break;

                        if (Functions.InRange(MapLocation, User.CurrentLocation, 2))
                        {
                            if (direction != User.Direction)
                            {
                                User.QueuedAction = new QueuedAction { Action = MirAction.Standing, Direction = direction, Location = User.CurrentLocation };
                            }

                            return;
                        }

                        GameScene.CanRun = User.FastRun ? true : GameScene.CanRun;

                        if (GameScene.CanRun && CanRun(direction) && CMain.Time > GameScene.NextRunTime && User.HP >= 10 && (!User.Sneaking || (User.Sneaking && User.Sprint))) //slow removed
                        {
                            int distance = User.RidingMount || User.Sprint && !User.Sneaking ? 3 : 2;
                            bool fail = false;
                            for (int i = 0; i <= distance; i++)
                            {
                                if (!CheckDoorOpen(Functions.PointMove(User.CurrentLocation, direction, i)))
                                    fail = true;
                            }

                            if (!fail)
                            {
                                User.QueuedAction = new QueuedAction
                                {
                                    Action = MirAction.Running,
                                    Direction = direction,
                                    Location = Functions.PointMove(User.CurrentLocation, direction, User.RidingMount || (User.Sprint && !User.Sneaking) ? 3 : 2)
                                };
                                return;
                            }
                        }

                        if ((CanWalk(direction, out direction)) && (CheckDoorOpen(Functions.PointMove(User.CurrentLocation, direction, 1))))
                        {
                            User.QueuedAction = new QueuedAction { Action = MirAction.Walking, Direction = direction, Location = Functions.PointMove(User.CurrentLocation, direction, 1) };
                            return;
                        }

                        if (direction != User.Direction)
                        {
                            User.QueuedAction = new QueuedAction { Action = MirAction.Standing, Direction = direction, Location = User.CurrentLocation };
                            return;
                        }

                        break;
                }
            }

            if (AutoPath)
            {
                if (CurrentPath == null || CurrentPath.Count == 0)
                {
                    AutoPath = false;
                    return;
                }

                var path = GameScene.Scene.MapControl.PathFinder.FindPath(MapObject.User.CurrentLocation, CurrentPath.Last().Location);

                if (path != null && path.Count > 0)
                    GameScene.Scene.MapControl.CurrentPath = path;
                else
                {
                    AutoPath = false;
                    return;
                }

                Node currentNode = CurrentPath.SingleOrDefault(x => User.CurrentLocation == x.Location);
                if (currentNode != null)
                {
                    while (true)
                    {
                        Node first = CurrentPath.First();
                        CurrentPath.Remove(first);

                        if (first == currentNode)
                            break;
                    }
                }

                if (CurrentPath.Count > 0)
                {
                    MirDirection dir = Functions.DirectionFromPoint(User.CurrentLocation, CurrentPath.First().Location);

                    if (GameScene.CanRun && CanRun(dir) && CMain.Time > GameScene.NextRunTime && User.HP >= 10 && CurrentPath.Count > (User.RidingMount ? 2 : 1))
                    {
                        User.QueuedAction = new QueuedAction { Action = MirAction.Running, Direction = dir, Location = Functions.PointMove(User.CurrentLocation, dir, User.RidingMount ? 3 : 2) };
                        return;
                    }

                    if (CanWalk(dir))
                    {
                        User.QueuedAction = new QueuedAction { Action = MirAction.Walking, Direction = dir, Location = Functions.PointMove(User.CurrentLocation, dir, 1) };

                        return;
                    }
                }
            }

            if (MapObject.TargetObject == null || MapObject.TargetObject.Dead) return;
            if (((!MapObject.TargetObject.Name.EndsWith(")") && !(MapObject.TargetObject is PlayerObject)) || !CMain.Shift) &&
                (MapObject.TargetObject.Name.EndsWith(")") || !(MapObject.TargetObject is MonsterObject))) return;
            if (Functions.InRange(MapObject.TargetObject.CurrentLocation, User.CurrentLocation, 1)) return;
            if (User.Class == MirClass.Archer && User.HasClassWeapon && (MapObject.TargetObject is MonsterObject || MapObject.TargetObject is PlayerObject)) return; //ArcherTest - stop walking
            direction = Functions.DirectionFromPoint(User.CurrentLocation, MapObject.TargetObject.CurrentLocation);

            if (!CanWalk(direction, out direction)) return;

            User.QueuedAction = new QueuedAction { Action = MirAction.Walking, Direction = direction, Location = Functions.PointMove(User.CurrentLocation, direction, 1) };
        }

        public void UseMagic(ClientMagic magic, UserObject actor)
        {
            if (CMain.Time < GameScene.SpellTime || actor.Poison.HasFlag(PoisonType.Stun))
            {
                actor.ClearMagic();
                return;
            }

            if ((CMain.Time <= magic.CastTime + magic.Delay))
            {
                if (CMain.Time >= OutputDelay)
                {
                    OutputDelay = CMain.Time + 1000;
                    GameScene.Scene.OutputMessage(string.Format(GameLanguage.ClientTextMap[nameof(ClientTextKeys.CannotCastSpellSeconds)], GameLanguage.DbLocalization(magic.Spell.ToString()),
                        ((magic.CastTime + magic.Delay) - CMain.Time - 1) / 1000 + 1));
                }

                actor.ClearMagic();
                return;
            }

            int cost = magic.Level * magic.LevelCost + magic.BaseCost;

            if (magic.Spell == Spell.Teleport || magic.Spell == Spell.Blink || magic.Spell == Spell.StormEscape)
            {
                if (actor.Stats[Stat.TeleportManaPenaltyPercent] > 0)
                {
                    cost += (cost * actor.Stats[Stat.TeleportManaPenaltyPercent]) / 100;
                }
            }

            if (actor.Stats[Stat.ManaPenaltyPercent] > 0)
            {
                cost += (cost * actor.Stats[Stat.ManaPenaltyPercent]) / 100;
            }

            if (cost > actor.MP)
            {
                if (CMain.Time >= OutputDelay)
                {
                    OutputDelay = CMain.Time + 1000;
                    GameScene.Scene.OutputMessage(GameLanguage.ClientTextMap[nameof(ClientTextKeys.LowMana)]);
                }

                actor.ClearMagic();
                return;
            }

            //bool isTargetSpell = true;

            MapObject target = null;

            //Targeting
            switch (magic.Spell)
            {
                case Spell.FireBall:
                case Spell.GreatFireBall:
                case Spell.ElectricShock:
                case Spell.Poisoning:
                case Spell.ThunderBolt:
                case Spell.FlameDisruptor:
                case Spell.SoulFireBall:
                case Spell.TurnUndead:
                case Spell.FrostCrunch:
                case Spell.Vampirism:
                case Spell.Revelation:
                case Spell.Entrapment:
                case Spell.Hallucination:
                case Spell.DarkBody:
                case Spell.FireBounce:
                case Spell.MeteorShower:
                    if (actor.NextMagicObject != null)
                    {
                        if (!actor.NextMagicObject.Dead && actor.NextMagicObject.Race != ObjectType.Item && actor.NextMagicObject.Race != ObjectType.Merchant)
                            target = actor.NextMagicObject;
                    }

                    if (target == null) target = MapObject.MagicObject;

                    if (target != null && target.Race == ObjectType.Monster) MapObject.MagicObjectID = target.ObjectID;
                    break;
                case Spell.StraightShot:
                case Spell.DoubleShot:
                case Spell.ElementalShot:
                case Spell.DelayedExplosion:
                case Spell.BindingShot:
                case Spell.VampireShot:
                case Spell.PoisonShot:
                case Spell.CrippleShot:
                case Spell.NapalmShot:
                case Spell.SummonVampire:
                case Spell.SummonToad:
                case Spell.SummonSnakes:
                    if (!actor.HasClassWeapon)
                    {
                        GameScene.Scene.OutputMessage(GameLanguage.ClientTextMap[nameof(ClientTextKeys.MustWearBowForSkill)]);
                        actor.ClearMagic();
                        return;
                    }

                    if (actor.NextMagicObject != null)
                    {
                        if (!actor.NextMagicObject.Dead && actor.NextMagicObject.Race != ObjectType.Item && actor.NextMagicObject.Race != ObjectType.Merchant)
                            target = actor.NextMagicObject;
                    }

                    if (target == null) target = MapObject.MagicObject;

                    if (target != null && target.Race == ObjectType.Monster) MapObject.MagicObjectID = target.ObjectID;
                    break;
                case Spell.Stonetrap:
                    if (!User.HasClassWeapon)
                    {
                        GameScene.Scene.OutputMessage(GameLanguage.ClientTextMap[nameof(ClientTextKeys.MustWearBowForSkill)]);
                        User.ClearMagic();
                        return;
                    }

                    if (User.NextMagicObject != null)
                    {
                        if (!User.NextMagicObject.Dead && User.NextMagicObject.Race != ObjectType.Item && User.NextMagicObject.Race != ObjectType.Merchant)
                            target = User.NextMagicObject;
                    }

                    //if(magic.Spell == Spell.ElementalShot)
                    //{
                    //    isTargetSpell = User.HasElements;
                    //}

                    //switch(magic.Spell)
                    //{
                    //    case Spell.SummonVampire:
                    //    case Spell.SummonToad:
                    //    case Spell.SummonSnakes:
                    //        isTargetSpell = false;
                    //        break;
                    //}

                    break;
                case Spell.Purification:
                case Spell.Healing:
                case Spell.UltimateEnhancer:
                case Spell.EnergyShield:
                case Spell.PetEnhancer:
                    if (actor.NextMagicObject != null)
                    {
                        if (!actor.NextMagicObject.Dead && actor.NextMagicObject.Race != ObjectType.Item && actor.NextMagicObject.Race != ObjectType.Merchant)
                            target = actor.NextMagicObject;
                    }

                    if (target == null) target = User;
                    break;
                case Spell.FireBang:
                case Spell.MassHiding:
                case Spell.FireWall:
                case Spell.TrapHexagon:
                case Spell.HealingCircle:
                case Spell.CatTongue:
                    if (actor.NextMagicObject != null)
                    {
                        if (!actor.NextMagicObject.Dead && actor.NextMagicObject.Race != ObjectType.Item && actor.NextMagicObject.Race != ObjectType.Merchant)
                            target = actor.NextMagicObject;
                    }

                    break;
                case Spell.PoisonCloud:
                    if (actor.NextMagicObject != null)
                    {
                        if (!actor.NextMagicObject.Dead && actor.NextMagicObject.Race != ObjectType.Item && actor.NextMagicObject.Race != ObjectType.Merchant)
                            target = actor.NextMagicObject;
                    }

                    break;
                case Spell.Blizzard:
                case Spell.MeteorStrike:
                    if (actor.NextMagicObject != null)
                    {
                        if (!actor.NextMagicObject.Dead && actor.NextMagicObject.Race != ObjectType.Item && actor.NextMagicObject.Race != ObjectType.Merchant)
                            target = actor.NextMagicObject;
                    }

                    break;
                case Spell.Reincarnation:
                    if (actor == Hero && actor.NextMagicObject == null)
                        actor.NextMagicObject = User;
                    if (actor.NextMagicObject != null)
                    {
                        if (actor.NextMagicObject.Dead && actor.NextMagicObject.Race == ObjectType.Player)
                            target = actor.NextMagicObject;
                    }

                    break;
                case Spell.Trap:
                    if (actor.NextMagicObject != null)
                    {
                        if (!actor.NextMagicObject.Dead && User.NextMagicObject.Race != ObjectType.Item && actor.NextMagicObject.Race != ObjectType.Merchant)
                            target = actor.NextMagicObject;
                    }

                    break;
                case Spell.FlashDash:
                    if (actor.GetMagic(Spell.FlashDash).Level <= 1 && actor.IsDashAttack() == false)
                    {
                        actor.ClearMagic();
                        return;
                    }

                    //isTargetSpell = false;
                    break;
                default:
                    //isTargetSpell = false;
                    break;
            }

            MirDirection dir = (target == null || target == User) ? actor.NextMagicDirection : Functions.DirectionFromPoint(actor.CurrentLocation, target.CurrentLocation);

            Point location = target != null ? target.CurrentLocation : actor.NextMagicLocation;

            uint targetID = target != null ? target.ObjectID : 0;

            if (magic.Spell == Spell.FlashDash)
                dir = actor.Direction;

            if ((magic.Range != 0) && (!Functions.InRange(actor.CurrentLocation, location, magic.Range)))
            {
                if (CMain.Time >= OutputDelay)
                {
                    OutputDelay = CMain.Time + 1000;
                    GameScene.Scene.OutputMessage(GameLanguage.ClientTextMap[nameof(ClientTextKeys.TargetTooFar)]);
                }

                actor.ClearMagic();
                return;
            }

            GameScene.LogTime = CMain.Time + Globals.LogDelay;

            if (actor == User)
            {
                User.QueuedAction = new QueuedAction { Action = MirAction.Spell, Direction = dir, Location = User.CurrentLocation, Params = new List<object>() };
                User.QueuedAction.Params.Add(magic.Spell);
                User.QueuedAction.Params.Add(targetID);
                User.QueuedAction.Params.Add(location);
                User.QueuedAction.Params.Add(magic.Level);
            }
            else
            {
                Network.Enqueue(new C.Magic { ObjectID = actor.ObjectID, Spell = magic.Spell, Direction = dir, TargetID = targetID, Location = location, SpellTargetLock = CMain.SpellTargetLock });
            }
        }

        public static MirDirection MouseDirection(float ratio = 45F) //22.5 = 16
        {
            Point p = new Point(MouseLocation.X / CellWidth, MouseLocation.Y / CellHeight);
            if (Functions.InRange(new Point(OffSetX, OffSetY), p, 2))
                return Functions.DirectionFromPoint(new Point(OffSetX, OffSetY), p);

            PointF c = new PointF(OffSetX * CellWidth + CellWidth / 2F, OffSetY * CellHeight + CellHeight / 2F);
            PointF a = new PointF(c.X, 0);
            PointF b = MouseLocation;
            float bc = (float)Distance(c, b);
            float ac = bc;
            b.Y -= c.Y;
            c.Y += bc;
            b.Y += bc;
            float ab = (float)Distance(b, a);
            double x = (ac * ac + bc * bc - ab * ab) / (2 * ac * bc);
            double angle = Math.Acos(x);

            angle *= 180 / Math.PI;

            if (MouseLocation.X < c.X) angle = 360 - angle;
            angle += ratio / 2;
            if (angle > 360) angle -= 360;

            return (MirDirection)(angle / ratio);
        }

        public static int Direction16(Point source, Point destination)
        {
            PointF c = new PointF(source.X, source.Y);
            PointF a = new PointF(c.X, 0);
            PointF b = new PointF(destination.X, destination.Y);
            float bc = (float)Distance(c, b);
            float ac = bc;
            b.Y -= c.Y;
            c.Y += bc;
            b.Y += bc;
            float ab = (float)Distance(b, a);
            double x = (ac * ac + bc * bc - ab * ab) / (2 * ac * bc);
            double angle = Math.Acos(x);

            angle *= 180 / Math.PI;

            if (destination.X < c.X) angle = 360 - angle;
            angle += 11.25F;
            if (angle > 360) angle -= 360;

            return (int)(angle / 22.5F);
        }

        public static double Distance(PointF p1, PointF p2)
        {
            double x = p2.X - p1.X;
            double y = p2.Y - p1.Y;
            return Math.Sqrt(x * x + y * y);
        }

        public bool EmptyCell(Point p)
        {
            if ((M2CellInfo[p.X, p.Y].BackImage & 0x20000000) != 0 || (M2CellInfo[p.X, p.Y].FrontImage & 0x8000) != 0)
                return false;

            foreach (var ob in Objects.Values)
                if (ob.CurrentLocation == p && ob.Blocking)
                    return false;

            return true;
        }


        private bool CanWalk(MirDirection dir)
        {
            return EmptyCell(Functions.PointMove(User.CurrentLocation, dir, 1)) && !User.InTrapRock;
        }

        private bool CanWalk(MirDirection dir, out MirDirection outDir)
        {
            outDir = dir;
            if (User.InTrapRock) return false;

            if (EmptyCell(Functions.PointMove(User.CurrentLocation, dir, 1)))
                return true;

            dir = Functions.NextDir(outDir);
            if (EmptyCell(Functions.PointMove(User.CurrentLocation, dir, 1)))
            {
                outDir = dir;
                return true;
            }

            dir = Functions.PreviousDir(outDir);
            if (EmptyCell(Functions.PointMove(User.CurrentLocation, dir, 1)))
            {
                outDir = dir;
                return true;
            }

            return false;
        }

        private bool CheckDoorOpen(Point p)
        {
            if (M2CellInfo[p.X, p.Y].DoorIndex == 0) return true;
            Door DoorInfo = GetDoor(M2CellInfo[p.X, p.Y].DoorIndex);
            if (DoorInfo == null) return false; //if the door doesnt exist then it isnt even being shown on screen (and cant be open lol)
            if ((DoorInfo.DoorState == DoorState.Closed) || (DoorInfo.DoorState == DoorState.Closing))
            {
                if (CMain.Time > _doorTime)
                {
                    _doorTime = CMain.Time + 4000;
                    Network.Enqueue(new C.Opendoor() { DoorIndex = DoorInfo.index });
                }

                return false;
            }

            if ((DoorInfo.DoorState == DoorState.Open) && (DoorInfo.LastTick + 4000 > CMain.Time))
            {
                if (CMain.Time > _doorTime)
                {
                    _doorTime = CMain.Time + 4000;
                    Network.Enqueue(new C.Opendoor() { DoorIndex = DoorInfo.index });
                }
            }

            return true;
        }

        private long _doorTime = 0;


        private bool CanRun(MirDirection dir)
        {
            if (User.InTrapRock) return false;
            if (User.CurrentBagWeight > User.Stats[Stat.BagWeight]) return false;
            if (User.CurrentWearWeight > User.Stats[Stat.BagWeight]) return false;
            if (CanWalk(dir) && EmptyCell(Functions.PointMove(User.CurrentLocation, dir, 2)))
            {
                if (User.RidingMount || User.Sprint && !User.Sneaking)
                {
                    return EmptyCell(Functions.PointMove(User.CurrentLocation, dir, 3));
                }

                return true;
            }

            return false;
        }

        private bool CanRideAttack()
        {
            if (GameScene.User.RidingMount)
            {
                UserItem item = GameScene.User.Equipment[(int)EquipmentSlot.Mount];
                if (item == null || item.Slots.Length < 4 || item.Slots[(int)MountSlot.Bells] == null) return false;
            }

            return true;
        }

        public bool CanFish(MirDirection dir)
        {
            if (!GameScene.User.HasFishingRod || GameScene.User.FishingTime + 1000 > CMain.Time) return false;
            if (GameScene.User.CurrentAction != MirAction.Standing) return false;
            if (GameScene.User.Direction != dir) return false;
            if (GameScene.User.TransformType >= 6 && GameScene.User.TransformType <= 9) return false;

            Point point = Functions.PointMove(User.CurrentLocation, dir, 3);

            if (!M2CellInfo[point.X, point.Y].FishingCell) return false;

            return true;
        }

        public bool CanFly(Point target)
        {
            Point location = User.CurrentLocation;
            while (location != target)
            {
                MirDirection dir = Functions.DirectionFromPoint(location, target);

                location = Functions.PointMove(location, dir, 1);

                if (location.X < 0 || location.Y < 0 || location.X >= GameScene.Scene.MapControl.Width || location.Y >= GameScene.Scene.MapControl.Height) return false;

                if (!GameScene.Scene.MapControl.ValidPoint(location)) return false;
            }

            return true;
        }


        public bool ValidPoint(Point p)
        {
            //GameScene.Scene.ChatDialog.ReceiveChat(string.Format("cell: {0}", (M2CellInfo[p.X, p.Y].BackImage & 0x20000000)), ChatType.Hint);
            return (M2CellInfo[p.X, p.Y].BackImage & 0x20000000) == 0;
        }

        public bool HasTarget(Point p)
        {
            foreach (var ob in Objects.Values)
                if (ob.CurrentLocation == p && ob.Blocking)
                    return true;
            return false;
        }

        public bool CanHalfMoon(Point p, MirDirection d)
        {
            d = Functions.PreviousDir(d);
            for (int i = 0; i < 4; i++)
            {
                if (HasTarget(Functions.PointMove(p, d, 1))) return true;
                d = Functions.NextDir(d);
            }

            return false;
        }

        public bool CanCrossHalfMoon(Point p)
        {
            MirDirection dir = MirDirection.Up;
            for (int i = 0; i < 8; i++)
            {
                if (HasTarget(Functions.PointMove(p, dir, 1))) return true;
                dir = Functions.NextDir(dir);
            }

            return false;
        }

        #region Disposable

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Objects.Clear();

                MapButtons = 0;
                MouseLocation = Point.Empty;
                InputDelay = 0;
                NextAction = 0;

                M2CellInfo = null;
                Width = 0;
                Height = 0;

                Index = 0;
                FileName = String.Empty;
                Title = String.Empty;
                MiniMap = 0;
                BigMap = 0;
                Lights = 0;
                FloorValid = false;
                LightsValid = false;
                MapDarkLight = 0;
                Music = 0;

                AnimationCount = 0;
                Effects.Clear();
            }

            base.Dispose(disposing);
        }

        #endregion

        public void UpdateWeather()
        {
            for (int i = GameScene.Scene.ParticleEngines.Count - 1; i > 0; i--)
                GameScene.Scene.ParticleEngines[i].Dispose();

            GameScene.Scene.ParticleEngines.Clear();
            List<ParticleImageInfo> textures = new List<ParticleImageInfo>();
            foreach (WeatherSetting itemWeather in Enum.GetValues(typeof(WeatherSetting)).Cast<object>().ToArray())
            {
                //if not enabled skip
                if ((Weather & itemWeather) != itemWeather)
                    continue;

                //foreach (WeatherSetting itemWeather in Weather)
                //{
                switch (itemWeather)
                {
                    case WeatherSetting.Leaves:
                        textures = new List<ParticleImageInfo>();
                        textures.Add(new ParticleImageInfo(Libraries.Weather, 359, 170, 50));
                        textures.Add(new ParticleImageInfo(Libraries.Weather, 531, 55, 50));
                        textures.Add(new ParticleImageInfo(Libraries.Weather, 587, 200, 50));


                        ParticleEngine LeavesEngine2 = new ParticleEngine(textures, new Vector2(2f, 0), ParticleType.Leaves);
                        Vector2 lVelocity = new Vector2(0F, 0F);
                        for (int y = 512 * -1; y < Settings.ScreenHeight + 512; y += 512)
                        for (int x = 512 * -1; x < Settings.ScreenWidth + 512; x += 512)
                        {
                            Particle part = LeavesEngine2.GenerateNewParticle(ParticleType.Leaves);
                            part.Position = new Vector2(x, y);
                            part.Velocity = lVelocity;
                        }

                        LeavesEngine2.GenerateParticles = false;
                        GameScene.Scene.ParticleEngines.Add(LeavesEngine2);
                        break;
                    case WeatherSetting.FireyLeaves:
                        textures = new List<ParticleImageInfo>();
                        textures.Add(new ParticleImageInfo(Libraries.Weather, 359, 170, 50));
                        textures.Add(new ParticleImageInfo(Libraries.Weather, 531, 55, 50));
                        textures.Add(new ParticleImageInfo(Libraries.Weather, 587, 200, 50));


                        ParticleEngine FLeavesEngine2 = new ParticleEngine(textures, new Vector2(2f, 0), ParticleType.FireyLeaves);
                        Vector2 FlVelocity = new Vector2(0F, 0F);
                        for (int y = 512 * -1; y < Settings.ScreenHeight + 512; y += 512)
                        for (int x = 512 * -1; x < Settings.ScreenWidth + 512; x += 512)
                        {
                            Particle part = FLeavesEngine2.GenerateNewParticle(ParticleType.FireyLeaves);
                            part.Position = new Vector2(x, y);
                            part.Velocity = FlVelocity;
                        }

                        FLeavesEngine2.GenerateParticles = false;
                        GameScene.Scene.ParticleEngines.Add(FLeavesEngine2);
                        break;
                    case WeatherSetting.Rain:
                        textures = new List<ParticleImageInfo>();
                        //Rain
                        textures.Add(new ParticleImageInfo(Libraries.Weather, 164, 150, 50));


                        ParticleEngine RainEngine2 = new ParticleEngine(textures, new Vector2(2f, 0), ParticleType.Rain);
                        Vector2 rsevelocity = new Vector2(0F, 0F);
                        var xVar = 512;
                        var yVar = 512;
                        for (int y = yVar * -1; y < Settings.ScreenHeight + yVar; y += yVar)
                        for (int x = xVar * -1; x < Settings.ScreenWidth + xVar; x += xVar)
                        {
                            Particle part = RainEngine2.GenerateNewParticle(ParticleType.Rain);
                            part.Position = new Vector2(x, y);
                            part.Velocity = rsevelocity;
                        }

                        RainEngine2.GenerateParticles = false;
                        GameScene.Scene.ParticleEngines.Add(RainEngine2);
                        break;

                    case WeatherSetting.Snow:
                        textures = new List<ParticleImageInfo>();
                        textures.Add(new ParticleImageInfo(Libraries.Weather, 43, 20, 50));

                        ParticleEngine RainEngine = new ParticleEngine(textures, new Vector2(0, 0), ParticleType.Snow);
                        Vector2 rsvelocity = new Vector2(1F, -1F);

                        for (int y = -400; y < Settings.ScreenHeight + 400; y += 400)
                        for (int x = -400; x < Settings.ScreenWidth + 400; x += 400)
                        {
                            Particle part = RainEngine.GenerateNewParticle(ParticleType.Snow);
                            part.Position = new Vector2(x, y);
                            part.Velocity = rsvelocity;
                        }

                        RainEngine.GenerateParticles = false;
                        GameScene.Scene.ParticleEngines.Add(RainEngine);

                        break;
                    case WeatherSetting.Fog:
                        List<ParticleImageInfo> ftextures = new List<ParticleImageInfo>();
                        ftextures.Add(new ParticleImageInfo(Libraries.Weather, 0));
                        ParticleEngine fengine = new ParticleEngine(ftextures, new Vector2(0, 0), ParticleType.Fog);
                        fengine.UpdateDelay = TimeSpan.FromMilliseconds(20);

                        Vector2 fvelocity = new Vector2(2F, -2F);
                        for (int y = -512; y < Settings.ScreenHeight + 512; y += 512)
                        for (int x = -512; x < Settings.ScreenWidth + 512; x += 512)
                        {
                            Particle part = fengine.GenerateNewParticle(ParticleType.Fog);
                            part.Position = new Vector2(x, y);
                            part.Velocity = fvelocity;
                        }


                        fengine.GenerateParticles = false;
                        GameScene.Scene.ParticleEngines.Add(fengine);
                        break;
                    case WeatherSetting.RedEmber:
                        var rtextures = new List<ParticleImageInfo>();
                        rtextures.Add(new ParticleImageInfo(Libraries.Weather, 1, 9, 150));

                        var rengine = new ParticleEngine(rtextures, new Vector2(0, 0), ParticleType.RedFogEmber);
                        GameScene.Scene.ParticleEngines.Add(rengine);
                        break;
                    case WeatherSetting.WhiteEmber:

                        textures = new List<ParticleImageInfo>();
                        textures.Add(new ParticleImageInfo(Libraries.Weather, 1, 9, 150));
                        var whiteEmberEngine = new ParticleEngine(textures, new Vector2(0, 0), ParticleType.WhiteEmber);
                        GameScene.Scene.ParticleEngines.Add(whiteEmberEngine);
                        break;
                    case WeatherSetting.PurpleLeaves:

                        textures = new List<ParticleImageInfo>();
                        textures.Add(new ParticleImageInfo(Libraries.Weather, 359, 170, 50));
                        textures.Add(new ParticleImageInfo(Libraries.Weather, 531, 55, 50));
                        textures.Add(new ParticleImageInfo(Libraries.Weather, 587, 200, 50));
                        //textures.Add(new ParticleImageInfo(Libraries.Weather, 10, 20, 50));

                        var pEmberEngine = new ParticleEngine(textures, new Vector2(0, 0), ParticleType.PurpleLeaves);

                        for (int y = 512 * -1; y < Settings.ScreenHeight + 512; y += 512)
                        for (int x = 512 * -1; x < Settings.ScreenWidth + 512; x += 512)
                        {
                            Particle part = pEmberEngine.GenerateNewParticle(ParticleType.PurpleLeaves);
                            part.Position = new Vector2(x, y);
                            part.Velocity = new Vector2(0, 0);
                        }

                        pEmberEngine.GenerateParticles = false;
                        GameScene.Scene.ParticleEngines.Add(pEmberEngine);
                        break;

                    case WeatherSetting.YellowEmber:

                        textures = new List<ParticleImageInfo>();
                        textures.Add(new ParticleImageInfo(Libraries.Weather, 1, 9, 100));

                        var yellowEmberEngine = new ParticleEngine(textures, new Vector2(0, 0), ParticleType.YellowEmber);
                        GameScene.Scene.ParticleEngines.Add(yellowEmberEngine);
                        break;
                    case WeatherSetting.FireParticle:

                        textures = new List<ParticleImageInfo>();
                        //textures.Add(new ParticleImageInfo(Libraries.StateEffect, 640)); << TODO - Win
                        //   textures.Add(new ParticleImageInfo(Libraries.Prguse4, 642));
                        var fEmberEngine = new ParticleEngine(textures, new Vector2(0, 0), ParticleType.Bird);
                        GameScene.Scene.ParticleEngines.Add(fEmberEngine);
                        break;
                }
            }
        }

        public void RemoveObject(MapObject ob)
        {
            M2CellInfo[ob.MapLocation.X, ob.MapLocation.Y].RemoveObject(ob);
        }

        public void AddObject(MapObject ob)
        {
            M2CellInfo[ob.MapLocation.X, ob.MapLocation.Y].AddObject(ob);
        }

        public MapObject FindObject(uint ObjectID, int x, int y)
        {
            return M2CellInfo[x, y].FindObject(ObjectID);
        }

        public void SortObject(MapObject ob)
        {
            M2CellInfo[ob.MapLocation.X, ob.MapLocation.Y].Sort();
        }

        public Door GetDoor(byte Index)
        {
            for (int i = 0; i < Doors.Count; i++)
            {
                if (Doors[i].index == Index)
                    return Doors[i];
            }

            return null;
        }

        public void Processdoors()
        {
            for (int i = 0; i < Doors.Count; i++)
            {
                if ((Doors[i].DoorState == DoorState.Opening) || (Doors[i].DoorState == DoorState.Closing))
                {
                    if (Doors[i].LastTick + 50 < CMain.Time)
                    {
                        Doors[i].LastTick = CMain.Time;
                        Doors[i].ImageIndex++;

                        if (Doors[i].ImageIndex == 1) //change the 1 if you want to animate doors opening/closing
                        {
                            Doors[i].ImageIndex = 0;
                            Doors[i].DoorState = (DoorState)Enum.ToObject(typeof(DoorState), ((byte)++Doors[i].DoorState % 4));
                        }

                        FloorValid = false;
                    }
                }

                if (Doors[i].DoorState == DoorState.Open)
                {
                    if (Doors[i].LastTick + 5000 < CMain.Time)
                    {
                        Doors[i].LastTick = CMain.Time;
                        Doors[i].DoorState = DoorState.Closing;
                        FloorValid = false;
                    }
                }
            }
        }

        public void OpenDoor(byte Index, bool closed)
        {
            Door Info = GetDoor(Index);
            if (Info == null) return;
            Info.DoorState = (closed ? DoorState.Closing : Info.DoorState == DoorState.Open ? DoorState.Open : DoorState.Opening);
            Info.ImageIndex = 0;
            Info.LastTick = CMain.Time;
        }
    }
}