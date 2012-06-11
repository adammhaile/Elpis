using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RequestDecrypter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var unix = PandoraSharp.Time.Unix();
            Console.WriteLine(unix);
        }

        private void btnDecrypt_Click(object sender, RoutedEventArgs e)
        {
            //txtDecrypted.Text = PandoraSharp.PandoraCrypt.DecryptRPCRequest(txtEncrypted.Text);
        }
    }
}
