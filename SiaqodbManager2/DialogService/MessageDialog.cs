using SiaqodbManager.MacWinInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SiaqodbManager.DialogService
{
    public class MessageDialog:IMessageBox
    {
        public void Show(string message)
        {
            MessageBox.Show(message);
        }


        public bool Show(string message, string title, bool YesNo)
        {
            bool result;
            if (YesNo){
                var response= MessageBox.Show(message,title,MessageBoxButton.YesNo);
                result = response==MessageBoxResult.Yes;
            }
            else
            {
                MessageBox.Show(message, title);
                result = true;
            }
            return result;
        }
    }
}
