using EvolutionSimulation.Genetics;
using System;

namespace EvolutionSimulation.Entities.Status
{
    /// <summary>
    /// This status makes the owner be in heat
    /// </summary>
    public class Rut : Status
    {
        public Rut(int duration) : base(duration)
        {
        } 

        public override void OnApply()
        {
            Console.WriteLine("On heat applied");
            owner.stats.InHeat = true;
        }

        public override void OnRemove()
        {
            Console.WriteLine("On heat removed");
            owner.stats.InHeat = false;
        }

        public override void OnExpire()
        {
            Console.WriteLine("On heat expired");
            owner.stats.InHeat = false;
        }
    }
}
