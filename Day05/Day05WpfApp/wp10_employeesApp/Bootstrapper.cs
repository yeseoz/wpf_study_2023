using Caliburn.Micro;
using System.Windows;
using wp10_employeesApp.ViewModels;

namespace wp10_employeesApp
{
    public class Bootstrapper:BootstrapperBase
    {
        public Bootstrapper() 
        { 
            Initialize();
        }

        protected async override void OnStartup(object sender, StartupEventArgs e)
        {
            //base.OnStartup(sender, e);
            await DisplayRootViewForAsync<MainViewModel>();
        }
    }
}
