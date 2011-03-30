using Microsoft.Xna.Framework.Graphics;

namespace Sand.Tools
{
    public enum EnergyConsumptionMode
    {
        Drain,
        Instant
    } ;

    internal abstract class Tool
    {
        public string Name { get; protected set; }
        public string Description { get; protected set; }
        public Texture2D Icon { get; protected set; }
        public double Modifier { get; set; }

        private bool _active;

        public bool Active
        {
            get { return _active; }
            set
            {
                var oldValue = _active;

                _active = value;

                if(oldValue != _active)
                {
                    if(_active)
                    {
                        Activate();
                    }
                    else
                    {
                        Deactivate();
                    }
                }
            }
        }

        public double TotalEnergy { get; protected set; }
        public EnergyConsumptionMode EnergyConsumptionMode { get; protected set; }
        public double EnergyConsumptionRate { get; protected set; }

        protected abstract void Activate();
        protected abstract void Deactivate();
    }
}