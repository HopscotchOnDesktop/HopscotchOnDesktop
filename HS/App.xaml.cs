using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace HS
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            HS.Properties.Settings.Default.Start_Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            HS.Properties.Settings.Default.Save();
            EventManager.RegisterClassHandler(typeof(Window), Window.UnloadedEvent, new RoutedEventHandler(Window_Unload));
        }

        void Window_Unload(object sender, RoutedEventArgs e)
        {
            long timespent = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - HS.Properties.Settings.Default.Start_Timestamp;
            HS.Properties.Settings.Default.Seconds_Spent += timespent;
            HS.Properties.Settings.Default.Start_Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            HS.Properties.Settings.Default.Save();
        }
    }
}
