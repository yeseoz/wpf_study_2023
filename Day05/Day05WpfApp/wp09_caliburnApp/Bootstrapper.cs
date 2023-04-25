using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using wp09_caliburnApp.ViewModels;

namespace wp09_caliburnApp
{
    internal class Bootstrapper:BootstrapperBase
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
