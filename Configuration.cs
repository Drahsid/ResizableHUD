using Dalamud.Configuration;
using Dalamud.Plugin;
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
        public List<ResNodeConfig> nodeConfigs;
        public bool WindowEverOpened = false;
        public float EpsillonAmount = 0.025f;
        #endregion

        public bool WindowOpen = false;

        private readonly DalamudPluginInterface pluginInterface;

        public Configuration(DalamudPluginInterface pi)
        {
            this.pluginInterface = pi;
        }

        public void Save()
        {
            if (this.pluginInterface != null)
            {
                this.pluginInterface.SavePluginConfig(this);
            }
        }
    }
}
