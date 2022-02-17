using EvolutionSimulation.Genetics;
using System;

namespace EvolutionSimulation.Entities.Status
{
    /// <summary>
    /// Interface detailing methods a status effect must have
    /// </summary>
    public class Poison : Status
    {
        // Damage the poison will do per tick
        int damage;

        public Poison(int d, int dmg) : base(d)
        {
            damage = dmg;
        } 

        public override bool OnTick()
        {
            owner.stats.CurrHealth -= damage;
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
