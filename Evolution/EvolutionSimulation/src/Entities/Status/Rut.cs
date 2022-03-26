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
#if DEBUG
            Console.WriteLine("On heat applied");
#endif
            owner.stats.InHeat = true;
        }

        public override void OnRemove()
        {
#if DEBUG
            Console.WriteLine("On heat removed");
#endif
            owner.stats.InHeat = false;
        }

        public override void OnExpire()
        {
#if DEBUG
            Console.WriteLine("On heat expired");
#endif
            owner.stats.InHeat = false;
        }
    }
}
