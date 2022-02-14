using EvolutionSimulation.Genetics;

namespace EvolutionSimulation
{
    static public class NameGenerator
    {
        static public string GenerateName(CreatureChromosome ch)
        {
            //Vowels usable in the name
            char[] vowels = { 'a', 'e', 'i', 'o', 'u' };
            //Consonants usable in the name
            char[] consonants = { 'b', 'c', 'd', 'n', 'g', 'h', 't', 's', 'l', 'm', 'f', 'p', 'r', 'k', 'j', 'v', 'x', 'z', 'y' };
            string name = "";
            bool space = false; //If the space has been placed or not
            int value, amount, num = (int)CreatureFeature.Count; //Number of genes

            int i = 0;
            while (i < num) //While there are genes to translate
            {
                value = 0;
                amount = RandomGenerator.Next(4, 6); //How many genes are going to be translated at the same time
                for (int j = 0; j < amount && i < num; ++j)
                    value += ch.GetFeature((CreatureFeature)i++); //The values are added

                //With a 20% chance one of the first, better sounding consonants, is always chosen
                if (RandomGenerator.Next() % 5 == 0)
                    name += consonants[value % (consonants.Length / 2)];
                else
                    name += consonants[value % consonants.Length];

                //After the consonant a vowel is added, with a 20% a second one is added too
                //+2 to make the first 2 vowels in the array more common
                name += vowels[RandomGenerator.Next(vowels.Length + 2) % vowels.Length];
                if (RandomGenerator.Next() % 5 == 0)
                    name += vowels[RandomGenerator.Next(vowels.Length + 2) % vowels.Length];

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
    }
}
