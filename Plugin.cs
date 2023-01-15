using Dalamud.Game.ClientState;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using ImGuiNET;
using System;
using System.Numerics;

[assembly: System.Reflection.AssemblyVersion("1.0.0.*")]

namespace ResizableHUD
{
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
            PluginInterface.UiBuilder.OpenConfigUi += ToggleConfig;

            Globals.CommandManager = new PluginCommandManager<Plugin>(this, commandManager);
        }

        private void ToggleConfig()
        {
            Globals.Config.WindowOpen = !Globals.Config.WindowOpen;
            Globals.WindowSystem.GetWindow(ConfigWindow.ConfigWindowName).IsOpen= Globals.Config.WindowOpen;
        }

        

        #region IDisposable Support
        protected virtual void Dispose(bool disposing) {
            if (!disposing) return;

            Globals.CommandManager.Dispose();

            PluginInterface.SavePluginConfig(Globals.Config);

            PluginInterface.UiBuilder.Draw -= OnDraw;
            Globals.WindowSystem.RemoveAllWindows();
        }

        private void OnDraw()
        {
            AddonManager.UpdateAddons();
            Globals.WindowSystem.Draw();
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
