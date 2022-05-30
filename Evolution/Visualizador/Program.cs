using System;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;
using EvolutionSimulation;
using EvolutionSimulation.IO;

namespace Visualizador
{
    static class Program
    {
        static WindowLoadingBar loadingBar;

        /// <summary>
        /// Application entry point
        /// </summary>
        [STAThread]
        static void Main()
        {
            EventSimulation s = new EventSimulation();
            s.OnSimulationBegin += (e) => { loadingBar = new WindowLoadingBar(UserInfo.Years); };
            s.OnSimulationStep += (e) => { if(e.CurrentTick % e.YearTicks == 0) loadingBar.StepElapsed(); };
#if DEBUG
            s.Init(10, 20, 20, "../../ProgramData/", "../../ResultingData/", null);
#else
            if (!AskInfoUsingWindows(s))
                return;
#endif
            s.Run();
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

            // Input data directory request
            browser.Description = "Choose the folder where the program will search for the data files. " +
                "After this window a new one will open; if it does not show, minimize any other application open.";
            browser.SelectedPath = Path.GetFullPath("../../ProgramData");
            DialogResult result = browser.ShowDialog();
            if (result == DialogResult.OK)
                dataDir = browser.SelectedPath + "\\";
            else
                return false;

            // Output data directory request
            browser.Description = "Choose the folder where the program will save the program's resulting data.";
            browser.SelectedPath = Path.GetFullPath("../../ResultingData");
            result = browser.ShowDialog();
            if (result == DialogResult.OK)
                exportDir = browser.SelectedPath + "\\";
            else
                return false;
            int userEntry;
            bool ret;
            // World information check and request if needed
            // If a simulation world is not given, a new one has to be generated.
            WorldGenConfig config = null;
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
                }
                else // If nothing is given, a size and a map type have to be requested from the user 
                {
                    // Map size request
                    int mapSize;                   
                    do
                    {
                        ret = InstantiatePrompt("Input how big in squares the world is going to be.\nMust be a number larger than or equal to: " + UserInfo.MinWorldSize(), out userEntry);
                    } while (!ret || userEntry < UserInfo.MinWorldSize());
                    mapSize = userEntry;

                    // Map type request
                    World.MapType[] mapTypes = (World.MapType[])Enum.GetValues(typeof(World.MapType));
                    int lengthWithoutLast = mapTypes.Length - 1;    // last type is custom, and cannot be provided this way
                    string label = "Input the number of the type of map that should be generated:\n";
                    string separator;
                    for (int i = 0; i < mapTypes.Length - 1; i++) 
                    {
                        separator = i % mapTypes.Length < lengthWithoutLast - 1 ? " | " : "";
                        label += $"<{i}> {mapTypes[i]}{separator}";
                    }
                    label += "\n";
                    do
                    {
                        ret = InstantiatePrompt(label, out userEntry, 360);                            
                    } while (!ret || userEntry < 0 || userEntry >= mapTypes.Length - 1);

                    config = new WorldGenConfig(mapTypes[userEntry])
                    {
                        mapSize = mapSize
                    };
                }
            }

            // Simulation duration request
            do
            {
                ret = InstantiatePrompt("Input how many years of evolution are going to be\nsimulated:", out userEntry);
            } while (!ret || userEntry < 0);
            years = userEntry;

            // Original species request
            int minSpecies = UserInfo.MinSpeciesAmount();
            do
            {
                ret = InstantiatePrompt("Input how many species are going to be created\ninitially. Must be a number larger than or equal to: " + UserInfo.MinSpeciesAmount(), out userEntry);
            } while (!ret || userEntry < minSpecies);
            species = userEntry;

            // Initial individuals per species request
            int minIndividuals = UserInfo.MinIndividualsAmount();
            do
            {
                ret = InstantiatePrompt("Input how many individuals per species are going to be\ncreated. Must be a number larger than or equal to: " + UserInfo.MinIndividualsAmount(), out userEntry);
            } while (!ret || userEntry < minIndividuals);
            individuals = userEntry;

            // Simulation initialization after all information has been provided
            s.Init(years, species, individuals, dataDir, exportDir, config);
            return true;
        }

        static private bool InstantiatePrompt(string text, out int value, int width = 280, int height = 150)
        {
            Form prompt = new Form()
            {
                Width = width,
                Height = height,
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
                try
                {
                    value = int.Parse(textBox.Text);
                    return true;
                }
                catch { value = -1; return false; }                
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
