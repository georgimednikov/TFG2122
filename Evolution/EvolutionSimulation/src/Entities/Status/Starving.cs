using System;

namespace EvolutionSimulation.Entities.Status
{
    /// <summary>
    /// This status debuff some stats of the owner. It ends when the creature´s energy is over a threshold
    /// </summary>
    public class Starving : Status
    {
        float starvingThreshHold;
        float debuffMultiplier;

        public Starving(float starvingThreshHold, float debuffMultiplier, int duration = -1) : base(duration)
        {
            this.starvingThreshHold = starvingThreshHold;
            this.debuffMultiplier = debuffMultiplier;
        }

        public override bool OnTick()
        {
            return base.OnTick() || owner.stats.CurrEnergy > starvingThreshHold * owner.stats.MaxEnergy;
        }

        public override void OnApply()
        {
            owner.stats.Perception = (int)(owner.stats.Perception * debuffMultiplier);
            owner.stats.GroundSpeed = (int)(owner.stats.GroundSpeed * debuffMultiplier);
            owner.stats.ArborealSpeed = (int)(owner.stats.ArborealSpeed * debuffMultiplier);
            owner.stats.AerialSpeed = (int)(owner.stats.AerialSpeed * debuffMultiplier);
            owner.stats.Intimidation = (int)(owner.stats.Intimidation * debuffMultiplier);
        }

        public override void OnRemove()
        {
            Console.WriteLine("Starving removed");
        }

        public override void OnExpire()
        {
            owner.stats.Perception = (int)(owner.stats.Perception / debuffMultiplier);
            owner.stats.GroundSpeed = (int)(owner.stats.GroundSpeed / debuffMultiplier);
            owner.stats.ArborealSpeed = (int)(owner.stats.ArborealSpeed / debuffMultiplier);
            owner.stats.AerialSpeed = (int)(owner.stats.AerialSpeed / debuffMultiplier);
            owner.stats.Intimidation = (int)(owner.stats.Intimidation * debuffMultiplier);
        }
    }
}
