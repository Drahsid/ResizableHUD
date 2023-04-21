using Dalamud.Interface;
using Dalamud.Logging;
using Dalamud.Game.ClientState.Keys;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using static ResizableHUD.ResNodeConfig;

namespace ResizableHUD;

internal class AddonManager
{
    private const uint NodeColor = 0xFF00FFFF;
    private const uint NodeEditColor = 0xFFFF00FF;
    private const uint NodeAnchorColor = 0xFFFFFF00;
    private static Vector2 LastMousePos = Vector2.Zero;

    public static unsafe bool CheckIfInConfig(AtkUnitBase* unit) {
        string name = Marshal.PtrToStringAnsi(new IntPtr(unit->Name));
        bool dupe = false;
        foreach (ResNodeConfig config in Globals.Config.nodeConfigs) {
            if (config.Name == name) {
                dupe = true;
                break;
            }
        }

        return dupe;
    }

    public static unsafe void AddToConfig(AtkUnitBase* unit) {
        string name = Marshal.PtrToStringAnsi(new IntPtr(unit->Name));
        AtkResNode* res = unit->RootNode;
        Vector2 pos = RaptureAtkUnitManagerHelper.GetNodePosition(res);
        Vector2 size = RaptureAtkUnitManagerHelper.GetNodeScaledSize(res);
        Vector2 scale = RaptureAtkUnitManagerHelper.GetNodeScale(res);
        Vector2 vp = ImGui.GetMainViewport().Size;
        bool visible = RaptureAtkUnitManagerHelper.GetNodeVisible(res);

        if (!CheckIfInConfig(unit)) {
            ResNodeConfig cfg = new ResNodeConfig();
            cfg.Name = name;
            cfg.DoNotPosition = false;
            cfg.DoNotScale = false;
            cfg.PosX = pos.X;
            cfg.PosY = pos.Y;
            cfg.PosPercentX = pos.X / vp.X;
            cfg.PosPercentY = pos.Y / vp.Y;
            cfg.ForceVisible = false;
            cfg.ScaleX = scale.X;
            cfg.ScaleY = scale.Y;
            cfg.UsePercentagePos = false;
            cfg.UsePercentageScale = false;
            Globals.Config.nodeConfigs.Add(cfg);
            Globals.Config.nodeConfigs.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase));
        }
    }

    public static unsafe void UpdateAddon(ref ResNodeConfig nodeConfig) {
        RaptureAtkUnitManager* manager = AtkStage.GetSingleton()->RaptureAtkUnitManager;
        AtkUnitBase* unit = null;
        Vector2 vp = ImGui.GetMainViewport().Size;

        unit = manager->GetAddonByName(nodeConfig.Name);
        if (unit != null) {
            AtkResNode* res = unit->RootNode;

            if (nodeConfig.ForceVisible || unit->IsVisible) {
                float ratio_x = vp.X / (float)Globals.Config.BaseResolutionX;
                float ratio_y = vp.Y / (float)Globals.Config.BaseResolutionY;
                float scale_x = nodeConfig.ScaleX;
                float scale_y = nodeConfig.ScaleY;


                if (nodeConfig.UsePercentageScale) {
                    scale_x *= ratio_x;
                    scale_y *= ratio_y;
                }

                if (nodeConfig.DoNotScale == false) {
                    unit->RootNode->SetScale(scale_x, scale_y);
                }

                Vector2 size = RaptureAtkUnitManagerHelper.GetNodeScaledSize(res);
                Vector2 pos = GetTLPos(ref nodeConfig);

                if (nodeConfig.UsePercentagePos) {
                    nodeConfig.PosX = nodeConfig.PosPercentX * vp.X;
                    nodeConfig.PosY = nodeConfig.PosPercentY * vp.Y;
                }

                if (nodeConfig.DoNotPosition == false) {
                    unit->RootNode->SetPositionFloat(pos.X, pos.Y);
                }


                if (nodeConfig.ForceVisible) {
                    unit->IsVisible = nodeConfig.ForceVisible;
                }
            }
        }
    }

    public static void UpdateAddons() {
        ResNodeConfig nodeConfig = null;

        if (Globals.Config.nodeConfigs == null) {
            return;
        }

        for (int cndex = 0; cndex < Globals.Config.nodeConfigs.Count; cndex++) {
            nodeConfig = Globals.Config.nodeConfigs[cndex];
            UpdateAddon(ref nodeConfig);
        }
    }

    public static void DrawAddonNodes()
    {
        ResNodeConfig nodeConfig = null;

        for (int cndex = 0; cndex < Globals.Config.nodeConfigs.Count; cndex++)
        {
            nodeConfig = Globals.Config.nodeConfigs[cndex];
            if (nodeConfig == null)
            {
                continue;
            }

            if (ImGui.TreeNode(nodeConfig.Name + "##RESIZABLEHUD_DROPDOWN_" + nodeConfig.Name))
            {
                float WIDTH = ImGui.CalcTextSize("F").X * 12;

                if (nodeConfig.Editing) {
                    DrawNodeEditor(ref nodeConfig);
                }
                else {
                    DrawNodePreview(ref nodeConfig);
                }

                DrawAnchorOption(ref nodeConfig);
                ImGuiStuff.DrawCheckboxTooltip("Edit", ref nodeConfig.Editing, "When enabled, allows you to edit the transform with your keyboard using the arrow keys. Holding Shift will allow you to scale");
                ImGuiStuff.DrawCheckboxTooltip("No position", ref nodeConfig.DoNotPosition, "When enabled, does not update the position");
                ImGui.SameLine();
                ImGuiStuff.DrawCheckboxTooltip("No scale", ref nodeConfig.DoNotScale, "When enabled, does not update the scale");
                ImGuiStuff.DrawCheckboxTooltip("Force visibility", ref nodeConfig.ForceVisible, "When enabled, the addon is internally forced to be visible");

                ImGui.Separator();
                if (!nodeConfig.DoNotPosition) {
                    DrawPosOption(ref nodeConfig, WIDTH);
                    ImGuiStuff.DrawCheckboxTooltip("Use relative##RESIZABLEHUD_DROPDOWN_POS_PERCENT", ref nodeConfig.UsePercentagePos, "When enabled, the position value will represent the percent on-screen that the addon is positioned");
                }

                ImGui.Separator();
                if (!nodeConfig.DoNotScale) {
                    DrawScaleOption(ref nodeConfig, WIDTH);
                    ImGuiStuff.DrawCheckboxTooltip("Use relative##RESIZABLEHUD_DROPDOWN_SCL_PERCENT", ref nodeConfig.UsePercentageScale, "When enabled, the scale value will be scaled by the base resolution");
                }
                

                if (ImGui.Button("Remove")) {
                    Globals.Config.nodeConfigs.Remove(nodeConfig);
                    break;
                }

                ImGui.Separator();
                ImGui.TreePop();
            }
            if (ImGui.IsItemHovered()) {
                DrawNodePreview(ref nodeConfig);
            }
        }

        LastMousePos = ImGui.GetMousePos();
    }

    private static unsafe void DrawNodePreview(ref ResNodeConfig nodeConfig) {
        RaptureAtkUnitManager* manager = AtkStage.GetSingleton()->RaptureAtkUnitManager;

        AtkUnitBase* unit = manager->GetAddonByName(nodeConfig.Name);
        if (unit != null) {
            Vector2 size = RaptureAtkUnitManagerHelper.GetNodeScaledSize(unit->RootNode);
            Vector2 offset = GetAnchorOffset(nodeConfig.anchor, size);
            Vector2 pos = GetTLPos(ref nodeConfig);

            ImGui.GetForegroundDrawList(ImGuiHelpers.MainViewport).AddRect(pos, pos + size, NodeColor);
            ImGui.GetForegroundDrawList(ImGuiHelpers.MainViewport).AddCircleFilled(pos + offset, 4.0f, NodeAnchorColor);
            ImGui.GetForegroundDrawList(ImGuiHelpers.MainViewport).AddText(pos, NodeColor, nodeConfig.Name);
        }
    }

    private static unsafe void DrawNodeEditor(ref ResNodeConfig nodeConfig) {
        RaptureAtkUnitManager* manager = AtkStage.GetSingleton()->RaptureAtkUnitManager;
        AtkUnitBase* unit = manager->GetAddonByName(nodeConfig.Name);

        //ImGui.SetNextFrameWantCaptureMouse(true);

        if (unit != null) {
            Vector2 size = RaptureAtkUnitManagerHelper.GetNodeScaledSize(unit->RootNode);
            Vector2 pos = GetTLPos(ref nodeConfig);
            Vector2 offset = GetAnchorOffset(nodeConfig.anchor, size);
            Vector2 box_size = new Vector2(16.0f, 16.0f) * ImGuiHelpers.GlobalScale;
            Vector2 bottom = pos + GetAnchorOffset(PositionAnchor.BOTTOM_CENTER, size) - (box_size * 0.5f);
            Vector2 right = pos + GetAnchorOffset(PositionAnchor.CENTER_RIGHT, size) - (box_size * 0.5f);
            

            ImGui.GetForegroundDrawList(ImGuiHelpers.MainViewport).AddRect(pos, pos + size, NodeEditColor);
            ImGui.GetForegroundDrawList(ImGuiHelpers.MainViewport).AddCircleFilled(pos + offset, 4.0f, NodeAnchorColor);
            //ImGui.GetForegroundDrawList(ImGuiHelpers.MainViewport).AddRectFilled(bottom, bottom + box_size, NodeAnchorColor);
            //ImGui.GetForegroundDrawList(ImGuiHelpers.MainViewport).AddRectFilled(right, right + box_size, NodeAnchorColor);
            ImGui.GetForegroundDrawList(ImGuiHelpers.MainViewport).AddText(pos, NodeColor, nodeConfig.Name);

            // doesn't work well (refresh rate related?
            // NodeEditorMouseControls(ref nodeConfig, pos, size, bottom, right, box_size);
            NodeEditorKeyboardControls(ref nodeConfig, pos, size, box_size);
        }
    }

    private static unsafe void DrawAnchorOption(ref ResNodeConfig nodeConfig) {
        string[] anchor_names = Enum.GetNames(typeof(PositionAnchor));
        string current_anchor_label = anchor_names[(int)nodeConfig.anchor];
        Vector2 offset = Vector2.Zero;
        Vector2 size = Vector2.Zero;
        Vector2 vp = ImGui.GetMainViewport().Size;
        RaptureAtkUnitManager* manager = AtkStage.GetSingleton()->RaptureAtkUnitManager;
        AtkUnitBase* unit = manager->GetAddonByName(nodeConfig.Name);

        if (unit != null) {
            size = RaptureAtkUnitManagerHelper.GetNodeScaledSize(unit->RootNode);
            offset = GetAnchorOffset(nodeConfig.anchor, size);
        }

        if (ImGui.BeginCombo("Position Anchor", current_anchor_label)) {
            for (int index = 0; index < anchor_names.Length; index++) {
                bool is_selected = (nodeConfig.anchor == (PositionAnchor)index);

                // Add each enum value as a selectable item in the dropdown
                if (ImGui.Selectable(anchor_names[index], is_selected)) {
                    nodeConfig.anchor = (PositionAnchor)index;

                    if (unit != null) {
                        offset -= GetAnchorOffset(nodeConfig.anchor, size);
                        nodeConfig.PosX -= offset.X;
                        nodeConfig.PosY -= offset.Y;
                        nodeConfig.PosPercentX = nodeConfig.PosX / vp.X;
                        nodeConfig.PosPercentY = nodeConfig.PosY / vp.Y;
                    }
                }

                // Set the currently selected item as highlighted
                if (is_selected) {
                    ImGui.SetItemDefaultFocus();
                }
            }

            ImGui.EndCombo();
        }
    }

    private static unsafe void NodeEditorMouseControls(ref ResNodeConfig nodeConfig, Vector2 tl_pos, Vector2 size, Vector2 bottom, Vector2 right, Vector2 box_size) {
        RaptureAtkUnitManager* manager = AtkStage.GetSingleton()->RaptureAtkUnitManager;
        AtkUnitBase* unit = manager->GetAddonByName(nodeConfig.Name);
        Vector2 mpos = ImGui.GetMousePos();

        if (RaptureAtkUnitManagerHelper.GetPointIntersectsNode(unit->RootNode, mpos)) {
            Vector2 mdelta = mpos - LastMousePos;
            Vector2 mdelta_pos = mdelta;
            Vector2 mdelta_scale = mdelta;
            Vector2 vp = ImGui.GetMainViewport().Size;
            float ratio_x = vp.X / (float)Globals.Config.BaseResolutionX;
            float ratio_y = vp.Y / (float)Globals.Config.BaseResolutionY;
            bool down = ImGui.IsMouseDown(ImGuiMouseButton.Left);

            if (nodeConfig.UsePercentagePos) {
                mdelta_pos.X = mdelta.X / vp.X;
                mdelta_pos.Y = mdelta.Y / vp.Y;
            }

            if (nodeConfig.UsePercentageScale) {
                mdelta_scale.X *= ratio_x;
                mdelta_scale.Y *= ratio_y;
            }

            mdelta_scale *= 0.01f;

            if (down) {
                Vector2 newsize;

                if (RaptureAtkUnitManagerHelper.GetPointIntersectsRect(mpos, bottom, box_size)) {
                    nodeConfig.ScaleY += mdelta_scale.Y;

                    UpdateAddon(ref nodeConfig);
                    newsize = RaptureAtkUnitManagerHelper.GetNodeScaledSize(unit->RootNode);

                    nodeConfig.PosY += mdelta.Y * 0.5f;
                    nodeConfig.PosPercentY = nodeConfig.PosY / vp.Y;
                }
                else if (RaptureAtkUnitManagerHelper.GetPointIntersectsRect(mpos, right, box_size)) {
                    nodeConfig.ScaleX += mdelta_scale.X;

                    UpdateAddon(ref nodeConfig);
                    newsize = RaptureAtkUnitManagerHelper.GetNodeScaledSize(unit->RootNode);

                    nodeConfig.PosX += mdelta.X * 0.5f;
                    nodeConfig.PosPercentX = nodeConfig.PosX / vp.X;

                }
                else if (RaptureAtkUnitManagerHelper.GetPointIntersectsRect(mpos, tl_pos, size)) {
                    if (nodeConfig.UsePercentagePos) {
                        nodeConfig.PosPercentX += mdelta_pos.X;
                        nodeConfig.PosPercentY += mdelta_pos.Y;
                    }
                    else {
                        nodeConfig.PosX += mdelta_pos.X;
                        nodeConfig.PosY += mdelta_pos.Y;
                    }
                }
            }
        }
    }

    private static unsafe void NodeEditorKeyboardControls(ref ResNodeConfig nodeConfig, Vector2 tl_pos, Vector2 size, Vector2 box_size) {
        RaptureAtkUnitManager* manager = AtkStage.GetSingleton()->RaptureAtkUnitManager;
        AtkUnitBase* unit = manager->GetAddonByName(nodeConfig.Name);
        Vector2 vp = ImGui.GetMainViewport().Size;
        Vector2 deltasize;
        float v = 0.0f;
        float h = 0.0f;
        bool scale = Globals.KeyState[VirtualKey.SHIFT];

        v -= Globals.KeyState[VirtualKey.UP] ? 1.0f : 0.0f;
        h += Globals.KeyState[VirtualKey.RIGHT] ? 1.0f : 0.0f;
        v -= Globals.KeyState[VirtualKey.DOWN] ? -1.0f : 0.0f;
        h += Globals.KeyState[VirtualKey.LEFT] ? -1.0f : 0.0f;

        if (v != 0 || h != 0) {
            if (scale) {
                nodeConfig.ScaleX += h * 0.01f;
                nodeConfig.ScaleY += v * 0.01f;

                UpdateAddon(ref nodeConfig);
                deltasize = (RaptureAtkUnitManagerHelper.GetNodeScaledSize(unit->RootNode) - size) * 0.5f;

                nodeConfig.PosX -= deltasize.X;
                nodeConfig.PosY -= deltasize.Y;
                nodeConfig.PosPercentX = nodeConfig.PosX / vp.X;
                nodeConfig.PosPercentY = nodeConfig.PosY / vp.Y;
            }
            else {
                nodeConfig.PosX += h;
                nodeConfig.PosY += v;
                nodeConfig.PosPercentX = nodeConfig.PosX / vp.X;
                nodeConfig.PosPercentY = nodeConfig.PosY / vp.Y;
            }
        }
    }

    private static unsafe Vector2 GetTLPos(ref ResNodeConfig nodeConfig) {
        RaptureAtkUnitManager* manager = AtkStage.GetSingleton()->RaptureAtkUnitManager;
        AtkUnitBase* unit = manager->GetAddonByName(nodeConfig.Name);

        if (unit != null) {
            Vector2 size = RaptureAtkUnitManagerHelper.GetNodeScaledSize(unit->RootNode);
            Vector2 offset = GetAnchorOffset(nodeConfig.anchor, size);
            return new Vector2(nodeConfig.PosX - offset.X, nodeConfig.PosY - offset.Y);
        }

        return new Vector2(nodeConfig.PosX, nodeConfig.PosY);
    }

    private static Vector2 GetAnchorOffset(PositionAnchor anchor, Vector2 size) {
        float x = 0;
        float y = 0;

        switch (anchor) {
            case PositionAnchor.TOP_LEFT:
                x = 0;
                y = 0;
                break;
            case PositionAnchor.TOP_CENTER:
                x = size.X / 2;
                y = 0;
                break;
            case PositionAnchor.TOP_RIGHT:
                x = size.X;
                y = 0;
                break;
            case PositionAnchor.CENTER_LEFT:
                x = 0;
                y = size.Y / 2;
                break;
            case PositionAnchor.CENTER_CENTER:
                x = size.X / 2;
                y = size.Y / 2;
                break;
            case PositionAnchor.CENTER_RIGHT:
                x = size.X;
                y = size.Y / 2;
                break;
            case PositionAnchor.BOTTOM_LEFT:
                x = 0;
                y = size.Y;
                break;
            case PositionAnchor.BOTTOM_CENTER:
                x = size.X / 2;
                y = size.Y;
                break;
            case PositionAnchor.BOTTOM_RIGHT:
                x = size.X;
                y = size.Y;
                break;
        }

        return new Vector2(x, y);
    }

    private static unsafe void DrawScaleOption(ref ResNodeConfig nodeConfig, float WIDTH)
    {
        RaptureAtkUnitManager* manager = AtkStage.GetSingleton()->RaptureAtkUnitManager;
        AtkUnitBase* unit = manager->GetAddonByName(nodeConfig.Name);
        Vector2 size = Vector2.Zero;

        if (unit != null) {
            size = RaptureAtkUnitManagerHelper.GetNodeScaledSize(unit->RootNode);
        }

        ImGui.SetNextItemWidth(WIDTH);
        ImGui.InputFloat("Scale X##RESIZABLEHUD_INPUT_SCALEX", ref nodeConfig.ScaleX);
        ImGui.SameLine();
        ImGui.SetNextItemWidth(WIDTH);
        ImGui.InputFloat("Scale Y##RESIZABLEHUD_INPUT_SCALEY", ref nodeConfig.ScaleY);
        ImGui.SameLine();
        if (size != Vector2.Zero) {
            ImGui.TextDisabled($"({size.X}x{size.Y}) px");
        }
        else {
            ImGui.TextDisabled("??x?? px");
        }
    }

    private static unsafe void DrawPosOption(ref ResNodeConfig nodeConfig, float WIDTH)
    {
        Vector2 vp = ImGui.GetMainViewport().Size;

        if (nodeConfig.UsePercentagePos == false)
        {
            ImGui.SetNextItemWidth(WIDTH);
            if (ImGui.InputFloat("Pos X##RESIZABLEHUD_INPUT_POSX", ref nodeConfig.PosX)) {
                nodeConfig.PosPercentX = nodeConfig.PosX / vp.X;
            }
            ImGui.SameLine();
            ImGui.SetNextItemWidth(WIDTH);
            if (ImGui.InputFloat("Pos Y##RESIZABLEHUD_INPUT_POSY", ref nodeConfig.PosY)) {
                nodeConfig.PosPercentY = nodeConfig.PosY / vp.Y;
            }
        }
        else
        {
            ImGui.SetNextItemWidth(WIDTH);
            ImGui.InputFloat("Pos X##RESIZABLEHUD_INPUT_POSX%", ref nodeConfig.PosPercentX);
            ImGui.SameLine();
            ImGui.SetNextItemWidth(WIDTH);
            ImGui.InputFloat("Pos Y##RESIZABLEHUD_INPUT_POSY%", ref nodeConfig.PosPercentY);
        }
    }
}
