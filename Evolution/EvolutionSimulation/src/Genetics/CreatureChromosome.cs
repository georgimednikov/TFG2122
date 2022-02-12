using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EvolutionSimulation.Genetics
{
    public struct Relation
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public CreatureFeature feature;
        public float percentage;

        public Relation(float percentage, CreatureFeature relation) : this()
        {
            this.percentage = percentage;
            feature = relation;
        }
    }
    /// <summary>
    /// Contains the information of a gene: its maximum value and the other genes
    /// it is related to as well as the percentage of the relation
    /// </summary>
    public class Gene : IComparable<Gene>
    {

        [JsonConverter(typeof(StringEnumConverter))]
        public CreatureFeature feature;
        public int maxValue;
        public List<Relation> relations { get; }

        public Gene(CreatureFeature feat, int max)
        {
            maxValue = max;
            feature = feat;
            relations = new List<Relation>();
        }

        /// <summary>
        /// Add a new relation and the percentage of the relation.
        /// Check that the gene does not have other relation with the same CreatureFeature
        /// </summary>
        public void AddRelation(float percentage, CreatureFeature relation)
        {
            // Check that the relation is unique
            foreach (Relation cF in relations)
                if (cF.feature == relation)
                    return;
            // Add the relation
            relations.Add(new Relation(percentage, relation));
        }

        /// <summary>
        /// Comparator to order arrays or lists of Genes by features
        /// </summary>
        public int CompareTo(Gene other)
        {
            if (other.feature == feature) return 0;
            return other.feature > feature ? -1 : 1;
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
        /// It is ordeded by how you pass the Genes in method SetStructure
        /// </summary>
        static private Gene[] geneInfo;
        /// <summary>
        /// First: First bit of each gene
        /// Second: The length of the gene
        /// </summary>
        static private Tuple<int, int>[] genePos;
        /// <summary>
        /// Max values of each gene ORDERED BY FEATURE
        /// </summary>
        static private int[] geneMaxValues;



        /// <summary>
        /// Chromosome in bits and its amount
        /// </summary>
        BitArray chromosome;
        /// <summary>
        /// The current value of each gene
        /// It is ordered by CreatureFeatures 
        /// </summary>
        public int[] geneValues;

        /// <summary>
        /// Sets the internal extructure of the chromosome given the genes and their respectives max values.
        /// The genes MUST be given in such an order that there is no incomplete dependency between them.
        /// Example: Gene A depends on Gene B, therefore Gene B must be passed before Gene A
        /// </summary>
        public static void SetStructure(List<Gene> genes)
        {
            if (genes.Count != (int)CreatureFeature.Count)
                throw new Exception("The number of genes and max values must be the same as the total features");
            
            geneInfo = genes.ToArray();
            geneMaxValues = new int[genes.Count];
            for (int i = 0; i < genes.Count; ++i)
            {
                geneMaxValues[(int)genes[i].feature] = genes[i].maxValue;
            }

            genePos = new Tuple<int, int>[genes.Count];
            
            chromosomeSize = 1;//first bit is for gender
            foreach (Gene gene in genes)
            {
                //The numeric value of the feature
                int featureIndex = (int)gene.feature;

                int relationsMaxValue = 0;
                foreach (Relation relation in gene.relations)
                {
                    //If the relation is positive, the max value of the gene can be surpassed so the max value of the relations is substracted
                    //If the relation is negative, the max value of the gene must not be surpassed with an addition, so it is not accounted.
                    //In other words, a negative relation may not modify the value, but if it is accounted for it would add extra bits surpassing the max value.
                    if (relation.percentage > 0)
                        //The highest possible value of the features related is calculated (percentaje * maxValue)
                        relationsMaxValue += (int)Math.Ceiling(relation.percentage * geneMaxValues[featureIndex]); //The percetaje gets truncated!!!
                    
                }
                int leftover = geneMaxValues[featureIndex] - relationsMaxValue;
                if (leftover <= 0)
                    throw new Exception("The genes must not depend completely on other genes and must have percetage values in their relations between 0 and 1");

                genePos[featureIndex] = new Tuple<int, int>(chromosomeSize, leftover); //Start and length in bits of the feature
                chromosomeSize += leftover;
            }
            init = true;
        }


        /// <summary>
        /// Creates a new, random chromosome. 
        /// It is created with randoms between 0 and the max value of the gene that is being setting
        /// and adding the same bits of the random as 1 to the chromosome
        /// </summary>
        public CreatureChromosome()
        {
            chromosome = new BitArray(chromosomeSize);
            chromosome[0] = RandomGenerator.Next(0, 2) == 0;//first chromosome is for creature's gender

            for (int i = 0; i < genePos.Length; ++i)
            {
                int ini = genePos[i].Item1;
                int genRandom = RandomGenerator.Next(0, genePos[i].Item2 + 1);
                for (int j = 0; j < genePos[i].Item2 && j + genePos[i].Item2 < chromosomeSize; ++j)
                {
                    chromosome[ini + j] = j < genRandom;
                }
                //shuffle the bits to not have all the 1s at the initial positions of the gene and the 0s afterwards
                for (int j = 0; j < genePos[i].Item2 && j + genePos[i].Item2 < chromosomeSize; ++j)
                {
                    int randomPos = RandomGenerator.Next(0, genePos[i].Item2 + 1);
                    bool tmp = chromosome[ini + j];
                    chromosome[ini + j] = chromosome[randomPos];
                    chromosome[randomPos] = tmp;
                }
            }
           
            SetFeatures();
        }

        /// <summary>
        /// Creates a chromosome given the genes
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
            //We need to order geneInfo because the relations of the genes 
            Gene[] geneInfoOrdered = (Gene[])geneInfo.Clone();
            Array.Sort(geneInfoOrdered);
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
                foreach (Relation relation in geneInfo[i].relations)
                {
                    if (geneValues[(int)relation.feature] < 0)
                        throw new Exception("The genes must have been passed in order of dependency of relations");
                    //total += percentage of usage of the related gene * the value of the gene
                    //total += (int)(relation.percentage * geneValues[(int)relation.feature]);

                    //total += relationValue / (relationMaxValue / (relationPercentage * featureMaxValue))
                    int relationMaxValue = geneInfoOrdered[(int)relation.feature].maxValue;
                    int relationValue = geneValues[(int)relation.feature];
                    total += (int)Math.Ceiling(relationValue / (relationMaxValue / (relation.percentage * geneInfo[i].maxValue)));
                }
                geneValues[feature] = Math.Max(0, total);
            }
        }

        /// <summary>
        /// Get the numeric value of the desired feature (attribute or ability)
        /// Returns -1 if not initilized or the value asked does not exist
        /// </summary>
        public Gender GetGender()
        {
            if (!init) throw new Exception("The chromosome was not initialized");
            if (chromosome[0]) return Gender.Male;
            else return Gender.Female;
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
        /// Get the max value of the desired feature (attribute or ability)
        /// Returns -1 if not initilized or the value asked does not exist
        /// </summary>
        public int GetFeatureMax(CreatureFeature feat)
        {
            if (!init) throw new Exception("The chromosome was not initialized");
            return geneMaxValues[(int)feat];
        }

        /// <summary>
        /// Writes on console the values of the genes of the chromosome, as well as its binary value
        /// </summary>
        public void PrintChromosome()
        {
            for (int i = 0; i < geneValues.Length; ++i)
            {
                Console.WriteLine("Gene " + geneInfo[i].feature + ": " + geneValues[(int)geneInfo[i].feature] + " out of " + geneInfo[i].maxValue);
            }
            Console.WriteLine();
            foreach (bool bit in chromosome)
            {
                Console.Write(bit ? 1 : 0);
            }
            Console.WriteLine();
        }

        /// <summary>
        /// Sets the chromosome structure reading the values of the given JSON file.
        /// </summary>
        /// <param name="json">Address of the JSON file</param>
        static public void SetChromosome(string json)
        {
            if (!File.Exists(json))
                throw new Exception("Cannot find JSON with chromosome information");
            string file = File.ReadAllText(json);
            List<Gene> genes = JsonConvert.DeserializeObject<List<Gene>>(file);
            SetStructure(genes);
        }
    }
}
