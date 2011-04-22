using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sand.Tools;

namespace Sand
{
    internal class ToolChooserButton : Actor
    {
        private readonly string _toolSetName;
        private readonly Dictionary<Type, Button> _toolButtons;

        public Type SelectedTool;
        private readonly Label _nameLabel;
        private readonly Label _descriptionLabel;

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
                toolButton.SetAction((a, b) => ChooseTool(a, b), toolClass);
                toolButton.SetHoverAction(HoverTool, toolClass);
                toolButton.SetUnhoverAction(UnhoverTool, toolClass);

                offsetX += toolButton.Width + 24;

                Children.Add(toolButton);
                _toolButtons.Add(toolClass, toolButton);
            }

            _nameLabel = new Label(Game, 850, origin.Y + 20, "", "Calibri48Bold")
                         {
                             PositionGravity = new Tuple<Gravity.Vertical, Gravity.Horizontal>(Gravity.Vertical.Bottom,
                                                                                               Gravity.Horizontal.Left)
                         };

            _descriptionLabel = new Label(Game, 850, origin.Y + 10, "", "Calibri24")
                                {
                                    PositionGravity =
                                        new Tuple<Gravity.Vertical, Gravity.Horizontal>(Gravity.Vertical.Top,
                                                                                        Gravity.Horizontal.Left)
                                };

            Children.Add(_nameLabel);
            Children.Add(_descriptionLabel);

            if(toolClasses.Count > 0)
            {
                ChooseTool(null, toolClasses[0], false);
            }
        }

        public void ChooseTool(object sender, object userInfo, bool playSound = true)
        {
            SelectedTool = userInfo as Type;

            foreach(KeyValuePair<Type, Button> pair in _toolButtons)
            {
                //pair.Value.AcceptsClick = pair.Key != SelectedTool;
                pair.Value.TeamColor = (pair.Key != SelectedTool)
                                           ? new Color(0.2f, 0.2f, 0.2f)
                                           : Color.White;
            }

            var toolType = (ToolType)SelectedTool.GetMethod("_type").Invoke(null, null);

            if(playSound)
            {
                Tool.SoundForTool(toolType).CreateInstance().Play();
            }

            UnhoverTool(null, null);
        }

        public void HoverTool(object sender, object userInfo)
        {
            var toolclass = userInfo as Type;

            if(toolclass != null)
            {
                _nameLabel.Text = toolclass.GetMethod("_name").Invoke(null, null) as string;
                _descriptionLabel.Text = toolclass.GetMethod("_description").Invoke(null, null) as string;
            }
        }

        public void UnhoverTool(object sender, object userInfo)
        {
            var toolclass = SelectedTool;

            if(toolclass != null)
            {
                _nameLabel.Text = toolclass.GetMethod("_name").Invoke(null, null) as string;
                _descriptionLabel.Text = toolclass.GetMethod("_description").Invoke(null, null) as string;
            }
        }
    }
}