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
            modifier = 1.1f - (c.stats.Aggressiveness / c.chromosome.GetFeatureMax(Genetics.CreatureFeature.Aggressiveness)) * UniverseParametersManager.parameters.chaseCostMultiplier; // TODO: Modificador que dependa bien, ahora mismo a mas agresividad mejor persigue
        }

        public override int GetCost()
        {
            return (int)(creature.GetNextCostOnPath() * modifier);
        }

        public override void OnEntry()
        {
            Entities.Creature objective = creature.GetClosestCreatureReachable();
            objX = objective.x;
            objY = objective.y; 
            creature.SetPath(objX, objY);   // This MUST be set up for the cost of the action to work
        }

        public override void Action()
        {
            Vector3 nextPos = creature.GetNextPosOnPath();  // Follow the specified path
            creature.Place((int)nextPos.X, (int)nextPos.Y, (Entities.Creature.HeightLayer)nextPos.Z);

            Entities.Creature objective = creature.GetClosestCreatureReachable();   // This is NOT cached because objective can change to another creature
            if (objX != objective.x ||  // If objective is somewhere else,
                objY != objective.y)    // adjust path accordingly
            {
                objX = objective.x;
                objY = objective.y;
                creature.SetPath(objX, objY);   // Set the path the creature must follow
            }
            // All of this is done AFTER the action due to the fact that GetCost reflects the cost of the older path
            // Were it to be changed BEFORE, the new position's cost may not be the same as the one returned before
        }

        public override string ToString()
        {
            return "ChaseEnemyState";
        }
    }
}
