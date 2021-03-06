using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using EvolutionSimulation.Entities;
using EvolutionSimulation.IO;

namespace EvolutionSimulation.Genetics
{
    public class Species
    {

        public string name;
        public string progenitor;
        public Creature original;
        public List<Creature> members;

        public int startTick, endTick;

        public Species(Creature creature)
        {
            original = creature;
            //The species by default is the parents', so that's the progenitor.
            progenitor = creature.speciesName;

            name = NameGenerator.GenerateName(original.chromosome);
            //The name of the species the creature creates is assigned.
            creature.speciesName = name;
            creature.progenitorSpeciesName = progenitor;
            members = new List<Creature>();
            members.Add(creature);
        }
    }

    public struct SpeciesExport
    {
        public string name;
        public CreatureBaseStats stats;

        public SpeciesExport(string name, CreatureStats stats)
        {
            this.name = name;
            this.stats = stats.GetBaseStats();
        }

        public static SpeciesExport GetExportFromJSON(string json)
        {
            return JsonLoader.Deserialize<SpeciesExport>(json);
        }
    }

    public class GeneticTaxonomy
    {
        // The different weights of each gene when calculating the genetic similarity between two creatures
        static float[] speciesGeneWeights;
        // The mininum genetic similarity to be the same species
        static float minGeneticSimilarity;

        // List containing the information of every species that is alive
        List<Species> existingSpecies;
        // List containing the information of every species that has spawned, dead AND alive, as well as the forming tree.
        // A new species is inserted right after its progenitor, so the order to create the tree is mantained.
        // See RenderSpeciesTree for details about the tree structure
        List<Species> speciesRecord;

        TicksComparer ticksComparator;

        /// <summary>
        /// Create the GeneticTaxonomy. SetTaxonomy has to be called
        /// before calling any other taxonomy method
        /// </summary>
        public GeneticTaxonomy()
        {
            existingSpecies = new List<Species>();
            speciesRecord = new List<Species>();
            ticksComparator = new TicksComparer();
        }


        static public void SetTaxonomy()
        {
            SetTaxonomy(UserInfo.GeneSimilarityFile());
        }


        /// <summary>
        /// Initialized GeneticTaxonomy reading the weights and the genetic similarity threshold from the files named
        /// SimilarityGeneWeight.json and SimilaritySpecies.json that are supposed to be found in the given Data Directory.
        /// </summary>
        static public void SetTaxonomy(string geneWeightsRaw)
        {
            if (geneWeightsRaw == null)
                throw new Exception("Cannot find JSON with gene weights to calculate genetic similarities");

            //In tuples to facilitate the modification of the file
            //(the feature name is written instead of a number)
            Tuple<CreatureFeature, float>[] weights = JsonLoader.Deserialize<Tuple<CreatureFeature, float>[]>(geneWeightsRaw);
            Validator.Validate(weights);
            speciesGeneWeights = new float[weights.Length];
            foreach (var t in weights)
            {
                speciesGeneWeights[(int)t.Item1] = t.Item2;
            }

            minGeneticSimilarity = UniverseParametersManager.parameters.percentageSimilaritySpecies;

        }

        /// <summary>
        /// Search in the given list a species with the given name
        /// </summary>
        /// <returns> The species or null if the list doesn't contains a species with this name</returns>
        private Species FindSpecies(string name, List<Species> list)
        {
            foreach (Species sp in list)
                if (sp.name == name)
                    return sp;
            return null;
        }

        /// <summary>
        /// This method is supposed to be called when a new creature is spawned.
        /// It is added to an existing species or a new one is created based on its genetics.
        /// </summary>
        public void AddCreatureToSpecies(Creature creature)
        {
            Species sp = FindSpecies(creature.speciesName, existingSpecies);

            //The most similar valid species is saved to add the creature to its members, if there is one.
            if (sp != null)
            {
                float similarity = GeneticSimilarity(creature.chromosome, sp.original.chromosome);
                if (similarity > minGeneticSimilarity)
                {
                    creature.progenitorSpeciesName = sp.progenitor;
                    sp.members.Add(creature);
                    return;
                }
            }


            //Else a new species is created
            Species newSpecies = new Species(creature);
            existingSpecies.Add(newSpecies);
            newSpecies.startTick = newSpecies.endTick = creature.world.CurrentTick;
            //Now the new species is added to the record based on if it has a progenitor species or not

            //If the new species is made from scratch, it is simply added to the record
            if (newSpecies.progenitor == "None")
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
            if (i >= speciesRecord.Count)
            {
                throw new Exception("There is a discrepancy between the new  species' progenitor name and the existing species' names");
            }
        }

        /// <summary>
        /// Check what is the most similar choromose to the child comparing with his parents
        /// to give his speciesName. It could be that father's species is the progenitor of the mother's species
        /// or vice versa
        /// </summary>
        /// <returns> Returns the string of the species who is more similar to the child</returns>
        public string MostSimilarityParent(CreatureChromosome childChromosome, Creature father, Creature mother)
        {
            if (mother.speciesName == father.speciesName)
                return mother.speciesName;

            float similarityFather = GeneticSimilarity(childChromosome, father.chromosome);
            float similarityMother = GeneticSimilarity(childChromosome, mother.chromosome);
            if (similarityFather > similarityMother) return father.speciesName;
            return mother.speciesName;
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
                        int index = speciesRecord.FindIndex(x => x.name == sp.name);
                        speciesRecord[index].endTick = creature.world.CurrentTick;
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
            float total = 0;
            //Check the similarity of each gene
            for (int i = 0; i < (int)CreatureFeature.Count; ++i)
            {
                float genEqual = 0;
                BitArray gen1 = creature1.GetGeneBits((CreatureFeature)i);
                BitArray gen2 = creature2.GetGeneBits((CreatureFeature)i);
                //Compare bit by bit
                for (int j = 0; j < gen1.Length; ++j)
                {
                    if (gen1[j] == gen2[j])
                        genEqual++;
                }

                genEqual /= gen1.Length;
                //The value is multiplied by its weight, and the added values of the weights is 1
                //Therefore, the total value goes from 0 to 1
                total += genEqual * speciesGeneWeights[i];
            }
            return total;
        }


        /// <summary>
        /// Renders the record of all the species created, alive and dead, giving the dependencies between them
        /// SpeciesRecord has to be ordered, the progenitor has to be before the species or it will not work
        /// </summary>
        /// <param name="path">path to save in a file the speciesTree</param>
        public void RenderSpeciesTree(string path, int tick = 0)
        {
            StreamWriter writer = new StreamWriter(path);
            //runs through all species 
            for (int i = 0; i < speciesRecord.Count; ++i)
            {
                i = RenderSpeciesTree(speciesRecord[i].name, 0, speciesRecord[i].startTick, speciesRecord[i].endTick, i, writer);
            }
            writer.Close();
        }


        /// <summary>
        /// Recursive method that really saves the speciesTree in the file
        /// The stop condition is to be at the end of the speciesRecord list
        /// It receive the name of the species that is going to be save and analyze the next 
        /// species in the speciesRecord list 
        /// 
        /// We differentiate 4 cases:
        /// - The species being analyzed progenitor's name is the name of 
        /// the species that is being saved (parent-child) (A and A1)
        /// 
        /// - The species being analyzed progenitor's name is the progenitor's name 
        /// of the species that is being saved (siblings) (A11 and A12)
        /// 
        /// - The species being analyzed progenitor's name is a far aways progenitor's name 
        /// of the species that is being saved(cousin, aunts...)(A1 and A2)
        /// 
        /// -The species being analyzed progenitor's name is "None"(A and B)
        ///  
        /// Example
        /// A
        ///     A1
        ///         A11
        ///         A12
        ///     A2
        /// B
        /// </summary>
        /// <param name="name"> name of the species</param>
        /// <param name="lvl"> level of the generations</param>
        /// <param name="index"> index in the list </param>
        /// <param name="writer"> where to save the tree</param>
        /// <returns> Returns index to not check again the same species</returns>
        private int RenderSpeciesTree(string name, int lvl, int bornTick, int lastTick, int index, StreamWriter writer)
        {
            SaveTree(name, lvl, writer, bornTick, lastTick);

            // Stop condition, end of the species
            if (index >= speciesRecord.Count - 1) return index;


            // child
            if (speciesRecord[index + 1].progenitor == name)
                index = RenderSpeciesTree(speciesRecord[index + 1].name, ++lvl, speciesRecord[index + 1].startTick, speciesRecord[index + 1].endTick, ++index, writer);
            // sibling without childs between them
            else if (speciesRecord[index + 1].progenitor == speciesRecord[index].progenitor && speciesRecord[index].progenitor != "None")
                index = RenderSpeciesTree(speciesRecord[index + 1].name, lvl, speciesRecord[index + 1].startTick, speciesRecord[index + 1].endTick, ++index, writer);
            // sibling with childs between them
            else if (speciesRecord[index + 1].progenitor != "None")
            {
                bool siblings = false;
                int cont = index - 1;
                //find the lvl of the progenitor
                while (!siblings && cont > 0 && speciesRecord[cont].progenitor != "None")
                {
                    if (speciesRecord[cont].progenitor == speciesRecord[index + 1].progenitor)
                        siblings = true;
                    else
                        cont--;
                }
                if (siblings)
                    index = RenderSpeciesTree(speciesRecord[index + 1].name, lvl - 1, speciesRecord[index + 1].startTick, speciesRecord[index + 1].endTick, ++index, writer);
            }
            return index;
        }

        /// <summary>
        /// Method used to save the evolution tree over time
        /// </summary>
        /// <param name="name">Name of the species </param>
        /// <param name="lvl">Number of progenitors that the species has</param>
        /// <param name="writer">Where to save the tree</param>
        private void SaveTree(string name, int lvl, StreamWriter writer, int bornTick, int lastTick)
        {
            if (lvl != 0)
            {
                for (int i = 0; i < lvl; ++i)
                {
                    writer.Write("│" + "".PadLeft(4));
                }
                writer.WriteLine("└───" + name + "".PadLeft(4) + "First born tick: " + bornTick + "".PadLeft(4) + "Last alive tick: " + lastTick);
            }
            else
            {
                writer.WriteLine("├───" + name + "".PadLeft(4) + "First born tick: " + bornTick + "".PadLeft(4) + "Last alive tick: " + lastTick);
            }
        }

        /// <summary>
        /// Exports the existing species as JSONs in the folder "ResultingSpecies" named "SpeciesN" being n the order of writing.
        /// </summary>
        public void ExportSpecies(string path)
        {
            //Export all the existing species
            for (int i = 0; i < existingSpecies.Count; i++)
            {
                existingSpecies[i].endTick = existingSpecies[i].members[0].world.CurrentTick;
                speciesRecord[speciesRecord.FindIndex(x => x == existingSpecies[i])].endTick = existingSpecies[i].endTick;
                Species sp = existingSpecies[i];
                SpeciesExport export = new SpeciesExport(sp.name, sp.original.stats);
                string species = JsonConvert.SerializeObject(export, Formatting.Indented);
                File.WriteAllText($"{path}/Species_{i}.json", species);
            }

            speciesRecord.Sort(new TicksComparer());
            //Export at least the same number of species as the number of species that the simulation has begun
            int speciesToExport = UserInfo.Species - existingSpecies.Count;
            for (int i = 0; i < speciesRecord.Count && i < speciesToExport; i++)
            {
                if (existingSpecies.Contains(speciesRecord[i])) continue;
                Species sp = speciesRecord[i];
                SpeciesExport export = new SpeciesExport(sp.name, sp.original.stats);
                string species = JsonConvert.SerializeObject(export, Formatting.Indented);
                File.WriteAllText($"{path}/Species_{i + existingSpecies.Count}.json", species);
            }
        }

        /// <summary>
        /// Get the number of existing species 
        /// </summary>
        public int GetSpeciesNumber()
        {
            return existingSpecies.Count;
        }

        /// <summary>
        /// Check if the name of 2 species are related
        /// They are related if they are the same species, they have the same progenitor,
        /// one is the progenitor or grandprogenitor of the other
        /// </summary>
        /// <param name="speciesName1"></param>
        /// <param name="speciesName2"></param>
        /// <returns></returns>
        public bool AreRelated(string speciesName1, string speciesName2)
        {
            if (speciesName1 == speciesName2)
                return true;
            int index1 = speciesRecord.FindIndex(x => x.name == speciesName1);
            int index2 = speciesRecord.FindIndex(x => x.name == speciesName2);

            int indexP1 = speciesRecord.FindIndex(x => x.name == speciesRecord[index1].progenitor);
            int indexP2 = speciesRecord.FindIndex(x => x.name == speciesRecord[index2].progenitor);
            //it should not happend, but just in case
            if (index1 == -1 || index2 == -1)
                return false;

            if (speciesRecord[index1].name == speciesRecord[index2].progenitor ||    //parent-child
                    speciesRecord[index2].name == speciesRecord[index1].progenitor ||   //parent-child
                    (speciesRecord[index1].progenitor == speciesRecord[index2].progenitor && speciesRecord[index2].progenitor != "None"))  //siblins
                return true;

            if (indexP1 != -1 && speciesRecord[index2].name == speciesRecord[indexP1].progenitor)//grandparent-grandchild
                return true;
            if (indexP2 != -1 && speciesRecord[index1].name == speciesRecord[indexP2].progenitor)//grandparent-grandchild
                return true;
            return false;
        }

        /// <summary>
        /// Given a list of edible plants, these are ordered based on distance from it. The shortest goes first.
        /// </summary>
        private class TicksComparer : Comparer<Species>
        {

            public TicksComparer() { }

            public override int Compare(Species a, Species b)
            {
                return (b.endTick - b.startTick).CompareTo(a.endTick - a.startTick);
            }
        }

    }

}


