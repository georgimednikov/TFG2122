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
            if (poison = creature.HasAbility(Genetics.CreatureFeature.Venomous, 0.5f))  
                attackMod += 100 * creature.stats.Venom;    // Costs 100 more per point in Venom                                                      
            return (int)(1000 + attackMod); // TODO: 1000
        }

        // Increases current rest
        public override void Action()
        {
            if (creature.GetClosestCreature() == null) return;
            
            if(poison)
                creature.GetClosestCreatureReachable().ReceiveInteraction(creature, Entities.Interactions.poison);
            creature.GetClosestCreatureReachable().ReceiveInteraction(creature, Entities.Interactions.attack);
            Console.WriteLine(creature.speciesName + " ATTACK KILL");
        }

        //// No longer cornered, as combat is done
        //public override void OnExit()
        //{
        //    creature.cornered = false;
        //}

        public override string ToString()
        {
            return "AttackingState";
        }
    }
}
