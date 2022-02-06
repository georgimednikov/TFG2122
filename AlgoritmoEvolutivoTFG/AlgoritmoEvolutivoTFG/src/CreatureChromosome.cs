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
        /// Number of bits of the Choromose
        /// </summary>
        static private int bitLength;

        BitArray[] bitChromosome;

        /// <summary>
        /// Start(inclusive) and end(exclusive) of the genes in the Choromosome
        /// </summary>
        static private Vector2[] geneRanges;

        /// <summary>
        /// Sets the genes' ranges of a creature's chromosome
        /// </summary>
        public static void SetGeneRange(params int[] genes)
        {
            geneRanges = new Vector2[genes.Length];
           
            bitLength = 0;
            int geneStart = 0;
            for (int i = 0; i < genes.Length; ++i)
            {
                geneRanges[i].X = geneStart;
                geneRanges[i].Y = geneStart + genes[i];
                geneStart += genes[i];
            }
            bitLength = geneStart; //Because count starts at 0
        }

        public CreatureChromosome() : base(geneRanges.Length) //Number of genes, not bits
        {
            bitChromosome = new BitArray[geneRanges.Length];
            CreateGenes();
            int a = 0;
        }

        /// <summary>
        /// Generates a random value for the gene in position "geneIndex"
        /// </summary>
        public override Gene GenerateGene(int geneIndex)
        {
            Vector2 gene = geneRanges[geneIndex];
            int size = (int)gene.Y - (int)gene.X; //Number of bit in gene
            var value = RandomizationProvider.Current.GetInt(0, (int)Math.Pow(2, size));
            BitArray bits = new BitArray(new int[] { value });
            bitChromosome[geneIndex] = bits;
            return new Gene(value);
        }

        /// <summary>
        /// Creates an empty copy of the structure of the Chromosome
        /// </summary>
        public override IChromosome CreateNew()
        {
            return new CreatureChromosome();
        }

        //public int GetGeneBit(int geneIndex, int bitIndex)
        //{
        //    int size = geneSize[geneIndex];
        //    int value = (int)GetGene(geneIndex).Value;
        //    BitArray bits = new BitArray(new int[] { value });
        //    return bits[bitIndex] ? 1 : 0;
        //}

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
            if (feature >= geneRanges.Length || feature < 0)
                return -1;
            return (int)GetGene(feature).Value;
        }
    }
}
