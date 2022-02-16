using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionSimulation
{
    public class EdiblePlant : Plant
    {
        public bool eaten { get; protected set; } = false;
        protected int regrowhtTime;
        protected int startRegrowthTime = 0;
        protected float nutritionalValue;

        override public void Tick()
        {
            if (eaten)
                eaten = startRegrowthTime >= regrowhtTime;
        }

        public float Eat()
        {
            startRegrowthTime = 0;
            eaten = false;
            return nutritionalValue;
        }
    }
}
