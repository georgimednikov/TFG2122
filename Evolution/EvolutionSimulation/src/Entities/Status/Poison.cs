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
            Console.WriteLine("Poison dealt " + damage.ToString() + " damage to " + owner.speciesName + "(" + owner.x + "," + owner.y + ")");
            return base.OnTick();
        }

        public override void OnApply()
        {
            Console.WriteLine("Poison applied for " + damage.ToString() + " damage/tick to " + owner.speciesName + "(" + owner.x + "," + owner.y + ")");
        }

        public override void OnRemove()
        {
            Console.WriteLine("Poison removed from " + owner.speciesName + "(" + owner.x + "," + owner.y + ")");
        }

        public override void OnExpire()
        {
            Console.WriteLine("Poison expired from " + owner.speciesName + "(" + owner.x + "," + owner.y + ")");
        }
    }
}
