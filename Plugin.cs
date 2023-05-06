using Dalamud.Game.ClientState;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using ImGuiNET;
using System;
using System.Diagnostics;
using System.Numerics;

[assembly: System.Reflection.AssemblyVersion("1.1.7")]

namespace ResizableHUD;

public class Plugin : IDalamudPlugin {
    public string Name => "ResizableHUD";
    private DalamudPluginInterface PluginInterface;

    public Plugin(DalamudPluginInterface pluginInterface, CommandManager commandManager, ChatGui chat, ClientState clientState) {
        PluginInterface = pluginInterface;
        Globals.Chat = chat;
        Globals.ClientState = clientState;

        Globals.Config = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Globals.Config.Initialize(PluginInterface);

        Globals.WindowSystem = new WindowSystem(typeof(Plugin).AssemblyQualifiedName);
        Globals.WindowSystem.AddWindow(new ConfigWindow());
        PluginInterface.UiBuilder.Draw += OnDraw;
        PluginInterface.UiBuilder.OpenConfigUi += Commands.ToggleConfig;

        Globals.CommandManager = commandManager;
        Globals.PluginCommandManager = new PluginCommandManager<Plugin>(this, commandManager);
        Commands.Initialize();

        PluginInterface.Create<Globals>(); // blah blah should do this right
    }

    private void OnDraw()
    {
        AddonManager.UpdateAddons();
        Globals.WindowSystem.Draw();
    }

    #region IDisposable Support
    protected virtual void Dispose(bool disposing) {
        if (!disposing) return;

        Commands.Uninitialize();
        Globals.PluginCommandManager.Dispose();

        PluginInterface.SavePluginConfig(Globals.Config);

        PluginInterface.UiBuilder.Draw -= OnDraw;
        Globals.WindowSystem.RemoveAllWindows();
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    #endregion
}
