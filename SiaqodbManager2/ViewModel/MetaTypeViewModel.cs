using Sqo;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace SiaqodbManager.ViewModel
{
    public class MetaTypeViewModel : INotifyPropertyChanged
    {
        private string name;
        public MetaType MetaType { get; set; }
        public MetaTypeViewModel(MetaType metaType)
        {
            var allFields = metaType.Fields.Select(f=>new MetaFieldViewModel(f));
            Fields = new ObservableCollection<MetaFieldViewModel>(allFields);
            MetaType = metaType;
            Name = metaType.Name;
        }

        public  ObservableCollection<MetaFieldViewModel> Fields {get;set;}

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        //EVENT HANDLER
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
