﻿/*
    Copyright 2011 MCForge
    
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

namespace MCGalaxy.Eco {
    
    public sealed class TitleItem : SimpleItem {
        
        public TitleItem() {
            Aliases = new[] { "titles", "title" };
            NoArgsResetsItem = true;
        }
        
        public override string Name { get { return "Title"; } }
        static char[] trimChars = { ' ' };
        
        protected override void OnBuyCommand(Player p, string message, string[] args) {
            if (args.Length == 1) {
                Command.all.Find("title").Use(null, p.name);
                Player.SendMessage(p, "%aYour title was removed for free."); return;
            }
            
            string title = message.Split(trimChars, 2)[1]; // keep spaces this way
            if (title == p.title) {
                Player.SendMessage(p, "%cYou already have that title."); return;
            }
            if (title.Length >= 20) {
                Player.SendMessage(p, "%cTitles must be under 20 characters."); return;
            }
            
            Command.all.Find("title").Use(null, p.name + " " + title);
            Player.SendMessage(p, "%aYour title was changed to [" + p.titlecolor + title + "%a]");
            MakePurchase(p, Price, "%3Title: %f" + title);
        }
    }
	
	public sealed class NickItem : SimpleItem {
        
        public NickItem() {
            Aliases = new[] { "nickname", "nick", "nicks", "name", "names" };
            NoArgsResetsItem = true;
        }
        
        public override string Name { get { return "Nickname"; } }
        static char[] trimChars = { ' ' };
        
        protected override void OnBuyCommand(Player p, string message, string[] args) {
            if (args.Length == 1) {
                Command.all.Find("nick").Use(null, p.name);
                Player.SendMessage(p, "%aYour nickname was removed for free."); return;
            }
            
            string nick = message.Split(trimChars, 2)[1]; // keep spaces this way
            if (nick == p.DisplayName) {
                Player.SendMessage(p, "%cYou already have that nickname."); return;
            }
            if (nick.Length >= 30) {
                Player.SendMessage(p, "%cNicknames must be under 30 characters."); return;
            }
            
            Command.all.Find("nick").Use(null, p.name + " " + nick);
            Player.SendMessage(p, "%aYour nickname was changed to [" + p.color + nick + "%a]");
            MakePurchase(p, Price, "%3Nickname: %f" + nick);
        }
    }
    
    public sealed class TitleColorItem : SimpleItem {
        
        public TitleColorItem() {
            Aliases = new[] { "tcolor", "tcolors", "titlecolor", "titlecolors", "tc" };
        }
        
        public override string Name { get { return "TitleColor"; } }
        
        protected override void OnBuyCommand(Player p, string message, string[] args) {            
            if (!args[1].StartsWith("&") || !args[1].StartsWith("%")) {
                args[1] = Colors.Parse(args[1]);
                if (args[1] == "") { Player.SendMessage(p, "%cThat wasn't a color"); return; }
            }
            if (args[1] == p.titlecolor) {
                Player.SendMessage(p, "%cYou already have a " + args[1] + Colors.Name(args[1]) + "%c titlecolor"); return;
            }
            
            Command.all.Find("tcolor").Use(null, p.name + " " + Colors.Name(args[1]));
            Player.SendMessage(p, "%aYour titlecolor was changed to " + args[1] + Colors.Name(args[1]));
            MakePurchase(p, Price, "%3Titlecolor: " + args[1] + Colors.Name(args[1]));
        }
    }
    
    public sealed class ColorItem : SimpleItem {
        
        public ColorItem() {
            Aliases = new[] { "colors", "color", "colours", "colour" };
        }
        
        public override string Name { get { return "Color"; } }

        protected override void OnBuyCommand(Player p, string message, string[] args) {
            if (!args[1].StartsWith("&") || !args[1].StartsWith("%")) {
                args[1] = Colors.Parse(args[1]);
                if (args[1] == "") { Player.SendMessage(p, "%cThat wasn't a color"); return; }
            }
            if (args[1] == p.color) {
                Player.SendMessage(p, "%cYou already have a " + args[1] + Colors.Name(args[1]) + "%c color"); return;
            }
            
            Command.all.Find("color").Use(null, p.name + " " + Colors.Name(args[1]));
            MakePurchase(p, Price, "%3Color: " + args[1] + Colors.Name(args[1]));
        }
    }
}
