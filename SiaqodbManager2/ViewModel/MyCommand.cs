using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SiaqodbManager.ViewModel
{
    public class MyCommand<T> : ICommand
    {
        readonly Action<T> callback;

        public MyCommand(Action<T> callback)
        {
            this.callback = callback;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (callback != null) callback((T)parameter);
        }

        public event EventHandler CanExecuteChanged;
    }
}
