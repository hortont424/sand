using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Sand.Tools
{
    public enum EnergyConsumptionMode
    {
        Drain,
        Instant,
        Custom
    } ;

    public enum MouseButton
    {
        None,
        LeftButton,
        RightButton
    } ;

    public abstract class Tool
    {
        public double Modifier { get; set; }
        public LocalPlayer Player { get; protected set; }

        public string Name
        {
            get
            {
                return GetType().GetMethod("_name").Invoke(null, null) as string;
            }
        }

        public string Description
        {
            get
            {
                return GetType().GetMethod("_description").Invoke(null, null) as string;
            }
        }

        public Texture2D Icon
        {
            get
            {
                return GetType().GetMethod("_icon").Invoke(null, null) as Texture2D;
            }
        }

        public Keys Key
        {
            get
            {
                var method = GetType().GetMethod("_key");

                if(method != null)
                {
                    return (Keys)method.Invoke(null, null);
                }
                else
                {
                    return Keys.None;
                }
            }
        }

        public MouseButton Button
        {
            get
            {
                var method = GetType().GetMethod("_button");

                if(method != null)
                {
                    return (MouseButton)method.Invoke(null, null);
                }
                else
                {
                    return MouseButton.None;
                }
            }
        }

        private bool _active;

        public bool Active
        {
            get
            {
                return _active;
            }
            set
            {
                var oldValue = _active;

                _active = value;

                if(oldValue != _active)
                {
                    if(_active)
                    {
                        if(Energy > EnergyConsumptionRate)
                        {
                            Activate();

                            if(EnergyConsumptionMode == EnergyConsumptionMode.Instant)
                            {
                                Energy -= EnergyConsumptionRate;
                            }
                        }
                        else
                        {
                            Active = false;
                        }

                        if(EnergyConsumptionMode == EnergyConsumptionMode.Instant)
                        {
                            Active = false;
                        }
                    }
                    else
                    {
                        Deactivate();
                    }
                }
            }
        }

        public double Energy { get; protected set; }
        public double TotalEnergy { get; protected set; }
        public EnergyConsumptionMode EnergyConsumptionMode { get; protected set; }
        public double EnergyConsumptionRate { get; protected set; }
        public double EnergyRechargeRate { get; protected set; }

        private readonly Animation _energyAnimation;
        private readonly AnimationGroup _energyAnimationGroup;
        private TimeSpan _cooldownTime;
        protected bool _inCooldown;

        protected Tool(LocalPlayer player)
        {
            Player = player;

            _energyAnimation = new Animation { CompletedDelegate = EnergyTick };

            _energyAnimationGroup = new AnimationGroup(_energyAnimation, 10) { Loops = true };

            Storage.AnimationController.AddGroup(_energyAnimationGroup);
        }

        private void EnergyTick()
        {
            switch(EnergyConsumptionMode)
            {
                case EnergyConsumptionMode.Drain:
                    if(Active)
                    {
                        if(Energy == 0.0f)
                        {
                            return;
                        }

                        Energy -= EnergyConsumptionRate;

                        if(Energy <= 0.0f)
                        {
                            Energy = 0.0f;

                            Storage.Sound("Drained").CreateInstance().Play();

                            _inCooldown = true;
                            _cooldownTime =
                                new TimeSpan(Storage.CurrentTime.TotalGameTime.Ticks).Add(new TimeSpan(0, 0, 2));

                            Active = false;
                        }
                    }
                    else
                    {
                        RechargeTick();
                    }

                    break;
                case EnergyConsumptionMode.Instant:
                case EnergyConsumptionMode.Custom:
                    RechargeTick();

                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void RechargeTick()
        {
            if(_inCooldown)
            {
                if(Storage.CurrentTime.TotalGameTime.Ticks > _cooldownTime.Ticks)
                {
                    _inCooldown = false;
                }
            }
            else
            {
                Energy += EnergyRechargeRate;

                if(Energy > TotalEnergy)
                {
                    Energy = TotalEnergy;
                }
            }
        }

        protected virtual void Activate()
        {
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
        }

        protected virtual void Deactivate()
        {
        }

        public void Reset()
        {
            Energy = TotalEnergy;
            _inCooldown = false;
        }

        private bool MouseButtonState(MouseState state, MouseButton button)
        {
            switch(button)
            {
                case MouseButton.None:
                    return false;
                case MouseButton.LeftButton:
                    return state.LeftButton == ButtonState.Pressed;
                case MouseButton.RightButton:
                    return state.RightButton == ButtonState.Pressed;
                default:
                    throw new ArgumentOutOfRangeException("button");
            }
        }

        public bool ShouldDisable(KeyboardState newKeyState, MouseState newMouseState, KeyboardState oldKeyState,
                                  MouseState oldMouseState)
        {
            if(Key != Keys.None)
            {
                if(newKeyState.IsKeyUp(Key) && oldKeyState.IsKeyDown(Key))
                {
                    return true;
                }
            }

            if(Button != MouseButton.None)
            {
                if(!MouseButtonState(newMouseState, Button) && MouseButtonState(oldMouseState, Button))
                {
                    return true;
                }
            }

            return false;
        }

        public bool ShouldEnable(KeyboardState newKeyState, MouseState newMouseState, KeyboardState oldKeyState,
                                 MouseState oldMouseState)
        {
            if(Key != Keys.None)
            {
                if(oldKeyState.IsKeyUp(Key) && newKeyState.IsKeyDown(Key))
                {
                    return true;
                }
            }

            if(Button != MouseButton.None)
            {
                if(!MouseButtonState(oldMouseState, Button) && MouseButtonState(newMouseState, Button))
                {
                    return true;
                }
            }

            return false;
        }
    }
}