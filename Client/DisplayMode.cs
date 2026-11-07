using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public enum DisplayMode
    {
        /// <summary>Windowed: the client area equals the render resolution.</summary>
        Windowed = 0,

        /// <summary>
        /// Borderless window covering the screen. The output follows the monitor and the game
        /// canvas is fitted inside it without changing its aspect ratio or logical resolution.
        /// This is not exclusive fullscreen.
        /// </summary>
        Maximized = 1,

        /// <summary>Exclusive fullscreen at the desktop output mode, with an independent game canvas.</summary>
        FullScreen = 2
    }
}
