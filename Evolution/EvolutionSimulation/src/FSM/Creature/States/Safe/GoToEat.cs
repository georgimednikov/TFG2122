using System;
using System.Numerics;

namespace EvolutionSimulation.FSM.Creature.States
{
    class GoToEat : CreatureState
    {
        Vector2Int foodPos;
        bool notAtDestiny;
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
            if (notAtDestiny)  // If it is already there, Eating transition must trigger the next tick
            {
                Vector3 nextPos = creature.GetNextPosOnPath();
                if (nextPos.X != -1 && nextPos.Y != -1) // TODO: esta comp no haria falta ahora con el alreadyThere
                    creature.Place((int)nextPos.X, (int)nextPos.Y, (Entities.Creature.HeightLayer)nextPos.Z);
                SetPath();
            }
        }

        public override string ToString()
        {
            return "GoToEatState";
        }
        public override string GetInfo()
        {
            return creature.speciesName + " with ID: " + creature.ID + " IN (" + creature.x + ", " + creature.y + ")GOES TO EAT AT (" + foodPos.x + ", " + foodPos.y + ")";
        }

        /// <summary>
        /// Set the path of the creature depending on his diet and the closest 
        /// food that he knows
        /// </summary>
        private void SetPath()
        {
            Vector2Int posCorpse;
            Vector2Int posPlant;

            creature.Corpse(out _, out posCorpse);
            creature.Plant(out _, out posPlant);

            // Goes to the closest food source
            // If one of them does not exist their distance is considered infinite
            int distCorpse = creature.DistanceToObjective(posCorpse),
                distPlant = creature.DistanceToObjective(posPlant);

            if (distPlant < distCorpse)
                foodPos = posPlant;
            else
                foodPos = posCorpse;

            // If the creature is not already at destiny, the path is set
            notAtDestiny = foodPos.x != creature.x || foodPos.y != creature.y;
            if (notAtDestiny)
                creature.SetPath(foodPos.x, foodPos.y);
        }
    }
}
