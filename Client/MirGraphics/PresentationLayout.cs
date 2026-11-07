namespace Client.MirGraphics
{
    /// <summary>One aspect-preserving layout shared by presentation and pointer input.</summary>
    internal readonly struct PresentationLayout
    {
        public Size LogicalSize { get; }
        public Size OutputSize { get; }
        public Rectangle Viewport { get; }
        public bool IsValid => !Viewport.IsEmpty;

        public PresentationLayout(Size logicalSize, Size outputSize)
        {
            LogicalSize = logicalSize;
            OutputSize = outputSize;
            Viewport = Rectangle.Empty;

            if (logicalSize.Width <= 0 || logicalSize.Height <= 0 || outputSize.Width <= 0 || outputSize.Height <= 0)
                return;

            int width, height;
            if ((long)outputSize.Width * logicalSize.Height <= (long)outputSize.Height * logicalSize.Width)
            {
                width = outputSize.Width;
                height = Math.Max(1, (int)((long)width * logicalSize.Height / logicalSize.Width));
            }
            else
            {
                height = outputSize.Height;
                width = Math.Max(1, (int)((long)height * logicalSize.Width / logicalSize.Height));
            }

            Viewport = new Rectangle((outputSize.Width - width) / 2, (outputSize.Height - height) / 2, width, height);
        }

        public Point ToGamePoint(Point clientPoint, Size clientSize, bool clamp = false)
        {
            if (!IsValid || clientSize.Width <= 0 || clientSize.Height <= 0)
                return new Point(-1, -1);

            if (!clamp && !new Rectangle(Point.Empty, clientSize).Contains(clientPoint))
                return new Point(-1, -1);

            int outputX = (int)((long)clientPoint.X * OutputSize.Width / clientSize.Width);
            int outputY = (int)((long)clientPoint.Y * OutputSize.Height / clientSize.Height);
            if (!clamp && !Viewport.Contains(outputX, outputY))
                return new Point(-1, -1);

            outputX = Math.Clamp(outputX - Viewport.X, 0, Viewport.Width - 1);
            outputY = Math.Clamp(outputY - Viewport.Y, 0, Viewport.Height - 1);
            return new Point((int)((long)outputX * LogicalSize.Width / Viewport.Width),
                (int)((long)outputY * LogicalSize.Height / Viewport.Height));
        }

        public Rectangle GetClientViewport(Size clientSize)
        {
            if (!IsValid || clientSize.Width <= 0 || clientSize.Height <= 0)
                return Rectangle.Empty;

            int left = (int)((long)Viewport.Left * clientSize.Width / OutputSize.Width);
            int top = (int)((long)Viewport.Top * clientSize.Height / OutputSize.Height);
            int right = (int)((long)Viewport.Right * clientSize.Width / OutputSize.Width);
            int bottom = (int)((long)Viewport.Bottom * clientSize.Height / OutputSize.Height);
            return Rectangle.FromLTRB(left, top, right, bottom);
        }
    }
}
