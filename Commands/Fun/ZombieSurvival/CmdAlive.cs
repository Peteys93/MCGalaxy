/*
    Copyright 2010 MCLawl Team - Written by Valek (Modified for use with MCGalaxy)
 
    Dual-licensed under the Educational Community License, Version 2.0 and
    the GNU General Public License, Version 3 (the "Licenses"); you may
    not use this file except in compliance with the Licenses. You may
    obtain a copy of the Licenses at
    
    http://www.osedu.org/licenses/ECL-2.0
    http://www.gnu.org/licenses/gpl-3.0.html
    
    Unless required by applicable law or agreed to in writing,
    software distributed under the Licenses are distributed on an "AS IS"
    BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
    or implied. See the Licenses for the specific language governing
    permissions and limitations under the Licenses.
*/
using MCGalaxy.Games;

namespace MCGalaxy.Commands {
    
    public sealed class CmdAlive : Command {
        public override string name { get { return "alive"; } }
        public override string shortcut { get { return "alive"; } }
        public override string type { get { return CommandTypes.Games; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Banned; } }
        public override CommandEnable Enabled { get { return CommandEnable.Zombie; } }
        public CmdAlive() { }
        
         public override void Use(Player p, string message) {
            Player[] alive = Server.zombie.Alive.Items;
            if (alive.Length == 0) {
                Player.SendMessage(p, "No one is alive."); return;
            }
            
            Player.SendMessage(p, "Players who are " + Colors.green + "alive %Sare:");
            string list = "";
            foreach (Player pl in alive)
                list = list + pl.group.color + pl.DisplayName + "%S, ";
            Player.SendMessage(p, list);
        }
        
        public override void Help(Player p) {
            Player.SendMessage(p, "/alive - shows who is alive");
        }
    }
}
