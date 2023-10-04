using Dalamud.Game.Command;
using DrahsidLib;

namespace ResizableHUD;

internal static class Commands {
    public static void Initialize() {
        Service.CommandManager.AddHandler("/prhud", new CommandInfo(OnPrHud)
        {
            HelpMessage = "Toggle the visibility of the configuration window.",
            ShowInHelp= true,
        });
    }

    public static void Dispose() {
        Service.CommandManager.RemoveHandler("/prhud");
    }

    public static void ToggleConfig() {
        Windows.Config.IsOpen = !Windows.Config.IsOpen;
    }

    public static void OnPrHud(string command, string args) {
        ToggleConfig();
    }
}
