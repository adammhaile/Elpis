using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Microsoft.Shell;
using NDesk.Options;
using Util;

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

 /*       protected override void OnStartup(StartupEventArgs e)
        {            
            base.OnStartup(e);
 
            bool show_help = false;
            string configPath = null;
            string startupStation = null;

            OptionSet p = new OptionSet()
              .Add("c|config=", "a {CONFIG} file to load ", delegate(string v) { configPath = v; })
              .Add("h|?|help", "show this message and exit", delegate(string v) { show_help = v != null; })
              .Add("s|station=", "start Elpis tuned to station \"{STATIONNAME}\" - puts quotes around station names with spaces", delegate(string v) { startupStation = v; });

            List<string> extra;
            try
            {
                p.Parse(e.Args);
            }
            catch (OptionException ex)
            {
                Console.Write("Elpis: ");
                Console.WriteLine(ex.Message);
                Console.WriteLine("Try `Elpis --help' for more information.");
                return;
            }

            if (startupStation != null)
            {
                Dispatcher.BeginInvoke(() => ((Elpis.MainWindow) MainWindow).StartupStation = startupStation);
            }

            ((Elpis.MainWindow)MainWindow).ConfigLocation = configPath;

            if (show_help)
            {
                ShowHelp(p);
                return;
            }
        }*/

        private void ShowHelp(OptionSet optionSet)
        {
            throw new NotImplementedException();
        }

        public bool HandleCommandLine(IList<string> args)
        {
            string station = null;
            bool playpause = false;
            bool skiptrack = false;
            bool thumbsUp = false;
            bool thumbsdown = false;
            bool show_help = false;
            string configPath = null;

            OptionSet p = new OptionSet()
               .Add("c|config=", "a {CONFIG} file to load ", delegate(string v) { configPath = v; })
               .Add("h|?|help", "show this message and exit", delegate(string v) { show_help = v != null; })
               .Add("playpause", "toggles playback", delegate(string v) { playpause = v != null; })
               .Add("next", "skips current track", delegate(string v) { skiptrack = v != null; })
               .Add("thumbsup", "rates the song as suitable for this station", delegate(string v) { thumbsUp = v != null; })
               .Add("thumbsdown", "rates the song as unsuitable for this station", delegate(string v) { thumbsdown = v != null; })
               .Add("s|station=", "starts station \"{STATIONNAME}\" - puts quotes around station names with spaces", delegate(string v) { station = v; });

            try
            {
                p.Parse(args);
            }
            catch (OptionException e)
            {
                //TODO: Throw up a dialogue?
            }

            if (skiptrack && MainWindow != null)
            {
                ((Elpis.MainWindow)MainWindow).SkipTrack(null, null);
            }

            if (playpause && MainWindow != null)
            {
                ((Elpis.MainWindow)MainWindow).PlayPauseToggled(null, null);
            }

            if (thumbsUp && MainWindow != null)
            {
                ((Elpis.MainWindow)MainWindow).ExecuteThumbsUp(null, null);
            }

            if (thumbsdown && MainWindow != null)
            {
                ((Elpis.MainWindow)MainWindow).ExecuteThumbsDown(null, null);
            }

            if (station != null && MainWindow != null)
            {
                ((Elpis.MainWindow)MainWindow).LoadStation(station);
            }

            if (MainWindow != null)
            {
                var mw = (Elpis.MainWindow)MainWindow;
                mw.ShowWindow();
            }

            return true;
        }

        #region ISingleInstanceApp Members
        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            return HandleCommandLine(args);
        }

        #endregion
    }
}