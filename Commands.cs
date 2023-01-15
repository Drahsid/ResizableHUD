using ResizableHUD.Attributes;
using Dalamud.Game.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResizableHUD
{
    internal class Commands
    {
        public static void Initialize()
        {
            Globals.CommandManager.AddHandler("/prhud", new CommandInfo(OnPrHud)
            {
                HelpMessage = "Toggle the visibility of the configuration window.",
                ShowInHelp= true,
            });

            Globals.CommandManager.AddHandler("/prhudadd", new CommandInfo(ResizableHud_Add)
            {
                HelpMessage = "add [a] unit[s] to the Globals.Config. For example \"/prhud add _TargetInfoCastBar _TargetCursor\".",
                ShowInHelp = true,
            });
            Globals.CommandManager.AddHandler("/prhudrem", new CommandInfo(ResizableHud_Rem)
            {
                HelpMessage = "remove [a] unit[s] from the Globals.Config. For example \"/prhud rem _TargetInfoCastBar _TargetCursor\".",
                ShowInHelp = true,
            });
            Globals.CommandManager.AddHandler("/prhudscale", new CommandInfo(ResizableHud_Scale) {
                HelpMessage = "Change the X or Y scale of a [unit]. For example \"/prhud scale _TargetInfoCastBar X 3\".",
                ShowInHelp = true,
            });
            Globals.CommandManager.AddHandler("/prhudpos", new CommandInfo(ResizableHud_Pos)
            {
                HelpMessage = "Change the X or Y pos of a [unit]. For example \"/prhud pos _TargetInfoCastBar X 320\".",
                ShowInHelp = true,
            });
        }

        public static void ToggleConfig()
        {
            Globals.Config.WindowOpen = !Globals.Config.WindowOpen;
            Globals.WindowSystem.GetWindow(ConfigWindow.ConfigWindowName).IsOpen = Globals.Config.WindowOpen;
        }

        public static void OnPrHud(string command, string args)
        {
            ToggleConfig();
        }

        public static unsafe void ResizableHud_Add(string command, string args)
        {
            string[] argv = args.Split(' ');
            string targ;
            ResNodeConfig nodeConfig;
            ResNodeConfig newConfig;
            int index = 0;

            if (argv == null || argv.Length == 0)
            {
                Globals.Chat.PrintError("argv null?");
                return;
            }

            if (Globals.Config.nodeConfigs == null || Globals.Config.nodeConfigs.Count == 0)
            {
                newConfig = new ResNodeConfig();
                newConfig.Name = argv[0];
                Globals.Config.nodeConfigs = new List<ResNodeConfig> { newConfig };
                index++;
                Globals.Chat.Print("Added first config.");
            }

            for (; index < argv.Length; index++)
            {
                bool add = true;
                targ = argv[index];
                for (int cndex = 0; cndex < Globals.Config.nodeConfigs.Count; cndex++)
                {
                    nodeConfig = Globals.Config.nodeConfigs[cndex];
                    if (nodeConfig.Name == targ)
                    {
                        add = false;
                        break;
                    }
                }

                if (add)
                {
                    newConfig = new ResNodeConfig();
                    newConfig.Name = targ;
                    Globals.Config.nodeConfigs.Add(newConfig);
                    Globals.Chat.Print("Added Globals.Config.");
                }
            }

            Globals.Config.Save();
        }

        public static unsafe void ResizableHud_Rem(string command, string args)
        {
            string[] argv = args.Split(' ');
            string targ;
            ResNodeConfig nodeConfig;

            for (int index = 0; index < argv.Length; index++)
            {
                targ = argv[index];
                for (int cndex = 0; cndex < Globals.Config.nodeConfigs.Count; cndex++)
                {
                    nodeConfig = Globals.Config.nodeConfigs[cndex];
                    if (nodeConfig.Name == targ)
                    {
                        Globals.Config.nodeConfigs.RemoveAt(cndex);

                        Globals.Chat.Print("Removed Globals.Config.");
                    }
                }
            }

            Globals.Config.Save();
        }

        public static unsafe void ResizableHud_Scale(string command, string args)
        {
            string[] argv = args.Split(' ');
            string targ;
            ResNodeConfig nodeConfig;

            targ = argv[0];
            for (int cndex = 0; cndex < Globals.Config.nodeConfigs.Count; cndex++)
            {
                nodeConfig = Globals.Config.nodeConfigs[cndex];
                if (nodeConfig.Name == targ)
                {
                    targ = argv[1];
                    switch (targ.ToUpper())
                    {
                        case ("X"):
                            {
                                nodeConfig.ScaleX = float.Parse(argv[2]);
                                break;
                            }
                        case ("Y"):
                            {
                                nodeConfig.ScaleY = float.Parse(argv[2]);
                                break;
                            }
                        default:
                            {
                                Globals.Chat.PrintError("Please choose X, or Y.");
                                return;
                                break;
                            }
                    }
                    Globals.Chat.Print("Scaled.");
                }
                else
                {
                    Globals.Chat.PrintError("Please make sure the unit exists.");
                }
            }

            Globals.Config.Save();
        }

        public static unsafe void ResizableHud_Pos(string command, string args)
        {
            string[] argv = args.Split(' ');
            string targ;
            ResNodeConfig nodeConfig;

            targ = argv[0];
            for (int cndex = 0; cndex < Globals.Config.nodeConfigs.Count; cndex++)
            {
                nodeConfig = Globals.Config.nodeConfigs[cndex];
                if (nodeConfig.Name == targ)
                {
                    targ = argv[1];
                    switch (targ.ToUpper())
                    {
                        case ("X"):
                            {
                                nodeConfig.PosX = float.Parse(argv[2]);
                                break;
                            }
                        case ("Y"):
                            {
                                nodeConfig.PosY = float.Parse(argv[2]);
                                break;
                            }
                        default:
                            {
                                Globals.Chat.PrintError("Please choose X, or Y.");
                                return;
                                break;
                            }
                    }
                    Globals.Chat.Print("Scaled.");
                }
                else
                {
                    Globals.Chat.PrintError("Please make sure the unit exists.");
                }
            }

            Globals.Config.Save();
        }
    }
}
