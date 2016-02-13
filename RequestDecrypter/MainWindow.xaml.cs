using System;
using System.Windows;

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
            //txtDecrypted.Text =
            string text = PandoraSharp.Crypto.out_key.Decrypt(txtEncrypted.Text);
        }
    }
}
