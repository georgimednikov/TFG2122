using System;
using System.Collections.Generic;

namespace EvolutionSimulation.Comparers
{
    public class SortByDistance : Comparer<Entities.Creature>
    {
        Entities.Creature origin;
        public SortByDistance(Entities.Creature creature)
        {
            origin = creature;
        }

        public override int Compare(Entities.Creature c1, Entities.Creature c2)
        {
            int x1, y1, x2, y2;
            x1 = Math.Abs(origin.x - c1.x);
            y1 = Math.Abs(origin.y - c1.y);
            x2 = Math.Abs(origin.x - c2.x);
            y2 = Math.Abs(origin.y - c2.y);
            int dist1 = (int)Math.Sqrt(Math.Pow(x1, 2) + Math.Pow(y1, 2)),
                dist2 = (int)Math.Sqrt(Math.Pow(x2, 2) + Math.Pow(y2, 2));
            return Math.Abs(dist1 - dist2);
        }
    }
}
