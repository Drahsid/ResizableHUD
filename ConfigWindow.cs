using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;

namespace ResizableHUD;

internal class ConfigWindow : Window, IDisposable
{
    public static string ConfigWindowName = "Resizable HUD Config";

    private List<IntPtr> PopupCollisions = null;
    private int PressSafetyFrames = 0;

    public ConfigWindow() : base(ConfigWindowName) { }
    
    private unsafe List<IntPtr> MouseCollision() {
        RaptureAtkUnitManager* manager = AtkStage.GetSingleton()->RaptureAtkUnitManager;
        AtkUnitList* unitManagers = &manager->AtkUnitManager.DepthLayerOneList;
        AtkUnitBase* unit = null;
        List<IntPtr> ret = new List<IntPtr>();

        for (int index = 0; index < RaptureAtkUnitManagerHelper.UnitListCount; index++) {
            AtkUnitList* unitManager = &unitManagers[index];
            AtkUnitBase** unitBaseArray = &unitManager->AtkUnitEntries;

            if (Globals.Config.OnlyPeekInLayer != RaptureAtkUnitManagerHelper.UnitListEntry.Count && index != (int)Globals.Config.OnlyPeekInLayer) {
                continue;
            }

            for (int qndex = 0; qndex < unitManager->Count; qndex++) {
                AtkUnitBase* unitBase = unitBaseArray[qndex];
                string name = Marshal.PtrToStringAnsi(new IntPtr(unitBase->Name));

                if (Globals.Config.OnlyPeekVisible && unitBase->IsVisible == false) {
                    continue;
                }

                if (unitBase->RootNode != null) {
                    if (RaptureAtkUnitManagerHelper.DrawMouseIntersection(unitBase->RootNode, name)) {
                        ret.Add((IntPtr)unitBase);
                    }
                }
            }
        }

        return ret;
    }

    private void DrawTail()
    {
        ImGui.Separator();
        if (ImGui.Button("Save"))
        {
            if (Globals.Config != null)
            {
                Globals.Config.Save();
            }
        }
    }

    public unsafe void DrawAddonInspector() {
        bool popup_open = ImGui.IsPopupOpen("AddonInspectorContextMenu");
        List<IntPtr> collisions = PopupCollisions;

        if (PressSafetyFrames > 0) {
            PressSafetyFrames--;
        }

        if (popup_open) {
            for (int index = 0; index < PopupCollisions.Count; index++) {
                AtkUnitBase* unit = (AtkUnitBase*)PopupCollisions[index];
                string name = Marshal.PtrToStringAnsi(new IntPtr(unit->Name));
                RaptureAtkUnitManagerHelper.DrawOutline(unit->RootNode, name);
            }
        }
        else {
            collisions = MouseCollision();
        }

        if (ImGui.IsMouseDown(ImGuiMouseButton.Right)) {
            if (!popup_open && PressSafetyFrames == 0) {
                ImGui.OpenPopup("AddonInspectorContextMenu");
                PopupCollisions = collisions;
                PressSafetyFrames = 8;
            }
        }

        if (ImGui.BeginPopup("AddonInspectorContextMenu")) {
            if (ImGui.IsMouseDown(ImGuiMouseButton.Right) && PressSafetyFrames == 0) {
                ImGui.CloseCurrentPopup();
            }
            if (PopupCollisions != null) {
                for (int index = 0; index < PopupCollisions.Count; index++) {
                    AtkUnitBase* unit = (AtkUnitBase*)PopupCollisions[index];
                    string name = Marshal.PtrToStringAnsi(new IntPtr(unit->Name));

                    if (!AddonManager.CheckIfInConfig(unit)) {
                        if (ImGui.Button(name)) {
                            AddonManager.AddToConfig(unit);
                        }
                    }
                }
            }
            ImGui.EndPopup();
        }
    }

    public override unsafe void Draw()
    {
        Vector2 vp = ImGui.GetMainViewport().Size;
        ImGui.InputInt("Base Resolution X", ref Globals.Config.BaseResolutionX);
        ImGuiStuff.DrawTooltip("Sets the width of the base resolution. The base resolution is used for scaling the UI scale parameters");

        ImGui.InputInt("Base Resolution Y", ref Globals.Config.BaseResolutionY);
        ImGuiStuff.DrawTooltip("Sets the height of the base resolution. The base resolution is used for scaling the UI scale parameters");

        if (ImGui.Button("Set from Viewport")) {
            Globals.Config.BaseResolutionX = (int)vp.X;
            Globals.Config.BaseResolutionY = (int)vp.Y;
        }
        ImGuiStuff.DrawTooltip("Sets the base resolution to your current resolution. The base resolution is used for scaling the UI scale parameters");

        ImGuiStuff.DrawCheckboxTooltip("Draw Addon Inspector", ref Globals.Config.DrawAddonInspector, "When enabled, you will see an overlay of addons (UI elements). Pressing Right Mouse will bring up a context menu with the addons that are under your mouse. Clicking on one of these will add it to the list");
        if (Globals.Config.DrawAddonInspector) {
            ImGuiStuff.DrawCheckboxTooltip("Draw Only Visible", ref Globals.Config.OnlyPeekVisible, "When enabled, the addon inspector will only draw addons that are marked as visible");
        }

        if (Globals.Config.DrawAddonInspector) {
            ImGui.SetNextFrameWantCaptureMouse(true);
            DrawAddonInspector();
        }

        ImGui.Separator();
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
