using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using System.Numerics;

namespace ResizableHUD
{
    internal class ConfigWindow : Window, IDisposable
    {
        public static string ConfigWindowName = "Resizable HUD Config";

        public ConfigWindow() : base(ConfigWindowName) { }

        private void DrawTail()
        {
            if (ImGui.Button("Save"))
            {
                if (Globals.Config != null)
                {
                    Globals.Config.Save();
                }
            }
        }

        public override void Draw()
        {
            ImGui.Text("Units");
            if (Globals.Config.nodeConfigs == null)
            {
                DrawTail();
                return;
            }
            AddonManager.DrawAddonNodes();
            DrawTail();
        }

        public void Dispose() { }
    }
}
