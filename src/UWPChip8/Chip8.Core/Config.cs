using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chip8.Core
{
    public static class Config
    {
        public static int ScreenWidth { get { return 64; } }
        public static int ScreenHeight { get { return 32; } }
        public static float Hz { get { return 1000 / 60; } }
    }
}