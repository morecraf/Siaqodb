using Microsoft.Win32;
using SiaqodbManager.MacWinInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiaqodbManager.DialogService
{
    class ReferenceFileService:IDialogService
    {
        public string OpenDialog()
        {
            OpenFileDialog opf = new OpenFileDialog();
            opf.Filter = "assembly files (*.dll;*.exe)|*.dll;*.exe";
            opf.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            opf.Multiselect = false;
            var res = opf.ShowDialog();
            if (res.HasValue && res.Value)
            {
                return opf.FileName;
            }
            return "";
        }
    }
}
