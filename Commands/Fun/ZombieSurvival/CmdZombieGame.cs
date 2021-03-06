/*
    Copyright 2010 MCLawl Team -
    Created by Snowl (David D.) and Cazzar (Cayde D.)

    Dual-licensed under the    Educational Community License, Version 2.0 and
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
using System;
using MCGalaxy.Games;

namespace MCGalaxy.Commands
{
    public sealed class CmdZombieGame : Command
    {
        public override string name { get { return "zombiegame"; } }
        public override string shortcut { get { return "zg"; } }
        public override string type { get { return CommandTypes.Games; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public override CommandAlias[] Aliases {
        	get { return new[] { new CommandAlias("zs"), new CommandAlias("zombiesurvival") }; }
        }        
        public CmdZombieGame() { }
        
        public override void Use(Player p, string message) {
            if (message == "") { Help(p); return; }
            string[] args = message.ToLower().Split(' ');
            switch (args[0]) {
                    case "status": HandleStatus(p, message, args); break;
                    case "start": HandleStart(p, message, args); break;
                    case "stop": HandleStop(p, message, args); break;
                    case "force": HandleForceStop(p, message, args); break;
                    case "hitbox": HandleHitbox(p, message, args); break;
                    case "maxmove": HandleMaxMove(p, message, args); break;
            }
        }
        
        static void HandleStatus(Player p, string message, string[] args) {
            switch (Server.zombie.Status) {
                case ZombieGameStatus.NotStarted:
                    Player.SendMessage(p, "Zombie Survival is not ccurrently running."); return;
                case ZombieGameStatus.InfiniteRounds:
                    Player.SendMessage(p, "Zombie Survival is currently in progress with infinite rounds."); return;
                case ZombieGameStatus.SingleRound:
                    Player.SendMessage(p, "Zombie Survival game currently in progress."); return;
                case ZombieGameStatus.VariableRounds:
                    Player.SendMessage(p, "Zombie Survival game currently in progress with " + Server.zombie.MaxRounds + " rounds."); return;
                case ZombieGameStatus.LastRound:
                    Player.SendMessage(p, "Zombie Survival game currently in progress, with this round being the final round."); return;
            }
        }
        
        static void HandleStart(Player p, string message, string[] args) {
            if (Server.zombie.Running) {
                Player.SendMessage(p, "There is already a Zombie Survival game currently in progress."); return;
            }
            if (args.Length == 2) {
                int rounds = 1;
                if (!int.TryParse(args[1], out rounds)) {
                    Player.SendMessage(p, "You need to specify a valid option!"); return;
                }
                ZombieGameStatus status = rounds == 0 ?
                    ZombieGameStatus.InfiniteRounds : ZombieGameStatus.VariableRounds;
                Server.zombie.Start(status, rounds);
            } else {
                Server.zombie.Start(ZombieGameStatus.SingleRound, 0);
            }
        }
        
        static void HandleStop(Player p, string message, string[] args) {
            if (!Server.zombie.Running) {
                Player.SendMessage(p, "There is no Zombie Survival game currently in progress."); return;
            }
            Player.GlobalMessage("The current game of Zombie Survival will end this round!");
            Server.zombie.Status = ZombieGameStatus.LastRound;
        }
        
        static void HandleForceStop(Player p, string message, string[] args) {
            if (!Server.zombie.Running) {
                Player.SendMessage(p, "There is no Zombie Survival game currently in progress."); return;
            }
        	
            string src = p == null ? "(console)" : p.name;
            Server.s.Log("Zombie Survival ended forcefully by " + src);
            Server.zombie.Alive.Clear();
            Server.zombie.ResetState();
        }
        
        static void HandleHitbox(Player p, string message, string[] args) {
            byte precision;
            if (args.Length == 1) {
                Player.SendMessage(p, "Hitbox detection is currently &a" + ZombieGame.HitboxPrecision + " %Sunits apart.");
            } else if (!byte.TryParse(args[1], out precision)) {
                Player.SendMessage(p, "Hitbox detection must be an integer between 0 and 256.");
            } else {
                ZombieGame.HitboxPrecision = precision;
                Player.SendMessage(p, "Hitbox detection set to &a" + precision + " %Sunits apart.");
                SrvProperties.Save();
            }
        }
        
        static void HandleMaxMove(Player p, string message, string[] args) {
            byte distance;
            if (args.Length == 1) {
                Player.SendMessage(p, "Maxmium move distance is currently &a" + ZombieGame.MaxMoveDistance + " %Sunits apart.");
            } else if (!byte.TryParse(args[1], out distance)) {
                Player.SendMessage(p, "Maximum move distance must be an integer between 0 and 256.");
            } else {
                ZombieGame.MaxMoveDistance = distance;
                Player.SendMessage(p, "Maximum move distance set to &a" + distance + " %Sunits apart.");
                SrvProperties.Save();
            }
        }
        
        public override void Help(Player p) {
            Player.SendMessage(p, "/zg status - shows the current status of the Zombie Survival game.");
            Player.SendMessage(p, "/zg start 0 - Starts a Zombie Survival game for an unlimited amount of rounds.");
            Player.SendMessage(p, "/zg start [x] - Starts a Zombie Survival game for [x] amount of rounds.");
            Player.SendMessage(p, "/zg stop - Stops the Zombie Survival game after the round has finished.");
            Player.SendMessage(p, "/zg force - Force stops the Zombie Survival game immediately.");
            Player.SendMessage(p, "/zg hitbox [distance] - Sets how far apart players need to be before " +
                               "they are considered a 'collision'. (32 units = 1 block).");
            Player.SendMessage(p, "/zg maxmove [distance] - Sets how far apart players are allowed to move in a" +
                               "movement packet before they are considered speedhacking. (32 units = 1 block).");
        }
    }
}
