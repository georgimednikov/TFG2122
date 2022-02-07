using System;
using System.Collections;

namespace EvolutionSimulation.src
{
    public class GeneticFunctions
    {
        private float crossProb;
        private float mutationProb;

        /// <param name="cross">Probability of choosing the father's genes in crossover</param>
        /// <param name="mutation">Probability of flipping a bit when mutating the chromosome</param>
        public GeneticFunctions(float cross, float mutation)
        {
            crossProb = cross;
            mutationProb = mutation;
        }

        /// <summary>
        /// Function that receives 2 chromosomes and uses UniformCrossover to cross them
        /// UniformCrossover means that all the bits in both chromosomes have the same
        /// probability to be selected (50%)
        /// </summary>
        public CreatureChromosome UniformCrossover(CreatureChromosome male, CreatureChromosome female)
        {
            Random rnd = new Random();
            BitArray mc = male.GetChromosome();
            BitArray fc = female.GetChromosome();
            BitArray cc = new BitArray(mc.Length);

            for (int i = 0; i < cc.Length; ++i)
            {
                cc[i] = rnd.NextDouble() < crossProb ? mc[i] : fc[i];
            }

            return new CreatureChromosome(cc);
        }

        /// <summary>
        /// Function that receives a chromosome and randomly flips bits given a probability
        /// </summary>
        public void Mutation(CreatureChromosome creature)
        {
            Random rnd = new Random();
            BitArray chromosome = creature.GetChromosome();
            for (int i = 0; i < chromosome.Length; ++i)
            {
                if (rnd.NextDouble() < mutationProb) chromosome[i] = !chromosome[i];
            }
        }
    }
}
