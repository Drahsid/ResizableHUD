using Dalamud.Game.ClientState;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Game.ClientState.Keys;
using Dalamud.IoC;
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
        private readonly DalamudPluginInterface pluginInterface;
        private readonly ChatGui chat;
        private readonly ClientState clientState;

        private readonly PluginCommandManager<Plugin> commandManager;
        private readonly Configuration config;
        private readonly WindowSystem windowSystem;

        public string Name => "ResizableHUD";

        public Plugin(DalamudPluginInterface pi, CommandManager com, ChatGui ch, ClientState cs) {
            pluginInterface = pi;
            chat = ch;
            clientState = cs;

            // Get or create a configuration object
            config = (Configuration)pi.GetPluginConfig() ?? new Configuration();
            config.Initialize();

            // Initialize the UI
            windowSystem = new WindowSystem(typeof(Plugin).AssemblyQualifiedName);
            pi.UiBuilder.Draw += OnDraw;

            // Load all of our commands
            commandManager = new PluginCommandManager<Plugin>(this, com);
        }

        private void SaveConfig()
        {
            pluginInterface.SavePluginConfig(config);
        } 

        private unsafe void UpdateAddons()
        {
            RaptureAtkUnitManager* manager = AtkStage.GetSingleton()->RaptureAtkUnitManager;
            AtkUnitBase* unit = null;
            ResNodeConfig nodeConfig = null;

            if (config.nodeConfigs == null)
            {
                return;
            }

            for (int cndex = 0; cndex < config.nodeConfigs.Count; cndex++)
            {
                nodeConfig = config.nodeConfigs[cndex];
                unit = manager->GetAddonByName(nodeConfig.Name);

                if (unit != null)
                {
                    if (nodeConfig.ForceVisible || unit->IsVisible)
                    {
                        if (nodeConfig.DoNotScale == false)
                        {
                            unit->RootNode->SetScale(nodeConfig.ScaleX, nodeConfig.ScaleY);
                        }
                        if (nodeConfig.DoNotPosition == false)
                        {
                            unit->RootNode->SetPositionFloat(nodeConfig.PosX, nodeConfig.PosY);
                        }
                        if (nodeConfig.ForceVisible)
                        {
                            unit->IsVisible = nodeConfig.ForceVisible;
                        }
                    }
                }
            }
        }

        private void OnDraw()
        {
            ResNodeConfig nodeConfig = null;

            if (config.WindowOpen)
            {
                if (!config.WindowEverOpened)
                {
                    ImGui.SetNextWindowSize(new Vector2(320, 240));
                    ImGui.SetNextWindowPos(new Vector2(0, 0));
                    config.WindowEverOpened = true;
                }

                ImGui.Begin("Resizable HUD###RESIZABLEHUDuAeh7Aq0vLJEL4Ov9s");
                ImGui.Text("Units");
                if (config.nodeConfigs == null)
                {
                    ImGui.End();
                    return;
                }

                for (int cndex = 0; cndex < config.nodeConfigs.Count; cndex++)
                {
                    nodeConfig = config.nodeConfigs[cndex];
                    if (nodeConfig == null)
                    {
                        continue;
                    }

                    if (ImGui.TreeNode(nodeConfig.Name + "##RESIZABLEHUD_DROPDOWN_" + nodeConfig.Name))
                    {
                        float WIDTH = ImGui.CalcTextSize("F").X * 12;

                        DrawFuncs.DrawScaleOption(ref nodeConfig, WIDTH, config.EpsillonAmount);
                        DrawFuncs.DrawPosOption(ref nodeConfig, WIDTH, config.EpsillonAmount);
                        ImGui.Checkbox("Do not change position", ref nodeConfig.DoNotPosition);
                        ImGui.Checkbox("Do not change scale", ref nodeConfig.DoNotScale);
                        ImGui.Separator();
                        ImGui.Spacing();
                    }
                }

                ImGui.Separator();

                if (ImGui.Button("Save"))
                {
                    if (config != null)
                    {
                        SaveConfig();
                    }
                }

                ImGui.End();
            }
        }

        [Command("/prhud")]
        [HelpMessage("Toggle the visibility of the configuration window.")]
        public void OnPrHud(string command, string args)
        {
            config.WindowOpen = !config.WindowOpen;
        }

        [Command("/prhudadd")]
        [HelpMessage("add [a] unit[s] to the config. For example \"/prhud add _TargetInfoCastBar _TargetCursor\".")]
        public unsafe void ResizableHud_Add(string command, string args) {
            string[] argv = args.Split(' ');
            string targ;
            ResNodeConfig nodeConfig;
            ResNodeConfig newConfig;
            int index = 0;

            if (argv == null || argv.Length == 0)
            {
                chat.PrintError("argv null?");
                return;
            }

            if (config.nodeConfigs == null || config.nodeConfigs.Count == 0) {
                newConfig = new ResNodeConfig();
                newConfig.Name = argv[0];
                config.nodeConfigs = new List<ResNodeConfig>{ newConfig };
                index++;
                chat.Print("Added first config.");
            }

            for (; index < argv.Length; index++) {
                bool add = true;
                targ = argv[index];
                for (int cndex = 0; cndex < config.nodeConfigs.Count; cndex++) {
                    nodeConfig = config.nodeConfigs[cndex];
                    if (nodeConfig.Name == targ)
                    {
                        add = false;
                        break;
                    }
                }

                if (add)
                {
                    newConfig = new ResNodeConfig();
                    newConfig.Name = targ;
                    config.nodeConfigs.Add(newConfig);
                    chat.Print("Added config.");
                }
            }

            SaveConfig();
        }

        [Command("/prhudrem")]
        [HelpMessage("remove [a] unit[s] from the config. For example \"/prhud rem _TargetInfoCastBar _TargetCursor\".")]
        public unsafe void ResizableHud_Rem(string command, string args) {
            string[] argv = args.Split(' ');
            string targ;
            ResNodeConfig nodeConfig;

            for (int index = 0; index < argv.Length; index++) {
                targ = argv[index];
                for (int cndex = 0; cndex < config.nodeConfigs.Count; cndex++) {
                    nodeConfig = config.nodeConfigs[cndex];
                    if (nodeConfig.Name == targ) {
                        config.nodeConfigs.RemoveAt(cndex);
 
                        chat.Print("Removed config.");
                    }
                }
            }

            SaveConfig();
        }

        [Command("/prhudscale")]
        [HelpMessage("Change the X or Y scale of a [unit]. For example \"/prhud scale _TargetInfoCastBar X 3\".")]
        public unsafe void ResizableHud_Scale(string command, string args) {
            string[] argv = args.Split(' ');
            string targ;
            ResNodeConfig nodeConfig;

            targ = argv[0];
            for (int cndex = 0; cndex < config.nodeConfigs.Count; cndex++) {
                nodeConfig = config.nodeConfigs[cndex];
                if (nodeConfig.Name == targ) {
                    targ = argv[1];
                    switch (targ.ToUpper()) {
                        case ("X"): {
                            nodeConfig.ScaleX = float.Parse(argv[2]);
                            break;
                        }
                        case ("Y"): {
                            nodeConfig.ScaleY = float.Parse(argv[2]);
                            break;
                        }
                        default: {
                            chat.PrintError("Please choose X, or Y.");
                            return;
                            break;
                        }
                    }
                    chat.Print("Scaled.");
                }
                else {
                    chat.PrintError("Please make sure the unit exists.");
                }
            }

            SaveConfig();
        }

        [Command("/prhudpos")]
        [HelpMessage("Change the X or Y pos of a [unit]. For example \"/prhud pos _TargetInfoCastBar X 320\".")]
        public unsafe void ResizableHud_Pos(string command, string args) {
            string[] argv = args.Split(' ');
            string targ;
            ResNodeConfig nodeConfig;

            targ = argv[0];
            for (int cndex = 0; cndex < config.nodeConfigs.Count; cndex++) {
                nodeConfig = config.nodeConfigs[cndex];
                if (nodeConfig.Name == targ) {
                    targ = argv[1];
                    switch (targ.ToUpper()) {
                        case ("X"): {
                            nodeConfig.PosX = float.Parse(argv[2]);
                            break;
                        }
                        case ("Y"): {
                            nodeConfig.PosY = float.Parse(argv[2]);
                            break;
                        }
                        default: {
                            chat.PrintError("Please choose X, or Y.");
                            return;
                            break;
                        }
                    }
                    chat.Print("Scaled.");
                }
                else {
                    chat.PrintError("Please make sure the unit exists.");
                }
            }

            SaveConfig();
        }

        #region IDisposable Support
        protected virtual void Dispose(bool disposing) {
            if (!disposing) return;

            commandManager.Dispose();

            pluginInterface.SavePluginConfig(config);

            pluginInterface.UiBuilder.Draw -= OnDraw;
            windowSystem.RemoveAllWindows();
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
