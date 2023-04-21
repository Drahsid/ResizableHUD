using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;

namespace ResizableHUD;

internal class Globals
{
    public static Configuration Config;
    public static ChatGui Chat;
    public static ClientState ClientState;
    public static CommandManager CommandManager;
    public static PluginCommandManager<Plugin> PluginCommandManager;
    public static WindowSystem WindowSystem;
    [PluginService] public static KeyState KeyState { get; private set; } = null!;
}
