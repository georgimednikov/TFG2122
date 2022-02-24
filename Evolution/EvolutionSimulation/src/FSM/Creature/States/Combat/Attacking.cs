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

        // This move is energy neutral, costing the same nergy that is obtained in a tick
        public override bool canPerformAction(int actionPoints)
        {
            attackMod = 0;
            if (poison = creature.HasAbility(Genetics.CreatureFeature.Venomous, 0.5f))    // TODO: No se que poner aqui
                attackMod += 100;   // TODO: poner esto que dependa de algo lo que sea
            return actionPoints >= 1000 + attackMod;
        }

        // Increases current rest
        public override int Action()
        {
            if (creature.nearestEnemy == null) return 1000 + (int)attackMod;   // TODO: esto va asi?
            // TODO: ahora mismo si matas a la criatura que ataques te quedas atacando a la nada, 
            // Poner transicion de attacking a idle bien
            if(poison)
                creature.nearestEnemy.ReceiveInteraction(creature, Entities.Interactions.poison);
            creature.nearestEnemy.ReceiveInteraction(creature, Entities.Interactions.attack);
            Console.WriteLine("Criatura de " + creature.x + ", " + creature.y + " atacando a criatura de " + creature.nearestEnemy.x + ", " + creature.nearestEnemy.y + "!");
            return 1000 + (int)attackMod; // Cost of the action performed
        }

        public override string ToString()
        {
            return "AttackingState";
        }
    }
}
