using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static HS.Community;

namespace HS
{
    /// <summary>
    /// Interaction logic for NewUser.xaml
    /// </summary>
    public partial class NewUser : Window
    {
        public class userdata
        {
            public int id { get; set; }
            public string nickname { get; set; }
            public bool not_found = false;
        }

        [JsonObject]
        public class Users
        {
            [JsonProperty("users")]
            public List<user> users;
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
        public class badges
        {
            [JsonProperty("top_planter")]
            public bool top_planter;
        }

        public NewUser()
        {
            InitializeComponent();
            this.WindowState = WindowState.Maximized;
            ShowSentances();
        }

        string username = "";

        public async void ShowSentances()
        {
            [DllImport("wininet.dll")]
            extern static bool InternetGetConnectedState(out int description, int reservedValue);

            int description;
            bool hasInternet = InternetGetConnectedState(out description, 0);
            if (!hasInternet)
            {
                main_text.TextAlignment = TextAlignment.Center;
                main_text.FontSize = 20;
                main_text.Text = "Please connect to the internet and restart this application.\n\nInternet is only required the first time.";
                await In();
            }
            else
            {
                main_text.Text = "Hello";
                await In();
                await Task.Delay(2000);
                await Out();

                main_text.Text = "Thank You For Beta Testing";
                await In();
                await Task.Delay(3000);
                await Out();

                main_btn.Content = "Begin Setup";
                await In(1);
            }
            
        }

        public async Task In(int type = 0)
        {
            if (type == 0)
            {
                main_text.Opacity = 0;
                for (int a = 0; a < 100; a++)
                {
                    main_text.Opacity += 0.01;
                    await Task.Delay(10);
                }
            }
            else if (type == 1)
            {
                main_btn.Opacity = 0;
                for (int a = 0; a < 100; a++)
                {
                    main_btn.Opacity += 0.01;
                    await Task.Delay(10);
                }
            }
            else if (type == 2)
            {
                main_inputGroup.Opacity = 0;
                for (int a = 0; a < 100; a++)
                {
                    main_inputGroup.Opacity += 0.01;
                    await Task.Delay(10);
                }
            }
            else if (type == 3)
            {
                resultsListBox.Opacity = 0;
                for (int a = 0; a < 100; a++)
                {
                    resultsListBox.Opacity += 0.01;
                    await Task.Delay(10);
                }
            }
            else if (type == 4)
            {
                selectAccountBtn.Opacity = 0;
                for (int a = 0; a < 100; a++)
                {
                    selectAccountBtn.Opacity += 0.01;
                    await Task.Delay(10);
                }
            }
        }

        public async Task Out(int type = 0)
        {
            if (type == 0)
            {
                main_text.Opacity = 1;
                for (int b = 0; b < 100; b++)
                {
                    main_text.Opacity -= 0.01;
                    await Task.Delay(10);
                }
            }
            else if (type == 1)
            {
                main_btn.Opacity = 1;
                for (int b = 0; b < 100; b++)
                {
                    main_btn.Opacity -= 0.01;
                    await Task.Delay(10);
                }
            }
            else if (type == 2)
            {
                main_inputGroup.Opacity = 1;
                for (int b = 0; b < 100; b++)
                {
                    main_inputGroup.Opacity -= 0.01;
                    await Task.Delay(10);
                }
            }
            else if (type == 3)
            {
                resultsListBox.Opacity = 1;
                for (int b = 0; b < 100; b++)
                {
                    resultsListBox.Opacity -= 0.01;
                    await Task.Delay(10);
                }
            }
            else if (type == 4)
            {
                selectAccountBtn.Opacity = 1;
                for (int b = 0; b < 100; b++)
                {
                    selectAccountBtn.Opacity -= 0.01;
                    await Task.Delay(10);
                }
            }
        }

        async void main_btn_Click(object sender, RoutedEventArgs e)
        {
            await Out(1);

            main_btn.Width = 0;
            main_btn.Height = 0;

            main_text.Text = "Let's find your Hopscotch account.";
            await In();
            await Task.Delay(2000);
            await Out();

            main_text.Text = "Please enter your Hopscotch username.";
            In();
            In(2);
        }

        async void username_btn_click(object sender, RoutedEventArgs e)
        {
            username = main_input.Text;
            Out();
            await Out(2);

            main_text.Text = "Searching for username\n\"" + username + "\"";
            main_text.TextAlignment = TextAlignment.Center;
            await In();

            var client = new HttpClient();
            var request = new HttpRequestMessage();
            request = new HttpRequestMessage(HttpMethod.Get, "https://community.gethopscotch.com/api/v2/users/search?nickname=" + HttpUtility.UrlEncode(username));
            var response = await client.SendAsync(request);

            var users = JsonConvert.DeserializeObject<Users>(await response.Content.ReadAsStringAsync());

            foreach (var user in users.users)
            {
                ListBoxItem item = new ListBoxItem();

                userdata userdata = new userdata();
                userdata.id = user.id;
                userdata.nickname = user.nickname;
                item.Tag = userdata;
                item.Content = user.nickname;

                resultsListBox.Items.Add(item);
            }

            await Out();

            resultsListBox.Height = 200;
            resultsListBox.Width = 350;
            main_text.VerticalAlignment = VerticalAlignment.Top;
            main_text.Margin = new Thickness(0, 200, 0, 0);

            main_text.Text = "Select your account from the list, then click the button.";
            selectAccountBtn.Content = "Continue";
            In();
            In(3);
            await In(4);

        }

        async void select_user(object sender, RoutedEventArgs e)
        {
            if (resultsListBox.SelectedItem != null) {
                Out();
                Out(3);
                await Out(4);
                main_text.VerticalAlignment = VerticalAlignment.Center;
                main_text.Margin = new Thickness(0);
                resultsListBox.Height = 0;
                resultsListBox.Width = 0;
                userdata userdata = (userdata)((ListBoxItem)resultsListBox.SelectedItem).Tag;
                if (userdata.not_found == true)
                {
                    // in case too many people cant find their accounts in the future then we can put an alternative way here 
                } else
                {
                    foundAccount(userdata);
                }
            }
            else
            {
                MessageBox.Show("Please select an account before continuing.");
            }
        }

        async void foundAccount(userdata userdata)
        {
            HS.Properties.Settings.Default.User_Id = userdata.id;
            HS.Properties.Settings.Default.Users_Viewed = 0;
            HS.Properties.Settings.Default.Projects_Created = 0;
            HS.Properties.Settings.Default.Projects_Played = 0;
            HS.Properties.Settings.Default.Seconds_Spent = 0;
            HS.Properties.Settings.Default.Is_Setup = true;
            HS.Properties.Settings.Default.Save();
            main_text.Text = "Welcome, " + userdata.nickname;
            await In();
            await Task.Delay(3000);
            await Out();
            Community community = new Community();
            community.Show();
            this.Close();
        }
    }
}
