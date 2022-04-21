using System;
using System.Collections;

namespace EvolutionSimulation.Genetics
{
    public class GeneticFunctions
    {
        /// <summary>
        /// Function that receives 2 chromosomes and uses UniformCrossover to cross them
        /// UniformCrossover means that male chromosome's bits has prob probability to be selected 
        /// if you want that all the bits in both chromosomes have the same
        /// probability to be selected, prob has to be 0.5
        /// </summary>
        static public CreatureChromosome UniformCrossover(CreatureChromosome male, CreatureChromosome female, float prob = 0.5f)
        {
            BitArray mc = male.GetChromosome();
            BitArray fc = female.GetChromosome();
            BitArray cc = new BitArray(mc.Length);

            for (int i = 0; i < cc.Length; ++i)
            {
                cc[i] = RandomGenerator.NextDouble() < prob ? mc[i] : fc[i];
            }

            return new CreatureChromosome(cc);
        }

        private static void Checks(int length, ref int endIndex, ref int startIndex)
        {
            if (endIndex <= -1 || endIndex > length) endIndex = length;
            if (startIndex < 0) startIndex = 0;
        }

        /// <summary>
        /// Function that receives a chromosome and randomly flips bits given a probability
        /// startIndex is the start index to mutate in the chromosome and endIndex the end of it
        /// </summary>
        static public void FlipBitMutation(ref CreatureChromosome creature, float prob, int startIndex = 0, int endIndex = -1)
        {
            BitArray chromosome = creature.GetChromosome();
            Checks(chromosome.Length, ref endIndex, ref startIndex);
           
            for (int i = startIndex; i < endIndex; ++i)
            {
                if (RandomGenerator.NextDouble() < prob) chromosome[i] = !chromosome[i];
            }
            creature.SetFeatures();
        }

        /// <summary>
        /// Function that receives a chromosome and randomly modify bits given a probability
        /// startIndex is the start index to mutate in the chromosome and endIndex the end of it
        /// </summary>
        static public void UniformMutation(ref CreatureChromosome creature, float prob, int startIndex = 0, int endIndex = -1)
        {
            BitArray chromosome = creature.GetChromosome();
            Checks(chromosome.Length, ref endIndex, ref startIndex);
            for (int i = startIndex; i < endIndex; ++i)
            {
                if (RandomGenerator.NextDouble() < prob)
                {
                    if (RandomGenerator.NextDouble() < 0.5f)
                        chromosome[i] = false;
                    else
                        chromosome[i] = true;
                }
            }
            creature.SetFeatures();
        }
    }
}