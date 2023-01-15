using ResizableHUD.Attributes;
using Dalamud.Game.Command;
using System;
using System.Collections.Generic;

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

        public static void Uninitialize()
        {
            Globals.CommandManager.RemoveHandler("/prhud");
            Globals.CommandManager.RemoveHandler("/prhudadd");
            Globals.CommandManager.RemoveHandler("/prhudrem");
            Globals.CommandManager.RemoveHandler("/prhudscale");
            Globals.CommandManager.RemoveHandler("/prhudpos");
        }

        public static void ToggleConfig()
        {
            Globals.WindowSystem.GetWindow(ConfigWindow.ConfigWindowName).IsOpen = !Globals.WindowSystem.GetWindow(ConfigWindow.ConfigWindowName).IsOpen;
        }

        public static void OnPrHud(string command, string args)
        {
            ToggleConfig();
        }

        public static unsafe void ResizableHud_Add(string command, string args)
        {
            string[] argv = args.Split(' ');
            string targ;
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
                    if (Globals.Config.nodeConfigs[cndex].Name == targ)
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
                    Globals.Chat.Print("Added config.");
                }
            }

            Globals.Config.Save();
        }

        public static unsafe void ResizableHud_Rem(string command, string args)
        {
            string[] argv = args.Split(' ');
            string targ;

            for (int index = 0; index < argv.Length; index++)
            {
                targ = argv[index];
                for (int cndex = 0; cndex < Globals.Config.nodeConfigs.Count; cndex++)
                {
                    if (Globals.Config.nodeConfigs[cndex].Name == targ)
                    {
                        Globals.Config.nodeConfigs.RemoveAt(cndex);
                        Globals.Chat.Print("Removed config.");
                    }
                }
            }

            Globals.Config.Save();
        }

        public static unsafe void ResizableHud_Scale(string command, string args)
        {
            string[] argv = args.Split(' ');
            string targ;
            bool found = false;

            targ = argv[0];
            for (int cndex = 0; cndex < Globals.Config.nodeConfigs.Count; cndex++)
            {
                if (Globals.Config.nodeConfigs[cndex].Name == targ)
                {
                    float value = float.Parse(argv[2]);
                    found = true;
                    targ = argv[1];
                    switch (targ.ToUpper())
                    {
                        case ("X"):
                            {
                                Globals.Config.nodeConfigs[cndex].ScaleX = value;
                                break;
                            }
                        case ("Y"):
                            {
                                Globals.Config.nodeConfigs[cndex].ScaleY = value;
                                break;
                            }
                        default:
                            {
                                Globals.Chat.PrintError("Please choose X, or Y.");
                                return;
                                break;
                            }
                    }
                    Globals.Chat.Print($"Scaled to {targ} {value} {(Globals.Config.nodeConfigs[cndex].UsePercentage ? "%" : "")}");
                }
            }

            if (found == false)
            {
                Globals.Chat.PrintError("Please make sure the unit exists.");
            }

            Globals.Config.Save();
        }

        public static unsafe void ResizableHud_Pos(string command, string args)
        {
            string[] argv = args.Split(' ');
            string targ;
            bool found = false;

            targ = argv[0];
            for (int cndex = 0; cndex < Globals.Config.nodeConfigs.Count; cndex++)
            {
                if (Globals.Config.nodeConfigs[cndex].Name == targ)
                {
                    float value = float.Parse(argv[2]);
                    found = true;
                    targ = argv[1];
                    switch (targ.ToUpper())
                    {
                        case ("X"):
                            {
                                if (Globals.Config.nodeConfigs[cndex].UsePercentage)
                                {
                                    Globals.Config.nodeConfigs[cndex].PosPercentX = value;
                                }
                                else {
                                    Globals.Config.nodeConfigs[cndex].PosX = value;
                                }
                                break;
                            }
                        case ("Y"):
                            {
                                if (Globals.Config.nodeConfigs[cndex].UsePercentage) { 
                                    Globals.Config.nodeConfigs[cndex].PosPercentY = value;
                                }
                                else {
                                    Globals.Config.nodeConfigs[cndex].PosY = value;
                                }
                                break;
                            }
                        default:
                            {
                                Globals.Chat.PrintError("Please choose X, or Y.");
                                return;
                                break;
                            }
                    }
                    Globals.Chat.Print($"Moved to {targ} {value} {(Globals.Config.nodeConfigs[cndex].UsePercentage ? "%" : "")}");
                }
            }

            if (found == false) {
                Globals.Chat.PrintError("Please make sure the unit exists.");
            }

            Globals.Config.Save();
        }
    }
}
