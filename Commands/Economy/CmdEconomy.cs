/*
    Copyright 2011 MCForge
    
    Dual-licensed under the    Educational Community License, Version 2.0 and
    the GNU General Public License, Version 3 (the "Licenses"); you may
    not use this file except in compliance with the Licenses. You may
    obtain a copy of the Licenses at
    
    http://www.opensource.org/licenses/ecl2.php
    http://www.gnu.org/licenses/gpl-3.0.html
    
    Unless required by applicable law or agreed to in writing,
    software distributed under the Licenses are distributed on an "AS IS"
    BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
    or implied. See the Licenses for the specific language governing
    permissions and limitations under the Licenses.
 */
using System;
using System.Globalization;
using System.Threading;
using MCGalaxy.Eco;
using MCGalaxy.SQL;
namespace MCGalaxy.Commands {
    
    /// <summary> Economy Beta v1.0 QuantumHive  </summary>
    public sealed class CmdEconomy : Command {
        public override string name { get { return "economy"; } }
        public override string shortcut { get { return "eco"; } }
        public override string type { get { return CommandTypes.Economy; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Guest; } }
        public override CommandPerm[] AdditionalPerms {
            get { return new[] { new CommandPerm(LevelPermission.Operator, "The lowest rank that can setup the economy") }; }
        }
        
        public override void Use(Player p, string message) {
            string[] raw = message.Split(' ');
            string[] args = { "", "", "", "", "", "", "", "" };
            for (int i = 0; i < Math.Min(args.Length, raw.Length); i++)
                args[i] = raw[i];
            HandleSetup(p, message, args);
        }
        
        void HandleSetup(Player p, string message, string[] args) {
        	if (!CheckAdditionalPerm(p)) { MessageNeedPerms(p, "can setup the economy."); return; }
            
        	switch (args[0].ToLower()) {
                case "apply":
                    Economy.Load();
                    Player.SendMessage(p, "%aApplied changes");
                    return;                  

                case "enable":
                    Player.SendMessage(p, "%aThe economy system is now enabled"); 
                    Economy.Enabled = true; 
                    Economy.Save(); return;

                case "disable":
                    Player.SendMessage(p, "%aThe economy system is now disabled"); 
                    Economy.Enabled = false; 
                    Economy.Save();return;
                    
                case "help":
                    SetupHelp(p); return;

                default:
                    foreach (Item item in Economy.Items)
                        foreach (string alias in item.Aliases) 
                    {
                        if (args[0].CaselessEq(alias)) {
                            item.OnSetupCommand(p, args); 
                            Economy.Save(); return;
                        }
                    }
                                        
                    if (args[1] == "") { SetupHelp(p); return; }
                    Player.SendMessage(p, "%cThat wasn't a valid command addition!");
                    return;
            }
        }
        
        public override void Help(Player p) {
            Player.SendMessage(p, "%cMost commands have been removed from /economy, " +
                               "use the appropriate command from %T/help economy %cinstead.");
        	if (CheckAdditionalPerm(p)) {
                Player.SendMessage(p, "%f/eco <type> %e- to setup economy");
                Player.SendMessage(p, "%f/eco help %e- get more specific help for setting up the economy");
            }
        }

        void SetupHelp(Player p) {
            Player.SendMessage(p, "%4/eco apply %e- reloads changes made to 'economy.properties'");
            Player.SendMessage(p, "%4/eco [%aenable%4/%cdisable%4] %e- to enable/disable the economy system");
            Player.SendMessage(p, "%4/eco [title/color/tcolor/rank/map] [%aenable%4/%cdisable%4] %e- to enable/disable that feature");
            Player.SendMessage(p, "%4/eco [title/color/tcolor] [%3price%4] %e- to setup the prices for these features");
            Player.SendMessage(p, "%4/eco rank price [%frank%4] [%3price%4] %e- to set the price for that rank");
            Player.SendMessage(p, "%4/eco rank maxrank [%frank%4] %e- to set the max buyable rank");
            Player.SendMessage(p, "%4/eco map new [%fname%4] [%fx%4] [%fy%4] [%fz%4] [%ftype%4] [%3price%4] %e- to setup a map preset");
            Player.SendMessage(p, "%4/eco map delete [%fname%4] %e- to delete a map");
            Player.SendMessage(p, "%4/eco map edit [%fname%4] [name/x/y/z/type/price] [%fvalue%4] %e- to edit a map preset");
        }
    }
}
