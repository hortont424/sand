using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Sand.GameState
{
    public class PlayState : GameState
    {
        public PlayState(Sand game) : base(game)
        {
        }

        public override void Enter(Dictionary<string, object> data)
        {
            var sandGame = Game;

            foreach(var gamer in Storage.NetworkSession.AllGamers)
            {
                Game.Components.Add((Player)gamer.Tag);
            }

            sandGame.GameMap = new Map(Game, "01");

            Game.Components.Add(sandGame.GameMap);

            var mobilityIcon = new ToolIcon(Game, (Storage.NetworkSession.LocalGamers[0].Tag as LocalPlayer).Mobility)
            {
                Position = new Vector2(sandGame.GameMap.Width + 10.0f + 148.0f, 10.0f + 148.0f)
            };

            Game.Components.Add(mobilityIcon);
        }

        public override void Update()
        {
            Messages.Update();
        }

        public override Dictionary<string, object> Leave()
        {
            var sandGame = Game;

            foreach(var gamer in Storage.NetworkSession.AllGamers)
            {
                Game.Components.Remove((Player)gamer.Tag);
            }

            Game.Components.Remove(sandGame.GameMap);

            return null;
        }
    }
}