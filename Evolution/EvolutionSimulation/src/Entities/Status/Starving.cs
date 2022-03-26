using System;

namespace EvolutionSimulation.Entities.Status
{
    /// <summary>
    /// This status penalty some stats of the owner. It ends when the creature´s energy is over a threshold
    /// </summary>
    public class Starving : Status
    {
        float starvingThreshHold;
        float penaltyMultiplier;

        public Starving(float starvingThreshHold, float penaltyMultiplier, int duration = -1) : base(duration)
        {
            this.starvingThreshHold = starvingThreshHold;
            this.penaltyMultiplier = penaltyMultiplier;
        }

        public override bool OnTick()
        {
            return base.OnTick() || owner.stats.CurrEnergy > starvingThreshHold * owner.stats.MaxEnergy;
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
#if DEBUG
            Console.WriteLine("Starving removed");
#endif
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
