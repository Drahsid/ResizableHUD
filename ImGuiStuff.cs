using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ResizableHUD; 
internal class ImGuiStuff {
    public static void DrawTooltip(string text) {
        if (ImGui.IsItemHovered()) {
            ImGui.SetTooltip(text);
        }
    }

    public static bool DrawCheckboxTooltip(string label, ref bool v, string tooltip) {
        bool ret = ImGui.Checkbox(label, ref v);
        DrawTooltip(tooltip);

        return ret;
    }

    public static unsafe uint ColorToUint(Vector4* color) {
        byte r = (byte)(color->X * 255);
        byte g = (byte)(color->Y * 255);
        byte b = (byte)(color->Z * 255);
        byte a = (byte)(color->W * 255);

        return (uint)((a << 24) | (r << 16) | (g << 8) | b);
    }

}
