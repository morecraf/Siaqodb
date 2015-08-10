using Microsoft.Win32;
using SiaqodbManager.MacWinInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SiaqodbManager.DialogService
{
    class OpenDBFolderDialog:IDialogService
    {

        public string OpenDialog()
        {
            FolderBrowserDialog opf = new FolderBrowserDialog();
            var res = opf.ShowDialog();
            if (res == DialogResult.OK)
            {
                return opf.SelectedPath;
            }
            return "";
        }
    }
}
