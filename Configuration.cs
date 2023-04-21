using Dalamud.Configuration;
using Dalamud.Plugin;
using Dalamud.Logging;
using System;
using System.Collections.Generic;
using ImGuiNET;
using static ResizableHUD.RaptureAtkUnitManagerHelper;

namespace ResizableHUD;

public class ResNodeConfig {
    public enum PositionAnchor {
        TOP_LEFT, TOP_CENTER, TOP_RIGHT,
        CENTER_LEFT, CENTER_CENTER, CENTER_RIGHT,
        BOTTOM_LEFT, BOTTOM_CENTER, BOTTOM_RIGHT,
    };

    public string Name = "";
    public float ScaleX = 1.0f;
    public float ScaleY = 1.0f;
    public float PosX = 0.0f;
    public float PosY = 0.0f;
    public float PosPercentX = 0.0f;
    public float PosPercentY = 0.0f;
    [Obsolete] public bool? UsePercentage = false;
    public bool UsePercentagePos = false;
    public bool UsePercentageScale = false;
    public bool ForceVisible = false;
    public bool DoNotPosition = false;
    public bool DoNotScale = false;
    public bool Editing = false;
    public PositionAnchor anchor = PositionAnchor.TOP_LEFT;
}

public class Configuration : IPluginConfiguration {
    int IPluginConfiguration.Version { get; set; }

    #region Saved configuration values
    public List<ResNodeConfig> nodeConfigs = new List<ResNodeConfig>();
    public float EpsillonAmount = 0.025f;
    public int BaseResolutionX = -1;
    public int BaseResolutionY = -1;
    public bool OnlyPeekVisible = false;
    public bool DrawAddonInspector = false;
    public RaptureAtkUnitManagerHelper.UnitListEntry OnlyPeekInLayer = RaptureAtkUnitManagerHelper.UnitListEntry.LoadedUnits;
    #endregion

    private DalamudPluginInterface PluginInterface;

    public void Initialize(DalamudPluginInterface pluginInterface) {
        PluginInterface = pluginInterface;

        if (nodeConfigs == null)
        {
            return;
        }

        if (BaseResolutionX == -1) {
            BaseResolutionX = (int)ImGui.GetMainViewport().Size.X;
        }
        if (BaseResolutionY == -1) {
            BaseResolutionY = (int)ImGui.GetMainViewport().Size.Y;
        }

        // upgrade old config
        for (int index = 0; index < nodeConfigs.Count; index++)
        {
            ResNodeConfig nodeConfig = nodeConfigs[index];
            if (nodeConfig != null) {
                if (nodeConfig.DoNotPosition == null)
                {
                    nodeConfig.DoNotPosition = false;
                }
                if (nodeConfig.DoNotScale == null)
                {
                    nodeConfig.DoNotScale = false;
                }

                if (nodeConfig.UsePercentage != null) {
                    if ((bool)nodeConfig.UsePercentage) {
                        nodeConfig.UsePercentagePos = (bool)nodeConfig.UsePercentage;
                        nodeConfig.UsePercentage = null;
                    }
                }
            }
        }
    }

    public void Save()
    {
        PluginInterface.SavePluginConfig(this);
    }
}
