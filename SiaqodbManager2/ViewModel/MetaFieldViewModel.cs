using Sqo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace SiaqodbManager.ViewModel
{
    class MetaFieldViewModel : INotifyPropertyChanged
    {
        private string name;

        public MetaFieldViewModel(MetaField field)
        {
            if(field.FieldType != null){
                Name = field.Name + "(" + field.FieldType.ToString() + ")";
            }
            else
            {
                Name = field.Name + "(ComplexType)";
            }
            FieldType = field.FieldType;
            ActualName = field.Name;
        }

        public MetaFieldViewModel()
        {
            // TODO: Complete member initialization
        }
        public Type FieldType { get; set; }

        public string Name {
            get { return name; }
            set { name = value;
                OnPropertyChanged();
            }
        }
        public string ActualName { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        //EVENT HANDLER
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
