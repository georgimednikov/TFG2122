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

        public Mating(Entities.Creature c, int time) : base(c){ this.time = time; startTime = time; }

        // This move is energy netural, costing the same energy that is obtained in a tick
        public override int GetCost()
        {
            return (int)UniverseParametersManager.parameters.baseActionCost;//TODO
        }

        /// <summary>
        /// Stay doing nothing (mating) while the timer is decreasing. When the timer is 0
        /// then create some creatures
        /// </summary>
        public override void Action()
        {
            Console.WriteLine("Mating action");

            time--;
            if (time == 0)
            {
                if (creature.stats.Gender == Genetics.Gender.Female)
                {
                    if (creature.GetClosestPossibleMatePosition() == null) return;
                    // Create a random number of childs
                    int numberChilds = RandomGenerator.Next(1, 5);//TODO: que el numero de hijos dependa de algo del cromosoma?
                    for (int i = 0; i < numberChilds; ++i)
                    {
                        //TODO: cuidado con el problema del diamante
                        // Crossover with male and female chromosomes
                        Genetics.CreatureChromosome childC = Genetics.GeneticFunctions.UniformCrossover(creature.matingCreature.chromosome, creature.chromosome);
                        // Mutate the chromosome
                        Genetics.GeneticFunctions.UniformMutation(ref childC, 0.1f);// TODO que la probabilidad la coja de algun sitio
                        // The new creature's pos (near to the parents)
                        int nx = creature.x + RandomGenerator.Next(-1, 2);
                        int ny = creature.y + RandomGenerator.Next(-1, 2);
                        Entities.Animal child = creature.world.CreateCreature<Entities.Animal>(nx, ny, childC, creature.speciesName);

                        // Add the parents to the new creature and the child to the parents
                        child.father = creature.matingCreature;
                        child.mother = creature;
                        creature.matingCreature.childs.Add(child);
                        creature.childs.Add(child);
                        // Follow randomly the father or the mother
                        if (RandomGenerator.Next(0, 2) == 0)
                            child.parentToFollow = child.father;
                        else
                            child.parentToFollow = child.mother;
                    }
                    creature.timeToBeInHeat = -1;
                }                
            }
        }

        /// <summary>
        /// At the end of the action whatever the reason was, reset the timer
        /// and interact with the mating creature to stop mating
        /// </summary>
        public override void OnExit()
        {
            time = startTime;
            creature.matingCreature.ReceiveInteraction(creature, Entities.Interactions.stopMate);
        }

        public override string ToString()
        {
            return "MatingState";
        }
    }
}
