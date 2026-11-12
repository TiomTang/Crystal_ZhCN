using System.Drawing;
using System.Text;
using Client.MirControls;
using Client.MirObjects;

namespace Client.MirScenes.Dialogs
{
    // Hover tooltip for map objects. Lives in the scene control tree but never accepts
    // mouse input, so it can be drawn above the map without stealing clicks.
    public sealed class ObjectInfoDialog : MirControl
    {
        private readonly MirLabel _body;

        public ObjectInfoDialog()
        {
            BackColour = Color.FromArgb(255, 0, 0, 0);
            Border = true;
            BorderColour = Color.FromArgb(255, 144, 144, 0);
            DrawControlTexture = true;
            NotControl = true;
            Opacity = 0.8F;
            Visible = false;

            _body = new MirLabel
            {
                AutoSize = true,
                BackColour = Color.Transparent,
                ForeColour = Color.Yellow,
                Font = new Font(Settings.FontName, 10F),
                Parent = this,
            };
            _body.SizeChanged += (o, e) => Size = _body.Size;
        }

        public void HideInfo()
        {
            Visible = false;
        }

        public void ShowInfo(MapObject target)
        {
            if (target == null || target == MapObject.User)
            {
                HideInfo();
                return;
            }

            Visible = true;

            _body.Text = BuildText(target);

            Point point = CMain.MPoint.Add(24, 22);

            if (point.X + Size.Width > Settings.ScreenWidth)
                point.X = CMain.MPoint.X - Size.Width - 8;
            if (point.Y + Size.Height > Settings.ScreenHeight)
                point.Y = CMain.MPoint.Y - Size.Height - 8;

            if (point.X < 0) point.X = 0;
            if (point.Y < 0) point.Y = 0;

            Location = point;
        }

        private static string BuildText(MapObject target)
        {
            StringBuilder text = new StringBuilder(target.Name);

            // NPCs have no health or combat values to show.
            if (target is NPCObject) return text.ToString();

            text.Append('\n');

            if (!GameScene.ObjectInfoCache.TryGetValue(target.ObjectID, out GameScene.ObjectInfoEntry info))
            {
                // The reply is still in flight: show only what the client already knows.
                text.Append(GameLanguage.Get("Health: {0}", target.PercentHealth + "%"));

                if (target is PlayerObject player)
                {
                    text.Append('\n');
                    text.Append(GameLanguage.Get("Level: {0}", player.Level));
                }

                return text.ToString();
            }

            text.Append(GameLanguage.Get("Level: {0}", info.Level));
            text.Append('\n');

            int percent = info.MaxHP <= 0 ? 0 : (int)((long)info.HP * 100 / info.MaxHP);
            text.Append(GameLanguage.Get("HP: {0} / {1} ({2}%)", info.HP, info.MaxHP, percent));

            if (!info.Combat) return text.ToString();

            text.Append('\n');
            text.Append(GameLanguage.Get("DC: {0}~{1}", info.MinDC, info.MaxDC));
            text.Append("   ");
            text.Append(GameLanguage.Get("AC: {0}~{1}", info.MinAC, info.MaxAC));
            text.Append("   ");
            text.Append(GameLanguage.Get("MAC: {0}~{1}", info.MinMAC, info.MaxMAC));

            return text.ToString();
        }
    }
}
