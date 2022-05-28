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
        Vector3Int objective;
        string objSpecies;

        // In case the chase ends and it does not/cannot attack
        bool await = false;

        public ChaseEnemy(Entities.Creature c) : base(c) 
        { 
            creature = c;
            modifier = 1.1f -
                (c.stats.Aggressiveness / c.chromosome.GetFeatureMax(Genetics.CreatureFeature.Aggressiveness) * UniverseParametersManager.parameters.chaseCostMultiplier);
        }

        public override int GetCost()
        {
            int cost = creature.GetNextCostOnPath();
            if (cost <= 0)
            {
                await = true;
                return UniverseParametersManager.parameters.baseActionCost;
            }
            else
            {
                await = false;
                return (int)(creature.GetNextCostOnPath() * modifier);
            }
        }

        public override void OnEntry()
        {
            creature.Enemy(out objectiveID, out objective);
            Entities.Creature objCreature = creature.world.GetCreature(objectiveID);

#if TRACKER_ENABLED
            Telemetry.Tracker.Instance.Track(new Telemetry.Events.CreatureStateEntryNotSafe(creature.world.CurrentTick, creature.ID, creature.speciesName, ToString(), objectiveID, creature.x, creature.y, objCreature == null ? " " : objCreature.speciesName));
#endif
            Entities.Creature tmp = creature.world.GetCreature(objectiveID);
            objSpecies = tmp == null ? " " : creature.world.GetCreature(objectiveID).speciesName;
            if (objective.x != creature.x && objective.y != creature.y)
            {
                if (creature.CanReach((Entities.Creature.HeightLayer)objective.z))
                {
                    if (!creature.CanGoToLayer((Entities.Creature.HeightLayer)objective.z))
                    {
                        objective.z = (int)Entities.Creature.HeightLayer.Ground;
                    }
                    if (creature.world.CanMove(objective.x, objective.y, (Entities.Creature.HeightLayer)objective.z))
                        creature.SetPath(objective);    // Set the path the creature must follow if he can reach it
                    else await = true;
                }
                else
                {
                    objective.z = (int)Entities.Creature.HeightLayer.Ground;
                    if (creature.world.CanMove(objective.x, objective.y, (Entities.Creature.HeightLayer)objective.z))
                        creature.SetPath(objective);    // Set the path the creature must follow if he can reach it
                    else await = true;
                }
            }
            else
            {
                await = true;
            }
        }

        public override void Action()
        {
            if (!await) {
                Vector3 nextPos = creature.GetNextPosOnPath();  // Follow the specified path
                creature.Place((int)nextPos.X, (int)nextPos.Y, (Entities.Creature.HeightLayer)nextPos.Z);
            }

            int otherID; Vector3Int otherObj;
            creature.Enemy(out otherID, out otherObj);   // This is NOT cached because objective can change to another creature

            if(otherID == -1)   // Target has died! This entails looking for another ONLY if there is a menace, or none if there is not
            {                   // This will only take place when the creature still feels threatened and is cornered, because it won't be able to flee
                creature.Menace(out otherID, out otherObj);  
                if(Math.Abs(otherObj.x - creature.x) >= creature.stats.Perception / 2 || Math.Abs(otherObj.y - creature.y) >= creature.stats.Perception / 2)
                {   // Creature is sufficiently far away to not pose immediate danger
                    creature.cornered = false;
                    await = false;
                    return;
                }
            }

            if(otherID != objectiveID)  // If objective creature is a different one, adjust accordingly
            {
                objectiveID = otherID;
                Entities.Creature tmp = creature.world.GetCreature(objectiveID);
                objSpecies = tmp == null ? " " : creature.world.GetCreature(objectiveID).speciesName;
                creature.TargetEnemy(otherID, otherObj);  // Redundant, except when current target is the menace
            }

            if ((otherObj.x != objective.x || otherObj.y != objective.y || otherObj.z != objective.z) && (otherObj.x != creature.x || otherObj.y != creature.y))  // If objective position is a different one, adjust accordingly
            {
                objective.x = otherObj.x;
                objective.y = otherObj.y;
                objective.z = otherObj.z;
                if (creature.CanReach((Entities.Creature.HeightLayer)objective.z))
                {
                    if (!creature.CanGoToLayer((Entities.Creature.HeightLayer)objective.z))
                    {
                        objective.z = (int)Entities.Creature.HeightLayer.Ground;
                    }
                    if(creature.world.CanMove(objective.x, objective.y, (Entities.Creature.HeightLayer)objective.z))
                        creature.SetPath(objective);    // Set the path the creature must follow if he can reach it
                }
                else
                {
                    objective.z = (int)Entities.Creature.HeightLayer.Ground;
                    if (creature.world.CanMove(objective.x, objective.y, (Entities.Creature.HeightLayer)objective.z))
                        creature.SetPath(objective);    // Set the path the creature must follow if he can reach it
                }
            }

            await = false;
        }

        // No longer cornered, as combat is done
        public override void OnExit()
        {
            creature.cornered = false;
        }

        public override string GetInfo()
        {
            return creature.speciesName + " with ID: " + creature.ID + " CHASES " + objSpecies + " with ID: " + objectiveID;
        }

        public override string ToString()
        {
            return "ChaseEnemyState";
        }
    }
}
