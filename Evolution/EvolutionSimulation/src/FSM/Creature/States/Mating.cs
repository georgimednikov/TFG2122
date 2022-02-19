﻿namespace EvolutionSimulation.FSM.Creature.States
{
    /// <summary>
    /// State reproducing. After a given time (in tick) if is a female it wil create
    /// a random number of creatures using the crossover function and the mutation function
    /// </summary>
    class Mating : CreatureState
    {

        int time;

        public Mating(Entities.Creature c, int time) : base(c){ this.time = time;}

        // This move is energy netural, costing the same nergy that is obtained in a tick
        public override bool canPerformAction(int actionPoints)
        {
            return actionPoints >= 1000;
        }

        // Increases current rest
        public override int Action()
        {
            time--;
            if (time == 0)
            {
                if(creature.stats.Gender == Genetics.Gender.Female)
                {
                    Entities.Creature obj = creature.objective as Entities.Creature;
                    if (obj == null) return 1000;
                    // Create a random number of childs
                    int numberChilds = RandomGenerator.Next(1, 5);//TODO: que el numero de hijos dependa de algo del cromosoma?
                    for(int i = 0; i < numberChilds; ++i)
                    {
                        //TODO: cuidado con el problema del diamante
                        // Crossover with male and female chromosomes
                        Genetics.CreatureChromosome childC = Genetics.GeneticFunctions.UniformCrossover(obj.chromosome, creature.chromosome);
                        // Mutate the chromosome
                        Genetics.GeneticFunctions.UniformMutation(ref childC, 0.1f);// Todo que la probabilidad la coja de algun sitio
                        // The new creature's pos (near to the parents)
                        int nx = creature.x + RandomGenerator.Next(-1, 2);
                        int ny = creature.y + RandomGenerator.Next(-1, 2);
                        creature.world.CreateCreature<Entities.Animal>(nx , ny, childC, creature.speciesName);
                    }
                }
            }
            
            //obj.ReceiveInteraction(creature, Entities.Interactions.mate);
            return 1000; // Cost of the action performed
        }

        public override string ToString()
        {
            return "MatingState";
        }
    }
}
