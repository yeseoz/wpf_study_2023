using MahApps.Metro.Controls;
using wp08_personalInfoApp.ViewModels;

namespace wp08_personalInfoApp.Views
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainView : MetroWindow
    {
        public MainView()
        {
            InitializeComponent();
            this.DataContext = new MainViewModel();// 뷰모델 바인딩외
            // 어떠한 이벤트핸들러 코드도 없음
        }
    }
}
