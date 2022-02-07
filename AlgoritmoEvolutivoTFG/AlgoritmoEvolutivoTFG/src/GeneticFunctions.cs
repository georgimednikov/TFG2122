using System;
using System.Collections;

namespace EvolutionSimulation.src
{
    public class GeneticFunctions
    {
        /// <summary>
        /// Function that receives 2 chromosomes and use UniformCrossover to cross them
        /// UniformCrossover means that all the bits in both chromosomes have the same
        /// probability to be selected (50%)
        /// </summary>
        static public CreatureChromosome UniformCrossover(CreatureChromosome male, CreatureChromosome female)
        {
            Random rnd = new Random();
            BitArray mc = male.GetChromosome();
            BitArray fc = male.GetChromosome();
            BitArray cc = new BitArray(mc.Length);
            
            for (int i = 0; i < cc.Length; ++i)
            {
                cc[i] = rnd.Next(0, 2) == 0 ? mc[i] : fc[i];
            }

            return new CreatureChromosome(cc);
        }

        static public void Mutation(CreatureChromosome creature)
        {
            Random rnd = new Random();
            BitArray chromosome = creature.GetChromosome(); //Referencia?
            for (int i = 0; i < chromosome.Length; ++i)
            {
                if (rnd.Next(0, 10) == 0) chromosome[i] = !chromosome[i];
            }
        }
    }
}
