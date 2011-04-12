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

    public abstract class Tool
    {
        public double Modifier { get; set; }
        public LocalPlayer Player { get; protected set; }

        public string Name
        {
            get
            {
                return this.GetType().GetMethod("_name").Invoke(null, null) as string;
            }
        }

        public string Description
        {
            get
            {
                return this.GetType().GetMethod("_description").Invoke(null, null) as string;
            }
        }

        public Texture2D Icon
        {
            get
            {
                return this.GetType().GetMethod("_icon").Invoke(null, null) as Texture2D;
            }
        }

        public Keys Key
        {
            get
            {
                return (Keys)this.GetType().GetMethod("_key").Invoke(null, null);
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
        private bool _inCooldown;

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

                            Active = false;

                            _inCooldown = true;
                            _cooldownTime =
                                new TimeSpan(Storage.CurrentTime.TotalGameTime.Ticks).Add(new TimeSpan(0, 0, 2));
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
    }
}