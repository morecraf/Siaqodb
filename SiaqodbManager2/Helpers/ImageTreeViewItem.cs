using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace SiaqodbManager.Helpers
{
    class ImageTreeViewItem : TreeViewItem
    {

        TextBlock text;

        Image img;

        ImageSource srcSelected, srcUnselected;

        public ImageTreeViewItem()
        {

            DockPanel stack = new DockPanel();

            //stack.Orientation = Orientation.Horizontal;

            Header = stack;

            img = new Image();

            img.VerticalAlignment = VerticalAlignment.Center;

            img.Margin = new Thickness(0, 0, 2, 0);

            img.Source = srcSelected;

            stack.Children.Add(img);

            text = new TextBlock();

            text.VerticalAlignment = VerticalAlignment.Center;

            stack.Children.Add(text);

        }

        public string Text
        {

            set { text.Text = value; }

            get { return text.Text; }

        }

        public ImageSource SelectedImage
        {

            set
            {

                srcSelected = value;

                img.Source = srcSelected;

            }

            get { return srcSelected; }

        }
        public static System.Windows.Media.Imaging.BitmapImage Createimage(string path)
        {

            System.Windows.Media.Imaging.BitmapImage myBitmapImage = new System.Windows.Media.Imaging.BitmapImage();

            myBitmapImage.BeginInit();

            myBitmapImage.UriSource = new Uri(path);

            myBitmapImage.EndInit();

            return myBitmapImage;

        }
        public ImageSource UnselectedImage
        {

            set
            {

                srcUnselected = value;

            }

            get { return srcUnselected; }

        }



        protected override void OnSelected(RoutedEventArgs args)
        {

            base.OnSelected(args);

            img.Source = srcSelected;

        }

        protected override void OnUnselected(RoutedEventArgs args)
        {

            base.OnUnselected(args);

            if (srcUnselected != null)

                img.Source = srcUnselected;

            else

                img.Source = srcSelected;

        }

    }
}
