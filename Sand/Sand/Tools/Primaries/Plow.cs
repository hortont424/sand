using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Sand.Tools.Primaries
{
    class Plow : Tool
    {
        public Plow(LocalPlayer player) : base(player)
        {

        }

        public static string _name()
        {
            return "Plow";
        }

        public static string _description()
        {
            return "Move Sand!";
        }

        public static Texture2D _icon()
        {
            return Storage.Sprite("Plow");
        }

        public static MouseButton _button()
        {
            return MouseButton.LeftButton;
        }
    }
}
