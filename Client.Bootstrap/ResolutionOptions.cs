using System;
using System.Collections.Generic;
using System.Linq;

namespace Client.Bootstrap
{
    /// <summary>
    /// Resolution choices for the launcher drop-down. Settings stores only the width, which cannot tell 1280x800 and 1280x720 apart,
    /// so the height is stored separately next to it.
    /// </summary>
    public static class ResolutionOptions
    {
        public readonly struct Option
        {
            public Option(int width, int height)
            {
                Width = width;
                Height = height;
            }

            public int Width { get; }
            public int Height { get; }
            public string Label => string.Format("{0}x{1} ({2})", Width, Height, Aspect(Width, Height));
        }

        public static readonly Option[] All =
        {
            new Option(1024, 768),
            new Option(1280, 800),
            new Option(1366, 768),
            new Option(1280, 720),
            new Option(1920, 1080),
        };

        /// <summary>
        /// Find the option that matches the saved value. Fall back to the first option with the same width when the height does not match, and to the smallest option when nothing matches.
        /// </summary>
        public static int IndexOf(int width, int height)
        {
            for (int i = 0; i < All.Length; i++)
                if (All[i].Width == width && All[i].Height == height)
                    return i;

            for (int i = 0; i < All.Length; i++)
                if (All[i].Width == width)
                    return i;

            return 0;
        }

        /// <summary>
        /// The server sends the maximum allowed width. Above it, fall back to the largest option that does not exceed the limit.
        /// </summary>
        public static Option Resolve(int width, int height, int maxWidth)
        {
            Option option = All[IndexOf(width, height)];

            if (maxWidth <= 0 || option.Width <= maxWidth)
                return option;

            Option fallback = default;

            for (int i = 0; i < All.Length; i++)
                if (All[i].Width <= maxWidth && (fallback.Width == 0 || All[i].Width > fallback.Width))
                    fallback = All[i];

            return fallback.Width == 0 ? All[0] : fallback;
        }

        private static string Aspect(int width, int height)
        {
            double ratio = (double)width / height;

            if (Math.Abs(ratio - 16d / 9d) < 0.02) return "16:9";
            if (Math.Abs(ratio - 16d / 10d) < 0.02) return "16:10";
            if (Math.Abs(ratio - 4d / 3d) < 0.02) return "4:3";
            if (Math.Abs(ratio - 5d / 4d) < 0.02) return "5:4";

            return string.Format("{0:0.##}:1", ratio);
        }
    }
}
