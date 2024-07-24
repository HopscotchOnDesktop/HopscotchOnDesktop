using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Resources;
using System.Windows.Shapes;
using static System.Net.WebRequestMethods;

namespace HS.Home_Windows
{
    /// <summary>
    /// Interaction logic for PixelArt.xaml
    /// </summary>
    public partial class PixelArt : Window
    {
        Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
        Nullable<bool> result = null;

        public PixelArt()
        {
            InitializeComponent();
        }

        private void selectFile(object sender, RoutedEventArgs e)
        {
            dialog.Filter = @"All Image Files|*.BMP;*.bmp;*.JPG;*.JPEG*.jpg;*.jpeg;*.PNG;*.png;*.GIF;*.gif;*.tif;*.tiff;*.ico;*.ICO|PNG|*.png|JPEG|*.jpeg|JPG|*.jpg|Bitmap|*.bmp|GIF|*.gif|TIFF|*.tiff|ICO|*.ico;";
            result = dialog.ShowDialog();

            if (result == true)
            {
                pixelart_grid.Children.Remove(upload_btn);
                filename_preview.Text = dialog.SafeFileName.Truncate(10);
                System.Drawing.Image img = System.Drawing.Image.FromFile(dialog.FileName);
                original_width.Text = img.Width.ToString() + " px";
                original_height.Text = img.Height.ToString() + " px";
            }
        }

        private void refreshActual(object sender, TextChangedEventArgs _e)
        {
            //https://c.gethopscotch.com/api/v1/projects/13zl4jrtzu
                if (width_input != null && height_input != null && pixelsize_input != null && int.TryParse(width_input.Text, out _) && int.TryParse(height_input.Text, out _) && int.TryParse(pixelsize_input.Text, out _))
                {
                    actual_size.Text = (Convert.ToInt32(width_input.Text) * Convert.ToInt32(pixelsize_input.Text)) + " x " + (Convert.ToInt32(height_input.Text) * Convert.ToInt32(pixelsize_input.Text));
                }
                else
                {
                    actual_size.Text = "NaN";
                }
        }

        private async void generate(object sender, RoutedEventArgs _e)
        {
            if (result == true)
            {
                Mouse.OverrideCursor = Cursors.Wait;
                int width = 0;
                int height = 0;
                int pixelsize = 0;
                if (int.TryParse(width_input.Text, out _) && int.TryParse(height_input.Text, out _) && int.TryParse(pixelsize_input.Text, out _))
                {
                    width = Convert.ToInt32(width_input.Text);
                    height = Convert.ToInt32(height_input.Text);
                    pixelsize = Convert.ToInt32(pixelsize_input.Text);
                }
                else
                {
                    MessageBox.Show("The width, height, and pixel size values must be numerical.", "Pixel Art Generator", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (width > 200 || height > 200)
                {
                    MessageBox.Show("Generating a pixel art so large could freeze your device. Please select a height/width value that is less than 200 and try again.", "Pixel Art Generator", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (pixelsize > 50)
                {
                    pixelsize = 50;
                    MessageBox.Show("A pixel size of " + pixelsize + " is too big. It has been reduced to 50.", "Pixel Art Generator", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                // add sample to grid
                for (int _a = 0; _a < width; _a++)
                {
                    ColumnDefinition columnDefinition = new ColumnDefinition();
                    columnDefinition.Width = new GridLength(pixelsize, GridUnitType.Pixel);
                    previewGrid.ColumnDefinitions.Add(columnDefinition);
                }
                for (int _b = 0; _b < height; _b++)
                {
                    RowDefinition rowDefinition = new RowDefinition();
                    rowDefinition.Height = new GridLength(pixelsize, GridUnitType.Pixel);
                    previewGrid.RowDefinitions.Add(rowDefinition);
                }

                // Read file and resize
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(dialog.FileName);
                bitmap.DecodePixelHeight = height;
                bitmap.DecodePixelWidth = width;
                bitmap.EndInit();

                // Convert BitmapImage to Bitmap
                Bitmap myBitmap = null;
                using (MemoryStream outStream = new MemoryStream())
                {
                    BitmapEncoder enc = new BmpBitmapEncoder();
                    enc.Frames.Add(BitmapFrame.Create(bitmap));
                    enc.Save(outStream);
                    myBitmap = new System.Drawing.Bitmap(outStream);
                }

                // Extract pixel data
                System.Drawing.Color[,] a = new System.Drawing.Color[height, width];
                string d = "";

                for (int b = 0; b < myBitmap.Height; b++)
                    for (int c = 0; c < myBitmap.Width; c++)
                    {
                        System.Drawing.Color pixelColor = myBitmap.GetPixel(c, b);
                        a[b, c] = pixelColor;
                    }

                // Convert ARGB colours into HEX colours compile pixel data into a string
                for (int e = 0; e < a.GetLength(0); e++)
                {
                    for (int f = 0; f < a.GetLength(1); f++)
                    {
                        d += "#" + string.Format("{0:X2}{1:X2}{2:X2}", a[e, f].R, a[e, f].G, a[e, f].B);

                        System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle();
                        rect.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(a[e, f].A, a[e, f].R, a[e, f].G, a[e, f].B));
                        rect.SetValue(Grid.RowProperty, e);
                        rect.SetValue(Grid.ColumnProperty, f);
                        previewGrid.Children.Add(rect);
                    }
                }
                long timesaved = 3 * (width * height);
                HS.Properties.Settings.Default.Projects_Created += 1;
                HS.Properties.Settings.Default.Time_Saved += timesaved;
                HS.Properties.Settings.Default.Save();
                Mouse.OverrideCursor = null;
                StreamResourceInfo imageInfo = System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/Assets/Home/pixelart_template.json"));
                byte[] _bytes = ReadFully(imageInfo.Stream);

                string template = System.Text.Encoding.Default.GetString(_bytes);
                string editedat = DateTime.UtcNow.ToString("s", System.Globalization.CultureInfo.InvariantCulture) + "Z";

                string w = "0123456789abcdefghijklmnopqrstuvwxyz";
                decimal x = 0;
                long y = 0;
                string z = "";
                x = Convert.ToInt64((DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() * 65536)) / 1000;
                async Task base36()
                {
                    y = Convert.ToInt64(x % 36);
                    x = Math.Floor(Convert.ToDecimal(x / 36));
                    z = w[Convert.ToInt32(y % 36)] + z;
                    if (x != 0)
                    {
                        base36();
                    }
                }
                await base36();
                string uuid = z;
                string fileuuid = Guid.NewGuid().ToString();

                template = template.Replace(@"<<PIXEL_ART_CODE>>", d);
                template = template.Replace(@"<<PIXEL_ART_WIDTH>>", width.ToString());
                template = template.Replace(@"<<PIXEL_ART_HEIGHT>>", height.ToString());
                template = template.Replace(@"<<PIXEL_SIZE>>", pixelsize.ToString());

                string userid = HS.Properties.Settings.Default.User_Id.ToString();
                var client = new HttpClient();
                var request = new HttpRequestMessage();
                request = new HttpRequestMessage(HttpMethod.Get, "https://community.gethopscotch.com/api/v2/users/" + userid);
                var response = await client.SendAsync(request);
                userrq userrq = JsonConvert.DeserializeObject<userrq>(await response.Content.ReadAsStringAsync());

                template = template.Replace(@"<<USER_ID>>", userid);
                template = template.Replace(@"<<USER_AVATARTYPE>>", userrq.avatar_type.ToString());
                template = template.Replace(@"<<USER_CREATEDAT>>", userrq.created_at);
                template = template.Replace(@"<<USER_NICKNAME>>", userrq.nickname);
                template = template.Replace(@"<<USER_TOPPLANTER>>", userrq.badges.top_planter.ToString());
                template = template.Replace(@"<<USER_PROJECTSCOUNT>>", userrq.projects_count.ToString());

                template = template.Replace(@"<<ACTUAL_USERID>>", userid);
                template = template.Replace(@"<<UUID>>", uuid);
                template = template.Replace(@"<<FILEUUID>>", fileuuid);

                template = template.Replace(@"<<ORIGINAL_ID>>", userid);
                template = template.Replace(@"<<ORIGINAL_AVATARTYPE>>", userrq.avatar_type.ToString());
                template = template.Replace(@"<<ORIGINAL_CREATEDAT>>", userrq.created_at);
                template = template.Replace(@"<<ORIGINAL_NICKNAME>>", userrq.nickname);
                template = template.Replace(@"<<ORIGINAL_TOPPLANTER>>", userrq.badges.top_planter.ToString());
                template = template.Replace(@"<<ORIGINAL_PROJECTSCOUNT>>", userrq.projects_count.ToString());

                string actualtime = DateTime.UtcNow.ToString("s", System.Globalization.CultureInfo.InvariantCulture) + "Z";
                string roughtime = DateTime.UtcNow.ToString("yyyy-MM-dd") + "T00:00:00Z";
                template = template.Replace(@"<<PUBLISHED_AT>>", roughtime);
                template = template.Replace(@"<<PUBLISHED_CORRECT_AT>>", actualtime);
                template = template.Replace(@"<<EDITED_AT>>", editedat);

                ReadOnlyMemory<byte> _template = Encoding.UTF8.GetBytes(template.ToString());

                SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                saveFileDialog1.FileName = fileuuid;
                saveFileDialog1.Filter = "Hopscotch File|*.hopscotch";
                saveFileDialog1.Title = "Save a Hopscotch File";
                saveFileDialog1.ShowDialog();

                // If the file name is not an empty string open it for saving.
                if (saveFileDialog1.FileName != "")
                {
                    // Saves the Image via a FileStream created by the OpenFile method.
                    System.IO.FileStream fs =
                        (System.IO.FileStream)saveFileDialog1.OpenFile();
                    // Saves the Image in the appropriate ImageFormat based upon the
                    // File type selected in the dialog box.
                    // NOTE that the FilterIndex property is one-based.
                    using (StreamWriter outputFile = new StreamWriter(fs))
                    {
                        outputFile.Write(template);
                    }
                }

                MessageBox.Show("You have saved an estimated " + timesaved.ShortenSeconds() + timesaved.GetSuffix() + "!");
            } 
            else
            {
                MessageBox.Show("Please select an image first.", "Pixel Art Generator", MessageBoxButton.OK, MessageBoxImage.Information);
            }
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

        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }

    public static class LongExt
    {
        public static string ShortenSeconds(this long num)
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

        public static string GetSuffix(this long num)
        {
            if (num >= 31536000)
            {
                return " years";
            }
            if (num >= 2628000)
            {
                return " months";
            }
            if (num >= 86400)
            {
                return " days";
            }
            else if (num >= 3600)
            {
                return " hours";
            }
            else if (num >= 60)
            {
                return " minutes";
            }
            else
            {
                return " seconds";
            }
        }
    }
}
