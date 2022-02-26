using System;

namespace EvolutionSimulation.FSM.Creature.States
{
    class Attacking : CreatureState
    {
        // Additional cost of the attack action
        private float attackMod = 0;

        // If creature has poison
        private bool poison = false;

        public Attacking(Entities.Creature c) : base(c) { creature = c; }

        public override bool canPerformAction(int actionPoints)
        {
            attackMod = 0;
            if (poison = creature.HasAbility(Genetics.CreatureFeature.Venomous, 0.5f))  // TODO: No se que poner aqui
                attackMod += 100;                                                       // TODO: poner esto que dependa de algo lo que sea
            return actionPoints >= 1000 + attackMod;
        }

        // Increases current rest
        public override int Action()
        {
            if (creature.nearestEnemy == null) return 1000 + (int)attackMod;   // TODO: esto va asi?
            
            if(poison)
                creature.nearestEnemy.ReceiveInteraction(creature, Entities.Interactions.poison);
            creature.nearestEnemy.ReceiveInteraction(creature, Entities.Interactions.attack);
            return 1000 + (int)attackMod; // Cost of the action performed
        }

        public override string ToString()
        {
            return "AttackingState";
        }
    }
}
