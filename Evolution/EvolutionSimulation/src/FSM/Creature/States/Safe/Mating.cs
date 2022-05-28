using System;

namespace EvolutionSimulation.FSM.Creature.States
{
    /// <summary>
    /// State reproducing. After a given time (in tick) if is a female it will create
    /// a random number of creatures using the crossover function and the mutation function
    /// </summary>
    class Mating : CreatureState
    {
        int time;
        int startTime;
        Vector2Int matePos;
        string mateSpecies;
        public Mating(Entities.Creature c, int time) : base(c){ this.time = time; startTime = time; }

        public override void OnEntry()
        {
            base.OnEntry();
            creature.stats.ActionPerceptionPercentage = UniverseParametersManager.parameters.actionPerceptionPercentage;
            SetInfo();
        }

        // This move is energy netural, costing the same energy that is obtained in a tick
        public override int GetCost()
        {
            return (int)UniverseParametersManager.parameters.baseActionCost;
        }

        /// <summary>
        /// Stay doing nothing (mating) while the timer is decreasing. When the timer is 0
        /// then create some creatures
        /// </summary>
        public override void Action()
        {
            time--;
            if (time == 0)
            {
                if (creature.stats.Gender == Genetics.Gender.Female)
                {
                    Entities.Creature father = creature.world.GetCreature(creature.matingCreature);
                    // The creature whom it was mating has died or something
                    if (father == null) {
                        StopMating(); 
                        return; 
                    }
                    // Create a random number of childs
                    int numberChilds = RandomGenerator.Next(1, UniverseParametersManager.parameters.maxChildNumber + 1);
                    for (int i = 0; i < numberChilds; ++i)
                    {
                        // Crossover with male and female chromosomes          
                        Genetics.CreatureChromosome childC = Genetics.GeneticFunctions.UniformCrossover(father.chromosome, creature.chromosome);
                        // Mutate the chromosome
                        Genetics.GeneticFunctions.UniformMutation(ref childC, UniverseParametersManager.parameters.mutationChance);
                        // The new creature's pos (near to the parents)
                        int nx, ny;
                        do{
                            nx = creature.x + RandomGenerator.Next(-1, 2);
                            ny = creature.y + RandomGenerator.Next(-1, 2);
                        }while(!creature.world.CanMove(nx,ny));
                        
                        creature.world.CreateCreature<Entities.Animal>(nx, ny, childC, creature.world.GiveName(childC, father, creature), creature.matingCreature, creature.ID);
                    }

#if TRACKER_ENABLED
                    Telemetry.Tracker.Instance.Track(new Telemetry.Events.CreatureMating(creature.world.CurrentTick, creature.ID, creature.speciesName, father.ID, father.speciesName, numberChilds, creature.x, creature.y));
#endif
                    creature.timeToBeInHeat = -1;
                    creature.mating = false;
                    creature.CreateDanger();
                }
                else
                {
                    // The creature whom it was mating has died or something
                    if (creature.world.GetCreature(creature.matingCreature) == null) {
                        StopMating();
                    }
                }
            }
        }

        void StopMating()
        {
            creature.matingCreature = -1;
            creature.mating = false;
        }

        private void SetInfo()
        {
            Entities.Creature mate = creature.world.GetCreature(creature.matingCreature);
            if (mate != null)
            {
                matePos = new Vector2Int(mate.x, mate.y);
                mateSpecies = mate.speciesName;
            }
            else
            {
                matePos = new Vector2Int(-1, -1);
                mateSpecies = "";
            }
        }
        /// <summary>
        /// At the end of the action whatever the reason was, reset the timer
        /// and interact with the mating creature to stop mating
        /// </summary>
        public override void OnExit()
        {
            time = startTime;
            if(creature.world.GetCreature(creature.matingCreature) != null)
                creature.world.GetCreature(creature.matingCreature).ReceiveInteraction(creature, Entities.Interactions.stopMate);
            creature.stats.ActionPerceptionPercentage = 1;
            creature.matingCreature = -1;
        }

        public override string ToString()
        {
            return "MatingState";
        }

        /// <summary>
        /// Text used to display state in simulation
        /// </summary>
        public override string GetInfo()
        {
            return creature.speciesName + " with ID: " + creature.ID + " MATES WITH " + mateSpecies + " with ID: " + creature.matingCreature + " AT (" + matePos.x + ", " + matePos.y + ")" ;
        }
    }
}
