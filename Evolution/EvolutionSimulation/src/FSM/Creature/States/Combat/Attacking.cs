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

        public override int GetCost()
        {
            attackMod = 0;
            if (poison = creature.HasAbility(Genetics.CreatureFeature.Venomous, 0.5f))  // TODO: No se que poner aqui
                attackMod += 100;                                                       // TODO: poner esto que dependa de algo lo que sea
            return (int)(1000 + attackMod);
        }

        // Increases current rest
        public override void Action()
        {
            if (creature.GetClosestCreature() == null) return;   // TODO: esto va asi?
            
            if(poison)
                creature.GetClosestCreatureReachable().ReceiveInteraction(creature, Entities.Interactions.poison);
            creature.GetClosestCreatureReachable().ReceiveInteraction(creature, Entities.Interactions.attack);
        }

        public override string ToString()
        {
            return "AttackingState";
        }
    }
}
