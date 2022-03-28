using System;
using System.Numerics;

namespace EvolutionSimulation.FSM.Creature.States
{
    class ChaseEnemy : CreatureState
    {
        // How costly it is to move compared to regular safe movement
        private float modifier;
        // Position and ID of the objective
        int objectiveID;
        Vector2Int objective;
        string speciesName;

        // In case the chase ends and it does not end up attacking
        bool brake = false; // TODO: esto en teoria no deberia pasar nunca ?

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
            creature.Enemy(out objectiveID, out objective);
            speciesName = creature.world.GetCreature(objectiveID).speciesName;
            if(objective.x != creature.x || objective.y != creature.y)
                creature.SetPath(objective);   // This MUST be set up for the cost of the action to work
            else
                brake = true;
        }

        public override void Action()
        {
            if (!brake) {
                Vector3 nextPos = creature.GetNextPosOnPath();  // Follow the specified path
                creature.Place((int)nextPos.X, (int)nextPos.Y, (Entities.Creature.HeightLayer)nextPos.Z);
            }

            int otherID; Vector2Int obj;
            creature.Enemy(out otherID, out obj);   // This is NOT cached because objective can change to another creature

            if(otherID == -1)   // Target has died! This entails looking for another ONLY if there is a menace, or none if there is not
            {                   // This will only take place when the creature still feels threatened and is cornered, because it won't be able to flee
                creature.Menace(out otherID, out obj);  
                if(Math.Abs(obj.x - creature.x) >= creature.stats.Perception / 2 || Math.Abs(obj.y - creature.y) >= creature.stats.Perception / 2)
                {   // Creature is sufficiently far away to not pose immediate danger
                    creature.cornered = false;
                    brake = false;
                    return;
                }
            }

            if (otherID != objectiveID)  // If objective is a different one, adjust accordingly
            {
                objectiveID = otherID;
                speciesName = creature.world.GetCreature(objectiveID).speciesName;
                objective = obj;
                creature.TargetEnemy(otherID);  // Redundant, except when current target dies
                creature.SetPath(objective);    // Set the path the creature must follow
            }
            if (objective != obj && obj.x != creature.x && obj.y != creature.y)
            {
                objective = obj;
                creature.SetPath(objective);    // Set the path the creature must follow
            }
            brake = false;
        }

        // No longer cornered, as combat is done
        public override void OnExit()
        {
            creature.cornered = false;
        }

        public override string GetInfo()
        {
            return creature.speciesName + " with ID: " + creature.ID + " CHASES " + speciesName + " with ID: " + objectiveID;
        }

        public override string ToString()
        {
            return "ChaseEnemyState";
        }
    }
}
