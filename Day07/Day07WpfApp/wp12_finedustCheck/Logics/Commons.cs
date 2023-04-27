using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System.Threading.Tasks;
using System.Windows;

namespace wp12_finedustCheck
{
    public class Commons
    {
        // MySql연결 문자열
        public static readonly string myconnString = "Server=localhost;" +
                                                     "Port=3306;" +
                                                     "Database=miniproject;" + 
                                                     "Uid=root;" + 
                                                     "Password=12345;"; // Password도 가능

        // 메트로 다이럴로그창을 위한 정적 메서드
        public static async Task<MessageDialogResult> ShowMessageAsync(string title, string message,
            MessageDialogStyle style= MessageDialogStyle.Affirmative)
        {
            return await ((MetroWindow)Application.Current.MainWindow).ShowMessageAsync(title, message, style, null);
        }
    }
}
