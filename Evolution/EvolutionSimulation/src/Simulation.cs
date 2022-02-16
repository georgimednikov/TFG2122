using System;
using System.Collections.Generic;
using EvolutionSimulation.Entities;

namespace EvolutionSimulation
{
    /// <summary>
    /// Simulation of evolution
    /// </summary>
    public class Simulation : ISimulation
    {
        public void Init()
        {
            world = new World();
            world.Init(32);
        }

        public void Run()
        {
            while (true)
            {
                world.Tick();
                //entities.ForEach(delegate (IEntity e) { e.Tick(); });   // Orders the entity to perform a step
                //creatures.Sort(new SortByMetabolism());
                //
                //delete.ForEach(delegate (IEntity e) { entities.Remove(e); });
                //
                //delete.Clear();
            }
        }
        World world;
    }

    // TODO: esto moverlo a otro sitio
    public class SortByMetabolism : Comparer<Creature>
    {
        public override int Compare(Creature x, Creature y)
        {
            return -x.stats.Metabolism.CompareTo(y.stats.Metabolism);
        }
    }
}
