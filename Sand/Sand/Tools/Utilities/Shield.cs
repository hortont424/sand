namespace Sand.Tools.Utilities
{
    public class Shield : Tool
    {
        public Shield(LocalPlayer player)
            : base(player)
        {
            Name = "Shield";
            Description = "Protect Yourself!";
            Icon = Storage.Sprite("Shield");
            Modifier = 0.5;

            Energy = TotalEnergy = 100;
            EnergyConsumptionMode = EnergyConsumptionMode.Custom;
            EnergyConsumptionRate = 50;
            EnergyRechargeRate = 0.2;
        }

        protected override void Activate()
        {
        }

        public float DeflectShock(float strength)
        {
            var oldEnergy = Energy;

            Energy -= EnergyConsumptionRate;

            if(Energy < 0.0f)
            {
                Energy = 0.0f;
            }

            if(strength < (oldEnergy - Energy))
            {
                Player.Stunned = false;
                return 0.0f;
            }
            else
            {
                Player.Stunned = true;
                return (float)(strength - (oldEnergy - Energy));
            }
        }
    }
}