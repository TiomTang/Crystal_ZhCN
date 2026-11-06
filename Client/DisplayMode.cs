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
        /// Borderless window covering the screen: the client area follows the monitor while the
        /// back buffer stays at the render resolution, so D3D9 windowed Present stretches it to
        /// fit. This is not exclusive fullscreen.
        /// </summary>
        Maximized = 1,

        /// <summary>Exclusive fullscreen.</summary>
        FullScreen = 2
    }
}
