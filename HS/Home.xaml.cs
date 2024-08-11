using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : Window
    {
        public Home(int width, int height, WindowState windowState, double top, double left)
        {
            InitializeComponent();
            this.Width = width;
            this.Height = height;
            this.WindowState = windowState;
            this.Top = top;
            this.Left = left;
            this.SizeChanged += OnWindowSizeChanged;
        }

        protected void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            int i = 0;
            foreach (var item in grid.RowDefinitions)
            {
                i++;
                if (i % 2 == 0)
                {
                    item.Height = new GridLength(((Grid)item.Parent).ColumnDefinitions[1].ActualWidth);
                }
            }
        }

        private void EditProject(object sender, EventArgs e)
        {
            if (EditProjectID.Text.ToString() != "" && EditProjectID.Text.ToString() != null)
            {
                Edit editproject = new Edit(EditProjectID.Text.ToString());
                editproject.Show();
            }
        }

        private void OpenTile(object sender, MouseEventArgs e)
        {
            if (sender is Image _sender)
            {
                string _class = _sender.Tag.ToString();
                Assembly asm = this.GetType().Assembly;
                object wnd = asm.CreateInstance(_class);
                if (wnd == null)
                {
                    MessageBox.Show("Unable to create window: " + _class);
                }
                ((Window)wnd).Show();
            }
        }

        private void CommunityPage(object sender, MouseEventArgs e)
        {
            Community community = new Community(Convert.ToInt32(this.Width), Convert.ToInt32(this.Height), this.WindowState, this.Top, this.Left);
            community.Show();
            this.Close();
        }

        private void MePage(object sender, MouseEventArgs e)
        {
            Community community = new Community(true, Convert.ToInt32(this.Width), Convert.ToInt32(this.Height), this.WindowState, this.Top, this.Left);
            community.Show();
            this.Close();
        }

        private void settings(object sender, MouseEventArgs e)
        {
            Settings settings = new Settings();
            settings.Show();
        }
    }

    public class ShortenSeconds : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var a = value as long?;
            if (a != null)
            {
                return a.ShortenSeconds();
            }
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class GetSuffix : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var a = value as long?;
            if (a != null)
            {
                return a.GetSuffix();
            }
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public static class LongExt
    {
        public static string ShortenSeconds(this long? num)
        {
            if (num >= 31536000)
            {
                return Math.Floor((decimal)(num / 31536000)).ToString();
            }
            if (num >= 2628000)
            {
                return Math.Floor((decimal)(num / 2628000)).ToString();
            }
            if (num >= 86400)
            {
                return Math.Floor((decimal)(num / 86400)).ToString();
            }
            else if (num >= 3600)
            {
                return Math.Floor((decimal)(num / 3600)).ToString();
            }
            else if (num >= 60)
            {
                return Math.Floor((decimal)(num / 60)).ToString();
            }
            else
            {
                return num.ToString();
            }
        }

        public static string GetSuffix(this long? num)
        {
            if (num >= 31536000)
            {
                return " years";
            }
            if (num >= 2628000)
            {
                return " mnths";
            }
            if (num >= 86400)
            {
                return " days";
            }
            else if (num >= 3600)
            {
                return " hrs";
            }
            else if (num >= 60)
            {
                return " mins";
            }
            else
            {
                return " secs";
            }
        }
    }
}
