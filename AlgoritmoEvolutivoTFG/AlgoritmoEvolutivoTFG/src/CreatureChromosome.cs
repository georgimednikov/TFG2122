using GeneticSharp.Domain.Chromosomes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using GeneticSharp.Domain.Randomizations;
using System.Collections;

namespace EvolutionSimulation.src
{
    public class CreatureChromosome : BinaryChromosomeBase
    {
        /// <summary>
        /// The amount of possible values for each gene
        /// </summary>
        static int[] geneSize;
        /// <summary>
        /// Sets the genes' ranges of a creature's chromosome
        /// </summary>
        public static void SetGeneSize(params int[] genes)
        {
            geneSize = genes;
        }

        public CreatureChromosome() : base(geneSize.Length) //Number of genes, not bits
        {
            CreateGenes();

        }

        /// <summary>
        /// Generates a random value for the gene in position "geneIndex"
        /// </summary>
        public override Gene GenerateGene(int geneIndex)
        {
            int size = geneSize[geneIndex];
            var value = RandomizationProvider.Current.GetInt(0, size);
            return new Gene(value);
        }

        /// <summary>
        /// Creates an empty copy of the structure of the Chromosome
        /// </summary>
        public override IChromosome CreateNew()
        {
            return new CreatureChromosome();
        }

        public int GetGeneBit(int geneIndex, int bitIndex)
        {
            int size = geneSize[geneIndex];
            int value = (int)GetGene(geneIndex).Value;
            BitArray bits = new BitArray(new int[] { value });
            return bits[bitIndex] ? 1 : 0;
        }

        /// <summary>
        /// Get the numeric value of the desired attribute
        /// </summary>
        public int GetAttribute(CreatureAttribute attr)
        {
            return GetFeature((int)attr);
        }

        /// <summary>
        /// Get the numeric value of the desired ability
        /// </summary>
        public int GetAbilities(CreatureAbility ab)
        {
            return GetFeature((int)ab);
        }


        private int GetFeature(int feature)
        {
            if (feature >= geneSize.Length || feature < 0)
                return -1;
            return (int)GetGene(feature).Value;
        }
    }
}
