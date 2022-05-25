using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionSimulation.FSM.Creature.States
{
    /// <summary>
    /// State the creature will use in its FSM
    /// </summary>
    class CreatureState : IState
    {
        protected Entities.Creature creature;

        public CreatureState(Entities.Creature c) { creature = c; }

        public virtual int GetCost() { return 0; }

        public virtual void Action() {}

        public virtual void OnEntry() 
        {
#if TRACKER_ENABLED
            Telemetry.Tracker.Instance.Track(new Telemetry.Events.CreatureStateEntry(creature.world.CurrentTick, creature.ID, creature.speciesName, ToString(), creature.x, creature.y));
#endif
        }

        public virtual void OnExit() {}

        public virtual string GetInfo() { return ""; }
    }
}
