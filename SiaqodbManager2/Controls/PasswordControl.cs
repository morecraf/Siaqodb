using SiaqodbManager.MacWinInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SiaqodbManager.Controls
{
    public class PasswordControl:TextBox,IPasswordContainer
    {
        PasswordBox passwordBox;

        public PasswordControl()
        {
            passwordBox = new PasswordBox();         
        }
        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            if(Regex.IsMatch(Text, "^[/*]*$")){
                return;
            }
            int caretPosition=0;
            foreach(var change in e.Changes){
                passwordBox.Password = Password.Remove(change.Offset, change.RemovedLength);
                var changed = Text.Substring(change.Offset,change.AddedLength);
                passwordBox.Password = Password.Insert(change.Offset, changed);
            if (change.RemovedLength > 0)
            {
                caretPosition = change.Offset - change.RemovedLength;
            }
            if(change.AddedLength>0)
                caretPosition = change.AddedLength + change.Offset;
            }
            Text = Regex.Replace(Text, "[^*]", "*");
            CaretIndex = caretPosition;
            base.OnTextChanged(e);
        }
        public string Password
        {
            get
            {
                return passwordBox.Password;
            }
            set
            {
                var strBuilder = new StringBuilder();
                passwordBox.Password = value;
                for (int i = 0; i < value.Length;i++ )
                {
                    strBuilder.Append("*");
                }
                Text=strBuilder.ToString();
            }
        }
    }
}
