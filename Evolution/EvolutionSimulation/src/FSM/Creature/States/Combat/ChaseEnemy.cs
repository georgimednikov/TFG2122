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
            modifier = 1.1f - // TODO: Modificador que dependa bien, ahora mismo a mas agresividad mejor persigue
                (c.stats.Aggressiveness / c.chromosome.GetFeatureMax(Genetics.CreatureFeature.Aggressiveness) * 0.4f);  // TODO: PASAR A UNIVERSAL
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
            //base.OnEntry();
            creature.Enemy(out objectiveID, out objective);
            Entities.Creature objCreature = creature.world.GetCreature(objectiveID);

            Telemetry.Tracker.Instance.Track(new Telemetry.Events.CreatureStateEntryNotSafe(creature.world.tick, creature.ID, creature.speciesName, ToString(), objectiveID, objCreature == null ? " " : objCreature.speciesName));

            Entities.Creature tmp = creature.world.GetCreature(objectiveID);
            objSpecies = tmp == null ? " " : creature.world.GetCreature(objectiveID).speciesName;
            if (objective.x != creature.x && objective.y != creature.y && creature.CanReach((Entities.Creature.HeightLayer)objective.z)) // TODO: considerar alturas
            {
                creature.SetPath(objective);   // This MUST be set up for the cost of the action to work
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
                if(Math.Abs(otherObj.x - creature.x) >= creature.stats.Perception / 2 || Math.Abs(otherObj.y - creature.y) >= creature.stats.Perception / 2)    // TODO: ajustar criterio de no cornered
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
                if(creature.CanReach((Entities.Creature.HeightLayer)objective.z))
                creature.SetPath(objective);    // Set the path the creature must follow if he can reach it
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
