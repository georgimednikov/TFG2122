using System;
using System.Numerics;

namespace EvolutionSimulation.FSM.Creature.States
{
    class GoToEat : CreatureState
    {
        Vector3 foodPos;
        bool notAtDestiny;
        public GoToEat(Entities.Creature c) : base(c) { creature = c; }

        public override int GetCost()
        {
            return creature.GetNextCostOnPath();
        }

        public override void OnEntry()
        {
            base.OnEntry();
            SetPath();
        }

        public override void Action()
        {
            if (notAtDestiny)  // If it is already there, Eating transition must trigger the next tick
            {
                Vector3 nextPos = creature.GetNextPosOnPath();
                if (nextPos.X != -1 && nextPos.Y != -1) // TODO: esta comp no haria falta ahora con el notAtDestiny
                    creature.Place((int)nextPos.X, (int)nextPos.Y, (Entities.Creature.HeightLayer)nextPos.Z);
                else SetPath();
            }
        }

        public override string ToString()
        {
            return "GoToEatState";
        }
        public override string GetInfo()
        {
            return creature.speciesName + " with ID: " + creature.ID + " IN (" + creature.x + ", " + creature.y + ", " + creature.creatureLayer + ")GOES TO EAT AT (" + foodPos.X + ", " + foodPos.Y + ")";
        }

        /// <summary>
        /// Set the path of the creature depending on his diet and the closest 
        /// food that he knows
        /// </summary>
        private void SetPath()
        {
            Vector3Int posCorpse;
            Vector3Int posPlant;

            creature.Corpse(out _, out posCorpse);
            creature.Plant(out _, out posPlant);

            // Goes to the closest food source
            // If one of them does not exist their distance is considered infinite
            int distCorpse = creature.DistanceToObjective(posCorpse),
                distPlant = creature.DistanceToObjective(posPlant);

            if (distPlant < distCorpse)
                foodPos = new Vector3(posPlant.x, posPlant.y, 0);
            else
                foodPos = new Vector3(posCorpse.x, posCorpse.y, 0);

            // If the creature is not already at destiny, the path is set
            notAtDestiny = foodPos.X != creature.x || foodPos.Y != creature.y || foodPos.Z != (float)(creature.creatureLayer);
            if (notAtDestiny)
                creature.SetPath((int)foodPos.X, (int)foodPos.Y);
        }
    }
}
