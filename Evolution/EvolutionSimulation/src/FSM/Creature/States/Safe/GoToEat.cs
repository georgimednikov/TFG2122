using System;
using System.Numerics;

namespace EvolutionSimulation.FSM.Creature.States
{
    class GoToEat : CreatureState
    {
        Vector2Int foodPos;

        public GoToEat(Entities.Creature c) : base(c) { creature = c; }

        public override int GetCost()
        {
            return creature.GetNextCostOnPath();
        }

        public override void OnEntry()
        {
            SetPath();
        }

        public override void Action()
        {
            Vector3 nextPos = creature.GetNextPosOnPath();
            if (nextPos.X != -1 || nextPos.Y != -1 || nextPos.Z != -1)
                creature.Place((int)nextPos.X, (int)nextPos.Y, (Entities.Creature.HeightLayer)nextPos.Z);
            SetPath();
            Console.WriteLine(creature.speciesName + " GOES TO EAT (" + creature.x + ", " + creature.y + ")");
        }

        public override string ToString()
        {
            return "GoToEatState";
        }
        public override string GetInfo()
        {
            return foodPos.ToString();
        }

        /// <summary>
        /// Set the path of the creature depending on his diet and the closest 
        /// food that he knows
        /// </summary>
        private void SetPath()
        {
            int id; 
            Vector2Int pos;
            //Herbivore goes to an edible plant
            if (creature.IsHerbivorous())
                creature.Plant(out id, out pos);
            //If there is a corpse, that will be the objective of a carnivorous/omnivorous creature.
            //Because of how the creature memory works, if the creature is a scavenger the closest corpse will be considered fresh,
            //no matter its state.

            //Carnivore goes to a corpse
            else if (creature.IsCarnivorous())
            {
                creature.Corpse(out id, out pos);
            }
            else //Omnivore
            {
                //If there is a corpse and no plant, then it goes to the corpse.
                if (creature.Corpse(out id, out pos) && !creature.Plant())
                    foodPos = pos;
                //If there is a plant and no corpse, it goes to a plant.
                else if (!creature.Corpse(out id, out pos) && creature.Plant())
                    foodPos = pos;
                //If there is a corpse and a plant, it goes to the closest one.
                else 
                {
                    creature.Corpse(out id, out pos);
                    Vector2Int posPlant;
                    creature.Plant(out id, out posPlant);
                    // Goes to the closest food source
                    int distPlant = creature.DistanceToObjective(posPlant),
                        distCorpse = creature.DistanceToObjective(pos);

                    if (distPlant < distCorpse)
                        foodPos = posPlant;
                    else
                        foodPos = pos;
                }                
            }            
            creature.SetPath(foodPos.x, foodPos.y);
        }
    }
}
