using EvolutionSimulation.Genetics;

namespace EvolutionSimulation
{
    static public class NameGenerator
    {
        static public string GenerateName(CreatureChromosome ch)
        {
            //Vowels usable in the name
            char[] vowels = { 'a', 'e', 'i', 'o', 'u' };
            float[] vowelWeights = { 0.25f, 0.25f, 0.2f, 0.15f, 0.15f };
            //Consonants usable in the name
            char[] consonants = { 'b', 'c', 'd', 'n', 'g', 'h', 't', 's', 'l', 'm', 'f', 'p', 'r', 'k', 'j', 'v', 'w', 'x', 'z', 'y' };
            float[] consWeights = { 0.06f, 0.06f, 0.06f, 0.06f, 0.06f, 0.06f, 0.06f, 0.06f, 0.06f, 0.06f, 0.06f, 0.06f, 0.06f, 0.02f, 0.035f, 0.05f, 0.015f, 0.02f, 0.03f, 0.05f, };
            string name = "";
            bool space = false; //If the space has been placed or not
            int amount, num = (int)CreatureFeature.Count; //Number of genes
            double value, total;

            int i = 0;
            while (i < num) //While there are genes to translate
            {
                value = total = 0;
                amount = RandomGenerator.Next(4, 6); //How many genes are going to be translated at the same time
                for (int j = 0; j < amount && i < num; ++j)
                {
                    value += ch.GetFeature((CreatureFeature)i); //The values are added
                    total += ch.GetFeatureMax((CreatureFeature)i++);
                }

                name += consonants[GetIndexByWeight(value / total, consWeights)];

                //After the consonant a vowel is added, with a 16.6% a second one is added too
                //+2 to make the first 2 vowels in the array more common
                name += vowels[GetIndexByWeight(RandomGenerator.NextDouble(), vowelWeights)];
                if (RandomGenerator.Next() % 6 == 0)
                    name += vowels[GetIndexByWeight(RandomGenerator.NextDouble(), vowelWeights)];

                //If the space has already been placed skip the following code
                if (space) continue;
                //If 70% of the genes have been processed the space is always placed
                //Otherwise, if any amount between 30 and 70% have been processed,
                //there's a 33.3% chance of the space being placed
                if (i >= num * 0.7 || (i >= num * 0.3 && RandomGenerator.Next() % 3 == 0))
                {
                    name += ' ';
                    space = true;
                }
            }

            return name;
        }

        static private int GetIndexByWeight(double value, float[] weights)
        {
            float count = 0.001f; //In case the weights are slightly below 1 and value is precisely 1
            for (int i = 0; i < weights.Length; ++i)
            {
                count += weights[i];
                if (value <= count)
                    return i;
            }
            return -1;
        }
    }
}
