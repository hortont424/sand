using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sand
{
    internal class ToolChooserButton : Actor
    {
        private readonly string _toolSetName;
        private readonly Dictionary<Type, Button> _toolButtons;

        public delegate void HoverDelegate(Type toolClass, object userData);

        private HoverDelegate _hoverAction;
        private object _hoverUserData;

        public Type SelectedTool;

        public ToolChooserButton(Game game, Vector2 origin, string toolSetName, List<Type> toolClasses) : base(game)
        {
            _toolSetName = toolSetName;
            DrawOrder = 100;
            _toolButtons = new Dictionary<Type, Button>();

            var label = new Label(Game, origin.X, origin.Y, _toolSetName, "Calibri24Bold")
                        {
                            PositionGravity =
                                new Tuple<Gravity.Vertical, Gravity.Horizontal>(Gravity.Vertical.Center,
                                                                                Gravity.Horizontal.Right)
                        };
            Children.Add(label);

            var offsetX = origin.X + 24;

            foreach(var toolClass in toolClasses)
            {
                var toolIcon = toolClass.GetMethod("_icon").Invoke(null, null) as Texture2D;
                var toolButton = new Button(game, new Vector2(offsetX, origin.Y),
                                            toolIcon,
                                            Color.White, Color.Black) { Padding = 6 };
                toolButton.Y -= toolButton.Height / 2;
                toolButton.SetAction(ChooseTool, toolClass);
                toolButton.SetHoverAction(HoverTool, toolClass);

                offsetX += toolButton.Width + 24;

                Children.Add(toolButton);
                _toolButtons.Add(toolClass, toolButton);
            }

            if(toolClasses.Count > 0)
            {
                ChooseTool(null, toolClasses[0]);
            }
        }

        public void SetHoverAction(HoverDelegate hoverAction, object hoverUserData)
        {
            _hoverAction = hoverAction;
            _hoverUserData = hoverUserData;
        }

        public void ChooseTool(object sender, object userInfo)
        {
            SelectedTool = userInfo as Type;

            foreach(KeyValuePair<Type, Button> pair in _toolButtons)
            {
                pair.Value.AcceptsClick = pair.Key != SelectedTool;
                pair.Value.TeamColor = (pair.Key != SelectedTool)
                                           ? new Color(0.2f, 0.2f, 0.2f)
                                           : Color.White;
            }
        }

        public void HoverTool(object sender, object userInfo)
        {
            var hoveredTool = userInfo as Type;
            _hoverAction(hoveredTool, _hoverUserData);
        }
    }
}