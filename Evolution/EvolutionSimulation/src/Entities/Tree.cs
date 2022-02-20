using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionSimulation
{
    public class Tree : Plant
    {
        public float movementPenalty { get; private set; }

        public Tree()
        {
            movementPenalty = RandomGenerator.Next(50, 100);
        }
        public Tree(float movementPenalty)
        {
            this.movementPenalty = movementPenalty;
        }

        public new void Tick()
        {
        }
    }
}
