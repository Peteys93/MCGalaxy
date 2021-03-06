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
using System.Collections.Generic;
using MCGalaxy.BlockPhysics;

namespace MCGalaxy.Commands
{
    public sealed class CmdRestartPhysics : Command
    {
        public override string name { get { return "restartphysics"; } }
        public override string shortcut { get { return "rp"; } }
        public override string type { get { return CommandTypes.Building; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.AdvBuilder; } }
        public CmdRestartPhysics() { }

        public override void Use(Player p, string message) {
            CatchPos cpos = default(CatchPos);
            message = message.ToLower();
            if (message != "" && !ParseArgs(p, message, ref cpos)) return;

            p.blockchangeObject = cpos;
            Player.SendMessage(p, "Place two blocks to determine the edges.");
            p.ClearBlockchange();
            p.Blockchange += new Player.BlockchangeEventHandler(Blockchange1);
        }
        
        bool ParseArgs(Player p, string message, ref CatchPos cpos) {
            string[] parts = message.Split(' ');
            if (parts.Length % 2 == 1) {
                Player.SendMessage(p, "Number of parameters must be even");
                Help(p); return false;
            }          
            PhysicsArgs args = default(PhysicsArgs);
            byte type = 0, value = 0;
            
            if (parts.Length >= 2) {
                if (!Parse(p, parts[0], parts[1], ref type, ref value)) return false;
                args.Type1 = type; args.Value1 = value;
            }
            if (parts.Length >= 4) {
                if (!Parse(p, parts[2], parts[3], ref type, ref value)) return false;
                args.Type2 = type; args.Value2 = value;
            }
            if (parts.Length >= 6) {
            	Player.SendMessage(p, "You can only use up to two types of physics."); return false;
            }
            cpos.extraInfo = args; return true;
        }
        
        bool Parse(Player p, string name, string arg, ref byte type, ref byte value) {
            if (name == "revert") {
                byte block = Block.Byte(arg);
                if (block == Block.Zero) { Player.SendMessage(p, "Invalid block type."); return false; }
                type = PhysicsArgs.Revert; value = block;
                return true;
            }
            
            int temp;
            if (!int.TryParse(arg, out temp)) {
                Player.SendMessage(p, "/rp [type1] [num] [type2] [num]..."); return false;
            }
            if (temp < 0 || temp > 255) {
                Player.SendMessage(p, "Values must be between 0 and 255."); return false;
            }
            value = (byte)temp;
            
            switch (name) {
                case "drop": type = PhysicsArgs.Drop; return true;
                case "explode": type = PhysicsArgs.Explode; return true;
                case "dissipate": type = PhysicsArgs.Dissipate; return true;
                case "wait": type = PhysicsArgs.Wait; return true;
                case "rainbow": type = PhysicsArgs.Rainbow; return true;
            }
            Player.SendMessage(p, name + " type is not supported.");
            return false;
        }
        
        void Blockchange1(Player p, ushort x, ushort y, ushort z, byte type, byte extType) {
            RevertAndClearState(p, x, y, z);
            CatchPos bp = (CatchPos)p.blockchangeObject;
            bp.x = x; bp.y = y; bp.z = z; p.blockchangeObject = bp;
            p.Blockchange += new Player.BlockchangeEventHandler(Blockchange2);
        }
        
        void Blockchange2(Player p, ushort x, ushort y, ushort z, byte type, byte extType) {
            RevertAndClearState(p, x, y, z);
            CatchPos cpos = (CatchPos)p.blockchangeObject;
            List<int> buffer = new List<int>();
            
            for (ushort yy = Math.Min(cpos.y, y); yy <= Math.Max(cpos.y, y); ++yy)
                for (ushort zz = Math.Min(cpos.z, z); zz <= Math.Max(cpos.z, z); ++zz)
                    for (ushort xx = Math.Min(cpos.x, x); xx <= Math.Max(cpos.x, x); ++xx)
            {
                int index = p.level.PosToInt(xx, yy, zz);
                if (index >= 0 && p.level.blocks[index] != Block.air)
                    buffer.Add(index);
            }

            if (cpos.extraInfo.Raw == 0) {
                if (buffer.Count > Server.rpNormLimit) {
                    Player.SendMessage(p, "Cannot restart more than " + Server.rpNormLimit + " blocks.");
                    Player.SendMessage(p, "Tried to restart " + buffer.Count + " blocks.");
                    return;
                }
            } else if (buffer.Count > Server.rpLimit) {
                Player.SendMessage(p, "Tried to add physics to " + buffer.Count + " blocks.");
                Player.SendMessage(p, "Cannot add physics to more than " + Server.rpLimit + " blocks.");
                return;
            }

            foreach (int index in buffer)
                p.level.AddCheck(index, true, cpos.extraInfo);
            Player.SendMessage(p, "Activated " + buffer.Count + " blocks.");
            if (p.staticCommands)
                p.Blockchange += new Player.BlockchangeEventHandler(Blockchange1);
        }

        struct CatchPos { public ushort x, y, z; public PhysicsArgs extraInfo; }
        
        public override void Help(Player p) {
            Player.SendMessage(p, "/restartphysics ([type] [num]) ([type2] [num2]) - Restarts every physics block in an area");
            Player.SendMessage(p, "[type] will set custom physics for selected blocks");
            Player.SendMessage(p, "Possible [types]: drop, explode, dissipate, wait, rainbow, revert");
            Player.SendMessage(p, "/rp revert takes block names");
        }
    }
}
