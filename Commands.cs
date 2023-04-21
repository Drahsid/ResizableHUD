using ResizableHUD.Attributes;
using Dalamud.Game.Command;
using System;
using System.Collections.Generic;

namespace ResizableHUD;

internal class Commands
{
    public static void Initialize()
    {
        Globals.CommandManager.AddHandler("/prhud", new CommandInfo(OnPrHud)
        {
            HelpMessage = "Toggle the visibility of the configuration window.",
            ShowInHelp= true,
        });
    }

    public static void Uninitialize()
    {
        Globals.CommandManager.RemoveHandler("/prhud");
    }

    public static void ToggleConfig()
    {
        Globals.WindowSystem.GetWindow(ConfigWindow.ConfigWindowName).IsOpen = !Globals.WindowSystem.GetWindow(ConfigWindow.ConfigWindowName).IsOpen;
    }

    public static void OnPrHud(string command, string args)
    {
        ToggleConfig();
    }
}
