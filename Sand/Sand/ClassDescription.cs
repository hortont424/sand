using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sand
{
    internal class ClassDescription : Actor
    {
        private readonly Class _class;

        private static readonly Dictionary<Class, Tuple<string, string>> _toolPairs = new Dictionary
            <Class, Tuple<string, string>>
                                                                                      {
                                                                                          {
                                                                                              Class.Defense,
                                                                                              new Tuple<string, string>(
                                                                                              "Defense", "Create Sand")
                                                                                              },
                                                                                          {
                                                                                              Class.Offense,
                                                                                              new Tuple<string, string>(
                                                                                              "Offense", "Destroy Sand")
                                                                                              },
                                                                                          {
                                                                                              Class.Support,
                                                                                              new Tuple<string, string>(
                                                                                              "Support", "Move Sand")
                                                                                              }
                                                                                      };

        public ClassDescription(Game game, Class cls)
            : base(game)
        {
            _class = cls;
            PositionGravity = Gravity.Center;
        }

        public override void Draw(GameTime gameTime)
        {
            var pair = _toolPairs[_class];

            DrawString(pair.Item1, "Calibri32Bold", 2.7f);
            DrawString(pair.Item2, "Calibri24Bold", 3.7f);
        }

        public void DrawString(string name, string font, float location)
        {
            var textSize = Storage.Font(font).MeasureString(name);
            var textOrigin = new Vector2(textSize.X, textSize.Y) * Gravity.Offset(PositionGravity);

            _spriteBatch.DrawString(Storage.Font(font), name,
                                    new Vector2(X, location * 40 + Y),
                                    Color.White, 0.0f, textOrigin, 1.0f, SpriteEffects.None, 0.0f);
        }
    }
}