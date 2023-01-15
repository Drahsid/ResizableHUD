using Dalamud.Game.ClientState;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ResizableHUD.Attributes;
using ImGuiNET;
using System;
using System.Numerics;
using System.Collections.Generic;


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
            PluginInterface.UiBuilder.Draw += OnDraw;

            Globals.CommandManager = new PluginCommandManager<Plugin>(this, commandManager);
        }

        private void DrawTail()
        {
            if (ImGui.Button("Save"))
            {
                if (Globals.Config != null)
                {
                    Globals.Config.Save();
                }
            }

            ImGui.End();
        }

        private void OnDraw()
        {
            AddonManager.UpdateAddons();

            if (Globals.Config.WindowOpen)
            {
                if (!Globals.Config.WindowEverOpened)
                {
                    ImGui.SetNextWindowSize(new Vector2(320, 240));
                    ImGui.SetNextWindowPos(new Vector2(0, 0));
                    Globals.Config.WindowEverOpened = true;
                }

                ImGui.Begin("Resizable HUD###RESIZABLEHUDuAeh7Aq0vLJEL4Ov9s");
                ImGui.Text("Units");
                if (Globals.Config.nodeConfigs == null)
                {
                    DrawTail();
                    return;
                }
                AddonManager.DrawAddonNodes();
                DrawTail();
            }
        }

        #region IDisposable Support
        protected virtual void Dispose(bool disposing) {
            if (!disposing) return;

            Globals.CommandManager.Dispose();

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
}
