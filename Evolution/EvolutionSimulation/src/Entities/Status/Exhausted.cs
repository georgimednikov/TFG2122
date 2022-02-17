using System;

namespace EvolutionSimulation.Entities.Status
{
    /// <summary>
    /// This status debuff some stats of the owner. It ends when the creature rest over a threshold
    /// </summary>
    public class Exhausted : Status
    {
        float exhaustedThreshHold;
        float debuffMultiplier;

        public Exhausted(float exhaustedThreshHold, float debuffMultiplier, int duration = -1) : base(duration)
        {
            this.exhaustedThreshHold = exhaustedThreshHold;
            this.debuffMultiplier = debuffMultiplier;
        }

        public override bool OnTick()
        {
            return base.OnTick() || owner.stats.CurrRest > exhaustedThreshHold * owner.stats.MaxRest;
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
            Console.WriteLine("Exhausted removed");
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
