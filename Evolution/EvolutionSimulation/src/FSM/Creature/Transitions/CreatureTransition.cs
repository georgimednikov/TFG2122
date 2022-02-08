using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionSimulation.FSM.Creature.Transitions
{
    /// <summary>
    /// Abstract class to make transitios with a reference to 'la creatura'
    /// </summary>
    public abstract class CreatureTransition
    {
        protected EvolutionSimulation.Creature creature;
    }
}
