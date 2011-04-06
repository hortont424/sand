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

    public class Tool
    {
        public string Name { get; protected set; }
        public string Description { get; protected set; }
        public Texture2D Icon { get; protected set; }
        public double Modifier { get; set; }
        public Keys Key { get; protected set; }
        public LocalPlayer Player { get; protected set; }

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

        protected Tool(LocalPlayer player)
        {
            Player = player;

            _energyAnimation = new Animation { CompletedDelegate = EnergyTick };

            _energyAnimationGroup = new AnimationGroup(_energyAnimation, 100) { Loops = true };

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

                    break;
                case EnergyConsumptionMode.Instant:
                case EnergyConsumptionMode.Custom:
                    Energy += EnergyRechargeRate;

                    if(Energy > TotalEnergy)
                    {
                        Energy = TotalEnergy;
                    }

                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        protected virtual void Activate()
        {
        }

        protected virtual void Deactivate()
        {
        }
    }
}