using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CPLib
{


    public class NotifyObject : System.ComponentModel.INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged( string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.OnPropertyChangedOverride(propertyName);                
            }
        }


        protected virtual void OnPropertyChangedOverride(string propertyName)
        {
            this.PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }

    }
}
