using Dalamud.Interface;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ResizableHUD; 
public static class RaptureAtkUnitManagerHelper {
    const uint NodeVisibleColor = 0xFF00FF00;
    const uint NodeInvisibleColor = 0xFF0000FF;
    const uint NodeBackgroundColor = 0x08000000;
    public const int UnitListCount = 18;

    public enum UnitListEntry {
        DepthLayer1,
        DepthLayer2,
        DepthLayer3,
        DepthLayer4,
        DepthLayer5,
        DepthLayer6,
        DepthLayer7,
        DepthLayer8,
        DepthLayer9,
        DepthLayer10,
        DepthLayer11,
        DepthLayer12,
        DepthLayer13,
        LoadedUnits,
        FocusedUnits,
        Units16,
        Units17,
        Units18,
        Count
    };

    public static readonly string[] ListNames = new string[UnitListCount] {
        "Depth Layer 1",
        "Depth Layer 2",
        "Depth Layer 3",
        "Depth Layer 4",
        "Depth Layer 5",
        "Depth Layer 6",
        "Depth Layer 7",
        "Depth Layer 8",
        "Depth Layer 9",
        "Depth Layer 10",
        "Depth Layer 11",
        "Depth Layer 12",
        "Depth Layer 13",
        "Loaded Units",
        "Focused Units",
        "Units 16",
        "Units 17",
        "Units 18",
    };

    public static unsafe Vector2 GetNodePosition(AtkResNode* node) {
        Vector2 pos = new Vector2(node->X, node->Y);
        AtkResNode* parent = node->ParentNode;

        while (parent != null) {
            pos *= new Vector2(parent->ScaleX, parent->ScaleY);
            pos += new Vector2(parent->X, parent->Y);
            parent = parent->ParentNode;
        }
        return pos;
    }

    public static unsafe Vector2 GetNodeScale(AtkResNode* node) {
        if (node == null) {
            PluginLog.Warning("Node is null");
            return new Vector2(1, 1);
        }

        Vector2 scale = new Vector2(node->ScaleX, node->ScaleY);
        while (node->ParentNode != null) {
            node = node->ParentNode;
            scale *= new Vector2(node->ScaleX, node->ScaleY);
        }
        return scale;
    }

    public static unsafe Vector2 GetNodeScaledSize(AtkResNode* node) {
        if (node == null) {
            PluginLog.Warning("Node is null");
            return new Vector2(1, 1);
        }

        Vector2 scale = GetNodeScale(node);
        Vector2 size = new Vector2(node->Width, node->Height) * scale;
        return size;
    }

    public static unsafe bool GetNodeVisible(AtkResNode* node) {
        if (node == null) {
            return false;
        }

        while (node != null) {
            if ((node->Flags & (short)NodeFlags.Visible) != (short)NodeFlags.Visible) {
                return false;
            }
            node = node->ParentNode;
        }
        return true;
    }

    public static unsafe void DrawOutline(AtkResNode* node, string name) {
        Vector2 position = GetNodePosition(node);
        Vector2 size = GetNodeScaledSize(node);
        bool nodeVisible = GetNodeVisible(node);

        position += ImGuiHelpers.MainViewport.Pos;
        ImGui.GetForegroundDrawList(ImGuiHelpers.MainViewport).AddRectFilled(position, position + size, NodeBackgroundColor);
        ImGui.GetForegroundDrawList(ImGuiHelpers.MainViewport).AddRect(position, position + size, nodeVisible ? NodeVisibleColor : NodeInvisibleColor);
        ImGui.GetForegroundDrawList(ImGuiHelpers.MainViewport).AddText(position, nodeVisible ? NodeVisibleColor : NodeInvisibleColor, name);
    }

    public static unsafe bool GetPointIntersectsRect(Vector2 point, Vector2 rect_pos, Vector2 rect_size) {
        return point.X >= rect_pos.X &&
                   point.Y >= rect_pos.Y &&
                   point.X <= rect_pos.X + rect_size.X &&
                   point.Y <= rect_pos.Y + rect_size.Y;
    }

    public static unsafe bool GetPointIntersectsNode(AtkResNode* node, Vector2 point) {
        Vector2 position = GetNodePosition(node);
        Vector2 size = GetNodeScaledSize(node);

        return GetPointIntersectsRect(point, position, size);
    }

    public static unsafe bool DrawMouseIntersection(AtkResNode* node, string name) {
        Vector2 mpos = ImGui.GetMousePos();
        bool ret = GetPointIntersectsNode(node, mpos);

        if (ret) {
            DrawOutline(node, name);
        }

        return ret;
    }
}
