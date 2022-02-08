using System;
using System.Collections;

namespace EvolutionSimulation.src
{
    public class GeneticFunctions
    {
        /// <summary>
        /// Function that receives 2 chromosomes and uses UniformCrossover to cross them
        /// UniformCrossover means that all the bits in both chromosomes have the same
        /// probability to be selected
        /// </summary>
        static public CreatureChromosome UniformCrossover(CreatureChromosome male, CreatureChromosome female, float prob)
        {
            Random rnd = new Random();
            BitArray mc = male.GetChromosome();
            BitArray fc = female.GetChromosome();
            BitArray cc = new BitArray(mc.Length);

            for (int i = 0; i < cc.Length; ++i)
            {
                cc[i] = rnd.NextDouble() < prob ? mc[i] : fc[i];
            }

            return new CreatureChromosome(cc);
        }

        /// <summary>
        /// Function that receives a chromosome and randomly flips bits given a probability
        /// </summary>
        static public void Mutation(CreatureChromosome creature, float prob)
        {
            Random rnd = new Random();
            BitArray chromosome = creature.GetChromosome();
            for (int i = 0; i < chromosome.Length; ++i)
            {
                if (rnd.NextDouble() < prob) chromosome[i] = !chromosome[i];
            }
            creature.SetFeatures();
        }
    }
}