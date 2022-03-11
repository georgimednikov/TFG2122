using System;

namespace EvolutionSimulation.Entities.Status
{
    /// <summary>
    /// This status penalty some stats of the owner. It ends when the creature rest over a threshold
    /// </summary>
    public class Exhausted : Status
    {
        float exhaustedThreshHold;
        float penaltyMultiplier;

        public Exhausted(float exhaustedThreshHold, float penaltyMultiplier, int duration = -1) : base(duration)
        {
            this.exhaustedThreshHold = exhaustedThreshHold;
            this.penaltyMultiplier = penaltyMultiplier;
        }

        public override bool OnTick()
        {
            return base.OnTick() || owner.stats.CurrRest > exhaustedThreshHold * owner.stats.MaxRest;
        }


        public override void OnApply()
        {
            owner.stats.Perception = (int)(owner.stats.Perception * penaltyMultiplier);
            owner.stats.GroundSpeed = (int)(owner.stats.GroundSpeed * penaltyMultiplier);
            owner.stats.ArborealSpeed = (int)(owner.stats.ArborealSpeed * penaltyMultiplier);
            owner.stats.AerialSpeed = (int)(owner.stats.AerialSpeed * penaltyMultiplier);
            owner.stats.Intimidation = (int)(owner.stats.Intimidation * penaltyMultiplier);
        }

        public override void OnRemove()
        {
            Console.WriteLine("Exhausted removed");
        }

        public override void OnExpire()
        {
            owner.stats.Perception = (int)(owner.stats.Perception / penaltyMultiplier);
            owner.stats.GroundSpeed = (int)(owner.stats.GroundSpeed / penaltyMultiplier);
            owner.stats.ArborealSpeed = (int)(owner.stats.ArborealSpeed / penaltyMultiplier);
            owner.stats.AerialSpeed = (int)(owner.stats.AerialSpeed / penaltyMultiplier);
            owner.stats.Intimidation = (int)(owner.stats.Intimidation * penaltyMultiplier);
        }
    }
}
