using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Controls.Ribbon.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using static HS.Play.blocktypes;

namespace HS
{
    /// <summary>
    /// Interaction logic for Community.xaml
    /// </summary>
    public partial class Community : Window
    {
        public Community()
        {
            InitializeComponent();

            this.WindowState = WindowState.Maximized;
            this.SizeChanged += OnWindowSizeChanged;
            loading_img.BeginInit();
            loading_img.UriSource = new Uri("pack://application:,,,/Assets/General/hopscotch-logo.png");
            loading_img.EndInit();
            display();
        }

        public Community(int width, int height, WindowState windowState, double top, double left)
        {
            InitializeComponent();
            this.Width = width;
            this.Height = height;
            this.WindowState = windowState;
            this.Top = top;
            this.Left = left;
            this.SizeChanged += OnWindowSizeChanged;
            loading_img.BeginInit();
            loading_img.UriSource = new Uri("pack://application:,,,/Assets/General/hopscotch-logo.png");
            loading_img.EndInit();
            display();
        }

        public Community(bool isMe, int width, int height, WindowState windowState, double top, double left)
        {
            InitializeComponent();
            this.Width = width;
            this.Height = height;
            this.WindowState = windowState;
            this.Top = top;
            this.Left = left;
            this.SizeChanged += OnWindowSizeChanged;
            loading_img.BeginInit();
            loading_img.UriSource = new Uri("pack://application:,,,/Assets/General/hopscotch-logo.png");
            loading_img.EndInit();
            displayUserNoBitmap(HS.Properties.Settings.Default.User_Id);
        }

        protected void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
           projects_grid.MaxWidth = e.NewSize.Width;
            if (projects_grid.Children.Count > 0)
            {
                if (categories.SelectedIndex == 0)
                {
                    if (((ComboBoxItem)search_options.SelectedItem).Tag.ToString() == "1")
                    {
                        display_projects(0, new TabControl(), null, false, 0, true, global_search_val);
                    }
                }
                else if (!is_userpage)
                {
                    display_projects(global_page - 1, global_sender, null, false, 0);
                }
            }
        }

        int default_tab = 1;

        user_tab tab = user_tab.projects;

        private enum user_tab
        {
            projects = 1,
            favourites = 2
        }

        BitmapImage loading_img = new BitmapImage();
        bool search_click_before = false;

        [JsonObject]
        public class channels
        {
            [JsonProperty("status")]
            public string status;
            [JsonProperty("channels")]
            public List<channel> _channels;
        }

        [JsonObject]
        public class channel
        {
            [JsonProperty("id")]
            public string id;
            [JsonProperty("title")]
            public string title;
            [JsonProperty("path")]
            public string path;
            [JsonProperty("sort_order")]
            public int sort_order;
            [JsonProperty("description")]
            public string description;
        }

        [JsonObject]
        public class projects
        {
            [JsonProperty("projects")]
            public List<project> _projects;
        }

        [JsonObject]
        public class project
        {
            [JsonProperty("uuid")]
            public string uuid;
            [JsonProperty("author")]
            public string author;
            [JsonProperty("deleted_at")]
            public string deleted_at;
            [JsonProperty("edited_at")]
            public string edited_at;
            [JsonProperty("filename")]
            public string filename;
            [JsonProperty("number_of_stars")]
            public int number_of_stars;
            [JsonProperty("play_count")]
            public int play_count;
            [JsonProperty("plants")]
            public int plants;
            [JsonProperty("project_remixs_count")]
            public int project_remixs_count;
            [JsonProperty("published_remixes_count")]
            public int published_remixs_count;
            [JsonProperty("text_object_label")]
            public string text_object_label;
            [JsonProperty("title")]
            public string title;
            [JsonProperty("screenshot_url")]
            public string screenshot_url;
            [JsonProperty("has_been_removed")]
            public bool has_been_removed;
            [JsonProperty("in_moderation")]
            public bool in_moderation;
            [JsonProperty("published_at")]
            public string published_at;
            [JsonProperty("correct_published_at")]
            public string correct_published_at;
            [JsonProperty("user")]
            public user user;
            [JsonProperty("user_id")]
            public int user_id;
            [JsonProperty("original_user")]
            public user original_user;
            [JsonProperty("planted")]
            public bool planted;
            [JsonProperty("starred")]
            public bool starred;
        }

        [JsonObject]
        public class user
        {
            [JsonProperty("id")]
            public int id;
            [JsonProperty("avatar_type")]
            public int? avatar_type;
            [JsonProperty("created_at")]
            public string created_at;
            [JsonProperty("iphone_user")]
            public bool? iphone_user;
            [JsonProperty("nickname")]
            public string nickname;
            [JsonProperty("badges")]
            public badges badges;
            [JsonProperty("projects_count")]
            public int projects_count;
            [JsonProperty("remote_avatar_url")]
            public string remote_avatar_url;
        }

        [JsonObject]
        public class userrq
        {
            [JsonProperty("nickname")]
            public string nickname;
            [JsonProperty("avatar_type")]
            public int? avatar_type;
            [JsonProperty("plants")]
            public int plants;
            [JsonProperty("created_at")]
            public string created_at;           
            [JsonProperty("badges")]
            public badges badges;
            [JsonProperty("projects_count")]
            public int projects_count;
            [JsonProperty("remote_avatar_url")]
            public string remote_avatar_url;
        }

        [JsonObject]
        public class badges
        {
            [JsonProperty("top_planter")]
            public bool top_planter;
        }

        [JsonObject]
        public class Users
        {
            [JsonProperty("users")]
            public List<user> users;
        }

        private async void display()
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://community.gethopscotch.com/api/v2/channels/");
            var response = await client.SendAsync(request);
            channels channels = JsonConvert.DeserializeObject<channels>(await response.Content.ReadAsStringAsync());

            channels._channels = channels._channels.OrderBy(c => c.sort_order).ToList();

            foreach (var channel in channels._channels)
            {
                if (channel.id != "148") // following because we dont have access to user details
                {
                    TabItem ti = new TabItem();
                    ti.Header = channel.title;
                    ti.Tag = channel.path;
                    ti.Height = 50;
                    ti.FontFamily = new FontFamily("Arial");
                    ti.FontWeight = FontWeights.Bold;
                    ti.FontSize = 20;
                    ti.Padding = new Thickness(10);
                    ti.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#d4d4d4");
                    categories.Items.Add(ti);
                }
            }
            categories.SelectedIndex = default_tab;

        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (categories.SelectedItem != null)
            {
                is_userpage = false;
                if ((categories.SelectedItem as TabItem).Tag.ToString() != "_SEARCH_")
                {
                    global_is_search = false;
                    display_projects(1, sender, e);
                }
                else if (search_click_before)
                {
                    global_is_search = true;
                    categories.SelectedIndex = 0;
                    searchbox.Visibility = Visibility.Visible;
                }
                else
                {
                    search_click_before = true;
                }
            }
        }

        private async void search(object sender, EventArgs e)
        {
            global_is_search = true;
            // search for projects = 1, users = 2
            if (((ComboBoxItem)search_options.SelectedItem).Tag.ToString() == "1")
            {
                global_search_val = search_input.Text;
                display_projects(0, new TabControl(), null, false, 0, true, search_input.Text);
            }
            else
            {
                projects_grid.Children.Clear();
                var client = new HttpClient();
                var request = new HttpRequestMessage();
                request = new HttpRequestMessage(HttpMethod.Get, "https://community.gethopscotch.com/api/v2/users/search?nickname=" + HttpUtility.UrlEncode(search_input.Text));
                var response = await client.SendAsync(request);

                var users = JsonConvert.DeserializeObject<Users>(await response.Content.ReadAsStringAsync());
                int i = 0;
                int height = 100;
                int gap = 10;
                foreach (user user in users.users)
                {
                    i++;
                    Grid grid = new Grid();
                    grid.Height = height;
                    grid.VerticalAlignment = VerticalAlignment.Top;
                    grid.HorizontalAlignment = HorizontalAlignment.Stretch;
                    grid.Margin = new Thickness(0, ((i - (i % 4)) / 4) * (height + gap), 0, 0);
                    ColumnDefinition expandcol = new ColumnDefinition();
                    expandcol.Width = new GridLength(1, GridUnitType.Star);
                    ColumnDefinition coldef = new ColumnDefinition();
                    coldef.Width = GridLength.Auto;
                    grid.ColumnDefinitions.Add(expandcol);
                    grid.ColumnDefinitions.Add(coldef);

                    Brush bg = new SolidColorBrush(Color.FromArgb(211,211,211,1));
                    grid.Background = bg;

                    if (i % 4 == 0)
                    {
                        grid.Margin = new Thickness(0, grid.Margin.Top - (height + gap), 0, 0);
                        grid.SetValue(Grid.ColumnProperty, 7);
                    }
                    else
                    {
                        grid.SetValue(Grid.ColumnProperty, ((i % 4) * 2) - 1);
                    }

                    TextBlock textblock = new TextBlock();
                    textblock.Text = user.nickname.Truncate(18);
                    textblock.HorizontalAlignment = HorizontalAlignment.Left;
                    textblock.VerticalAlignment = VerticalAlignment.Center;
                    textblock.FontSize = 20;
                    textblock.Padding = new Thickness(15);
                    textblock.TextWrapping = TextWrapping.Wrap;
                    textblock.SetValue(Grid.ColumnProperty, 0);

                    Image img = new Image();
                    img.Width = 70;
                    img.Height = 70;
                    var url = user.remote_avatar_url;
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    if (url == null)
                    {
                        bitmap.UriSource = new Uri("https://ae-hopscotch.github.io/hs-tools/images/webavatars/" + user.avatar_type.ToString() + ".png", UriKind.Absolute);
                    }
                    else
                    {
                        bitmap.UriSource = new Uri(url, UriKind.Absolute);
                    }
                    bitmap.DecodePixelWidth = 150;
                    bitmap.DecodePixelHeight = 150;
                    bitmap.EndInit();
                    
                    
                    img.VerticalAlignment = VerticalAlignment.Center;
                    img.Margin = new Thickness(15);
                    img.Source = bitmap;
                    img.SetValue(Grid.ColumnProperty, 1);

                    grid.MouseDown += new MouseButtonEventHandler((object sender, MouseButtonEventArgs e) => {
                        show_user(user, bitmap);
                    });

                    grid.Children.Add(textblock);
                    grid.Children.Add(img);
                    projects_grid.Children.Add(grid);
                }
            }
            searchbox.Visibility = Visibility.Hidden;
        }

        private async void show_user(user user, BitmapImage pfp)
        {
            HS.Properties.Settings.Default.Users_Viewed += 1;
            HS.Properties.Settings.Default.Save();
            global_userid = user.id;
            is_userpage = true;
            projects_grid.Children.Clear();

            var client = new HttpClient();
            var request = new HttpRequestMessage();
            request = new HttpRequestMessage(HttpMethod.Get, "https://community.gethopscotch.com/api/v2/users/" + user.id.ToString());
            var response = await client.SendAsync(request);
            userrq userrq = JsonConvert.DeserializeObject<userrq>(await response.Content.ReadAsStringAsync());

            Grid userDetails = new Grid();
            userDetails.SetValue(Grid.ColumnProperty, 0);
            userDetails.SetValue(Grid.ColumnSpanProperty, 9);

            ColumnDefinition expandcols = new ColumnDefinition();
            ColumnDefinition _expandcols = new ColumnDefinition();
            expandcols.Width = new GridLength(1, GridUnitType.Star);
            _expandcols.Width = new GridLength(1, GridUnitType.Star);
            ColumnDefinition pfpcols = new ColumnDefinition();
            pfpcols.Width = GridLength.Auto;

            TextBlock joined = new TextBlock();
            joined.Text = "Joined " + user.created_at;
            joined.VerticalAlignment = VerticalAlignment.Center;
            joined.HorizontalAlignment = HorizontalAlignment.Right;
            joined.Padding = new Thickness(20);
            joined.FontSize = 25;
            joined.SetValue(Grid.ColumnProperty, 0);
            userDetails.Children.Add(joined);

            userDetails.ColumnDefinitions.Add(expandcols);
            userDetails.ColumnDefinitions.Add(pfpcols);
            userDetails.ColumnDefinitions.Add(_expandcols);
            Image img = new Image();
            img.Width = 150;
            img.Height = 150;
            img.VerticalAlignment = VerticalAlignment.Top;
            img.HorizontalAlignment = HorizontalAlignment.Center;
            img.Margin = new Thickness(0, 20, 0, 0);
            img.Source = pfp;
            img.SetValue(Grid.ColumnProperty, 1);
            userDetails.Children.Add(img);

            TextBlock username = new TextBlock();
            username.Text = user.nickname;
            username.SetValue(Grid.ColumnProperty, 1);
            username.VerticalAlignment = VerticalAlignment.Bottom;
            username.HorizontalAlignment = HorizontalAlignment.Center;
            username.Height = 50;
            username.FontSize = 25;
            userDetails.Children.Add(username);

            TextBlock plants = new TextBlock();
            BitmapImage plant_bitmap = new BitmapImage();
            plant_bitmap.BeginInit();
            plant_bitmap.UriSource = new Uri("pack://application:,,,/Assets/General/plant.png");
            plant_bitmap.EndInit();
            InlineUIContainer plants_container = new InlineUIContainer();
            Image plant = new Image();
            plant.Source = plant_bitmap;
            plant.Height = 28;
            plant.Width = 28;
            plants_container.Child = plant;
            plants.Inlines.Add(new Run() { Text = userrq.plants.Shorten(), FontSize = 25 });
            plants.Inlines.Add(plants_container);
            plants.FontSize = 12;
            plants.VerticalAlignment = VerticalAlignment.Bottom;
            plants.HorizontalAlignment = HorizontalAlignment.Center;
            plants.Padding = new Thickness(20);
            plants.ToolTip = userrq.plants.ToString() + " plants";
            plants.SetValue(Grid.ColumnProperty, 2);
            plants.VerticalAlignment = VerticalAlignment.Center;
            plants.HorizontalAlignment = HorizontalAlignment.Left;
            userDetails.Children.Add(plants);

            TextBlock projects = new TextBlock();
            BitmapImage project_bitmap = new BitmapImage();
            project_bitmap.BeginInit();
            project_bitmap.UriSource = new Uri("pack://application:,,,/Assets/General/play.png");
            project_bitmap.EndInit();
            InlineUIContainer projects_container = new InlineUIContainer();
            Image project = new Image();
            project.Source = project_bitmap;
            project.Height = 18;
            project.Width = 18;
            projects_container.Child = project;
            projects.Inlines.Add(new Run() { Text = userrq.projects_count.Shorten(), FontSize = 25 });
            projects.Inlines.Add(projects_container);
            projects.FontSize = 12;
            projects.VerticalAlignment = VerticalAlignment.Bottom;
            projects.HorizontalAlignment = HorizontalAlignment.Center;
            projects.Padding = new Thickness(20);
            projects.ToolTip = userrq.projects_count.ToString() + " projects";
            projects.SetValue(Grid.ColumnProperty, 2);
            projects.VerticalAlignment = VerticalAlignment.Center;
            projects.HorizontalAlignment = HorizontalAlignment.Left;
            projects.Margin = new Thickness(75,0,0,0);
            userDetails.Children.Add(projects);

            userDetails.Height = 240;
            userDetails.VerticalAlignment = VerticalAlignment.Top;
            userDetails.Background = new SolidColorBrush(Colors.AliceBlue);
            userDetails.Margin = new Thickness(0, -50, 0, 0);

            TextBlock projectButton = new TextBlock();
            TextBlock favouriteButton = new TextBlock();
            Border projectButtonBorder = new Border();
            Border favouriteButtonBorder = new Border();

            projectButton.Text = "Projects";
            projectButton.Background = null;
            projectButton.FontSize = 25;
            projectButton.HorizontalAlignment = HorizontalAlignment.Center;
            projectButtonBorder.Margin = new Thickness(0, 240, 0, 0);
            projectButtonBorder.Height = 60;
            projectButtonBorder.SetValue(Grid.ColumnProperty, 1);
            projectButtonBorder.SetValue(Grid.ColumnSpanProperty, 3);
            projectButtonBorder.VerticalAlignment = VerticalAlignment.Top;
            projectButtonBorder.HorizontalAlignment = HorizontalAlignment.Stretch;
            projectButtonBorder.BorderBrush = new SolidColorBrush(Colors.DarkBlue);
            projectButtonBorder.BorderThickness = new Thickness(0, 0, 0, 5);
            projectButtonBorder.Child = projectButton;
            projectButtonBorder.MouseDown += new MouseButtonEventHandler((object sender, MouseButtonEventArgs e) => {
                userTabClicked(user_tab.projects, projectButtonBorder, favouriteButtonBorder, user.id);
            });
            projects_grid.Children.Add(projectButtonBorder);

            favouriteButton.Text = "Favourites";
            favouriteButton.Background = null;
            favouriteButton.FontSize = 25;
            favouriteButton.HorizontalAlignment = HorizontalAlignment.Center;
            favouriteButtonBorder.Margin = new Thickness(0, 240, 0, 0);
            favouriteButtonBorder.Height = 60;
            favouriteButtonBorder.SetValue(Grid.ColumnProperty, 5);
            favouriteButtonBorder.SetValue(Grid.ColumnSpanProperty, 3);
            favouriteButtonBorder.VerticalAlignment = VerticalAlignment.Top;
            favouriteButtonBorder.HorizontalAlignment = HorizontalAlignment.Stretch;
            favouriteButtonBorder.BorderBrush = new SolidColorBrush(Colors.Black);
            favouriteButtonBorder.BorderThickness = new Thickness(0, 0, 0, 2);
            favouriteButtonBorder.Child = favouriteButton;
            favouriteButtonBorder.MouseDown += new MouseButtonEventHandler((object sender, MouseButtonEventArgs e) => {
                userTabClicked(user_tab.favourites, projectButtonBorder, favouriteButtonBorder, user.id);
            });
            projects_grid.Children.Add(favouriteButtonBorder);

            projects_grid.Children.Add(userDetails);

            userTabClicked(tab, projectButtonBorder, favouriteButtonBorder, user.id); // initial display
        }

        private async void show_user(int id, BitmapImage pfp)
        {
            HS.Properties.Settings.Default.Users_Viewed += 1;
            HS.Properties.Settings.Default.Save();
            global_userid = id;
            is_userpage = true;
            projects_grid.Children.Clear();

            var client = new HttpClient();
            var request = new HttpRequestMessage();
            request = new HttpRequestMessage(HttpMethod.Get, "https://community.gethopscotch.com/api/v2/users/" + id.ToString());
            var response = await client.SendAsync(request);
            userrq userrq = JsonConvert.DeserializeObject<userrq>(await response.Content.ReadAsStringAsync());

            Grid userDetails = new Grid();
            userDetails.SetValue(Grid.ColumnProperty, 0);
            userDetails.SetValue(Grid.ColumnSpanProperty, 9);

            ColumnDefinition expandcols = new ColumnDefinition();
            ColumnDefinition _expandcols = new ColumnDefinition();
            expandcols.Width = new GridLength(1, GridUnitType.Star);
            _expandcols.Width = new GridLength(1, GridUnitType.Star);
            ColumnDefinition pfpcols = new ColumnDefinition();
            pfpcols.Width = GridLength.Auto;

            TextBlock joined = new TextBlock();
            joined.Text = "Joined " + DateTime.Parse(userrq.created_at, null, System.Globalization.DateTimeStyles.RoundtripKind).ToString("MMMM yyyy", CultureInfo.InvariantCulture);
            joined.VerticalAlignment = VerticalAlignment.Center;
            joined.HorizontalAlignment = HorizontalAlignment.Right;
            joined.Padding = new Thickness(20);
            joined.FontSize = 25;
            joined.SetValue(Grid.ColumnProperty, 0);
            userDetails.Children.Add(joined);

            userDetails.ColumnDefinitions.Add(expandcols);
            userDetails.ColumnDefinitions.Add(pfpcols);
            userDetails.ColumnDefinitions.Add(_expandcols);
            Image img = new Image();
            img.Width = 150;
            img.Height = 150;
            img.VerticalAlignment = VerticalAlignment.Top;
            img.HorizontalAlignment = HorizontalAlignment.Center;
            img.Margin = new Thickness(0, 20, 0, 0);
            img.Source = pfp;
            img.SetValue(Grid.ColumnProperty, 1);
            userDetails.Children.Add(img);

            TextBlock username = new TextBlock();
            username.Text = userrq.nickname;
            username.SetValue(Grid.ColumnProperty, 1);
            username.VerticalAlignment = VerticalAlignment.Bottom;
            username.HorizontalAlignment = HorizontalAlignment.Center;
            username.Height = 50;
            username.FontSize = 25;
            userDetails.Children.Add(username);

            TextBlock plants = new TextBlock();
            BitmapImage plant_bitmap = new BitmapImage();
            plant_bitmap.BeginInit();
            plant_bitmap.UriSource = new Uri("pack://application:,,,/Assets/General/plant.png");
            plant_bitmap.EndInit();
            InlineUIContainer plants_container = new InlineUIContainer();
            Image plant = new Image();
            plant.Source = plant_bitmap;
            plant.Height = 28;
            plant.Width = 28;
            plants_container.Child = plant;
            plants.Inlines.Add(new Run() { Text = userrq.plants.Shorten(), FontSize = 25 });
            plants.Inlines.Add(plants_container);
            plants.FontSize = 12;
            plants.VerticalAlignment = VerticalAlignment.Bottom;
            plants.HorizontalAlignment = HorizontalAlignment.Center;
            plants.Padding = new Thickness(20);
            plants.ToolTip = userrq.plants.ToString() + " plants";
            plants.SetValue(Grid.ColumnProperty, 2);
            plants.VerticalAlignment = VerticalAlignment.Center;
            plants.HorizontalAlignment = HorizontalAlignment.Left;
            userDetails.Children.Add(plants);

            TextBlock projects = new TextBlock();
            BitmapImage project_bitmap = new BitmapImage();
            project_bitmap.BeginInit();
            project_bitmap.UriSource = new Uri("pack://application:,,,/Assets/General/play.png");
            project_bitmap.EndInit();
            InlineUIContainer projects_container = new InlineUIContainer();
            Image project = new Image();
            project.Source = project_bitmap;
            project.Height = 18;
            project.Width = 18;
            projects_container.Child = project;
            projects.Inlines.Add(new Run() { Text = userrq.projects_count.Shorten(), FontSize = 25 });
            projects.Inlines.Add(projects_container);
            projects.FontSize = 12;
            projects.VerticalAlignment = VerticalAlignment.Bottom;
            projects.HorizontalAlignment = HorizontalAlignment.Center;
            projects.Padding = new Thickness(20);
            projects.ToolTip = userrq.projects_count.ToString() + " projects";
            projects.SetValue(Grid.ColumnProperty, 2);
            projects.VerticalAlignment = VerticalAlignment.Center;
            projects.HorizontalAlignment = HorizontalAlignment.Left;
            projects.Margin = new Thickness(75, 0, 0, 0);
            userDetails.Children.Add(projects);

            userDetails.Height = 240;
            userDetails.VerticalAlignment = VerticalAlignment.Top;
            userDetails.Background = new SolidColorBrush(Colors.AliceBlue);
            userDetails.Margin = new Thickness(0, -50, 0, 0);

            TextBlock projectButton = new TextBlock();
            TextBlock favouriteButton = new TextBlock();
            Border projectButtonBorder = new Border();
            Border favouriteButtonBorder = new Border();

            projectButton.Text = "Projects";
            projectButton.Background = null;
            projectButton.FontSize = 25;
            projectButton.HorizontalAlignment = HorizontalAlignment.Center;
            projectButtonBorder.Margin = new Thickness(0, 240, 0, 0);
            projectButtonBorder.Height = 60;
            projectButtonBorder.SetValue(Grid.ColumnProperty, 1);
            projectButtonBorder.SetValue(Grid.ColumnSpanProperty, 3);
            projectButtonBorder.VerticalAlignment = VerticalAlignment.Top;
            projectButtonBorder.HorizontalAlignment = HorizontalAlignment.Stretch;
            projectButtonBorder.BorderBrush = new SolidColorBrush(Colors.DarkBlue);
            projectButtonBorder.BorderThickness = new Thickness(0, 0, 0, 5);
            projectButtonBorder.Child = projectButton;
            projectButtonBorder.MouseDown += new MouseButtonEventHandler((object sender, MouseButtonEventArgs e) => {
                userTabClicked(user_tab.projects, projectButtonBorder, favouriteButtonBorder, id);
            });
            projects_grid.Children.Add(projectButtonBorder);

            favouriteButton.Text = "Favourites";
            favouriteButton.Background = null;
            favouriteButton.FontSize = 25;
            favouriteButton.HorizontalAlignment = HorizontalAlignment.Center;
            favouriteButtonBorder.Margin = new Thickness(0, 240, 0, 0);
            favouriteButtonBorder.Height = 60;
            favouriteButtonBorder.SetValue(Grid.ColumnProperty, 5);
            favouriteButtonBorder.SetValue(Grid.ColumnSpanProperty, 3);
            favouriteButtonBorder.VerticalAlignment = VerticalAlignment.Top;
            favouriteButtonBorder.HorizontalAlignment = HorizontalAlignment.Stretch;
            favouriteButtonBorder.BorderBrush = new SolidColorBrush(Colors.Black);
            favouriteButtonBorder.BorderThickness = new Thickness(0, 0, 0, 2);
            favouriteButtonBorder.Child = favouriteButton;
            favouriteButtonBorder.MouseDown += new MouseButtonEventHandler((object sender, MouseButtonEventArgs e) => {
                userTabClicked(user_tab.favourites, projectButtonBorder, favouriteButtonBorder, id);
            });
            projects_grid.Children.Add(favouriteButtonBorder);

            projects_grid.Children.Add(userDetails);

            userTabClicked(tab, projectButtonBorder, favouriteButtonBorder, id); // initial display
        }

        private async void userTabClicked(user_tab _tab, Border projectbtnborder, Border favouritebtnborder, int user_id)
        {
            int i = 0;
            tab = _tab;
            updateUserTabs(projectbtnborder, favouritebtnborder);

            var client = new HttpClient();
            var request = new HttpRequestMessage();

            if (tab == user_tab.favourites)
            {
                request = new HttpRequestMessage(HttpMethod.Get, "https://community.gethopscotch.com/api/v2/users/" + user_id + "/favorite_projects");
            }
            else if (tab == user_tab.projects)
            {
                request = new HttpRequestMessage(HttpMethod.Get, "https://community.gethopscotch.com/api/v2/users/" + user_id + "/published_projects");
            }

            var response = await client.SendAsync(request);
            projects projects = JsonConvert.DeserializeObject<projects>(await response.Content.ReadAsStringAsync());

            double img_dimensions = (projects_grid.ActualWidth - 150) / 4;
            double height = (projects_grid.ActualWidth - 150) / 4 + 200;
            int gap = 10;
            foreach (var project in projects._projects)
            {
                i++;
                Grid grid = new Grid();
                grid.Height = height;
                grid.VerticalAlignment = VerticalAlignment.Top;
                grid.HorizontalAlignment = HorizontalAlignment.Stretch;
                grid.Margin = new Thickness(0, 320+((i - (i % 4)) / 4) * (height + gap), 0, 0);

                ImageBrush imgbrush = new ImageBrush();
                imgbrush.ImageSource = loading_img;

                grid.Background = imgbrush;

                if (i % 4 == 0)
                {
                    grid.Margin = new Thickness(0, grid.Margin.Top - (height + gap), 0, 0);
                    grid.SetValue(Grid.ColumnProperty, 7);
                }
                else
                {
                    grid.SetValue(Grid.ColumnProperty, ((i % 4) * 2) - 1);
                }

                TextBlock textblock = new TextBlock();
                textblock.Text = project.title.Truncate(60);
                textblock.Height = 200;
                textblock.HorizontalAlignment = HorizontalAlignment.Stretch;
                textblock.VerticalAlignment = VerticalAlignment.Bottom;
                textblock.Background = new SolidColorBrush(Colors.AliceBlue);
                textblock.Padding = new Thickness(15, 50, 15, 15);
                textblock.FontSize = 20;
                textblock.TextWrapping = TextWrapping.Wrap;

                TextBlock user_textblock = new TextBlock();
                user_textblock.MouseDown += new MouseButtonEventHandler((object _sender, MouseButtonEventArgs _e) => { categories.SelectedIndex = -1; displayUserNoBitmap(project.user.id); });
                user_textblock.Text = project.user.nickname;
                user_textblock.Height = 200;
                user_textblock.HorizontalAlignment = HorizontalAlignment.Stretch;
                user_textblock.VerticalAlignment = VerticalAlignment.Bottom;
                user_textblock.Padding = new Thickness(15);
                user_textblock.FontSize = 15;
                user_textblock.FontStyle = FontStyles.Italic;
                user_textblock.TextWrapping = TextWrapping.Wrap;

                Image img = new Image();
                img.Width = img_dimensions;
                img.Height = img_dimensions;
                img.VerticalAlignment = VerticalAlignment.Top;

                TextBlock likes = new TextBlock();
                BitmapImage heart_bitmap = new BitmapImage();
                heart_bitmap.BeginInit();
                heart_bitmap.UriSource = new Uri("pack://application:,,,/Assets/General/like.png");
                heart_bitmap.EndInit();
                InlineUIContainer heart_container = new InlineUIContainer();
                Image heart = new Image();
                heart.Source = heart_bitmap;
                heart.Height = 18;
                heart.Width = 18;
                heart_container.Child = heart;
                likes.Inlines.Add(heart_container);
                likes.Inlines.Add(new Run() { Text = project.number_of_stars.Shorten(), FontSize = 25 });
                likes.FontSize = 12;
                likes.VerticalAlignment = VerticalAlignment.Bottom;
                likes.HorizontalAlignment = HorizontalAlignment.Left;
                likes.Padding = new Thickness(20);
                likes.ToolTip = project.number_of_stars.ToString();

                TextBlock plays = new TextBlock();
                BitmapImage play_bitmap = new BitmapImage();
                play_bitmap.BeginInit();
                play_bitmap.UriSource = new Uri("pack://application:,,,/Assets/General/play.png");
                play_bitmap.EndInit();
                InlineUIContainer plays_container = new InlineUIContainer();
                Image play = new Image();
                play.Source = play_bitmap;
                play.Height = 18;
                play.Width = 18;
                plays_container.Child = play;
                plays.Inlines.Add(new Run() { Text = project.play_count.Shorten(), FontSize = 25 });
                plays.Inlines.Add(plays_container);
                plays.FontSize = 12;
                plays.VerticalAlignment = VerticalAlignment.Bottom;
                plays.HorizontalAlignment = HorizontalAlignment.Right;
                plays.Padding = new Thickness(20);
                plays.ToolTip = project.play_count.ToString();

                TextBlock plants = new TextBlock();
                BitmapImage plant_bitmap = new BitmapImage();
                plant_bitmap.BeginInit();
                plant_bitmap.UriSource = new Uri("pack://application:,,,/Assets/General/plant.png");
                plant_bitmap.EndInit();
                InlineUIContainer plants_container = new InlineUIContainer();
                Image plant = new Image();
                plant.Source = plant_bitmap;
                plant.Height = 28;
                plant.Width = 28;
                plants_container.Child = plant;
                plants.Inlines.Add(new Run() { Text = project.plants.Shorten(), FontSize = 25 });
                plants.Inlines.Add(plants_container);
                plants.FontSize = 12;
                plants.VerticalAlignment = VerticalAlignment.Bottom;
                plants.HorizontalAlignment = HorizontalAlignment.Center;
                plants.Padding = new Thickness(20);
                plants.ToolTip = project.plants.ToString();

                BitmapImage bitmap_img = new BitmapImage();
                bitmap_img.BeginInit();
                bitmap_img.UriSource = new Uri(project.screenshot_url, UriKind.Absolute);
                bitmap_img.DecodePixelHeight = (int)img_dimensions;
                bitmap_img.DecodePixelWidth = (int)img_dimensions;
                bitmap_img.EndInit();

                img.Source = bitmap_img;
                img.MouseDown += new MouseButtonEventHandler((object sender, MouseButtonEventArgs e)=>{ playProject(project.uuid); });
                img.Loaded += new RoutedEventHandler((object _sender, RoutedEventArgs _e) =>
                {
                    img_loaded(img);
                });

                grid.Children.Add(textblock);
                grid.Children.Add(user_textblock);
                grid.Children.Add(likes);
                grid.Children.Add(plants);
                grid.Children.Add(plays);
                grid.Children.Add(img);
                projects_grid.Children.Add(grid);
            }
        }
        private void updateUserTabs(Border projectbtnborder, Border favouritebtnborder)
        {
            if (tab == user_tab.projects)
            {
                projectbtnborder.BorderBrush = new SolidColorBrush(Colors.DarkBlue);
                projectbtnborder.BorderThickness = new Thickness(0, 0, 0, 5);

                favouritebtnborder.BorderBrush = new SolidColorBrush(Colors.Black);
                favouritebtnborder.BorderThickness = new Thickness(0, 0, 0, 2);
            } else if (tab == user_tab.favourites)
            {
                projectbtnborder.BorderBrush = new SolidColorBrush(Colors.Black);
                projectbtnborder.BorderThickness = new Thickness(0, 0, 0, 2);

                favouritebtnborder.BorderBrush = new SolidColorBrush(Colors.DarkBlue);
                favouritebtnborder.BorderThickness = new Thickness(0, 0, 0, 5);
            }
        }

        async void display_projects(int page, object sender, SelectionChangedEventArgs e, bool preserve_projects = false, int i = 0, bool is_search = false, string search_val = "", int userid = 0)
        {
            if (sender is TabControl tc)
            {
                if (!preserve_projects)
                {
                    projects_grid.Children.Clear();
                }
                TabItem ti = (TabItem)tc.SelectedItem;

                var client = new HttpClient();
                var request = new HttpRequestMessage();
                if (!global_is_search)
                {
                    request = new HttpRequestMessage(HttpMethod.Get, "https://community.gethopscotch.com/api/v2/" + ti.Tag + "?page=" + page);
                }
                else if (is_userpage && userid != 0)
                {
                    if (tab == user_tab.favourites)
                    {
                        request = new HttpRequestMessage(HttpMethod.Get, "https://community.gethopscotch.com/api/v2/users/" + userid + "/favorite_projects");
                    }
                    else if (tab == user_tab.projects)
                    {
                        request = new HttpRequestMessage(HttpMethod.Get, "https://community.gethopscotch.com/api/v2/users/" + userid + "/published_projects");
                    }
                }
                else
                {
                    //Dispatcher.BeginInvoke((Action)(() => tc.SelectedItem = 2));
                    request = new HttpRequestMessage(HttpMethod.Get, "https://community.gethopscotch.com/api/v2/projects/search?title=" + search_val);
                }
                var response = await client.SendAsync(request);
                projects projects = JsonConvert.DeserializeObject<projects>(await response.Content.ReadAsStringAsync());

                double height = (projects_grid.ActualWidth - 150) / 4 + 200;
                int gap = 10;
                foreach (var project in projects._projects)
                {
                    i++;
                    Grid grid = new Grid();

                    RowDefinition rd = new RowDefinition();
                    rd.Height = new GridLength(1,GridUnitType.Star);
                    RowDefinition rd1 = new RowDefinition();
                    rd1.Height = new GridLength(1, GridUnitType.Star);

                    grid.RowDefinitions.Add(rd);
                    grid.RowDefinitions.Add(rd1);

                    grid.VerticalAlignment = VerticalAlignment.Top;
                    grid.HorizontalAlignment = HorizontalAlignment.Stretch;
                    grid.Margin = new Thickness(0, ((i - (i % 4)) / 4) * (height + gap), 0, 0);
                    
                    ImageBrush imgbrush = new ImageBrush();
                    imgbrush.ImageSource = loading_img;

                    grid.Background = imgbrush;

                    if (i % 4 == 0)
                    {
                        grid.Margin = new Thickness(0, grid.Margin.Top - (height + gap), 0, 0);
                        grid.SetValue(Grid.ColumnProperty, 7);
                    }
                    else
                    {
                        grid.SetValue(Grid.ColumnProperty, ((i % 4) * 2) - 1);
                    }

                    TextBlock textblock = new TextBlock();
                    textblock.Text = project.title.Truncate(60);
                    textblock.Height = 200;
                    textblock.HorizontalAlignment = HorizontalAlignment.Stretch;
                    textblock.VerticalAlignment = VerticalAlignment.Bottom;
                    textblock.Background = new SolidColorBrush(Colors.AliceBlue);
                    textblock.Padding = new Thickness(15, 50, 15, 15);
                    textblock.FontSize = 20;
                    textblock.SetValue(Grid.RowProperty, 1);
                    textblock.TextWrapping = TextWrapping.Wrap;

                    TextBlock user_textblock = new TextBlock();
                    user_textblock.MouseDown += new MouseButtonEventHandler((object _sender, MouseButtonEventArgs _e) => { categories.SelectedIndex = -1; displayUserNoBitmap(project.user.id); });
                    user_textblock.Text = project.user.nickname;
                    user_textblock.Height = 200;
                    user_textblock.HorizontalAlignment = HorizontalAlignment.Stretch;
                    user_textblock.VerticalAlignment = VerticalAlignment.Bottom;
                    user_textblock.Padding = new Thickness(15);
                    user_textblock.FontSize = 15;
                    user_textblock.FontStyle = FontStyles.Italic;
                    user_textblock.SetValue(Grid.RowProperty, 1);
                    user_textblock.TextWrapping = TextWrapping.Wrap;

                    Image img = new Image();
                    img.HorizontalAlignment = HorizontalAlignment.Stretch;
                    img.SetValue(Grid.RowProperty, 0);
                    img.Height = img.Width;
                    img.VerticalAlignment = VerticalAlignment.Top;

                    TextBlock likes = new TextBlock();
                    BitmapImage heart_bitmap = new BitmapImage();
                    heart_bitmap.BeginInit();
                    heart_bitmap.UriSource = new Uri("pack://application:,,,/Assets/General/like.png");
                    heart_bitmap.EndInit();
                    InlineUIContainer heart_container = new InlineUIContainer();
                    Image heart = new Image();
                    heart.Source = heart_bitmap;
                    heart.Height = 18;
                    heart.Width = 18;
                    heart_container.Child = heart;
                    likes.Inlines.Add(heart_container);
                    likes.Inlines.Add(new Run() { Text = project.number_of_stars.Shorten(), FontSize = 25 });
                    likes.FontSize = 12;
                    likes.SetValue(Grid.RowProperty, 1);
                    likes.VerticalAlignment = VerticalAlignment.Bottom;
                    likes.HorizontalAlignment = HorizontalAlignment.Left;
                    likes.Padding = new Thickness(20);
                    likes.ToolTip = project.number_of_stars.ToString();

                    TextBlock plays = new TextBlock();
                    BitmapImage play_bitmap = new BitmapImage();
                    play_bitmap.BeginInit();
                    play_bitmap.UriSource = new Uri("pack://application:,,,/Assets/General/play.png");
                    play_bitmap.EndInit();
                    InlineUIContainer plays_container = new InlineUIContainer();
                    Image play = new Image();
                    play.Source = play_bitmap;
                    play.Height = 18;
                    play.Width = 18;
                    plays_container.Child = play;
                    plays.Inlines.Add(new Run() { Text = project.play_count.Shorten(), FontSize = 25 });
                    plays.Inlines.Add(plays_container);
                    plays.FontSize = 12;
                    plays.SetValue(Grid.RowProperty, 1);
                    plays.VerticalAlignment = VerticalAlignment.Bottom;
                    plays.HorizontalAlignment = HorizontalAlignment.Right;
                    plays.Padding = new Thickness(20);
                    plays.ToolTip = project.play_count.ToString();

                    TextBlock plants = new TextBlock();
                    BitmapImage plant_bitmap = new BitmapImage();
                    plant_bitmap.BeginInit();
                    plant_bitmap.UriSource = new Uri("pack://application:,,,/Assets/General/plant.png");
                    plant_bitmap.EndInit();
                    InlineUIContainer plants_container = new InlineUIContainer();
                    Image plant = new Image();
                    plant.Source = plant_bitmap;
                    plant.Height = 28;
                    plant.Width = 28;
                    plants_container.Child = plant;
                    plants.Inlines.Add(new Run() { Text = project.plants.Shorten(), FontSize = 25 });
                    plants.Inlines.Add(plants_container);
                    plants.FontSize = 12;
                    plants.SetValue(Grid.RowProperty, 1);
                    plants.VerticalAlignment = VerticalAlignment.Bottom;
                    plants.HorizontalAlignment = HorizontalAlignment.Center;
                    plants.Padding = new Thickness(20);
                    plants.ToolTip = project.plants.ToString();

                    BitmapImage bitmap_img = new BitmapImage();
                    bitmap_img.BeginInit();
                    bitmap_img.UriSource = new Uri(project.screenshot_url, UriKind.Absolute);
                    bitmap_img.DecodePixelHeight = 200;
                    bitmap_img.DecodePixelWidth = 200;
                    bitmap_img.EndInit();

                    img.Source = bitmap_img;
                    img.MouseDown += new MouseButtonEventHandler((object sender, MouseButtonEventArgs e) => { playProject(project.uuid); });
                    img.Loaded += new RoutedEventHandler((object _sender, RoutedEventArgs _e) =>
                    {
                        img_loaded(img);
                    });

                    grid.Children.Add(textblock);
                    grid.Children.Add(user_textblock);
                    grid.Children.Add(likes);
                    grid.Children.Add(plants);
                    grid.Children.Add(plays);
                    grid.Children.Add(img);
                    projects_grid.Children.Add(grid);
                }

                global_page = page + 1;
                global_i = i;
                global_sender = (TabControl)sender;
            }
        }

        void playProject(string uuid)
        {
            Play_Project play_Project = new Play_Project(Convert.ToInt32(this.Width), Convert.ToInt32(this.Height), this.WindowState, this.Top, this.Left, uuid);
            play_Project.Show();
        }

        async void displayUserNoBitmap(int id)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage();
            request = new HttpRequestMessage(HttpMethod.Get, "https://community.gethopscotch.com/api/v2/users/" + id.ToString());
            var response = await client.SendAsync(request);
            userrq userrq = JsonConvert.DeserializeObject<userrq>(await response.Content.ReadAsStringAsync());
            var url = userrq.remote_avatar_url;
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            if (url == null)
            {
                bitmap.UriSource = new Uri("https://ae-hopscotch.github.io/hs-tools/images/webavatars/" + userrq.avatar_type.ToString() + ".png", UriKind.Absolute);
            }
            else
            {
                bitmap.UriSource = new Uri(url, UriKind.Absolute);
            }
            bitmap.DecodePixelWidth = 150;
            bitmap.DecodePixelHeight = 150;
            bitmap.EndInit();
            show_user(id, bitmap);
        }

        
        bool global_is_search = false;
        string global_search_val = "";
        int global_page = 0;
        int global_i = 0;
        bool is_userpage = false;
        int global_userid = 0;
        TabControl global_sender = null;

        void destroy_button(Button btn)
        {
            Grid parent = (Grid)VisualTreeHelper.GetParent(btn);
            parent.Children.Remove(btn);
        }

        void img_loaded(Image img)
        {
            Grid parent = (Grid)VisualTreeHelper.GetParent(img);
            parent.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#d4d4d4");
        }

        private void scroll_ViewChanged(object sender, ScrollChangedEventArgs e)
        {
            if (projects_grid.Children.Count > 0)
            {
                var scrollViewer = (ScrollViewer)sender;
                if (scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight)
                {
                    if (categories.SelectedIndex == 0 && !is_userpage)
                    {
                        //display_projects(0, new TabControl(), null, false, 0, true, global_search_val);
                    }
                    else if (!is_userpage)
                    {
                        display_projects(global_page, global_sender, null, true, global_i);
                    }
                }
            }
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

    public static class IntExt
    {
        public static string Shorten(this int num)
        {
            if (num >= 1000000000)
            {
                return Math.Floor((decimal)(num / 1000000000)).ToString() + "B"; // billion because you never know what -1 will do next
            }
            else if (num >= 1000000)
            {
                return Math.Floor((decimal)(num / 1000000)).ToString() + "M";
            }
            else if (num >= 1000)
            {
                return Math.Floor((decimal)(num / 1000)).ToString() + "K";
            }
            else
            {
                return num.ToString();
            }
        }
    }

    public static class StringExt // yes this is from stack overflow because i am in a hurry
    {
        public static string? Truncate(this string? value, int maxLength, string truncationSuffix = "…")
        {
            return value?.Length > maxLength
                ? value.Substring(0, maxLength) + truncationSuffix
                : value;
        }
    }
}
