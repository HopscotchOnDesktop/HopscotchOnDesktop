using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HS
{
    /// <summary>
    /// Interaction logic for Launcher.xaml
    /// </summary>
    public partial class Launcher : Window
    {
        public Launcher()
        {
            InitializeComponent();
            if (HS.Properties.Settings.Default.Is_Setup)
            {
                Community community = new Community();
                community.Show();
                this.Close();
            }
            else
            {
                NewUser newUser = new NewUser();
                newUser.Show();
                this.Close();
            }
        }
    }
}
