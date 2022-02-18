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
        int damage;

        public Poison(int duration, int dmg) : base(duration)
        {
            damage = dmg;
        } 

        public override bool OnTick()
        {
            owner.stats.CurrHealth -= damage;
            Console.WriteLine("Posion dealt " + damage + " damage, " + owner.stats.CurrHealth + " hp remaining");
            return base.OnTick();   // TODO: Había alguna manera de que esto lo hiciera automáticamente pero no me acuerdo y me tengo que duchar en 5 minutos
        }

        public override void OnApply()
        {
            Console.WriteLine("Posion applied for " + damage + " damage per turn.");
        }

        public override void OnRemove()
        {
            Console.WriteLine("Posion removed.");
        }

        public override void OnExpire()
        {
            Console.WriteLine("Posion expired.");
        }
    }
}
