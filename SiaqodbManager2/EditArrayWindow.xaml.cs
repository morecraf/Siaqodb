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

namespace SiaqodbManager
{
    /// <summary>
    /// Interaction logic for EditArrayWindow.xaml
    /// </summary>
    public partial class EditArrayWindow : Window
    {
        public EditArrayWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.textBox1.Focus();
        }
        public void SetArrayValue(Array arr)
        {
            foreach (object obj in arr)
            {
                if (textBox1.Text == string.Empty)
                {
                    this.textBox1.AppendText(obj.ToString());
                }
                else
                {
                    this.textBox1.AppendText(Environment.NewLine+ obj.ToString());
                }



            }
        }
        private Array values;
        public Array GetArrayValues()
        {
            return values;
        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

            if (textBox1.Text.Trim() != string.Empty)
            {
                try
                {
                    string[] arrayStr = textBox1.Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    values = Array.CreateInstance(elementType, arrayStr.Length);
                    for (int i = 0; i < arrayStr.Length; i++)
                    {
                        values.SetValue(Convert.ChangeType(arrayStr[i], elementType), i);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
            }
            else
            {
                values = Array.CreateInstance(elementType, 0);
            }
            this.DialogResult = true;
        }


        internal void SetArrayType(Type type)
        {
             this.elementType=type.GetElementType();
        }
        Type elementType;
    }
}
