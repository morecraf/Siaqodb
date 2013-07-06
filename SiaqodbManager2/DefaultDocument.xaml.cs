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
using System.IO;
using AvalonDock;

using System.Reflection;
using System.Collections;
using System.Diagnostics;

namespace SiaqodbManager
{
    /// <summary>
    /// Interaction logic for Document.xaml
    /// </summary>
    public partial class DefaultDocument : DocumentContent
    {
        public DefaultDocument()
        {
            InitializeComponent();

            DataContext = this;
        }

        private void label3_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.Cursor = Cursors.Hand;
        }

        private void label3_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.Cursor = Cursors.Arrow;
        }

        private void label2_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.Cursor = Cursors.Hand;
        }

        private void label2_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.Cursor = Cursors.Arrow;
        }

        private void label1_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
             this.Cursor = Cursors.Hand;
        }

        private void label1_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.Cursor = Cursors.Arrow;
        }

        private void label3_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Process.Start("http://forum.siaqodb.com");
            }
            catch (Exception ex)
            {

            }
        }

        private void label1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Process.Start("http://siaqodb.com/?page_id=13");
            }
            catch (Exception ex)
            {

            }
        }

        private void label2_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Process.Start("mailto:support@siaqodb.com");
            }
            catch (Exception ex)
            {

            }
        }

        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Process.Start(AppDomain.CurrentDomain.BaseDirectory+"\\demo\\SiaqodbManager.mp4");
            }
            catch (Exception ex)
            {

            }
        }

        

      

        




    }
}
