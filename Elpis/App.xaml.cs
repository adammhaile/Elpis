using System;
using System.Collections.Generic;
using System.Windows;
using Microsoft.Shell;

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

        #region ISingleInstanceApp Members
        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            if (MainWindow != null)
            {
                var mw = (Elpis.MainWindow)MainWindow;
                mw.ShowWindow();
            }

            return true;
        }

        #endregion
    }
}