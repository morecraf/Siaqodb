using SiaqodbManager.MacWinInterface;
using Microsoft.Win32;
using SiaqodbManager.MacWinInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiaqodbManager.DialogService
{
    class SaveLinqDialogService : IDialogService
    {
        public string OpenDialog()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.DefaultExt = ".linq";
            saveFileDialog.Filter = "(*.linq)|*.linq|All Files(*.*)|*.*";
        
            saveFileDialog.ShowDialog();
            return saveFileDialog.FileName;
        }
    }
}
