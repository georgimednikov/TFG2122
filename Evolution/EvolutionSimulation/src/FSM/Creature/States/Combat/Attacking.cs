using System;

using Telemetry;
using Telemetry.Events;

namespace EvolutionSimulation.FSM.Creature.States
{
    class Attacking : CreatureState
    {
        // Additional cost of the attack action
        private float attackMod = 0;

        // If creature has poison
        private bool poison = false;
        int enemyID;
        string speciesName;
        public Attacking(Entities.Creature c) : base(c) { creature = c; }

        public override int GetCost()
        {
            attackMod = 0;
            if (poison = creature.chromosome.HasAbility(Genetics.CreatureFeature.Venomous, Genetics.CreatureChromosome.AbilityUnlock[Genetics.CreatureFeature.Venomous]))  
                attackMod += UniverseParametersManager.parameters.venomCostMultiplier * creature.stats.Venom;    // Costs 100 more per point in Venom                                                      
            return (int)(UniverseParametersManager.parameters.baseActionCost + attackMod);
        }

        // Increases current rest
        public override void Action()
        {
            if (!creature.Enemy(out enemyID, out _)) { speciesName = ""; return; }
            // TODO: Handlear la perdida posible de enmigo de la misma manera que chase? De momento no peta \[T]/
            Entities.Creature objCreature = creature.world.GetCreature(enemyID);
            if (objCreature != null)
            {
                speciesName = objCreature.speciesName;
                if (poison)
                {
                    Tracker.Instance.Track(new CreatureApplyPoison(creature.world.tick, creature.ID, creature.speciesName, 
                        objCreature.ID, objCreature.speciesName, creature.stats.Venom * 0.25f, (int)(creature.stats.Venom)));
                    objCreature.ReceiveInteraction(creature, Entities.Interactions.poison);
                }
                objCreature.ReceiveInteraction(creature, Entities.Interactions.attack);

                Tracker.Instance.Track(new CreatureAttack(creature.world.tick, creature.ID, creature.speciesName, 
                    objCreature.ID, objCreature.speciesName, creature.stats.Damage, creature.stats.Perforation));
            }
        }

        // No longer cornered, as combat is done
        public override void OnExit()
        {
            creature.cornered = false;
        }

        public override string ToString()
        {
            return "AttackingState";
        }

        public override string GetInfo()
        {
            return creature.speciesName + " with ID: " + creature.ID + " ATTACKS " + speciesName + " with ID: " + enemyID;
        }
    }
}
