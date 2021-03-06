/*
    Copyright 2010 MCSharp team (Modified for use with MCZall/MCLawl/MCGalaxy)
    
    Dual-licensed under the Educational Community License, Version 2.0 and
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
using System.Collections.Generic;
using System.IO;
using MCGalaxy.Levels.IO;

namespace MCGalaxy.Commands {
    
    public sealed class CmdMapInfo : Command {
        
        public override string name { get { return "mapinfo"; } }
        public override string shortcut { get { return "mi"; } }
        public override string type { get { return CommandTypes.Information; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Banned; } }
        public override CommandAlias[] Aliases {
            get { return new[] { new CommandAlias("winfo"), new CommandAlias("worldinfo") }; }
        }
        
        public override void Use(Player p, string message) {
            string[] args = message.Split(' ');
            string level;
            bool env = args[0].CaselessEq("env");
            level = env ? (args.Length > 1 ? args[1] : "") : args[0];
            bool perms = args[0].CaselessEq("perms");
            level = perms ? (args.Length > 1 ? args[1] : "") : args[0];
            
            Level lvl = level == "" ? p.level : LevelInfo.Find(level);
            MapInfoData data = new MapInfoData();
            if (lvl != null) {
                data.FromOnlineLevel(lvl);
            } else if (LevelInfo.ExistsOffline(level)) {
                data.FromOfflineLevel(level);
            } else {
                Player.SendMessage(p, "Could not find specified level."); return;
            }
            
            if (env) ShowEnv(p, data);
            else if (perms) ShowPermissions(p, data);
            else ShowNormal(p, data);
        }
        
        void ShowNormal(Player p, MapInfoData data) {
            Player.SendMessage(p, "&b" + data.Name + "%S: Width=" + data.Width + " Height=" + data.Height + " Depth=" + data.Length);
            string physicsState = CmdPhysics.states[data.Physics];
            Player.SendMessage(p, "Physics are " + physicsState + " %Son &b" + data.Name);

            string gunStatus = data.Guns ? "&aonline" : "&coffline";
            if (p == null || p.group.CanExecute("gun"))
                Player.SendMessage(p, "&cGuns &eare " + gunStatus + " &eon " + data.Name + ".");

            if (Directory.Exists(Server.backupLocation + "/" + data.Name)) {
                int latestBackup = Directory.GetDirectories(Server.backupLocation + "/" + data.Name).Length;
                DateTime time = Directory.GetCreationTime(LevelInfo.BackupPath(data.Name, latestBackup.ToString()));
                Player.SendMessage(p, "Latest backup: &a" + latestBackup + " %Sat &a" + time.ToString("yyyy-MM-dd HH:mm:ss"));
            } else {
                Player.SendMessage(p, "No backups for this map exist yet.");
            }
            Player.SendMessage(p, "Use %T/mi env " + data.Name + " %Sto see environment settings.");
            Player.SendMessage(p, "Use %T/mi perms " + data.Name + " %Sto see permission settings.");
            
            if (!Server.zombie.IsZombieMap(data.Name)) return;
            Player.SendMessage(p, "Map authors: " + data.Authors);
            int winChance = data.TotalRounds == 0 ? 100 : (data.HumanRounds * 100) / data.TotalRounds;
            Player.SendMessage(p, "&a" + data.TotalRounds + " %Srounds played total, with a &a" 
                               + winChance + "% %Swin chance for humans.");
            Player.SendMessage(p, "This map has &a" + data.Likes + " likes %Sand &c" + data.Dislikes + " dislikes");
        }
        
        void ShowPermissions(Player p, MapInfoData data) {
            Player.SendMessage(p, "Build rank = " + Group.findPerm(data.build).ColoredName +
                               " %S: Visit rank = " + Group.findPerm(data.visit).ColoredName);
            Player.SendMessage(p, "BuildMax Rank = " + Group.findPerm(data.buildmax).ColoredName +
                               " %S: VisitMax Rank = " + Group.findPerm(data.visitmax).ColoredName);
            List<string> whitelist = data.VisitWhitelist;
            List<string> blacklist = data.VisitBlacklist;
            GetBlacklistedPlayers(data.Name, blacklist);
            if (whitelist.Count > 0)
                Player.SendMessage(p, "Visit whitelist: &a" + String.Join("%S, &a", whitelist));
            if (blacklist.Count > 0)
                Player.SendMessage(p, "Visit blacklist: &c" + String.Join("%S, &c", blacklist));
        }
        
        void GetBlacklistedPlayers(string l, List<string> blacklist) {
            string path = "levels/blacklists/" + l + ".txt";
            if (!File.Exists(path)) return;
            
            string[] lines = File.ReadAllLines(path);
            foreach (string line in lines) {
                if (line.IndexOf(' ') < 0) continue;
                blacklist.Add(line.Split(' ')[1]);
            }
        }
        
        void ShowEnv(Player p, MapInfoData data) {
            if (data.TerrainUrl != "")
                Player.SendMessage(p, "Texture: %b" + data.TerrainUrl);
            else
                Player.SendMessage(p, "No custom texture set for this map.");
            
            if (data.TextureUrl != "")
                Player.SendMessage(p, "Texture pack: %b" + data.TextureUrl);
            else
                Player.SendMessage(p, "No custom texture pack set for this map.");
            
            const string format = "Colors: Fog {0}, Sky {1}, Clouds {2}, Sunlight {3}, Shadowlight {4}";
            Player.SendMessage(p, String.Format(format, Color(data.Fog), Color(data.Sky), Color(data.Clouds),
                                                Color(data.Light), Color(data.Shadow)));
            
            Player.SendMessage(p, "Water Level: %b" + data.EdgeLevel + "%S, Clouds height: %b" + data.CloudsHeight
                               + "%S, Max fog distance: %b" + data.MaxFogDistance);
            Player.SendMessage(p, "Edge Block: %b" + data.EdgeBlock + "%S, Horizon Block: %b" + data.HorizonBlock);
        }
        
        class MapInfoData {
            
            public ushort Width, Height, Length;
            public int Physics;
            public bool Guns;
            public string Name, TerrainUrl, TextureUrl;
            public string Fog, Sky, Clouds, Light, Shadow;
            public short EdgeLevel, CloudsHeight, MaxFogDistance;
            public byte EdgeBlock = Block.blackrock, HorizonBlock = Block.water;
            public LevelPermission visit, build, visitmax, buildmax;
            public List<string> VisitWhitelist = new List<string>();
            public List<string> VisitBlacklist = new List<string>();
            // Zombie data
            public string Authors;
            public int TotalRounds, HumanRounds;
            public int Likes, Dislikes;

            public void FromOnlineLevel(Level lvl) {
                Name = lvl.name;
                Width = lvl.Width; Height = lvl.Height; Length = lvl.Length;
                Physics = lvl.physics; Guns = lvl.guns;
                visit = lvl.permissionvisit; build = lvl.permissionbuild;
                visitmax = lvl.pervisitmax; buildmax = lvl.perbuildmax;
                VisitWhitelist = new List<string>(lvl.VisitWhitelist);
                VisitBlacklist = new List<string>(lvl.VisitBlacklist);
                
                Fog = lvl.FogColor; Sky = lvl.SkyColor; Clouds = lvl.CloudColor;
                Light = lvl.LightColor; Shadow = lvl.ShadowColor;
                EdgeLevel = lvl.EdgeLevel; CloudsHeight = lvl.CloudsHeight;
                MaxFogDistance = lvl.MaxFogDistance;
                EdgeBlock = lvl.EdgeBlock; HorizonBlock = lvl.HorizonBlock;
                
                TerrainUrl = lvl.terrainUrl != "" ?
                    lvl.terrainUrl : Server.defaultTerrainUrl;
                TextureUrl = lvl.texturePackUrl != "" ?
                    lvl.texturePackUrl : Server.defaultTextureUrl;
                
                Authors = lvl.Authors;
                TotalRounds = lvl.RoundsPlayed; HumanRounds = lvl.RoundsHumanWon;
                Likes = lvl.Likes; Dislikes = lvl.Dislikes;
            }
            
            public void FromOfflineLevel(string name) {
                this.Name = name;
                LvlFile.LoadDimensions(LevelInfo.LevelPath(name),
                                       out Width, out Height, out Length);
                string path = LevelInfo.GetPropertiesPath(name);
                if (path != null)
                    PropertiesFile.Read(path, ParseProperty, '=');
                
                path = "levels/level properties/" + name + ".env";
                if (File.Exists(path))
                    PropertiesFile.Read(path, ParseEnv, '=');
                if (Authors == null) Authors = "";
            }
            
            void ParseProperty(string key, string value) {
                switch (key.ToLower()) {
                    case "physics": Physics = int.Parse(value); break;
                    case "guns": Guns = bool.Parse(value); break;
                    case "texture": TerrainUrl = value; break;
                    case "texturepack": TextureUrl = value; break;
                    
                    case "perbuild": build = GetPerm(value); break;
                    case "pervisit": visit = GetPerm(value); break;
                    case "perbuildmax": buildmax = GetPerm(value); break;
                    case "pervisitmax": visitmax = GetPerm(value); break;
                    case "visitwhitelist":
                        VisitWhitelist = new List<string>(value.Split(',')); break;
                    case "visitblacklist":
                        VisitBlacklist = new List<string>(value.Split(',')); break;
                    
                    case "authors": Authors = value; break;
                    case "roundsplayed": TotalRounds = int.Parse(value); break;
                    case "RoundsHumanWon": HumanRounds = int.Parse(value); break;
                    case "likes": Likes = int.Parse(value); break;
                    case "dislikes": Dislikes = int.Parse(value); break;
                }
            }
            
            void ParseEnv(string key, string value) {
                switch (key.ToLower()) {
                    case "cloudcolor": Clouds = value; break;
                    case "fogcolor": Fog = value; break;
                    case "skycolor": Sky = value; break;
                    case "shadowcolor": Shadow = value; break;
                    case "lightcolor": Light = value; break;
                    case "edgeblock": EdgeBlock = byte.Parse(value); break;
                    case "edgelevel": EdgeLevel = short.Parse(value); break;
                    case "cloudsheight": CloudsHeight = short.Parse(value); break;
                    case "maxfog": MaxFogDistance = short.Parse(value); break;
                    case "horizonblock": HorizonBlock = byte.Parse(value); break;
                }
            }
            
            static LevelPermission GetPerm(string value) {
                LevelPermission perm = Level.PermissionFromName(value);
                return perm != LevelPermission.Null ? perm : LevelPermission.Guest;
            }
        }
        
        static string Color(string src) {
            return (src == null || src == "" || src == "-1") ? "%bnone%e" : "%b" + src + "%e";
        }
        
        public override void Help(Player p)  {
            Player.SendMessage(p, "/mapinfo <map> - Display details of <map>");
            Player.SendMessage(p, "/mapinfo env <map> - Display environment details of <map>");
            Player.SendMessage(p, "/mapinfo perms <map> - Display permission details of <map>");
        }
    }
}
