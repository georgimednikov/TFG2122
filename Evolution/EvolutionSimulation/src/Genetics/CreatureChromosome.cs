using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EvolutionSimulation.Genetics
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

        /// <summary>
        /// Add a new relation and the percentage of the relation.
        /// Check that the gene does not have other relation with the same CreatureFeature
        /// </summary>
        public void AddRelation(float percentage, CreatureFeature relation)
        {
            // Check that the relation is unique
            foreach (Tuple<float, CreatureFeature> cF in relations)
                if (cF.Item2 == relation)
                    return;
            // Add the relation
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

        static private Random rnd;


        /// <summary>
        /// Chromosome in bits and its amount
        /// </summary>
        BitArray chromosome;
        /// <summary>
        /// The current value of each gene
        /// It is ordered by CreatureFeatures 
        /// </summary>
        public int[] geneValues;

        //TODO: NO inclusivo, decir que gen ha petado

        /// <summary>
        /// Sets the internal extructure of the chromosome given the genes and their respectives max values.
        /// The genes MUST be given in such an order that there is no incomplete dependency between them.
        /// Example: Gene A depends on Gene B, therefore Gene B must be passed before Gene A
        /// </summary>
        public static void SetStructure(List<Gene> genes)
        {
            if (genes.Count != (int)CreatureFeature.Count)
                throw new Exception("The number of genes and max values must be the same as the total features");
            rnd = new Random();

            geneInfo = genes.ToArray();
            geneMaxValues = new int[genes.Count];
            for (int i = 0; i < genes.Count; ++i)
            {
                geneMaxValues[(int)genes[i].feature] = genes[i].maxValue;
            }

            genePos = new Tuple<int, int>[genes.Count];
            
            chromosomeSize = 1;
            float aux = 0;
            int aux2 = 0;
            int aux3 = 0;
            foreach (Gene gene in genes)
            {
                //The numeric value of the feature
                int featureIndex = (int)gene.feature;

                int relationsMaxValue = 0;
                foreach (Tuple<float, CreatureFeature> relation in gene.relations)
                {
                    //If the relation is positive, the max value of the gene can be surpassed so the max value of the relations is substracted
                    //If the relation is negative, the max value of the gene must not be surpassed with an addition, so it is not accounted.
                    //In other words, a negative relation may not modify the value, but if it is accounted for it would add extra bits surpassing the max value.
                    if (relation.Item1 > 0)
                        //The highest possible value of the features related is calculated (percentaje * maxValue)
                        relationsMaxValue += (int)Math.Ceiling(relation.Item1 * geneMaxValues[(int)relation.Item2]); //The percetaje gets truncated!!!
                    if (relation.Item1 > 0)
                        aux += relation.Item1 * geneMaxValues[(int)relation.Item2];
                }
                int leftover = geneMaxValues[featureIndex] - relationsMaxValue;
                aux2 += geneMaxValues[featureIndex];
                aux3 += relationsMaxValue;
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














        static public void SetChromosome()
        {
            List<Gene> genes = new List<Gene>();

            //Base Attributes

            Gene strength = new Gene(CreatureFeature.Strength, 50);
            genes.Add(strength);
            Gene constitution = new Gene(CreatureFeature.Constitution, 100);
            genes.Add(constitution);
            Gene fortitude = new Gene(CreatureFeature.Fortitude, 25);
            genes.Add(fortitude);
            Gene perception = new Gene(CreatureFeature.Perception, 50);
            genes.Add(perception);
            Gene aggressiveness = new Gene(CreatureFeature.Aggressiveness, 20);
            genes.Add(aggressiveness);
            Gene members = new Gene(CreatureFeature.Members, 50);
            genes.Add(members);
            //The rest of the genes IN ORDER OF DEPENDENCY

            //Other Attributes
            Gene resistence = new Gene(CreatureFeature.Resistence, 50);
            resistence.AddRelation(0.25f, CreatureFeature.Constitution);
            genes.Add(resistence);

            Gene piercing = new Gene(CreatureFeature.Piercing, 25);
            piercing.AddRelation(0.4f, CreatureFeature.Strength);
            genes.Add(piercing);


            //int maxSize = 80;
            Gene size = new Gene(CreatureFeature.Size, 200);
            size.AddRelation(0.4f, CreatureFeature.Constitution);
            size.AddRelation(0.25f, CreatureFeature.Strength);
            genes.Add(size);

            Gene knowledge = new Gene(CreatureFeature.Knowledge, 50);
            knowledge.AddRelation(0.1f, CreatureFeature.Size);
            knowledge.AddRelation(0.25f, CreatureFeature.Perception);
            genes.Add(knowledge);

            //if its a low value, it will be always 0 because of the negative dependency with size
            //int maxCamouflage = (int)(maxSize * 0.5);
            Gene camouflage = new Gene(CreatureFeature.Camouflage, 50);
            camouflage.AddRelation(-0.3f, CreatureFeature.Size);
            genes.Add(camouflage);//540

            Gene metabolism = new Gene(CreatureFeature.Metabolism, 50);
            metabolism.AddRelation(-0.2f, CreatureFeature.Size);
            genes.Add(metabolism);

            Gene idealTemp = new Gene(CreatureFeature.IdealTemperature, 50);
            idealTemp.AddRelation(0.15f, CreatureFeature.Metabolism);
            idealTemp.AddRelation(-0.25f, CreatureFeature.Size);
            genes.Add(idealTemp);

            Gene tempRange = new Gene(CreatureFeature.TemperatureRange, 20);
            tempRange.AddRelation(0.3f, CreatureFeature.Resistence);
            genes.Add(tempRange);

            Gene longevity = new Gene(CreatureFeature.Longevity, 50);
            longevity.AddRelation(-0.5f, CreatureFeature.Metabolism);
            genes.Add(longevity);

            Gene diet = new Gene(CreatureFeature.Diet, 15);
            diet.AddRelation(0.35f, CreatureFeature.Aggressiveness);
            genes.Add(diet);

            Gene mobility = new Gene(CreatureFeature.Mobility, 50);
            mobility.AddRelation(0.4f, CreatureFeature.Members);
            mobility.AddRelation(-0.2f, CreatureFeature.Size);
            mobility.AddRelation(-0.2f, CreatureFeature.Fortitude);
            genes.Add(mobility);

            //Abilities

            Gene arboreal = new Gene(CreatureFeature.Arboreal, 10);
            arboreal.AddRelation(-0.15f, CreatureFeature.Size);
            genes.Add(arboreal);
            Gene wings = new Gene(CreatureFeature.Wings, 10);
            wings.AddRelation(-0.3f, CreatureFeature.Size);
            genes.Add(wings);
            Gene venomous = new Gene(CreatureFeature.Venomous, 10);
            genes.Add(venomous);
            Gene nightvision = new Gene(CreatureFeature.NightVision, 10);
            nightvision.AddRelation(0.15f, CreatureFeature.Perception);
            genes.Add(nightvision);
            Gene horns = new Gene(CreatureFeature.Horns, 10);
            genes.Add(horns);
            Gene mimic = new Gene(CreatureFeature.Mimic, 10);
            genes.Add(mimic);
            Gene upright = new Gene(CreatureFeature.Upright, 10);
            genes.Add(upright);
            Gene thorns = new Gene(CreatureFeature.Thorns, 50);
            thorns.AddRelation(0.2f, CreatureFeature.Fortitude);
            genes.Add(thorns);
            Gene scavenger = new Gene(CreatureFeature.Scavenger, 10);
            genes.Add(scavenger);
            Gene hair = new Gene(CreatureFeature.Hair, 10);
            genes.Add(hair);
            Gene paternity = new Gene(CreatureFeature.Paternity, 10);
            genes.Add(paternity);

            CreatureChromosome.SetStructure(genes);
        }
    }
}
