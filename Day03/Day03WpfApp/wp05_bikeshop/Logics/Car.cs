using System.Windows.Media;

namespace wp05_bikeshop.Logics
{
    internal class Car:Notifier // 값이 바뀌는걸 인지해서 처리하겠다.
    {
        private string names;
        public string Names 
        {
            get => names;
            // 프로퍼티를 변경하는 것
            set
            {
                names = value;
                OnPropertyChanged("Names"); // Names 프로퍼티가 바꼈으니까 처리해 주세용!
            }
        } // Auto Property
        private double speed;
        public double Speed
        {
            get => speed;
            set
            {
                speed = value;
                OnPropertyChanged(nameof(Speed)); // == "Speed" 둘다 똑같은 것
            }
        }
        public Color Colorz { get; set; }
        public Human Driver { get; set; }
    }
}
