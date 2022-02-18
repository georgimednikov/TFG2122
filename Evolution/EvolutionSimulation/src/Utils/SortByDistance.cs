using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionSimulation.Utils
{
    public class SortByDistance : Comparer<Entities.Creature>
    {
        public override int Compare(Entities.Creature c1, Entities.Creature c2)
        {
            int x, y;
            x = Math.Abs(c1.x - c2.x);
            y = Math.Abs(c1.y - c2.y);
            int dist = (int)Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));
            return dist;
        }
    }
}
