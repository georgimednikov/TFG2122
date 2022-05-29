using System;
using System.Windows.Forms;

namespace Visualizador
{
    public class WindowLoadingBar
    {
        Form prompt;
        ProgressBar progressBar;
        Label text;

        public WindowLoadingBar(int max)
        {
            prompt = new Form()
            {
                Width = 280,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = "Evolution Simulation",
                StartPosition = FormStartPosition.CenterScreen
            };
            progressBar = new ProgressBar()
            {
                Visible = true,
                Width = 280,
                Height = 30,
                Top = 60,
                Minimum = 0,
                Maximum = max,
                Value = 0,
                Step = 1,
            };

            text = new Label() { Left = 0, Top = 25, Text = "Simulating... This may take a long time" };
            text.AutoSize = true;

            prompt.Controls.Add(progressBar);
            prompt.Controls.Add(text);
            prompt.Show();
            prompt.Refresh();
        }

        public void StepElapsed()
        {
            progressBar.Value++;
        }

    }
}
