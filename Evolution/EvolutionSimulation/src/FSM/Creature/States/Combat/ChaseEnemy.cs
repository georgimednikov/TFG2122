using System;
using System.Numerics;

namespace EvolutionSimulation.FSM.Creature.States
{
    class ChaseEnemy : CreatureState
    {
        // How costly it is to move compared to regular safe movement
        private float modifier;
        // Position and ID of the objective
        int enemyID;
        Vector2Int objective;
        string speciesName;

        // In case the chase ends and it does not end up attacking
        bool brake = false;

        public ChaseEnemy(Entities.Creature c) : base(c) 
        { 
            creature = c;
            modifier = 1.1f - // TODO: Modificador que dependa bien, ahora mismo a mas agresividad mejor persigue
                (c.stats.Aggressiveness / c.chromosome.GetFeatureMax(Genetics.CreatureFeature.Aggressiveness) * 0.4f);  // TODO: PASAR A UNIVERSAL
        }

        public override int GetCost()
        {
            int cost = creature.GetNextCostOnPath();
            if (cost < 0) {
                brake = true;
                return UniverseParametersManager.parameters.baseActionCost;
            } else return (int)(creature.GetNextCostOnPath() * modifier);
        }

        public override void OnEntry()
        {
            creature.Enemy(out enemyID, out objective);
            speciesName = creature.world.GetCreature(enemyID).speciesName;
            creature.SetPath(objective);   // This MUST be set up for the cost of the action to work
        }

        public override void Action()
        {
            if (!brake) {
                Vector3 nextPos = creature.GetNextPosOnPath();  // Follow the specified path
                creature.Place((int)nextPos.X, (int)nextPos.Y, (Entities.Creature.HeightLayer)nextPos.Z);
            }

            int otherID; Vector2Int obj;
            creature.Enemy(out otherID, out obj);   // This is NOT cached because objective can change to another creature
            if (otherID != enemyID)  // If objective is somewhere else, adjust path accordingly
            {
                enemyID = otherID;
                speciesName = creature.world.GetCreature(enemyID).speciesName;
                objective = obj;
                creature.SetPath(objective);   // Set the path the creature must follow
            }
            // All of this is done AFTER the action due to the fact that GetCost reflects the cost of the older path
            // Were it to be changed BEFORE, the new position's cost may not be the same as the one returned before
            brake = false;
        }

        //// No longer cornered, as combat is done
        //public override void OnExit()
        //{
        //    creature.cornered = false;
        //}

        public override string GetInfo()
        {
            return creature.speciesName + " with ID: " + creature.ID + " CHASES " + speciesName + " with ID: " + enemyID;
        }

        public override string ToString()
        {
            return "ChaseEnemyState";
        }
    }
}
