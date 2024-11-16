using DrahsidLib;
using Dalamud.Game.ClientState.Keys;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Interface.Utility;

using static ResizableHUD.ResNodeConfig;

// TODO: refactor this file

namespace ResizableHUD;

internal class AddonManager
{
    private const uint NodeColor = 0xFF00FFFF;
    private const uint NodeEditColor = 0xFFFF00FF;
    private const uint NodeAnchorColor = 0xFFFFFF00;
    private const uint NodeParentColor = 0xFFFFFFFF;
    private const uint NodeParentAnchorColor = 0xFF0000FF;
    private static Vector2 LastMousePos = Vector2.Zero;

    public static unsafe bool CheckIfInConfig(AtkUnitBase* unit) {
        List<ResNodeConfig> config = Globals.Config.GetCurrentNodeConfig();
        return config.Any(node => node.Name == unit->NameString);
    }

    public static unsafe ResNodeConfig GetAddonConfig(AtkUnitBase* unit, PositionAnchor anchor = PositionAnchor.TOP_LEFT) {
        AtkResNode* res = unit->RootNode;
        Vector2 pos = RaptureAtkUnitManagerHelper.GetNodePosition(res);
        Vector2 size = RaptureAtkUnitManagerHelper.GetNodeScaledSize(res);
        Vector2 scale = RaptureAtkUnitManagerHelper.GetNodeScale(res);
        Vector2 vp = ImGui.GetMainViewport().Size;

        pos += GetAnchorOffset(anchor, size);

        return new ResNodeConfig {
            Name = unit->NameString,
            DoNotPosition = false,
            DoNotScale = false,
            DoNotOpacity = true,
            PosX = pos.X,
            PosY = pos.Y,
            PosPercentX = pos.X / vp.X,
            PosPercentY = pos.Y / vp.Y,
            ForceVisible = false,
            ScaleX = scale.X,
            ScaleY = scale.Y,
            UsePercentagePos = false,
            UsePercentageScale = false,
            Attachment = "",
            Anchor = anchor,
            AttachmentAnchor = PositionAnchor.TOP_LEFT,
            Opacity = unit->Alpha
        };
    }

    public static unsafe void AddToConfig(AtkUnitBase* unit) {
        if (CheckIfInConfig(unit)) {
            return;
        }

        List<ResNodeConfig> config = Globals.Config.GetCurrentNodeConfig();
        config.Add(GetAddonConfig(unit));
        config.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase));
    }

    public static unsafe void UpdateAddon(ref ResNodeConfig node) {
        RaptureAtkUnitManager* manager = AtkStage.Instance()->RaptureAtkUnitManager;
        AtkUnitBase* unit = manager->GetAddonByName(node.Name);
        if (unit == null) {
            return;
        }

        if (!node.ForceVisible && !unit->IsVisible) {
            return;
        }

        AtkResNode* res = unit->RootNode;
        Vector2 vp = ImGui.GetMainViewport().Size;

        float ratioX = vp.X / (float)Globals.Config.BaseResolutionX;
        float ratioY = vp.Y / (float)Globals.Config.BaseResolutionY;
        float scaleX = node.ScaleX;
        float scaleY = node.ScaleY;

        if (node.UsePercentageScale) {
            scaleX *= ratioX;
            scaleY *= ratioY;
        }

        if (!node.DoNotOpacity) {
            unit->Alpha = (byte)node.Opacity;
        }

        if (!node.DoNotScale) {
            unit->RootNode->SetScale(scaleX, scaleY);
        }

        if (node.UsePercentagePos) {
            node.PosX = node.PosPercentX * vp.X;
            node.PosY = node.PosPercentY * vp.Y;
        }

        if (!node.DoNotPosition) {
            Vector2 pos = GetTLPos(ref node);
            unit->RootNode->SetPositionFloat(pos.X, pos.Y);
        }

        if (node.ForceVisible) {
            unit->IsVisible = node.ForceVisible;
        }
    }

    public static void UpdateAddons() {
        List<ResNodeConfig> config = Globals.Config.GetCurrentNodeConfig();

        if (config == null) {
            return;
        }

        for (int index = 0; index < config.Count; index++) {
            ResNodeConfig node = config[index];
            UpdateAddon(ref node);
        }
    }

    public static void DrawAddonNodes() {
        List<ResNodeConfig> config = Globals.Config.GetCurrentNodeConfig();

        if (config == null) {
            return;
        }

        float width = ImGui.CalcTextSize("F").X * 12;

        for (int index = 0; index < config.Count; index++) {
            ResNodeConfig node = config[index];

            if (ImGui.TreeNode(node.Name + "##RESIZABLEHUD_DROPDOWN_" + node.Name)) {
                if (node.Editing) {
                    DrawNodeEditor(ref node);
                }
                else {
                    DrawNodePreview(ref node);
                }

                WindowDrawHelpers.DrawCheckboxTooltip("Edit", ref node.Editing, "Allows editing the transform with the arrow keys. Hold Shift to scale");
                WindowDrawHelpers.DrawCheckboxTooltip("No position", ref node.DoNotPosition, "Disables positioning for this element");
                ImGui.SameLine();
                WindowDrawHelpers.DrawCheckboxTooltip("No scale", ref node.DoNotScale, "Disables scaling for this element");
                WindowDrawHelpers.DrawCheckboxTooltip("Force visibility", ref node.ForceVisible, "Forces the element to be visible.");
                WindowDrawHelpers.DrawCheckboxTooltip("No Opacity", ref node.DoNotOpacity, "Disables opacity for this element");
                ImGui.SliderInt("Opacity", ref node.Opacity, 0, 255);

                ImGui.Separator();
                DrawAnchorOption(ref node);

                if (!node.DoNotPosition) {
                    ImGui.Separator();
                    DrawPosOption(ref node, width);
                    WindowDrawHelpers.DrawCheckboxTooltip("Use relative##RESIZABLEHUD_DROPDOWN_POS_PERCENT", ref node.UsePercentagePos, "Position value represents a percentage instead of a pixel");
                }

                if (!node.DoNotScale) {
                    ImGui.Separator();
                    DrawScaleOption(ref node, width);
                    WindowDrawHelpers.DrawCheckboxTooltip("Use relative##RESIZABLEHUD_DROPDOWN_SCL_PERCENT", ref node.UsePercentageScale, "Scaling is scaled to base resolution");
                }

                ImGui.Separator();
                if (ImGui.Button("Remove")) {
                    config.Remove(node);
                    break;
                }

                ImGui.SameLine();
                if (ImGui.Button("Refresh Values")) {
                    RefreshValues(ref node);
                }
                if (ImGui.IsItemHovered()) {
                    ImGui.SetTooltip("Update the values that are currently not being used to the values on screen. For example, if 'No position' is toggled on, it will update the position.");
                }

                ImGui.Separator();
                ImGui.TreePop();
            }

            if (ImGui.IsItemHovered()) {
                DrawNodePreview(ref node);
            }
        }

        LastMousePos = ImGui.GetMousePos();
    }

    private static string FindClosestMatch(string input) {
        List<ResNodeConfig> config = Globals.Config.GetCurrentNodeConfig();
        string closestMatch = "";
        int minDifference = int.MaxValue;

        if (input == "") {
            return closestMatch;
        }

        foreach (ResNodeConfig node in config) {
            string nodeName = node.Name;

            if (nodeName.StartsWith(input, StringComparison.OrdinalIgnoreCase)) {
                int difference = nodeName.Length - input.Length;

                if (difference < minDifference) {
                    minDifference = difference;
                    closestMatch = nodeName;
                }
            }
        }

        return closestMatch;
    }

    private static unsafe void RefreshValues(ref ResNodeConfig node) {
        RaptureAtkUnitManager* manager = AtkStage.Instance()->RaptureAtkUnitManager;
        AtkUnitBase* unit = manager->GetAddonByName(node.Name);

        if (unit == null) {
            return;
        }

        var cfg = GetAddonConfig(unit, node.Anchor);
        if (node.DoNotScale) {
            node.ScaleX = cfg.ScaleX;
            node.ScaleY = cfg.ScaleY;
        }

        if (node.DoNotPosition) {
            node.PosX = cfg.PosX;
            node.PosY = cfg.PosY;
            node.PosPercentX = cfg.PosPercentX;
            node.PosPercentY = cfg.PosPercentY;
        }

        if (node.DoNotOpacity) {
            node.Opacity = cfg.Opacity;
        }
        
        if (node.Attachment != "") {
            AtkUnitBase* parent = manager->GetAddonByName(node.Attachment);
            if (parent == null) {
                return;
            }
            Vector2 pos = GetTLPos(ref node);
            Vector2 ppos = RaptureAtkUnitManagerHelper.GetNodePosition(parent->RootNode);
            Vector2 psize = RaptureAtkUnitManagerHelper.GetNodeScaledSize(parent->RootNode);
            Vector2 vp = ImGui.GetMainViewport().Size;

            Service.Logger.Info($"pos: {pos}, ppos: {ppos}, psize: {psize}, vp: {vp}, anchor: {GetAnchorOffset(node.AttachmentAnchor, psize)}");

            ppos += GetAnchorOffset(node.AttachmentAnchor, psize);

            node.PosX = pos.X + ppos.X;
            node.PosY = pos.Y + ppos.Y;
            node.PosPercentX = node.PosX / vp.X;
            node.PosPercentY = node.PosY / vp.Y;
        }
    }

    private static unsafe void DrawNodePreview(ref ResNodeConfig node) {
        RaptureAtkUnitManager* manager = AtkStage.Instance()->RaptureAtkUnitManager;
        AtkUnitBase* unit = manager->GetAddonByName(node.Name);

        if (unit == null) {
            return;
        }

        Vector2 size = RaptureAtkUnitManagerHelper.GetNodeScaledSize(unit->RootNode);
        Vector2 offset = GetAnchorOffset(node.Anchor, size);
        Vector2 pos = GetTLPos(ref node);

        ImDrawListPtr drawist = ImGui.GetForegroundDrawList(ImGuiHelpers.MainViewport);

        drawist.AddRect(pos, pos + size, NodeColor);
        drawist.AddCircleFilled(pos + offset, 4.0f, NodeAnchorColor);
        drawist.AddText(pos, NodeColor, node.Name);

        if (node.Attachment != "") {
            AtkUnitBase* parent = manager->GetAddonByName(node.Attachment);
            if (parent == null) {
                return;
            }
            Vector2 ppos = RaptureAtkUnitManagerHelper.GetNodePosition(parent->RootNode);
            Vector2 psize = RaptureAtkUnitManagerHelper.GetNodeScaledSize(parent->RootNode);
            Vector2 poffset = GetAnchorOffset(node.AttachmentAnchor, psize);
            drawist.AddRect(ppos, ppos + psize, NodeParentColor);
            drawist.AddCircleFilled(ppos + poffset, 4.0f, NodeParentAnchorColor);
            drawist.AddText(ppos, NodeParentColor, node.Attachment);
        }
    }

    private static unsafe void DrawNodeEditor(ref ResNodeConfig nodeConfig) {
        RaptureAtkUnitManager* manager = AtkStage.Instance()->RaptureAtkUnitManager;
        AtkUnitBase* unit = manager->GetAddonByName(nodeConfig.Name);

        // ImGui.SetNextFrameWantCaptureMouse(true);

        if (unit == null) {
            return;
        }

        Vector2 size = RaptureAtkUnitManagerHelper.GetNodeScaledSize(unit->RootNode);
        Vector2 pos = GetTLPos(ref nodeConfig);
        Vector2 offset = GetAnchorOffset(nodeConfig.Anchor, size);
        Vector2 box_size = new Vector2(16.0f, 16.0f) * ImGuiHelpers.GlobalScale;
        Vector2 bottom = pos + GetAnchorOffset(PositionAnchor.BOTTOM_CENTER, size) - (box_size * 0.5f);
        Vector2 right = pos + GetAnchorOffset(PositionAnchor.CENTER_RIGHT, size) - (box_size * 0.5f);
        ImDrawListPtr drawlist = ImGui.GetForegroundDrawList(ImGuiHelpers.MainViewport);

        drawlist.AddRect(pos, pos + size, NodeEditColor);
        drawlist.AddCircleFilled(pos + offset, 4.0f, NodeAnchorColor);
        // drawList.AddRectFilled(bottom, bottom + boxSize, NodeAnchorColor);
        // drawList.AddRectFilled(right, right + boxSize, NodeAnchorColor);
        drawlist.AddText(pos, NodeColor, nodeConfig.Name);

        // Doesn't work well (refresh rate related?)
        // NodeEditorMouseControls(ref nodeConfig, pos, size, bottom, right, boxSize);
        NodeEditorKeyboardControls(ref nodeConfig, pos, size, box_size);
    }


    private static unsafe void DrawAnchorOption(ref ResNodeConfig node) {
        string[] anchor_names = Enum.GetNames(typeof(PositionAnchor));
        string current_anchor_label = anchor_names[(int)node.Anchor];
        string current_attach_anchor_label = anchor_names[(int)node.AttachmentAnchor];
        Vector2 offset = Vector2.Zero;
        Vector2 size = Vector2.Zero;
        Vector2 vp = ImGui.GetMainViewport().Size;
        RaptureAtkUnitManager* manager = AtkStage.Instance()->RaptureAtkUnitManager;
        AtkUnitBase* unit = manager->GetAddonByName(node.Name);

        if (unit != null) {
            size = RaptureAtkUnitManagerHelper.GetNodeScaledSize(unit->RootNode);
            offset = GetAnchorOffset(node.Anchor, size);
        }

        if (!node.DoNotPosition) {
            if (ImGui.BeginCombo("Position Anchor", current_anchor_label)) {
                for (int index = 0; index < anchor_names.Length; index++) {
                    bool selected = (node.Anchor == (PositionAnchor)index);

                    // Add each enum value as a selectable item in the dropdown
                    if (ImGui.Selectable(anchor_names[index], selected)) {
                        node.Anchor = (PositionAnchor)index;

                        if (unit != null) {
                            offset -= GetAnchorOffset(node.Anchor, size);
                            node.PosX -= offset.X;
                            node.PosY -= offset.Y;
                            node.PosPercentX = node.PosX / vp.X;
                            node.PosPercentY = node.PosY / vp.Y;
                        }
                    }

                    // Set the currently selected item as highlighted
                    if (selected) {
                        ImGui.SetItemDefaultFocus();
                    }
                }

                ImGui.EndCombo();
            }
            WindowDrawHelpers.DrawTooltip("Point on this ui element that position is relative to");
        }

        if (ImGui.InputText("Attachment", ref node.AttachmentRef, 64, ImGuiInputTextFlags.EnterReturnsTrue)) {
            node.Attachment = node.AttachmentRef;
        }
        WindowDrawHelpers.DrawTooltip("Name of ui element to attach to, leave empty for none. You can press tab to autocomplete. Note that the auto completion only searches for addons that you have added");

        float startX = ImGui.CalcTextSize(node.AttachmentRef).X;
        string autoCompleteResult = FindClosestMatch(node.AttachmentRef);
        if (!string.IsNullOrEmpty(autoCompleteResult)) {
            Vector2 currentPos = ImGui.GetCursorScreenPos();

            // Set the cursor position on top of the InputText
            ImGui.SetCursorScreenPos(new Vector2(currentPos.X + startX + 4, currentPos.Y - ImGui.GetFrameHeightWithSpacing() + 3));

            // Set text color to match InputText
            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiStuff.ColorToUint(ImGui.GetStyleColorVec4(ImGuiCol.TextDisabled)));
            ImGui.TextUnformatted(autoCompleteResult.Substring(node.AttachmentRef.Length));
            ImGui.PopStyleColor();

            if (ImGui.IsKeyPressed(ImGuiKey.Tab)) {
                node.Attachment = autoCompleteResult;
                node.AttachmentRef = autoCompleteResult;
            }

            // needed to fix spacing inconsistencies?
            currentPos = ImGui.GetCursorScreenPos();
            ImGui.SetCursorScreenPos(new Vector2(currentPos.X, currentPos.Y - 1));

            ImGui.Spacing();
        }


        if (node.Attachment != "") {
            if (ImGui.BeginCombo("Attachment Anchor", current_attach_anchor_label)) {
                for (int index = 0; index < anchor_names.Length; index++) {
                    bool selected = (node.AttachmentAnchor == (PositionAnchor)index);

                    // Add each enum value as a selectable item in the dropdown
                    if (ImGui.Selectable(anchor_names[index], selected)) {
                        node.AttachmentAnchor = (PositionAnchor)index;
                    }

                    // Set the currently selected item as highlighted
                    if (selected) {
                        ImGui.SetItemDefaultFocus();
                    }
                }

                ImGui.EndCombo();
            }
            WindowDrawHelpers.DrawTooltip("Point on attached ui element (if any) to anchor to");
        }
    }

    private static unsafe void NodeEditorMouseControls(ref ResNodeConfig node, Vector2 tl_pos, Vector2 size, Vector2 bottom, Vector2 right, Vector2 box_size) {
        RaptureAtkUnitManager* manager = AtkStage.Instance()->RaptureAtkUnitManager;
        AtkUnitBase* unit = manager->GetAddonByName(node.Name);
        Vector2 mpos = ImGui.GetMousePos();

        if (RaptureAtkUnitManagerHelper.GetPointIntersectsNode(unit->RootNode, mpos)) {
            Vector2 mdelta = mpos - LastMousePos;
            Vector2 mdelta_pos = mdelta;
            Vector2 mdelta_scale = mdelta;
            Vector2 vp = ImGui.GetMainViewport().Size;
            float ratio_x = vp.X / (float)Globals.Config.BaseResolutionX;
            float ratio_y = vp.Y / (float)Globals.Config.BaseResolutionY;
            bool down = ImGui.IsMouseDown(ImGuiMouseButton.Left);

            if (node.UsePercentagePos) {
                mdelta_pos.X = mdelta.X / vp.X;
                mdelta_pos.Y = mdelta.Y / vp.Y;
            }

            if (node.UsePercentageScale) {
                mdelta_scale.X *= ratio_x;
                mdelta_scale.Y *= ratio_y;
            }

            mdelta_scale *= 0.01f;

            if (down) {
                Vector2 new_size;

                if (RaptureAtkUnitManagerHelper.GetPointIntersectsRect(mpos, bottom, box_size)) {
                    node.ScaleY += mdelta_scale.Y;

                    UpdateAddon(ref node);
                    new_size = RaptureAtkUnitManagerHelper.GetNodeScaledSize(unit->RootNode);

                    node.PosY += mdelta.Y * 0.5f;
                    node.PosPercentY = node.PosY / vp.Y;
                }
                else if (RaptureAtkUnitManagerHelper.GetPointIntersectsRect(mpos, right, box_size)) {
                    node.ScaleX += mdelta_scale.X;

                    UpdateAddon(ref node);
                    new_size = RaptureAtkUnitManagerHelper.GetNodeScaledSize(unit->RootNode);

                    node.PosX += mdelta.X * 0.5f;
                    node.PosPercentX = node.PosX / vp.X;
                }
                else if (RaptureAtkUnitManagerHelper.GetPointIntersectsRect(mpos, tl_pos, size)) {
                    if (node.UsePercentagePos) {
                        node.PosPercentX += mdelta_pos.X;
                        node.PosPercentY += mdelta_pos.Y;
                    }
                    else {
                        node.PosX += mdelta_pos.X;
                        node.PosY += mdelta_pos.Y;
                    }
                }
            }
        }
    }

    private static unsafe void NodeEditorKeyboardControls(ref ResNodeConfig node, Vector2 tl_pos, Vector2 size, Vector2 box_size) {
        RaptureAtkUnitManager* manager = AtkStage.Instance()->RaptureAtkUnitManager;
        AtkUnitBase* unit = manager->GetAddonByName(node.Name);
        Vector2 vp = ImGui.GetMainViewport().Size;
        float v = 0.0f;
        float h = 0.0f;
        bool scale = Service.KeyState[VirtualKey.SHIFT];

        v -= Service.KeyState[VirtualKey.UP] ? 1.0f : 0.0f;
        h += Service.KeyState[VirtualKey.RIGHT] ? 1.0f : 0.0f;
        v += Service.KeyState[VirtualKey.DOWN] ? 1.0f : 0.0f;
        h -= Service.KeyState[VirtualKey.LEFT] ? 1.0f : 0.0f;

        if (v != 0 || h != 0) {
            if (scale) {
                Vector2 delta_size;

                node.ScaleX += h * 0.01f;
                node.ScaleY += v * 0.01f;

                UpdateAddon(ref node);
                delta_size = (RaptureAtkUnitManagerHelper.GetNodeScaledSize(unit->RootNode) - size) * 0.5f;

                node.PosX -= delta_size.X;
                node.PosY -= delta_size.Y;
                node.PosPercentX = node.PosX / vp.X;
                node.PosPercentY = node.PosY / vp.Y;
            }
            else {
                node.PosX += h;
                node.PosY += v;
                node.PosPercentX = node.PosX / vp.X;
                node.PosPercentY = node.PosY / vp.Y;
            }
        }
    }

    private static unsafe Vector2 GetTLPos(ref ResNodeConfig node) {
        RaptureAtkUnitManager* manager = AtkStage.Instance()->RaptureAtkUnitManager;
        AtkUnitBase* unit = manager->GetAddonByName(node.Name);

        if (unit != null && unit->IsReady) {
            Vector2 size = RaptureAtkUnitManagerHelper.GetNodeScaledSize(unit->RootNode);
            Vector2 offset = GetAnchorOffset(node.Anchor, size);
            Vector2 pos = new Vector2(node.PosX - offset.X, node.PosY - offset.Y); // absolute top left

            if (node.Attachment != "") { 
                AtkUnitBase* parent = manager->GetAddonByName(node.Attachment);
                if (parent != null) {
                    Vector2 psize = RaptureAtkUnitManagerHelper.GetNodeScaledSize(parent->RootNode);
                    Vector2 ppos = RaptureAtkUnitManagerHelper.GetNodePosition(parent->RootNode);
                    psize = GetAnchorOffset(node.AttachmentAnchor, psize);
                    
                    pos.X += (ppos.X + psize.X);
                    pos.Y += (ppos.Y + psize.Y);
                }
            }

            return pos;
        }

        return new Vector2(node.PosX, node.PosY);
    }

    private static Vector2 GetAnchorOffset(PositionAnchor anchor, Vector2 size) {
        float x = 0;
        float y = 0;

        switch (anchor) {
            case PositionAnchor.TOP_LEFT:
                break;
            case PositionAnchor.TOP_CENTER:
                x = size.X / 2;
                break;
            case PositionAnchor.TOP_RIGHT:
                x = size.X;
                break;
            case PositionAnchor.CENTER_LEFT:
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

    private static unsafe void DrawScaleOption(ref ResNodeConfig node, float WIDTH) {
        RaptureAtkUnitManager* manager = AtkStage.Instance()->RaptureAtkUnitManager;
        AtkUnitBase* unit = manager->GetAddonByName(node.Name);
        Vector2 size = unit != null ? RaptureAtkUnitManagerHelper.GetNodeScaledSize(unit->RootNode) : Vector2.Zero;

        ImGui.SetNextItemWidth(WIDTH);
        ImGui.InputFloat("Scale X##RESIZABLEHUD_INPUT_SCALEX", ref node.ScaleX);
        ImGui.SameLine();
        ImGui.SetNextItemWidth(WIDTH);
        ImGui.InputFloat("Scale Y##RESIZABLEHUD_INPUT_SCALEY", ref node.ScaleY);
        ImGui.SameLine();

        ImGui.TextDisabled(size != Vector2.Zero ? $"({size.X}x{size.Y}) px" : "??x?? px");
    }

    private static unsafe void DrawPosOption(ref ResNodeConfig node, float WIDTH) {
        Vector2 vp = ImGui.GetMainViewport().Size;

        ImGui.SetNextItemWidth(WIDTH);

        if (!node.UsePercentagePos) {
            if (ImGui.InputFloat("Pos X##RESIZABLEHUD_INPUT_POSX", ref node.PosX)) {
                node.PosPercentX = node.PosX / vp.X;
            }
            ImGui.SameLine();
            ImGui.SetNextItemWidth(WIDTH);
            if (ImGui.InputFloat("Pos Y##RESIZABLEHUD_INPUT_POSY", ref node.PosY)) {
                node.PosPercentY = node.PosY / vp.Y;
            }
        }
        else {
            ImGui.InputFloat("Pos X##RESIZABLEHUD_INPUT_POSX%", ref node.PosPercentX);
            ImGui.SameLine();
            ImGui.SetNextItemWidth(WIDTH);
            ImGui.InputFloat("Pos Y##RESIZABLEHUD_INPUT_POSY%", ref node.PosPercentY);
        }
    }
}
