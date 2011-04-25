using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Sand
{
    internal class MapChooserButton : Actor
    {
        private readonly Dictionary<Map, Button> _mapButtons;

        public Map SelectedMap;

        public MapChooserButton(Game game, Vector2 origin)
            : base(game)
        {
            DrawOrder = 100;
            _mapButtons = new Dictionary<Map, Button>();

            var offsetX = origin.X + 24;

            foreach(var keyPair in _sandGame.MapManager.Maps)
            {
                var name = keyPair.Key;
                var map = keyPair.Value;

                var mapIcon = map.MapImage;
                var toolButton = new Button(game, new Vector2(offsetX, origin.Y),
                                            mapIcon,
                                            Color.White, Color.Black) { Padding = 6 };

                toolButton.Width = 512 + (toolButton.Padding * 2);
                toolButton.Height = 512 + (toolButton.Padding * 2);

                toolButton.Y -= toolButton.Height / 2;
                toolButton.SetAction(ChooseTool, map);

                offsetX += toolButton.Width + 48;

                Children.Add(toolButton);
                _mapButtons.Add(map, toolButton);
            }

            ChooseTool(null, _mapButtons.Keys.First());
        }

        public void ChooseTool(object sender, object userInfo)
        {
            SelectedMap = userInfo as Map;

            _sandGame.GameMap = SelectedMap;

            Console.WriteLine(SelectedMap);

            foreach(KeyValuePair<Map, Button> pair in _mapButtons)
            {
                pair.Value.TeamColor = (pair.Key != SelectedMap)
                                           ? new Color(0.2f, 0.2f, 0.2f)
                                           : Color.White;
            }
        }
    }
}