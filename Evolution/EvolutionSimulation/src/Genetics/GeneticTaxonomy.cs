using EvolutionSimulation.Entities;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace EvolutionSimulation.Genetics
{
    public class GeneticTaxonomy
    {
        struct Species
        {
            public string name;
            public CreatureChromosome chromosome;
            public List<Creature> members;

            public Species(Creature creature)
            {
                chromosome = creature.chromosome;
                members = new List<Creature>();
                members.Add(creature);
                name = "k";
            }
        }

        // The different weights of each gene when calculating the genetic similarity between two creatures
        float[] speciesGeneWeights;
        List<Species> existingSpecies;
        // The mininum genetic similarity to be the same species
        float minGeneticSimilarity;

        //Mapa extinctSpecies?

        /// <summary>
        /// Create the GeneticTaxonomy using both json
        /// </summary>
        /// <param name="jsonWeigths"> path with gene weights to calculate genetic similarities </param>
        /// <param name="jsonSimilarity">path with species similarity</param>
        public GeneticTaxonomy(string jsonWeigths, string jsonSimilarity)
        {
            //The different weights for each gene when calculating the similarities
            //between two creatures is read from the designated json file
            if (!File.Exists(jsonWeigths))
                throw new Exception("Cannot find JSON with gene weights to calculate genetic similarities");
           if (!File.Exists(jsonSimilarity))
                throw new Exception("Cannot find JSON with species similarity");
            string file = File.ReadAllText(jsonWeigths);
            //In tuples to facilitate the modification of the file
            //(the feature name is written instead of a number)
            Tuple<CreatureFeature, float>[] weights = JsonConvert.DeserializeObject<Tuple<CreatureFeature, float>[]>(file);
            if (weights.Length != (int)CreatureFeature.Count)
                throw new Exception("JSON with gene weights to calculate genetic similarities has an invalid size");
            speciesGeneWeights = new float[weights.Length];
            foreach (var t in weights)
            {
                speciesGeneWeights[(int)t.Item1] = t.Item2;
            }
            existingSpecies = new List<Species>();

            file = File.ReadAllText(jsonSimilarity);
            minGeneticSimilarity = JsonConvert.DeserializeObject<float>(file);
        }


        public void UpdateSpecies(Creature creature)
        {
            for (int i = 0; i < existingSpecies.Count; ++i)
            {
                if (GeneticSimilarity(creature.chromosome, existingSpecies[i].chromosome) > minGeneticSimilarity)
                {
                    existingSpecies[i].members.Add(creature);
                    return;
                }
            }
            Species newSpecies = new Species(creature);
            existingSpecies.Add(newSpecies);
        }

        /// <summary>
        /// Given 2 creatures, calculate the similarity of each gen in both chromosomes using 
        /// weigths to give more importance to gens that the user want
        /// To compare a gene check bit by bit is the same in the gene of both creatures
        /// </summary>
        /// <param name="weights"> weigths for each feature, the sum of all must be 1</param>
        /// <returns> Value between 0-1. 1 has the same values and 0 different values</returns>
        private float GeneticSimilarity(CreatureChromosome creature1, CreatureChromosome creature2)
        {

            if (speciesGeneWeights.Length != (int)CreatureFeature.Count - 1)
            {
                throw new Exception("Weigths must have the same length as CreatureFeatures");
            }
            float sum = 0;
            foreach (float w in speciesGeneWeights)
                sum += w;
            if (sum != 1)
            {
                throw new Exception("Weigths sum must be 1");
            }

            float total = 0;
            //Check the similarity of each gene
            for (int i = 0; i < (int)CreatureFeature.Count; ++i)
            {
                float genEqual = 0;
                BitArray gen1 = creature1.GetGeneBits((CreatureFeature)i);
                BitArray gen2 = creature2.GetGeneBits((CreatureFeature)i);
                //Comparate bit a bit
                for (int j = 0; j < gen1.Length; ++j)
                {
                    if (gen1[j] == gen2[j])
                        genEqual++;
                }

                genEqual /= gen1.Length;
                total += genEqual * speciesGeneWeights[i];
            }
            return total;
        }
    }
}
