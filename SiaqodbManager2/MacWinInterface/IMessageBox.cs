using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiaqodbManager.MacWinInterface
{
    public interface IMessageBox
    {
        void Show(string message);

        bool Show(string message, string title, bool YesNo);
    }
}
