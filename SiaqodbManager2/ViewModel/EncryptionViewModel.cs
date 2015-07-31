using SiaqodbManager.MacWinInterface;
using Sqo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SiaqodbManager.ViewModel
{
    public class EncryptionViewModel : INotifyPropertyChanged
    {
        private  bool isEncryptedChecked;
        private  string algorithm;
        public  IPasswordContainer passwordCont;
        public IMessageBox ConfirmationBox { get; set; }

        private static EncryptionViewModel UniqueInstance;

        public static EncryptionViewModel Instance
        {
            get{
                if(UniqueInstance == null)
                    UniqueInstance = new EncryptionViewModel();
                return UniqueInstance;
            }
        }
        private EncryptionViewModel()
        {
            EncryptCommand = new MyCommand<object>(OnEncryption);
            Algorithm = "AES";
        }

        private void OnEncryption(object obj)
        {
            passwordCont = obj as IPasswordContainer;
            if(ConfirmationBox == null){
                return;
            }
            if(ConfirmationBox.Show("Changing encryption settings will disconnect current database,continue?","Encryption",true)){
                if(passwordCont != null){
                    SetEncryptionSettings();
                }
                OnClosingRequest();
                if(Parent != null){
                    Parent.ClearTypes();
                    Parent.Dispose();
                }
            }
        }

        public  bool IsEncryptedChecked
        {
            get{
                return isEncryptedChecked;
            }
            set{
                isEncryptedChecked = value;
                OnPropertyChanged();
            }
        }

        //private void btnOK_Click(object sender, RoutedEventArgs e)
        //{
        //    if (MessageBox.Show("Changing encryption settings will disconnect current database,continue?", "Continue", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
        //    {
        //        SetEncryptionSettings();
        //        this.Close();
        //    }
        //}
        public  void SetEncryptionSettings()
        {
            SiaqodbConfigurator.EncryptedDatabase = IsEncryptedChecked;
            if (SiaqodbConfigurator.EncryptedDatabase)
            {
                SiaqodbConfigurator.SetEncryptor(Algorithm == "AES" ? BuildInAlgorithm.AES : BuildInAlgorithm.XTEA);

                if (!string.IsNullOrEmpty(passwordCont.Password))
                {
                    SiaqodbConfigurator.SetEncryptionPassword(passwordCont.Password);
                }
            }
        }

        public  string Algorithm
        {
            get
            {
                return algorithm;
            }
            set
            {
                algorithm = value;
                OnPropertyChanged();
            }
        }

        public MyCommand<object> EncryptCommand { get; set; }

        //EVENT HANDLER
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public  string Pwd
        {
            get
            {
                if(passwordCont != null){
                    return passwordCont.Password; 
                }
                return "";
            }
        }
        public void OnClosingRequest()
        {
            if(ClosingRequest != null){
                ClosingRequest(this,new EventArgs());
            }
        }

        public EventHandler<EventArgs> ClosingRequest;

        public MainViewModel Parent { get; set; }
    }
}
