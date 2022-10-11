using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Utility;
using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using Dalamud.Plugin;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Data;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.Gui.Toast;
using Dalamud.Game.Libc;
using Dalamud.IoC;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using ResizableHUD.Attributes;
using ImGuiNET;
using System;
using System.ComponentModel;
using System.Numerics;
using System.Collections;
using System.Text;
using System.Linq;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using System.Runtime.InteropServices;

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

        [PluginService] public static KeyState KeyState { get; private set; }

        public Plugin(DalamudPluginInterface pi, CommandManager commands, ChatGui chat, ClientState clientState) {
            this.pluginInterface = pi;
            this.chat = chat;
            this.clientState = clientState;

            // Get or create a configuration object
            this.config = (Configuration)this.pluginInterface.GetPluginConfig();
            if (this.config == null) {
                this.config = this.pluginInterface.Create<Configuration>();
                if (this.config == null)
                {
                    this.config = new Configuration(pi);
                }
            }

            // Initialize the UI
            this.windowSystem = new WindowSystem(typeof(Plugin).AssemblyQualifiedName);
            this.pluginInterface.UiBuilder.Draw += OnDraw;


            // Load all of our commands
            this.commandManager = new PluginCommandManager<Plugin>(this, commands);
        }

        private void DrawPercentOption(ref ResNodeConfig nodeConfig, float WIDTH) {
            ImGui.SetNextItemWidth(WIDTH);
            ImGui.InputFloat("Scale X##RESIZABLEHUD_INPUT_SCALEX", ref nodeConfig.ScaleX);
            ImGui.SameLine();
            ImGui.SetNextItemWidth(WIDTH);
            ImGui.InputFloat("Scale Y##RESIZABLEHUD_INPUT_SCALEY", ref nodeConfig.ScaleY);

            ImGui.SliderFloat("Scale X##RESIZABLEHUD_SLIDER_SCALEX", ref nodeConfig.ScaleX, 0, 8.0f);
            ImGui.SliderFloat("Scale Y##RESIZABLEHUD_SLIDER_SCALEY", ref nodeConfig.ScaleY, 0, 8.0f);
            ImGui.Separator();
        }

        private unsafe void OnDraw()
        {
            ResNodeConfig nodeConfig = null;

            //
            try {
                RaptureAtkUnitManager* manager = AtkStage.GetSingleton()->RaptureAtkUnitManager;
                AtkUnitList* allUnitList = &manager->AtkUnitManager.AllLoadedUnitsList;
                AtkUnitList* unitList = null;
                AtkUnitBase** entries = null;
                AtkUnitBase* unit = null;
                string unitName = "";

                if (allUnitList == null)
                {
                    throw new Exception("allUnitList null");
                }

                if (config.nodeConfigs == null)
                {
                    throw new Exception("No nodes");
                }

                for (int index = 0; index < allUnitList->Count; index++)
                {
                    unitList = &allUnitList[index];
                    entries = &unitList->AtkUnitEntries;

                    if (unitList == null)
                    {
                        continue;
                    }

                    if (entries == null)
                    {
                        continue;
                    }

                    for (int undex = 0; undex < unitList->Count; undex++)
                    {
                        unitName = "";

                        try {
                            unit = entries[undex];
                        }
                        catch (Exception e) {
                            break;
                        }

                        if (unit == null || (uint)unit <= (uint)0x40) {
                            break;
                        }

                        try {
                            unitName = Marshal.PtrToStringAnsi((IntPtr)unit->Name);
                        }
                        catch(Exception e) {}

                        if (unitName == "")
                        {
                            continue;
                        }

                        for (int cndex = 0; cndex < config.nodeConfigs.Count; cndex++)
                        {
                            nodeConfig = config.nodeConfigs[cndex];
                            if (unitName == nodeConfig.Name)
                            {
                                unit->RootNode->SetScale(nodeConfig.ScaleX, nodeConfig.ScaleY);
                                unit->RootNode->SetPositionFloat(nodeConfig.PosX, nodeConfig.PosY);
                                if (nodeConfig.ForceVisible)
                                {
                                    unit->IsVisible = nodeConfig.ForceVisible;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e) { }
            //

            if (config.WindowOpen)
            {
                if (!config.WindowEverOpened)
                {
                    ImGui.SetNextWindowSize(new Vector2(320, 240));
                    ImGui.SetNextWindowPos(new Vector2(0, 0));
                    config.WindowEverOpened = true;
                }
                nodeConfig = null;
                ImGui.Begin("Resizable HUD###RESIZABLEHUDuAeh7Aq0vLJEL4Ov9sLat4sWtaQdudqyOlfRpzK");
                ImGui.Text("Units");
                if (config.nodeConfigs == null)
                {
                    ImGui.End();
                    return;
                }
                for (int cndex = 0; cndex < config.nodeConfigs.Count; cndex++)
                {
                    nodeConfig = config.nodeConfigs[cndex];
                    if (ImGui.TreeNode(nodeConfig.Name + "##RESIZABLEHUD_DROPDOWN_" + nodeConfig.Name))
                    {
                        float WIDTH = ImGui.CalcTextSize("F").X * 12;

                        if (nodeConfig.UsePercentage == false)
                        {
                            DrawPercentOption(ref nodeConfig, WIDTH);

                            ImGui.SetNextItemWidth(WIDTH);
                            ImGui.InputFloat("Pos X##RESIZABLEHUD_INPUT_POSX", ref nodeConfig.PosX);
                            ImGui.SameLine();
                            ImGui.SetNextItemWidth(WIDTH);
                            ImGui.InputFloat("Pos Y##RESIZABLEHUD_INPUT_POSY", ref nodeConfig.PosY);

                            ImGui.SliderFloat("Pos X##RESIZABLEHUD_SLIDER_SCALEX", ref nodeConfig.PosX, 0, ImGui.GetMainViewport().Size.X);
                            ImGui.SliderFloat("Pos Y##RESIZABLEHUD_SLIDER_SCALEY", ref nodeConfig.PosY, 0, ImGui.GetMainViewport().Size.Y);
                            ImGui.Separator();

                            ImGui.Checkbox("Force Visibility##RESIZABLEHUD_CHECKBOX_VIS", ref nodeConfig.ForceVisible);
                            ImGui.Checkbox("Use relative %##RESIZABLEHUD_CHECKBOX_PERCENT", ref nodeConfig.UsePercentage);
                        }
                        else
                        {
                            DrawPercentOption(ref nodeConfig, WIDTH);

                            ImGui.SetNextItemWidth(WIDTH);
                            ImGui.InputFloat("Pos X##RESIZABLEHUD_INPUT_POSX%", ref nodeConfig.PosPercentX);
                            ImGui.SameLine();
                            ImGui.SetNextItemWidth(WIDTH);
                            ImGui.InputFloat("Pos Y##RESIZABLEHUD_INPUT_POSY%", ref nodeConfig.PosPercentY);

                            ImGui.SliderFloat("Pos X##RESIZABLEHUD_SLIDER_POSX%", ref nodeConfig.PosPercentX, 0, 1.0f);
                            ImGui.SliderFloat("Pos Y##RESIZABLEHUD_SLIDER_POSY%", ref nodeConfig.PosPercentY, 0, 1.0f);
                            ImGui.Separator();

                            ImGui.Checkbox("Force Visibility##RESIZABLEHUD_CHECKBOX_VIS", ref nodeConfig.ForceVisible);
                            ImGui.Checkbox("Use relative %##RESIZABLEHUD_CHECKBOX_PERCENT", ref nodeConfig.UsePercentage);

                            nodeConfig.PosX = ImGui.GetMainViewport().Size.X * nodeConfig.PosPercentX;
                            nodeConfig.PosY = ImGui.GetMainViewport().Size.Y * nodeConfig.PosPercentY;
                        }
                    }
                }

                ImGui.Separator();

                if (ImGui.Button("Save"))
                {
                    if (config != null)
                    {
                        this.config.Save();
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
                this.chat.PrintError("argv null?");
                return;
            }

            if (config.nodeConfigs == null || config.nodeConfigs.Count == 0) {
                newConfig = new ResNodeConfig();
                newConfig.Name = argv[0];
                config.nodeConfigs = new List<ResNodeConfig>();
                config.nodeConfigs.Add(newConfig);
                index++;
            }

            for (; index < argv.Length; index++) {
                targ = argv[index];
                for (int cndex = 0; cndex < config.nodeConfigs.Count; cndex++) {
                    nodeConfig = config.nodeConfigs[cndex];
                    if (nodeConfig.Name == targ)
                    {
                        continue;
                    }
                    else
                    {
                        newConfig = new ResNodeConfig();
                        newConfig.Name = targ;
                        config.nodeConfigs.Add(newConfig);
                        this.chat.Print("Added config.");
                    }
                }
            }

            this.config.Save();
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
 
                        this.chat.Print("Removed config.");
                    }
                }
            }

            this.config.Save();
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
                            this.chat.PrintError("Please choose X, or Y.");
                            return;
                            break;
                        }
                    }
                    this.chat.Print("Scaled.");
                }
                else {
                    this.chat.PrintError("Please make sure the unit exists.");
                }
            }

            this.config.Save();
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
                            this.chat.PrintError("Please choose X, or Y.");
                            return;
                            break;
                        }
                    }
                    this.chat.Print("Scaled.");
                }
                else {
                    this.chat.PrintError("Please make sure the unit exists.");
                }
            }

            this.config.Save();
        }

        #region IDisposable Support
        protected virtual void Dispose(bool disposing) {
            if (!disposing) return;

            this.commandManager.Dispose();

            this.pluginInterface.SavePluginConfig(this.config);

            this.pluginInterface.UiBuilder.Draw -= this.OnDraw;
            this.windowSystem.RemoveAllWindows();
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
