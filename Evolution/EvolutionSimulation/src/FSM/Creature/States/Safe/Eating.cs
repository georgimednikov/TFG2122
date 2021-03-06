using System;

namespace EvolutionSimulation.FSM.Creature.States
{
    /// <summary>
    /// Action that a creature eats an edible plant or a corpse depending on
    /// the creature's diet gaining some energy
    /// </summary>
    class Eating : CreatureState
    {
        int foodID;
        Vector3Int foodPos;
        string foodType;
        public Eating(Entities.Creature c) : base(c) { creature = c; }

        public override void OnEntry()
        {
            base.OnEntry();
            creature.stats.ActionPerceptionPercentage = UniverseParametersManager.parameters.actionPerceptionPercentage;
            foodID = -1;
            foodPos = new Vector3Int(0, 0, 0);
        }

        public override int GetCost()
        {
            return UniverseParametersManager.parameters.eatingCostMultiplier * creature.stats.Metabolism;
        }

        public override void Action()
        {
            if (creature.Corpse(out _, out foodPos) && creature.DistanceToObjective(foodPos) <= UniverseParametersManager.parameters.adjacentLength)
                EatCorpse();
            else if (creature.Plant())
            {
                EatPlant();
                creature.SafeEdiblePlant();
            }
        }

        public override void OnExit()
        {
            creature.stats.ActionPerceptionPercentage = 1;
        }

        public override string ToString()
        {
            return "EatingState";
        }

        /// <summary>
        /// Eat the nearest plant, gaining energy and letting the plant know
        /// that it has been eaten
        /// </summary>
        protected void EatPlant()
        {
            foodType = "EdiblePlant";
            creature.Plant(out foodID, out foodPos);
            Entities.EdiblePlant closest = creature.world.GetStaticEntity(foodID) as Entities.EdiblePlant;
            closest.ReceiveInteraction(creature, Entities.Interactions.eat);    
            //This has to be used because if the plant is eaten and the creature tries to eat again
            //in the same tick, it will eat just the air
            creature.mind.UpdatePlant();
        }

        /// <summary>
        /// Eat the nearest corpse, gaining energy and letting the corpse know
        /// that it has been eaten
        /// </summary>
        protected void EatCorpse()
        {
            foodType = "Corpse";
            Entities.Corpse closest;
            creature.Corpse(out foodID, out foodPos);
            closest = creature.world.GetStaticEntity(foodID) as Entities.Corpse;
            if(closest != null) // Just in case the corpse has dissapear this tick
                closest.ReceiveInteraction(creature, Entities.Interactions.eat);
        }

        /// <summary>
        /// Text used to display state in simulation
        /// </summary>
        public override string GetInfo() 
        {
            return creature.speciesName + " with ID: " + creature.ID + " EATS " + foodType + " with ID: " + foodID + " at position: (" + foodPos.x + ", " + foodPos.y + ")"; 
        }
    }
}
