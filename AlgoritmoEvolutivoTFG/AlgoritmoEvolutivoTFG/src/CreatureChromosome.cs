﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EvolutionSimulation.src
{
    /// <summary>
    /// Contains the information of a gene: its maximum value and the other genes
    /// it is related to as well as the percentage of the relation
    /// </summary>
    public class Gene
    {
        public int maxValue;
        public CreatureFeature feature;
        public List<Tuple<float, CreatureFeature>> relations { get; }

        public Gene(CreatureFeature feat, int max)
        {
            maxValue = max;
            feature = feat;
            relations = new List<Tuple<float, CreatureFeature>>();
        }

        public void AddRelation(float percentage, CreatureFeature relation)
        {
            relations.Add(new Tuple<float, CreatureFeature>(percentage, relation));
        }
    }







    public class CreatureChromosome
    {
        /// <summary>
        /// If the chromosome has been given initial values or not
        /// </summary>
        static private bool init = false;
        static private int chromosomeSize;

        /// <summary>
        /// The max value of each gene
        /// </summary>
        static private Gene[] geneInfo;
        /// <summary>
        /// First: First bit of each gene
        /// Second: The length of the gene
        /// </summary>
        static private Tuple<int, int>[] genePos;

        static private Random rnd;


        /// <summary>
        /// Chromosome in bits and its amount
        /// </summary>
        BitArray chromosome;
        /// <summary>
        /// The current value of each gene
        /// </summary>
        private int[] geneValues;


        /// <summary>
        /// Sets the internal extructure of the chromosome given the genes and their respectives max values
        /// </summary>
        public static void SetStructure(List<Gene> genes)
        {
            if (genes.Count != (int)CreatureFeature.Count)
                throw new Exception("The number of genes and max values must be the same as the total features");
            rnd = new Random();

            geneInfo = genes.ToArray();
            int[] maxValues = new int[genes.Count];
            for (int i = 0; i < genes.Count; ++i)
            {
                maxValues[(int)genes[i].feature] = genes[i].maxValue;
            }

            genePos = new Tuple<int, int>[genes.Count];
            chromosomeSize = 0;
            foreach (Gene gene in genes)
            {
                //The numeric value of the feature
                int featureIndex = (int)gene.feature;

                int relationsMaxValue = 0;
                foreach (Tuple<float, CreatureFeature> rel in gene.relations)
                {
                    //The highest possible value of the features related is calculated (percentaje * maxValue)
                    relationsMaxValue += (int)(rel.Item1 * maxValues[(int)rel.Item2]); //The percetaje gets truncated!!!
                }

                int leftover = maxValues[featureIndex] - relationsMaxValue;
                if (leftover <= 0)
                    throw new Exception("The genes must not depend completely on other genes and must have percetage values in their relations between 0 and 1");

                genePos[featureIndex] = new Tuple<int, int>(chromosomeSize, leftover); //Start and length in bits of the feature
                chromosomeSize += leftover;
            }

            init = true;
        }





        /// <summary>
        /// Creates a new, random chromosome.
        /// Must be initilized with SetGeneRange
        /// </summary>
        public CreatureChromosome()
        {
            chromosome = new BitArray(chromosomeSize);
            for (int i = 0; i < chromosomeSize; ++i)
                chromosome[i] = rnd.Next(0, 2) == 0;
            SetFeatures();
        }

        /// <summary>
        /// Creates a chromosome given the genes
        /// Must be initilized with SetGeneRange
        /// </summary>
        public CreatureChromosome(BitArray chromosome)
        {
            if (chromosome.Length != chromosomeSize)
                throw new Exception("The given chromosome must have the right size");
            this.chromosome = chromosome;
            SetFeatures();
        }

        /// <summary>
        /// Returns the chromosome. Returns an empty array if not initialized.
        /// </summary>
        public BitArray GetChromosome()
        {
            if (!init) throw new Exception("The chromosome was not initialized");
            return chromosome;
        }

        /// <summary>
        /// Given the chromosome values, calculates the values of the features and sets them.
        /// Only call this function is the chromosome is modified
        /// </summary>
        public void SetFeatures()
        {
            geneValues = Enumerable.Repeat(-1, genePos.Length).ToArray();
            //For each feature (gene)
            for (int i = 0; i < (int)CreatureFeature.Count; ++i)
            {
                //The genes have to be iterated following the order stablished in geneInfo so there's no
                //incomplete dependency between genes (gene A depends on gene B but B has not been calculated yet)
                //The values of the features have to be stored following the enum CreatureFeature for easy access
                int feature = (int)geneInfo[i].feature;
                int startPoint = genePos[feature].Item1;
                int total = 0;
                //The amount of its exclusive bits gets counted
                for (int j = startPoint; j < startPoint + genePos[feature].Item2; ++j)
                {
                    if (chromosome[j]) ++total; //If the bit is 1 then the total count is aumented
                }
                foreach (Tuple<float, CreatureFeature> relation in geneInfo[i].relations)
                {
                    if (geneValues[(int)relation.Item2] < 0)
                        throw new Exception("The genes must have been passed in order of dependency of relations");
                    //total += percentage of usage of the related gene * the value of the gene
                    total += (int)(relation.Item1 * geneValues[(int)relation.Item2]);
                }
                geneValues[feature] = total;
            }
        }

        /// <summary>
        /// Get the numeric value of the desired feature (attribute or ability)
        /// Returns -1 if not initilized or the value asked does not exist
        /// </summary>
        public int GetFeature(CreatureFeature feat)
        {
            if (!init) throw new Exception("The chromosome was not initialized");
            return geneValues[(int)feat];
        }

        /// <summary>
        /// Writes on console the values of the genes of the chromosome, as well as its binary value
        /// </summary>
        public void PrintChromosome()
        {
            for (int i = 0; i < geneValues.Length; ++i)
            {
                Console.WriteLine("Gene " + i + ": " + geneValues[i] + " out of " + geneInfo[i].maxValue);
            }
            Console.WriteLine();
            foreach (bool bit in chromosome)
            {
                Console.Write(bit ? 1 : 0);
            }
            Console.WriteLine();
        }
    }
}
