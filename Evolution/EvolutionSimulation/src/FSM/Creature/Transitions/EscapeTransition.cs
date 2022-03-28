using System;
using System.Collections.Generic;

namespace EvolutionSimulation.FSM.Creature.Transitions
{
    class EscapeTransition : CreatureTransition
    {
        public EscapeTransition(Entities.Creature creature)
        {
            this.creature = creature;
        }

        public override bool Evaluate()
        {
            Vector2Int enemyPos;

            // If this creature does not have an enemy targeted for combat.
            if (!creature.Enemy(out _, out enemyPos))
            {
                // The creature flees from combat if there is another creature that is considered dangerous and it is not cornered.
                return creature.Menace() && !creature.cornered;
            }
            // Else if the creature's pack thinks it's too weak to fight it, they will flee.
            else
            {
                List<Entities.Creature> pack = creature.CombatPack();
                return creature.stats.Aggressiveness * pack.Count < creature.PositionDanger(enemyPos.x, enemyPos.y);
            }
        }

        public override string ToString()
        {
            return "EscapeTransition";
        }

    }
}
