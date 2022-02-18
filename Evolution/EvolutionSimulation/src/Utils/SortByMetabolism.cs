using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionSimulation.Utils
{
    public class SortByMetabolism : Comparer<Entities.Creature>
    {
        public override int Compare(Entities.Creature x, Entities.Creature y)
        {
            return -x.stats.Metabolism.CompareTo(y.stats.Metabolism);
        }
    }
}
