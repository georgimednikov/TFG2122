using System.Collections.Generic;

namespace EvolutionSimulation.Comparers
{
    public class SortByMetabolism : Comparer<Entities.Creature>
    {
        public override int Compare(Entities.Creature x, Entities.Creature y)
        {
            return -x.stats.Metabolism.CompareTo(y.stats.Metabolism);
        }
    }
}
