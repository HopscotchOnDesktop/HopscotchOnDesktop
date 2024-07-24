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
    /// Interaction logic for Play_Project.xaml
    /// </summary>
    public partial class Play_Project : Window
    {
        public Play_Project(string uuid)
        {
            InitializeComponent();
            HS.Properties.Settings.Default.Projects_Played += 1;
            HS.Properties.Settings.Default.Save();
            player.Source = new Uri("https://c.gethopscotch.com/e/" + uuid);
        }

        public Play_Project(int width, int height, WindowState windowState, double top, double left, string uuid)
        {
            InitializeComponent();
            this.Width = width;
            this.Height = height;
            this.WindowState = windowState;
            this.Top = top;
            this.Left = left;
            HS.Properties.Settings.Default.Projects_Played += 1;
            HS.Properties.Settings.Default.Save();
            player.Source = new Uri("https://c.gethopscotch.com/e/" + uuid);
        }

        private void MePage(object sender, MouseEventArgs e)
        {
            Community community = new Community(true, Convert.ToInt32(this.Width), Convert.ToInt32(this.Height), this.WindowState, this.Top, this.Left);
            community.Show();
            this.Close();
        }

        private void HomePage(object sender, MouseEventArgs e)
        {
            Home home = new Home(Convert.ToInt32(this.Width), Convert.ToInt32(this.Height), this.WindowState, this.Top, this.Left);
            home.Show();
            this.Close();
        }

        private void CommunityPage(object sender, MouseEventArgs e)
        {
            Community community = new Community(Convert.ToInt32(this.Width), Convert.ToInt32(this.Height), this.WindowState, this.Top, this.Left);
            community.Show();
            this.Close();
        }
    }
}