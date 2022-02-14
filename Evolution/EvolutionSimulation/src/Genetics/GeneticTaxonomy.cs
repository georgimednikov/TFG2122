using EvolutionSimulation.Entities;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace EvolutionSimulation.Genetics
{
    public class Species
    {
        public string name;
        public string progenitor;
        public CreatureChromosome chromosome;
        public List<Creature> members;

        public Species(Creature creature)
        {
            if (creature.species == null)
                progenitor = creature.species.name;

            else
                progenitor = "None";

            name = "Nombre";
            chromosome = creature.chromosome;
            members = new List<Creature>();
            members.Add(creature);
            creature.species = this;
        }
    }

    public class GeneticTaxonomy
    {
        // The different weights of each gene when calculating the genetic similarity between two creatures
        float[] speciesGeneWeights;
        List<Species> existingSpecies;
        // List containing the information of every species that has spawned, as well as the forming tree.
        // A new species is inserted right after its progenitor, so the order to create the tree is mantained.
        // See RenderSpeciesTree for details about the tree structure
        List<Species> speciesRecord;
        // The mininum genetic similarity to be the same species
        float minGeneticSimilarity;


        /// <summary>
        /// Create the GeneticTaxonomy using both json
        /// </summary>
        /// <param name="jsonWeigths">Path with gene weights to calculate genetic similarities </param>
        /// <param name="jsonSimilarity">Path with species similarity</param>
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


        /// <summary>
        /// This method is supposed to be called when a new creature is spawned.
        /// It is added to an existing species or a new one is created based on its genetics.
        /// </summary>
        public void AddCreatureToSpecies(Creature creature)
        {
            //If the creature belongs to an existing species, it is added to its members
            foreach (Species sp in existingSpecies)
            {
                if (GeneticSimilarity(creature.chromosome, sp.chromosome) > minGeneticSimilarity)
                {
                    sp.members.Add(creature);
                    return;
                }
            }
            //Else a new species is created
            Species newSpecies = new Species(creature);
            existingSpecies.Add(newSpecies);

            //If the new species is made from scratch, it is simply added
            if (creature.species.progenitor == "None")
            {
                speciesRecord.Add(newSpecies);
                return;
            }

            //If not, it is added after its progenitor, following the tree structure of speciesRecord
            int i = 0;
            for (; i < speciesRecord.Count; ++i)
            {
                if (speciesRecord[i].name == newSpecies.progenitor)
                {
                    speciesRecord.Insert(i + 1, newSpecies);
                    break;
                }
            }
            if (i == speciesRecord.Count)
                throw new Exception("There is a discrepancy between the new species' progenitor name and the existing species' names");
        }

        /// <summary>
        /// This method is supposed to be called when a new creature dies.
        /// It is removed from its species pool of members. If it was the last one, the species is extinct.
        /// </summary>
        public void RemoveCreatureToSpecies(Creature creature)
        {
            foreach (Species sp in existingSpecies)
            {
                int pos = sp.members.FindIndex(x => x == creature);
                if (pos != -1)
                {
                    sp.members.RemoveAt(pos);
                    if (sp.members.Count == 0)
                    {
                        existingSpecies.Remove(sp);
                    }
                    return;
                }
            }
            throw new Exception("A creature was tried to be removed from the list of species but it was never added. " +
                "AddCreatureToSpecies has to be called when a creature is created");
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

        /// <summary>
        /// Renders the record of all the species created, alive and dead, giving the dependencies between them
        /// </summary>
        public void RenderSpeciesTree()
        {

        }
    }
}
