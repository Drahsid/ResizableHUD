using Dalamud.Configuration;
using Dalamud.Plugin;
using Dalamud.Logging;
using System;
using System.Collections.Generic;

namespace ResizableHUD
{
    public class ResNodeConfig {
        public string Name = "";
        public float ScaleX = 1.0f;
        public float ScaleY = 1.0f;
        public float PosX = 0.0f;
        public float PosY = 0.0f;
        public bool UsePercentage = false;
        public float PosPercentX = 0.0f;
        public float PosPercentY = 0.0f;
        public bool ForceVisible = false;
        public bool DoNotPosition = false;
        public bool DoNotScale = false;
    }

    public class Configuration : IPluginConfiguration {
        int IPluginConfiguration.Version { get; set; }

        #region Saved configuration values
        public List<ResNodeConfig> nodeConfigs = null;
        public float EpsillonAmount = 0.025f;
        #endregion

        public bool WindowOpen = false;
        private DalamudPluginInterface PluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface) {
            PluginInterface = pluginInterface;

            if (nodeConfigs == null)
            {
                return;
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
                }
            }
        }

        public void Save()
        {
            PluginInterface.SavePluginConfig(this);
        }
    }
}
