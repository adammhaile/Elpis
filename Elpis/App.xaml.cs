using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Microsoft.Shell;
using NDesk.Options;
using Util;
using System.IO;

namespace Elpis
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, ISingleInstanceApp
    {

        [STAThread]
        public static void Main()
        {
            if (SingleInstance<App>.InitializeAsFirstInstance("ElpisInstance"))
            {
                var application = new App();
                application.Init();
                application.Run();

                // Allow single instance code to perform cleanup operations
                SingleInstance<App>.Cleanup();
            }
        }

        public void Init()
        {
            this.InitializeComponent();
        }

        protected override void OnStartup(StartupEventArgs e)
        {       
            base.OnStartup(e);
            HandleCommandLine(System.Environment.GetCommandLineArgs());
            if(Elpis.MainWindow._clo.ShowHelp)
                Application.Current.Shutdown();
        }

        private void ShowHelp(OptionSet optionSet, string msg = null)
        {
            StringWriter sw = new StringWriter();
            optionSet.WriteOptionDescriptions(sw);
            string output = sw.ToString();
            if(msg != null)
                output += "\r\n\r\n" + msg;
            MessageBox.Show(output, "Elpis Options");
        }

        public bool HandleCommandLine(IList<string> args)
        {
            CommandLineOptions clo = new CommandLineOptions();
            OptionSet p = new OptionSet()
               .Add("c|config=", "a {CONFIG} file to load ", delegate(string v) { clo.ConfigPath = v; })
               .Add("h|?|help", "show this message and exit", delegate(string v) { clo.ShowHelp = v != null; })
               .Add("playpause", "toggles playback", delegate(string v) { clo.TogglePlayPause = v != null; })
               .Add("next", "skips current track", delegate(string v) { clo.SkipTrack = v != null; })
               .Add("thumbsup", "rates the song as suitable for this station", delegate(string v) { clo.DoThumbsUp = v != null; })
               .Add("thumbsdown", "rates the song as unsuitable for this station", delegate(string v) { clo.DoThumbsDown = v != null; })
               .Add("s|station=", "starts station \"{STATIONNAME}\" - puts quotes around station names with spaces", delegate(string v) { clo.StationToLoad = v; })
               .Add("exit|quit", "exits Elpis", delegate(string v) { clo.Exit = v != null; });

            try
            {
                p.Parse(args);
            }
            catch (OptionException e)
            {
                clo.ShowHelp = true;
                Elpis.MainWindow.SetCommandLine(clo);
                ShowHelp(p, e.Message);
            }

            Elpis.MainWindow.SetCommandLine(clo);

            if (clo.ShowHelp)
            {
                ShowHelp(p);
            }
            else
            {
                if (MainWindow != null)
                {
                    ((Elpis.MainWindow)MainWindow).DoCommandLine();
                }
            }

            return true;
        }

        #region ISingleInstanceApp Members
        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            //only bring app to front if called with no arguments.  If argument was passed assume user
            //is calling exe for control purposes only and does not want the app to be brought to front
            if (args.Count == 1)
            {
                ((Elpis.MainWindow)MainWindow).Show();
                ((Elpis.MainWindow)MainWindow).Activate();
                ((Elpis.MainWindow)MainWindow).WindowState = WindowState.Normal;
                ((Elpis.MainWindow)MainWindow).ShowInTaskbar = true;
            }

            return HandleCommandLine(args);
        }

        #endregion
    }
}