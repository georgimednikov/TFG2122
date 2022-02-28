using System;
using System.IO;
using System.Windows.Forms;

namespace EvolutionSimulation
{
    static public class UserInfo
    {
        static public int Years;
        static public string DataDirectory;
        static public string ExportDirectory;

        /// <summary>
        /// Asks the user where to look for the files containing the different values for the calculation of the chromosme, genes and stats,
        /// as well as the folder in which to save the resulting species. This method uses a Windows window to do so.
        /// </summary>
        /// <returns></returns>
        static public bool AskInfoUsingWindows()
        {
            FolderBrowserDialog browser = new FolderBrowserDialog();
            browser.Description = "Choose the folder where the program will search for the data files. " +
                "After this window a new one will open; if it does not show, minimize any other application open.";
            browser.SelectedPath = Path.GetFullPath("../../ProgramData");
            DialogResult result = browser.ShowDialog();
            DataDirectory = browser.SelectedPath + "\\";

            if (result == DialogResult.OK)
                DataDirectory = browser.SelectedPath + "\\";
            else
                return false;

            browser.Description = "Choose the folder where the program will save the program's resulting data.";
            browser.SelectedPath = Path.GetFullPath("../../ResultingSpecies");
            result = browser.ShowDialog();
            ExportDirectory = browser.SelectedPath + "\\";

            if (result == DialogResult.OK)
                ExportDirectory = browser.SelectedPath + "\\";
            else
                return false;

            Form prompt = new Form()
            {
                Width = 280,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = "Evolution Simulation",
                StartPosition = FormStartPosition.CenterScreen
            };
            Label textLabel = new Label() { Left = 0, Top = 0, Text = "Input how many years of evolution\nare going to be simulated:" };
            textLabel.AutoSize = true;
            TextBox textBox = new TextBox() { Left = 65, Top = 40, Width = 150 };
            Button confirmation = new Button() { Text = "Ok", Left = 90, Width = 100, Top = 70, DialogResult = DialogResult.OK };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;
            prompt.ShowDialog();

            if (result == DialogResult.OK && textBox.Text != "")
                Years = Int32.Parse(textBox.Text);
            else
                return false;

            return true;
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

            do
            {
                Console.WriteLine("Input a valid directory in which the resulting data will be saved:\n");
                ExportDirectory = Console.ReadLine() + "\\";
                Console.Clear();
            } while (!Directory.Exists(ExportDirectory));

            do
            {
                Console.WriteLine("Input how many years of evolution are going to be simulated:\n");
                string input = Console.ReadLine();
                Years = -1;
                if (input != "") Years = Int32.Parse(input);
                Console.Clear();
            } while (Years < 0);

            return true;
        }

        /// <summary>
        /// Only for debugging to not be asked constantly for the directories.
        /// </summary>
        static public void SetDebugInfo()
        {
            DataDirectory = "../../ProgramData/";
            ExportDirectory = "../../ResultingData/";
            Years = 10;
        }
    }
}
