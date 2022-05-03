using System;
using System.Windows.Forms;

namespace EvolutionSimulation
{
    public class LoadingBar
    {
        static public LoadingBar Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new LoadingBar();
                return _instance;
            }
        }
        static private LoadingBar _instance;

        public void Init(int max, bool console)
        {
            if (console) _instance = new ConsoleLoadingBar(max);
            else _instance = new WindowLoadingBar(max);
        }
        public virtual void StepElapsed() { }
        public virtual void NewAttempt() { }

        protected int attempts = 1;
    }

    public class ConsoleLoadingBar : LoadingBar
    {
        int max, current, barSize;

        public ConsoleLoadingBar(int max)
        {
            this.max = max;
            barSize = 10;
            current = 0;
            WriteLoadingBar();
        }

        public override void StepElapsed()
        {
            current++;
            WriteLoadingBar();
        }
        public override void NewAttempt()
        {
            attempts++;
            current = 0;
            WriteLoadingBar();
        }
        private void WriteLoadingBar()
        {
            Console.Clear();
            Console.WriteLine("Simulating... Attempts: " + attempts);
            Console.WriteLine();

            float percentage = (float)current / max * 100;
            int done = (int)(percentage / 10);

            for (int i = 0; i < done; i++) Console.Write("#");
            for (int i = 0; i < barSize - done; i++) Console.Write("-");
            Console.Write(" " + percentage + "% done");
        }
    }

    public class WindowLoadingBar : LoadingBar
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
                Text = "Loading Bar",
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
            attempts = 0;

            text = new Label() { Left = 0, Top = 25, Text = "Simulating... Attempts: " + attempts };
            text.AutoSize = true;

            prompt.Controls.Add(progressBar);
            prompt.Controls.Add(text);
            prompt.Show();
            prompt.Refresh();
        }

        public override void StepElapsed()
        {
            progressBar.Value++;
        }

        public override void NewAttempt()
        {
            attempts++;
            progressBar.Value = progressBar.Minimum;
            text.Text = "Simulating... Attempts: " + attempts;
            prompt.Refresh();
        }
    }
}
