using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResizableHUD
{
    internal static class DrawFuncs
    {
        public static void DrawIncrementDecrementButtons(ref ResNodeConfig nodeConfig, ref float value, string name, float epsillon)
        {
            ImGui.SameLine();
            if (ImGui.ArrowButton($"###RESIZABLEHUD_BUTTON_DEC{name}", ImGuiDir.Down))
            {
                value -= epsillon;

                if (value <= 0)
                {
                    value = 0;
                }
            }

            ImGui.SameLine();
            if (ImGui.ArrowButton($"###RESIZABLEHUD_BUTTON_INC{name}", ImGuiDir.Up))
            {
                value += epsillon;
            }
        }

        public static void DrawScaleOption(ref ResNodeConfig nodeConfig, float WIDTH, float epsillon)
        {
            ImGui.SetNextItemWidth(WIDTH);
            ImGui.InputFloat("Scale X##RESIZABLEHUD_INPUT_SCALEX", ref nodeConfig.ScaleX);
            ImGui.SameLine();
            ImGui.SetNextItemWidth(WIDTH);
            ImGui.InputFloat("Scale Y##RESIZABLEHUD_INPUT_SCALEY", ref nodeConfig.ScaleY);

            ImGui.SliderFloat("Scale X##RESIZABLEHUD_SLIDER_SCALEX", ref nodeConfig.ScaleX, 0, 8.0f);
            DrawIncrementDecrementButtons(ref nodeConfig, ref nodeConfig.ScaleX, "SCALEX", epsillon);
            ImGui.SliderFloat("Scale Y##RESIZABLEHUD_SLIDER_SCALEY", ref nodeConfig.ScaleY, 0, 8.0f);
            DrawIncrementDecrementButtons(ref nodeConfig, ref nodeConfig.ScaleY, "SCALEY", epsillon);
            ImGui.Spacing(); ImGui.Spacing();
        }

        public static unsafe void DrawPosOption(ref ResNodeConfig nodeConfig, float WIDTH, float epsillon)
        {
            if (nodeConfig.UsePercentage == false)
            {
                ImGui.SetNextItemWidth(WIDTH);
                ImGui.InputFloat("Pos X##RESIZABLEHUD_INPUT_POSX", ref nodeConfig.PosX);
                ImGui.SameLine();
                ImGui.SetNextItemWidth(WIDTH);
                ImGui.InputFloat("Pos Y##RESIZABLEHUD_INPUT_POSY", ref nodeConfig.PosY);

                ImGui.SliderFloat("Pos X##RESIZABLEHUD_SLIDER_SCALEX", ref nodeConfig.PosX, -ImGui.GetMainViewport().Size.X, ImGui.GetMainViewport().Size.X);
                DrawIncrementDecrementButtons(ref nodeConfig, ref nodeConfig.PosX, "POSX", epsillon);
                ImGui.SliderFloat("Pos Y##RESIZABLEHUD_SLIDER_SCALEY", ref nodeConfig.PosY, -ImGui.GetMainViewport().Size.Y, ImGui.GetMainViewport().Size.Y);
                DrawIncrementDecrementButtons(ref nodeConfig, ref nodeConfig.PosY, "POSY", epsillon);
                ImGui.Spacing(); ImGui.Spacing();

                ImGui.Checkbox("Force Visibility##RESIZABLEHUD_CHECKBOX_VIS", ref nodeConfig.ForceVisible);
                ImGui.Checkbox("Use relative %##RESIZABLEHUD_CHECKBOX_PERCENT", ref nodeConfig.UsePercentage);
            }
            else
            {
                ImGui.SetNextItemWidth(WIDTH);
                ImGui.InputFloat("Pos X##RESIZABLEHUD_INPUT_POSX%", ref nodeConfig.PosPercentX);
                ImGui.SameLine();
                ImGui.SetNextItemWidth(WIDTH);
                ImGui.InputFloat("Pos Y##RESIZABLEHUD_INPUT_POSY%", ref nodeConfig.PosPercentY);

                ImGui.SliderFloat("Pos X##RESIZABLEHUD_SLIDER_POSX%", ref nodeConfig.PosPercentX, -1.0f, 1.0f);
                DrawIncrementDecrementButtons(ref nodeConfig, ref nodeConfig.PosPercentX, "POS%X", epsillon);
                ImGui.SliderFloat("Pos Y##RESIZABLEHUD_SLIDER_POSY%", ref nodeConfig.PosPercentY, -1.0f, 1.0f);
                DrawIncrementDecrementButtons(ref nodeConfig, ref nodeConfig.PosPercentY, "POS%Y", epsillon);
                ImGui.Spacing(); ImGui.Spacing();

                ImGui.Checkbox("Force Visibility##RESIZABLEHUD_CHECKBOX_VIS", ref nodeConfig.ForceVisible);
                ImGui.Checkbox("Use relative %##RESIZABLEHUD_CHECKBOX_PERCENT", ref nodeConfig.UsePercentage);
                ImGui.Spacing();

                nodeConfig.PosX = ImGui.GetMainViewport().Size.X * nodeConfig.PosPercentX;
                nodeConfig.PosY = ImGui.GetMainViewport().Size.Y * nodeConfig.PosPercentY;
            }
        }
    }
}
