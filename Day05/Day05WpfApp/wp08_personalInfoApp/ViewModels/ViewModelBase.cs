using System.ComponentModel;

namespace wp08_personalInfoApp.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        // 포로퍼티가 변경되면 시스템에 알려주는 역할
        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
