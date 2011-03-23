using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;

namespace Sand
{
    public class Animation
    {
        private readonly PropertyInfo _property;
        private readonly object _obj;
        private readonly double _startValue, _endValue;
        private readonly EaseFunctionDelegate _easeFunc;
        private readonly EasingType _easeType;

        public delegate float EaseFunctionDelegate(double linearStep, EasingType type);

        public Animation(object obj, string propName, double startValue, double endValue)
            : this(obj, propName, startValue, endValue, Easing.EaseInOut, EasingType.Sine)
        {
        }

        public Animation(object obj, string propName, double endValue)
            : this(obj, propName, 0.0, endValue, Easing.EaseInOut, EasingType.Sine)
        {
            _startValue = (double)_property.GetValue(obj, null);
        }

        public Animation(object obj, string propName, double endValue, EaseFunctionDelegate easeFunc,
                         EasingType easeType) : this(obj, propName, 0.0, endValue, easeFunc, easeType)
        {
            _startValue = (double)_property.GetValue(obj, null);
        }

        public Animation(object obj, string propName, double startValue, double endValue, EaseFunctionDelegate easeFunc,
                         EasingType easeType)
        {
            _property = obj.GetType().GetProperty(propName);
            _obj = obj;
            _startValue = startValue;
            _endValue = endValue;
            _easeFunc = easeFunc;
            _easeType = easeType;
        }

        public void Update(double newProgress)
        {
            Type propertyType = _property.PropertyType;
            var easingProgress = _easeFunc(newProgress, _easeType);
            _property.SetValue(_obj,
                               Convert.ChangeType(_startValue + (easingProgress * (_endValue - _startValue)),
                                                  propertyType),
                               null);
        }
    }

    public class AnimationGroup
    {
        public List<Animation> Animations { get; set; }
        private double _progress;

        public bool Running;
        public double StartTime;
        public double Duration;

        public double Progress
        {
            get { return _progress; }
            set
            {
                _progress = value;

                if(_progress >= 1.0)
                {
                    Running = false;
                }

                UpdateAnimations();
            }
        }

        public AnimationGroup(double duration)
        {
            StartTime = 0;
            Animations = new List<Animation>();
            Duration = duration;
            Running = true;
        }

        public AnimationGroup(Animation animation, double duration) : this(duration)
        {
            Animations.Add(animation);
        }

        public void UpdateAnimations()
        {
            foreach(var animation in Animations)
            {
                animation.Update(_progress);
            }
        }
    }

    public class AnimationController : GameComponent
    {
        private readonly List<AnimationGroup> _animationGroups;

        public AnimationController(Game game) : base(game)
        {
            _animationGroups = new List<AnimationGroup>();
        }

        public void Add(Animation animation, double duration)
        {
            _animationGroups.Add(new AnimationGroup(animation, duration));
        }

        public void AddGroup(AnimationGroup animationGroup)
        {
            _animationGroups.Add(animationGroup);
        }

        public override void Update(GameTime gameTime)
        {
            for(int index = 0; index < _animationGroups.Count; ++index)
            {
                var animationGroup = _animationGroups[index];

                if(!animationGroup.Running)
                {
                    _animationGroups.Remove(animationGroup);
                    --index;
                }

                if(animationGroup.StartTime == 0)
                {
                    animationGroup.StartTime = gameTime.TotalGameTime.TotalMilliseconds;
                }
                else
                {
                    animationGroup.Progress =
                        (gameTime.TotalGameTime.TotalMilliseconds - animationGroup.StartTime) /
                        animationGroup.Duration;
                }
            }
        }
    }
}