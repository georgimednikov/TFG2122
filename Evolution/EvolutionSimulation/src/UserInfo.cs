using System;
using System.IO;
using System.Windows.Forms;

namespace EvolutionSimulation
{
    static public class UserInfo
    {
        static public string DataDirectory;
        static public string ExportDirectory;
        static public int Size;
        static public int Years;
        static public int Species;
        static public int Individuals;

        /// <summary>
        /// Asks the user where to look for the files containing the different values for the calculation of the chromosme, genes and stats,
        /// as well as the folder in which to save the resulting species. This method uses a Windows window to do so.
        /// </summary>
        /// <returns></returns>
        static public bool AskInfoUsingWindows()
        {
            //FolderBrowserDialog browser = new FolderBrowserDialog();
            //browser.Description = "Choose the folder where the program will search for the data files. " +
            //    "After this window a new one will open; if it does not show, minimize any other application open.";
            //browser.SelectedPath = Path.GetFullPath("../../ProgramData");
            //DialogResult result = browser.ShowDialog();
            //DataDirectory = browser.SelectedPath + "\\";

            //if (result == DialogResult.OK)
            //    DataDirectory = browser.SelectedPath + "\\";
            //else
            //    return false;

            //browser.Description = "Choose the folder where the program will save the program's resulting data.";
            //browser.SelectedPath = Path.GetFullPath("../../ResultingSpecies");
            //result = browser.ShowDialog();
            //ExportDirectory = browser.SelectedPath + "\\";

            //if (result == DialogResult.OK)
            //    ExportDirectory = browser.SelectedPath + "\\";
            //else
            //    return false;

            do
            {
                if (!InstantiatePrompt("Input how many years of evolution are going to be\nsimulated:", out Years))
                    return false;
            } while (Years < 1);

            do
            {
                if (!InstantiatePrompt("Input how big in squares the world is going to be.\nMust be a number larger than: " + MinWorldSize(), out Size))
                    return false;
            } while (Size < MinWorldSize());

            do
            {
                if (!InstantiatePrompt("Input how many species are going to be created\ninitially. Must be a number larger than: " + MinSpeciesAmount(), out Species))
                    return false;
            } while (Species < MinSpeciesAmount());

            do
            {
                if (!InstantiatePrompt("Input how individuals per species are going to be\ncreated. Must be a number larger than: " + MinIndividualsAmount(), out Individuals))
                    return false;
            } while (Individuals < MinIndividualsAmount());

            return true;
        }

        static public void SetInformation(int size, int years, int species, int individuals, string dataDirectory, string exportDirectory)
        {
            DataDirectory = dataDirectory;
            ExportDirectory = exportDirectory;
            Size = size;
            Years = years;
            Species = species;
            Individuals = individuals;
        }

        /// <summary>
        /// Asks the user where to look for the files containing the different values for the calculation of the chromosme, genes and stats,
        /// as well as the folder in which to save the resulting species. This method uses the program's console to do so.
        /// </summary>
        /// <returns></returns>
        static public bool AskInfoUsingConsole()
        {
            do
            {
                Console.WriteLine("Input a valid directory containing the necessary data files for the program (chromosome.json, etc.):\n");
                DataDirectory = Console.ReadLine() + "\\";
                Console.Clear();
            } while (!Directory.Exists(DataDirectory));

            Console.WriteLine("Input a valid directory in which the resulting data will be saved:\n");
            ExportDirectory = Console.ReadLine() + "\\";
            Console.Clear();
            if (!Directory.Exists(ExportDirectory))
                ExportDirectory = Directory.CreateDirectory(ExportDirectory).FullName;

            do
            {
                Console.WriteLine("Input how many years of evolution are going to be simulated:");
                string input = Console.ReadLine();
                Years = -1;
                if (input != "") Years = Int32.Parse(input);
                Console.Clear();
            } while (Years < 0);

            int minSize = MinWorldSize();
            do
            {
                Console.WriteLine("Input how big in squares the world is going to be. Must be a number larger than: " + minSize + "\n");
                string input = Console.ReadLine();
                Size = -1;
                if (input != "") Size = Int32.Parse(input);
                Console.Clear();
            } while (Size < minSize);

            int minSpecies = MinSpeciesAmount();
            do
            {
                Console.WriteLine("Input how many species are going to be created initially. Must be a number larger than: " + minSpecies + "\n");
                string input = Console.ReadLine();
                Species = -1;
                if (input != "") Species = Int32.Parse(input);
                Console.Clear();
            } while (Species < minSpecies);

            int minIndividuals = MinIndividualsAmount();
            do
            {
                Console.WriteLine("Input how individuals per species are going to be created. Must be a number larger than: " + minIndividuals + "\n");
                string input = Console.ReadLine();
                Individuals = -1;
                if (input != "") Individuals = Int32.Parse(input);
                Console.Clear();
            } while (Individuals < minIndividuals);

            return true;
        }

        /// <summary>
        /// Only for debugging to not be asked constantly for the directories.
        /// </summary>
        static public void SetDebugInfo()
        {
            Years = 10;
            Size = 512;
            Species = 10;
            Individuals = 10;
            DataDirectory = "../../ProgramData/";
            ExportDirectory = "../../ResultingData/";
        }





        static private bool InstantiatePrompt(string text, out int value)
        {
            Form prompt = new Form()
            {
                Width = 280,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = "Evolution Simulation",
                StartPosition = FormStartPosition.CenterScreen
            };
            Label textLabel = new Label() { Left = 0, Top = 0, Text = text };
            textLabel.AutoSize = true;
            TextBox textBox = new TextBox() { Left = 65, Top = 40, Width = 150 };
            Button confirmation = new Button() { Text = "Ok", Left = 90, Width = 100, Top = 70, DialogResult = DialogResult.OK };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;
            DialogResult result = prompt.ShowDialog();

            if (result == DialogResult.OK && textBox.Text != "")
            {
                value = Int32.Parse(textBox.Text);
                return true;
            }
            else
            {
                value = -1;
                return false;
            }
        }

        static private int MinWorldSize() { return 100; }
        static private int MinSpeciesAmount() { return Size / 20; }
        static private int MinIndividualsAmount() { return 2; }
    }
}
