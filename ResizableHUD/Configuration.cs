﻿using Dalamud.Configuration;
using System;
using System.Collections.Generic;
using ImGuiNET;
using DrahsidLib;

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
    public int Opacity = 255;
    [Obsolete] public bool? UsePercentage = false;
    public bool UsePercentagePos = false;
    public bool UsePercentageScale = false;
    public bool ForceVisible = false;
    public bool DoNotPosition = false;
    public bool DoNotScale = false;
    public bool DoNotOpacity = true;
    public bool Editing = false;
    public string Attachment = "";
    public string AttachmentRef = "";
    public PositionAnchor Anchor = PositionAnchor.TOP_LEFT;
    public PositionAnchor AttachmentAnchor = PositionAnchor.TOP_LEFT;
}

public class Configuration : IPluginConfiguration {
    int IPluginConfiguration.Version { get; set; }

    #region Saved configuration values
    [Obsolete] public List<ResNodeConfig>? nodeConfigs;
    public Dictionary<ulong, List<ResNodeConfig>> CIDNodeConfigMap = new Dictionary<ulong, List<ResNodeConfig>>();
    public int BaseResolutionX = -1;
    public int BaseResolutionY = -1;
    public bool OnlyPeekVisible = false;
    public bool DrawAddonInspector = false;
    public bool HideTooltips = false;
    public UnitListEntry OnlyPeekInLayer = UnitListEntry.LoadedUnits;
    #endregion

    public List<ResNodeConfig>? GetCurrentNodeConfig() {
        ulong cid = Service.ClientState.LocalContentId;
        if (CIDNodeConfigMap.ContainsKey(cid)) {
            return CIDNodeConfigMap[cid];
        }

        CIDNodeConfigMap.Add(cid, new List<ResNodeConfig>());
        return CIDNodeConfigMap[cid];
    }

    public void Initialize() {
        ulong cid = Service.ClientState.LocalContentId;

        if (nodeConfigs != null) {
            CIDNodeConfigMap.Add(cid, nodeConfigs);
            nodeConfigs = null;
        }

        if (BaseResolutionX == -1) {
            BaseResolutionX = (int)ImGui.GetMainViewport().Size.X;
        }
        if (BaseResolutionY == -1) {
            BaseResolutionY = (int)ImGui.GetMainViewport().Size.Y;
        }

        foreach (List<ResNodeConfig> node_config in CIDNodeConfigMap.Values) {
            // upgrade old config
            foreach (ResNodeConfig config in node_config) {
                if (config != null) {
                    if (config.DoNotPosition == null) {
                        config.DoNotPosition = false;
                    }
                    if (config.DoNotScale == null) {
                        config.DoNotScale = false;
                    }

                    if (config.Attachment == null) {
                        config.Attachment = "";
                    }

                    if (config.AttachmentRef == null) {
                        config.AttachmentRef = "";
                    }

                    config.AttachmentRef = config.Attachment;

                    if (config.UsePercentage != null) {
                        if ((bool)config.UsePercentage) {
                            config.UsePercentagePos = (bool)config.UsePercentage;
                            config.UsePercentage = null;
                        }
                    }

                    if (config.DoNotOpacity == null) {
                        config.DoNotOpacity = true;
                        config.Opacity = 255;
                    }
                }
            }
        }
    }

    public void Save() {
        Service.Interface.SavePluginConfig(this);
    }
}
