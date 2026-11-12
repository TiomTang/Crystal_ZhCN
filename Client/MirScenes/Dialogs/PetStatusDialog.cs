using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Client.MirControls;
using Client.MirGraphics;
using Client.MirObjects;
using Shared;
using SlimDX;

namespace Client.MirScenes.Dialogs
{
    // Horizontal roster pinned above the bottom HUD: the hero first, then pets.
    // Hovering or clicking a card targets that actor exactly as if it had been
    // clicked on the map, so healing and support spells cast from the card land
    // on the hero or the pet.
    public sealed class PetStatusDialog : MirControl
    {
        public const int CardWidth = 72;
        public const int CardHeight = 86;
        public const int CardGap = 4;

        private const int IconBox = 34;
        private const int IconX = 17;
        private const int IconY = 21;
        private const int NameY = 55;
        private const int BarX = 13;
        private const int BarY = 69;
        private const int BarWidth = 45;
        private const int BarHeight = 4;
        private const int MaxCards = 6;   // hero + up to 5 pets

        private static MLibrary _cardLibrary;
        private static bool _cardLibraryChecked;

        // Loaded on first use: a missing PetCard.Lib leaves the roster running with a
        // blank plate instead of throwing while the client is starting up.
        private static MLibrary CardLibrary
        {
            get
            {
                if (!_cardLibraryChecked)
                {
                    _cardLibraryChecked = true;
                    string file = Settings.DataPath + "PetCard.Lib";
                    if (System.IO.File.Exists(file))
                        _cardLibrary = new MLibrary(Settings.DataPath + "PetCard");
                }

                return _cardLibrary;
            }
        }

        private readonly List<MapObject> _actors = new List<MapObject>();
        private readonly PetCard[] _cards = new PetCard[MaxCards];
        private long _nextRefresh;

        private MapObject HoverActor { get; set; }

        public PetStatusDialog()
        {
            DrawControlTexture = false;
            Visible = false;

            for (int i = 0; i < MaxCards; i++)
            {
                PetCard card = new PetCard
                {
                    Parent = this,
                    Size = new Size(CardWidth, CardHeight),
                    Visible = false,
                };
                card.MouseDown += CardMouseDown;
                _cards[i] = card;
            }
        }

        public void Clear()
        {
            _actors.Clear();
            HoverActor = null;
            Visible = false;

            for (int i = 0; i < _cards.Length; i++)
            {
                _cards[i].Actor = null;
                _cards[i].Visible = false;
            }
        }

        // Returns true when the cursor sits on a card. The caller must then skip the
        // map's pixel based mouse-object scan so the actor remains the mouse target.
        public bool Process()
        {
            if (!Settings.PetStatus || MapObject.User == null)
            {
                if (Visible || _actors.Count > 0) Clear();
                return false;
            }

            if (CMain.Time >= _nextRefresh)
            {
                _nextRefresh = CMain.Time + 200;
                Rebuild();
            }

            if (!Visible || _actors.Count == 0)
            {
                HoverActor = null;
                return false;
            }

            if (!DisplayRectangle.Contains(CMain.MPoint))
            {
                HoverActor = null;
                return false;
            }

            int step = CardWidth + CardGap;
            int offset = CMain.MPoint.X - DisplayRectangle.X;
            if (offset < 0) return false;

            int slot = offset / step;
            int within = offset - (slot * step);

            if (slot < 0 || slot >= _actors.Count || within > CardWidth)
            {
                HoverActor = null;
                return false;
            }

            HoverActor = _actors[slot];
            MapObject.MouseObjectID = HoverActor.ObjectID;
            return true;
        }

        // Called from the tooltip code so hovering a card keeps the actor info visible.
        public static bool IsPetStatusControl(MirControl control)
        {
            while (control != null)
            {
                if (control is PetStatusDialog) return true;
                control = control.Parent;
            }

            return false;
        }

        private void Rebuild()
        {
            _actors.Clear();

            // Hero first so it keeps a fixed slot next to the HUD edge.
            HeroObject hero = MapObject.HeroObject;
            if (hero != null && !hero.Dead)
                _actors.Add(hero);

            List<MonsterObject> pets = new List<MonsterObject>();
            foreach (KeyValuePair<uint, MapObject> pair in MapControl.Objects)
            {
                MonsterObject pet = pair.Value as MonsterObject;
                if (pet == null) continue;
                if (pet.MasterObjectId == 0 || pet.MasterObjectId != MapObject.User.ObjectID) continue;
                if (pet.Dead) continue;

                pets.Add(pet);
            }

            pets.Sort((a, b) => a.ObjectID.CompareTo(b.ObjectID));

            int room = MaxCards - _actors.Count;
            for (int i = 0; i < pets.Count && i < room; i++)
                _actors.Add(pets[i]);

            int count = _actors.Count;
            Visible = count > 0;

            for (int i = 0; i < MaxCards; i++)
            {
                PetCard card = _cards[i];
                card.Actor = i < count ? _actors[i] : null;
                card.Visible = i < count;

                if (card.Actor != null)
                    card.RefreshName();
            }

            if (count == 0)
            {
                HoverActor = null;
                return;
            }

            int width = (CardWidth * count) + (CardGap * (count - 1));
            int bottom = Settings.ScreenHeight - 152;

            if (GameScene.Scene != null && GameScene.Scene.MainDialog != null)
                bottom = GameScene.Scene.MainDialog.Location.Y;

            if (Size.Width != width)
                Size = new Size(width, CardHeight);

            Point location = new Point(8, bottom - CardHeight - 6);
            if (Location != location)
                Location = location;

            for (int i = 0; i < count; i++)
                _cards[i].Location = new Point(i * (CardWidth + CardGap), 0);
        }

        private void CardMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            PetCard card = sender as PetCard;
            if (card == null || card.Actor == null || card.Actor.Dead) return;

            MapObject.MouseObjectID = card.Actor.ObjectID;
            MapObject.TargetObjectID = card.Actor.ObjectID;

            if (card.Actor.AI != 6)
                MapObject.MagicObjectID = card.Actor.ObjectID;
        }

        private sealed class PetCard : MirControl
        {
            private static readonly Color HoverBorder = Color.FromArgb(255, 246, 214, 132);
            private static readonly Color PetNameColour = Color.FromArgb(255, 244, 232, 206);
            private static readonly Color HeroNameColour = Color.FromArgb(255, 250, 214, 120);

            private readonly MirLabel _name;

            public MapObject Actor;

            public PetCard()
            {
                DrawControlTexture = false;
                BorderColour = HoverBorder;

                _name = new MirLabel
                {
                    AutoSize = false,
                    BackColour = Color.Transparent,
                    DrawFormat = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix,
                    Font = new Font(Settings.FontName, 9F),
                    ForeColour = PetNameColour,
                    Location = new Point(0, NameY),
                    OutLine = true,
                    OutLineColour = Color.Black,
                    Parent = this,
                    Size = new Size(CardWidth, 12),
                    Text = string.Empty,
                };
            }

            public void RefreshName()
            {
                if (Actor == null)
                {
                    _name.Text = string.Empty;
                    return;
                }

                string name = Actor.Name ?? string.Empty;
                int bracket = name.IndexOf('(');
                if (bracket > 0) name = name.Substring(0, bracket);

                _name.Text = name;
                _name.ForeColour = Actor is PlayerObject ? HeroNameColour : PetNameColour;
            }

            public override void Draw()
            {
                if (!Visible) return;

                // Plate first, then the portrait and the bar, then the child controls
                // (the name) and the hover outline on top.
                MLibrary card = CardLibrary;
                if (card != null)
                    card.Draw(0, DisplayRectangle.X, DisplayRectangle.Y);

                if (Actor != null)
                {
                    DrawIcon();
                    DrawHealthBar();
                }

                bool hovered = Actor != null && MapObject.MouseObject == Actor;
                if (Border != hovered)
                    Border = hovered;

                base.Draw();
            }

            private void DrawIcon()
            {
                MLibrary library = Actor.BodyLibrary;
                if (library == null) return;

                FrameSet frames = library.Frames ?? FrameSet.DefaultMonster;

                Frame standing;
                if (!frames.TryGetValue(MirAction.Standing, out standing))
                    standing = new Frame(0, 4, 0, 500);

                int index = standing.Start + (standing.OffSet * (byte)MirDirection.Down);

                // Player armour atlases keep the female frames at an offset, so a hero
                // portrait has to shift by it or it shows the wrong gender's poses.
                PlayerObject player = Actor as PlayerObject;
                if (player != null) index += player.ArmourOffSet;

                Size source = library.GetSize(index);
                if (source.Width <= 0 || source.Height <= 0) return;

                float scale = Math.Min((float)IconBox / source.Width, (float)IconBox / source.Height);
                Size size = new Size(Math.Max(1, (int)(source.Width * scale)), Math.Max(1, (int)(source.Height * scale)));

                Point point = new Point(
                    DisplayRectangle.X + IconX + ((IconBox - size.Width) / 2),
                    DisplayRectangle.Y + IconY + ((IconBox - size.Height) / 2));

                library.Draw(index, point, size, Color.White);
            }

            private void DrawHealthBar()
            {
                int fill = (int)(BarWidth * Actor.PercentHealth / 100F);
                if (fill <= 0) return;

                int x = DisplayRectangle.X + BarX;
                int y = DisplayRectangle.Y + BarY;

                DXManager.Draw(DXManager.RadarTexture, new Rectangle(0, 0, fill, BarHeight),
                    new Vector3(x, y, 0), Color.FromArgb(255, 176, 34, 30));
                DXManager.Draw(DXManager.RadarTexture, new Rectangle(0, 0, fill, 1),
                    new Vector3(x, y, 0), Color.FromArgb(255, 240, 124, 96));
            }
        }
    }
}
