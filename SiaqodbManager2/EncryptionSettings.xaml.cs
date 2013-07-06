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
using System.Windows.Shapes;
using Sqo;

namespace SiaqodbManager
{
    /// <summary>
    /// Interaction logic for EncryptionSettings.xaml
    /// </summary>
    public partial class EncryptionSettings : Window
    {
        public EncryptionSettings()
        {
            InitializeComponent();
        }

        private void checkBox1_Checked(object sender, RoutedEventArgs e)
        {
            textBox1.IsEnabled = true;
            cmbAlgo.IsEnabled = true;

        }
        private void checkBox1_Unchecked(object sender, RoutedEventArgs e)
        {
            textBox1.IsEnabled = false;
            cmbAlgo.IsEnabled = false;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Changing encryption settings will disconnect current database,continue?", "Continue", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                IsEncryptedChecked = checkBox1.IsChecked.Value;
                Algorithm = cmbAlgo.Text;
                Pwd = textBox1.Password;

                SetEncryptionSettings();
               
                this.DialogResult = true;

                this.Close();
            }
        }
        public static void SetEncryptionSettings()
        {
            SiaqodbConfigurator.EncryptedDatabase = IsEncryptedChecked;
            if (SiaqodbConfigurator.EncryptedDatabase)
            {
                SiaqodbConfigurator.SetEncryptor(Algorithm=="AES" ? BuildInAlgorithm.AES : BuildInAlgorithm.XTEA);

                if (!string.IsNullOrEmpty(Pwd))
                {
                    SiaqodbConfigurator.SetEncryptionPassword(Pwd);

                }

            }
        }
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {

        }
        
        public static bool IsEncryptedChecked { get; set; }
        public static string Algorithm { get; set; }
        public static string Pwd { get; set; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.checkBox1.IsChecked = IsEncryptedChecked;
            this.textBox1.Password = Pwd;
            this.cmbAlgo.Text = string.IsNullOrEmpty(Algorithm) ? "AES" : Algorithm;
        }

       
    }
}
