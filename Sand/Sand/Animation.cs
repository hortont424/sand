using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;

namespace Sand
{
    public class Animation
    {
        private readonly PropertyInfo _property;
        private readonly object _obj;
        private readonly float _startValue, _endValue;
        private readonly EaseFunctionDelegate _easeFunc;
        private readonly EasingType _easeType;

        public bool Reverse;
        public string PropertyName;

        public AnimationCompletedDelegate CompletedDelegate { get; set; }

        public delegate float EaseFunctionDelegate(double linearStep, EasingType type);

        public delegate void AnimationCompletedDelegate();

        public Animation()
        {
            _obj = null;
        }

        public Animation(object obj, string propName, float startValue, float endValue)
            : this(obj, propName, startValue, endValue, Easing.EaseInOut, EasingType.Sine)
        {
        }

        public Animation(object obj, string propName, float endValue)
            : this(obj, propName, 0.0f, endValue, Easing.EaseInOut, EasingType.Sine)
        {
            _startValue = Convert.ToSingle(_property.GetValue(obj, null));
        }

        public Animation(object obj, string propName, float endValue, EaseFunctionDelegate easeFunc,
                         EasingType easeType) : this(obj, propName, 0.0f, endValue, easeFunc, easeType)
        {
            _startValue = Convert.ToSingle(_property.GetValue(obj, null));
        }

        public Animation(object obj, string propName, float startValue, float endValue, EaseFunctionDelegate easeFunc,
                         EasingType easeType)
        {
            if(propName != null)
            {
                _property = obj.GetType().GetProperty(propName);
            }
            else
            {
                _property = null;
            }

            PropertyName = propName;
            _obj = obj;
            _startValue = startValue;
            _endValue = endValue;
            _easeFunc = easeFunc;
            _easeType = easeType;
        }

        public void Update(double newProgress)
        {
            if(_obj != null)
            {
                float fromValue, toValue;

                if(!Reverse)
                {
                    fromValue = _startValue;
                    toValue = _endValue;
                }
                else
                {
                    fromValue = _endValue;
                    toValue = _startValue;
                }

                Type propertyType = _property.PropertyType;
                var easingProgress = _easeFunc(newProgress, _easeType);

                if(_property != null)
                {
                    _property.SetValue(_obj,
                                       Convert.ChangeType(fromValue + (easingProgress * (toValue - fromValue)),
                                                          propertyType),
                                       null);
                }
            }
        }
    }

    public class AnimationGroup
    {
        public List<Animation> Animations { get; set; }
        private double _progress;

        public bool Running;
        public bool Paused;
        public double StartTime;
        public double Duration;
        public bool Loops;
        public double LastUpdate;

        public double Progress
        {
            get
            {
                return _progress;
            }
            set
            {
                _progress = value;

                if(_progress >= 1.0)
                {
                    foreach(var animation in Animations.Where(animation => animation.CompletedDelegate != null))
                    {
                        animation.CompletedDelegate();
                    }

                    if(Loops)
                    {
                        _progress = 0.0;
                        StartTime = Storage.CurrentTime.TotalGameTime.TotalMilliseconds;

                        foreach(var animation in Animations)
                        {
                            animation.Reverse = !animation.Reverse;
                        }
                    }
                    else
                    {
                        Running = false;
                    }
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

        public void Remove(Animation animation)
        {
            var removePairs = new List<Tuple<AnimationGroup, Animation>>();

            foreach(AnimationGroup group in _animationGroups)
            {
                foreach(Animation anim in group.Animations)
                {
                    if(anim == animation)
                    {
                        removePairs.Add(new Tuple<AnimationGroup, Animation>(group, anim));
                    }
                }
            }

            foreach(Tuple<AnimationGroup, Animation> removePair in removePairs)
            {
                removePair.Item1.Animations.Remove(removePair.Item2);

                if(removePair.Item1.Animations.Count == 0)
                {
                    RemoveGroup(removePair.Item1);
                }
            }
        }

        public void AddGroup(AnimationGroup animationGroup)
        {
            _animationGroups.Add(animationGroup);
        }

        public void RemoveGroup(AnimationGroup animationGroup)
        {
            _animationGroups.Remove(animationGroup);
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
                    if(animationGroup.Paused)
                    {
                        animationGroup.StartTime += gameTime.TotalGameTime.TotalMilliseconds - animationGroup.LastUpdate;
                    }
                    else
                    {
                        animationGroup.Progress =
                            (gameTime.TotalGameTime.TotalMilliseconds - animationGroup.StartTime) /
                            animationGroup.Duration;
                    }

                    animationGroup.LastUpdate = gameTime.TotalGameTime.TotalMilliseconds;
                }
            }
        }
    }
}