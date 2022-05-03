using System;
using System.IO;
using System.Windows.Forms;
using EvolutionSimulation;
using Newtonsoft.Json;

namespace Visualizador
{
    static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Simulation s = new Simulation();
#if DEBUG
            s.Init(10, 20, 20, "../../ProgramData/", "../../ResultingData/", null);
#else
            if (!AskInfoUsingWindows(s))
                return;
#endif
            s.Run();
            s.Export();
        }

        #region AskInfo
        /// <summary>
        /// Asks the user where to look for the files containing the different values for the calculation of the chromosme, genes and stats,
        /// as well as the folder in which to save the resulting species. This method uses a Windows window to do so.
        /// </summary>
        /// <returns></returns>
        static bool AskInfoUsingWindows(Simulation s)
        {
            string dataDir, exportDir;
            int years, species, individuals;

            FolderBrowserDialog browser = new FolderBrowserDialog();
            browser.Description = "Choose the folder where the program will search for the data files. " +
                "After this window a new one will open; if it does not show, minimize any other application open.";
            browser.SelectedPath = Path.GetFullPath("../../ProgramData");
            DialogResult result = browser.ShowDialog();
            //UserInfo.DataDirectory = browser.SelectedPath + "\\";

            if (result == DialogResult.OK)
                dataDir = browser.SelectedPath + "\\";
            else
                return false;

            browser.Description = "Choose the folder where the program will save the program's resulting data.";
            browser.SelectedPath = Path.GetFullPath("../../ResultingSpecies");
            result = browser.ShowDialog();
            //UserInfo.ExportDirectory = browser.SelectedPath + "\\";

            if (result == DialogResult.OK)
                exportDir = browser.SelectedPath + "\\";
            else
                return false;

            int userEntry;
            do
            {
                if (!InstantiatePrompt("Input how many years of evolution are going to be\nsimulated:", out userEntry))
                    return false;
            } while (userEntry < 1);
            years = userEntry;

            WorldGenConfig config = null;

            // If a simulation world is not given, a new one has to be created.
            if (!File.Exists(dataDir + UserInfo.WorldName) || !File.Exists(dataDir + UserInfo.RegionMapName))
            {
                // If a height map is provided, it is not created from scratch.
                if (File.Exists(dataDir + UserInfo.HeightMapName))
                {
                    string map = File.ReadAllText(dataDir + UserInfo.HeightMapName);
                    float[,] heights = JsonConvert.DeserializeObject<float[,]>(map);
                    config = new WorldGenConfig(World.MapType.Custom)
                    {
                        heightMap = heights,
                        mapSize = heights.GetLength(0)
                    };
                    UserInfo.Size = config.mapSize;
                }
                else // if nothing is given, a size has to be asked of the user. 
                {
                    do
                    {
                        if (!InstantiatePrompt("Input how big in squares the world is going to be.\nMust be a number larger than or equal to: " + UserInfo.MinWorldSize(), out userEntry))
                            return false;
                    } while (userEntry < UserInfo.MinWorldSize());
                    UserInfo.Size = userEntry;
                }
            }

            do
            {
                if (!InstantiatePrompt("Input how many species are going to be created\ninitially. Must be a number larger than or equal to: " + UserInfo.MinSpeciesAmount(), out userEntry))
                    return false;
            } while (userEntry < UserInfo.MinSpeciesAmount());
            species = userEntry;

            do
            {
                if (!InstantiatePrompt("Input how individuals per species are going to be\ncreated. Must be a number larger than or equal to: " + UserInfo.MinIndividualsAmount(), out userEntry))
                    return false;
            } while (userEntry < UserInfo.MinIndividualsAmount());
            individuals = userEntry;

            s.Init(years, species, individuals, dataDir, exportDir, config);
            LoadingBar.Instance.Init(years, false);

            return true;
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
                value = int.Parse(textBox.Text);
                return true;
            }
            else
            {
                value = -1;
                return false;
            }
        }
        #endregion
    }
}
