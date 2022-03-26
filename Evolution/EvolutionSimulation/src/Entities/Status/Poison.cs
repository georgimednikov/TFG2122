using EvolutionSimulation.Genetics;
using System;

namespace EvolutionSimulation.Entities.Status
{
    /// <summary>
    /// This status deal damage per second to the owner
    /// </summary>
    public class Poison : Status
    {
        // Damage the poison will do per tick
        float damage;

        public Poison(int duration, float dmg) : base(duration)
        {
            damage = dmg;
        } 

        public override bool OnTick()
        {
            owner.stats.CurrHealth -= damage;
#if DEBUG
            Console.WriteLine("POSIONE DEALT " + damage.ToString() + " DMG TO " + owner.speciesName);
#endif
            return base.OnTick();
        }

#if DEBUG
        public override void OnApply()
        {
            Console.WriteLine(owner.speciesName + " POSIONED FOR " + damage.ToString() + " DMG/TICK");
        }

        public override void OnRemove()
        {
            Console.WriteLine("POISON REMOVED FROM " + owner.speciesName);
        }

        public override void OnExpire()
        {
            Console.WriteLine("POISON EXPIRED FROM " + owner.speciesName);
        }
#endif
    }
}
