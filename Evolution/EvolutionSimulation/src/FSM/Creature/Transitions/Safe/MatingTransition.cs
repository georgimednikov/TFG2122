using System;

namespace EvolutionSimulation.FSM.Creature.Transitions
{
    /// <summary>
    /// Checks if the creature is close to the mating objective and wanted to mate
    /// TryMate -> Mating
    /// </summary>
    class MatingTransition : CreatureTransition
    {
        public MatingTransition(Entities.Creature creature)
        {
            this.creature = creature;
        }

        public override bool Evaluate()
        {
            //TODO  Comprobar tambien que
            //sea receptiva(no tenga que ir a comer, dormir, o este relacionandose con otro macho)
            //que la criatura no tenga necesidad de ir a comer, dormir... que haciendo la accion
            //no se va a parar hasta pasado un tiempo
            return creature.nearestMate != null
               && Math.Abs(creature.nearestMate.x - creature.x) < 1
               && Math.Abs(creature.nearestMate.y - creature.y) < 1;
        }

        public override string ToString()
        {
            return "MatingTransition";
        }

    }
}
