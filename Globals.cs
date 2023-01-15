using Dalamud.Game.ClientState;
using Dalamud.Game.Gui;
using Dalamud.Interface.Windowing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResizableHUD
{
    internal class Globals
    {
        public static Configuration Config;
        public static ChatGui Chat;
        public static ClientState ClientState;
        public static PluginCommandManager<Plugin> CommandManager;
        public static WindowSystem WindowSystem;
    }
}
