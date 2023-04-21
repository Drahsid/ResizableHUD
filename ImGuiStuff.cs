using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
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
}
