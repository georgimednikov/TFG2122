using System;
using System.Numerics;

namespace EvolutionSimulation.FSM.Creature.States
{
    class ChaseEnemy : CreatureState
    {
        // How costly it is to move compared to regular safe movement
        private float modifier;
        // Position of the objective
        int objX, objY;

        public ChaseEnemy(Entities.Creature c) : base(c) 
        { 
            creature = c;
            modifier = 1.1f - (c.stats.Aggressiveness / 20f) * 0.4f; // TODO: Modificador que dependa bien, ahora mismo a mas agresividad mejor persigue
        }

        public override int GetCost()
        {
            return (int)(creature.GetNextCostOnPath() * modifier);
        }

        public override void OnEntry()
        {
            objX = -1;
            objY = -1;  // This will trigger a path set on its next action
        }

        public override void Action()
        {
            Entities.Creature objective = creature.GetClosestCreatureReachable();   // This is NOT cached because objective can change to another creature
            if (objX != objective.x ||  // If objective is somewhere else,
                objY != objective.y)    // adjust path accordingly
            {
                objX = objective.x;
                objY = objective.y;
                creature.SetPath(objX, objY);   // Set the path the creature must follow
            }

            Vector3 nextPos = creature.GetNextPosOnPath();  // Follow the specified path
            creature.Place((int)nextPos.X, (int)nextPos.Y, (Entities.Creature.HeightLayer)nextPos.Z);
        }

        public override string ToString()
        {
            return "ChaseEnemyState";
        }
    }
}
