using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;

namespace ResizableHUD
{
    internal class AddonManager
    {
        public static unsafe void UpdateAddons()
        {
            RaptureAtkUnitManager* manager = AtkStage.GetSingleton()->RaptureAtkUnitManager;
            AtkUnitBase* unit = null;
            ResNodeConfig nodeConfig = null;

            if (Globals.Config.nodeConfigs == null)
            {
                return;
            }

            for (int cndex = 0; cndex < Globals.Config.nodeConfigs.Count; cndex++)
            {
                nodeConfig = Globals.Config.nodeConfigs[cndex];
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

        // ImGui code
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

                    DrawScaleOption(ref nodeConfig, WIDTH, Globals.Config.EpsillonAmount);
                    DrawPosOption(ref nodeConfig, WIDTH, Globals.Config.EpsillonAmount);
                    ImGui.Checkbox("Do not change position", ref nodeConfig.DoNotPosition);
                    ImGui.Checkbox("Do not change scale", ref nodeConfig.DoNotScale);
                    ImGui.Separator();
                    ImGui.Spacing();
                    ImGui.TreePop();
                }
            }

            ImGui.Separator();
        }

        public static void DrawIncrementDecrementButtons(ref ResNodeConfig nodeConfig, ref float value, string name)
        {
            ImGui.SameLine();
            if (ImGui.ArrowButton($"###RESIZABLEHUD_BUTTON_DEC{name}", ImGuiDir.Down))
            {
                value -= Globals.Config.EpsillonAmount;

                if (value <= 0)
                {
                    value = 0;
                }
            }

            ImGui.SameLine();
            if (ImGui.ArrowButton($"###RESIZABLEHUD_BUTTON_INC{name}", ImGuiDir.Up))
            {
                value += Globals.Config.EpsillonAmount;
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
            DrawIncrementDecrementButtons(ref nodeConfig, ref nodeConfig.ScaleX, "SCALEX");
            ImGui.SliderFloat("Scale Y##RESIZABLEHUD_SLIDER_SCALEY", ref nodeConfig.ScaleY, 0, 8.0f);
            DrawIncrementDecrementButtons(ref nodeConfig, ref nodeConfig.ScaleY, "SCALEY");
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
                DrawIncrementDecrementButtons(ref nodeConfig, ref nodeConfig.PosX, "POSX");
                ImGui.SliderFloat("Pos Y##RESIZABLEHUD_SLIDER_SCALEY", ref nodeConfig.PosY, -ImGui.GetMainViewport().Size.Y, ImGui.GetMainViewport().Size.Y);
                DrawIncrementDecrementButtons(ref nodeConfig, ref nodeConfig.PosY, "POSY");
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
                DrawIncrementDecrementButtons(ref nodeConfig, ref nodeConfig.PosPercentX, "POS%X");
                ImGui.SliderFloat("Pos Y##RESIZABLEHUD_SLIDER_POSY%", ref nodeConfig.PosPercentY, -1.0f, 1.0f);
                DrawIncrementDecrementButtons(ref nodeConfig, ref nodeConfig.PosPercentY, "POS%Y");
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
