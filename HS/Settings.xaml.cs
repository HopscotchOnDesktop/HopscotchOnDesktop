using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HS
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        JObject aliases = new JObject();
        JObject plugins = new JObject();

        public Settings()
        {
            InitializeComponent();

            string _aliases = Properties.Settings.Default.Aliases;
            aliases = (JObject)JsonConvert.DeserializeObject(_aliases);
            refreshAliasListBox();

            string _plugins = Properties.Settings.Default.Plugin_List;
            plugins = (JObject)JsonConvert.DeserializeObject(_plugins);
            refreshPluginListBox();

            switch (Properties.Settings.Default.Modded_Editor)
            {
                case true:
                    modded_editor_options.SelectedIndex = 0;
                    break;

                case false:
                    modded_editor_options.SelectedIndex = 1;
                    break;
            }
        }

        private void open_plugin(object sender, EventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.DefaultExt = ".zip";
            dialog.Filter = "Zip File | *.zip";
            bool? result = dialog.ShowDialog();
            Regex regex = new Regex(@".*\\");
            if (result == true)
            {
                bool successful = false;
                bool hasManifestAtRoot = false;
                string filename = regex.Replace(dialog.FileName, "").ToString().Replace(".zip", "");
                string path = dialog.FileName;

                foreach (var plugin in (JArray)((JObject)JsonConvert.DeserializeObject(Properties.Settings.Default.Plugin_List)).GetValue("plugins"))
                {
                    if (plugin.ToString() == filename)
                    {
                        MessageBox.Show("This plugin has already been added!");
                        return;
                    }
                }

                using (ZipArchive archive = ZipFile.OpenRead(path))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (entry.FullName == "manifest.json")
                        {
                            try
                            {
                                bool is_whitelisted = false;
                                hasManifestAtRoot = true;

                                entry.ExtractToFile(AppDomain.CurrentDomain.BaseDirectory + "/plugin_temp/" + entry.FullName);

                                string contents = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "/plugin_temp/manifest.json");

                                System.IO.DirectoryInfo di = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "/plugin_temp");

                                foreach (FileInfo file in di.GetFiles())
                                {
                                    file.Delete();
                                }
                                foreach (DirectoryInfo dir in di.GetDirectories())
                                {
                                    dir.Delete(true);
                                }
                                JObject manifest = JsonConvert.DeserializeObject<JObject>(contents);
                                if (manifest.ContainsKey("whitelist"))
                                {
                                    string whitelist_id = manifest.GetValue("whitelist").ToString();

                                    HttpClient client = new HttpClient();
                                    var response = client.GetStringAsync("https://api.jsonbin.io/v3/qs/665d2f64e41b4d34e4fdaa61").Result;
                                    JObject whitelisted_plugins = JsonConvert.DeserializeObject<JObject>(response.ToString());
                                    foreach (var id in whitelisted_plugins.Values("plugins"))
                                    {
                                        if (id.ToString() == whitelist_id)
                                        {
                                            is_whitelisted = true;
                                            break;
                                        }
                                    }
                                }
                                if (!is_whitelisted)
                                {
                                    if (MessageBox.Show("The plugin is not whitelisted. Do you wish to continue?", "Plugin Verification", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                                    {
                                        MessageBox.Show("The plugin has been imported");
                                    }
                                    else
                                    {
                                        MessageBox.Show("The plugin import has been cancelled");
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("The plugin is whitelisted and has been imported");
                                }
                                successful = true;
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("An error has occured while attempting to check if the plugin is whitelisted.\n\nThe possible causes are:\n1. You do not have internet connection\n2. The tool is no longer in service\n3. The tool has moved\n4. The plugin is invalid\n\nException Message:\n" + ex.Message + "\n\nTag @DogIcing on the forum for help.");
                            }
                        }
                    }
                }

                if (hasManifestAtRoot && successful)
                {
                    string extractPath = AppDomain.CurrentDomain.BaseDirectory + "/plugins/" + filename;

                    JObject _plugins = (JObject)JsonConvert.DeserializeObject(Properties.Settings.Default.Plugin_List);

                    ((JArray)_plugins.GetValue("plugins")).Add(filename);

                    Properties.Settings.Default.Plugin_List = JsonConvert.SerializeObject(_plugins);
                    Properties.Settings.Default.Save();
                    ZipFile.ExtractToDirectory(dialog.FileName, extractPath);
                    plugins = (JObject)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(_plugins));
                    refreshPluginListBox();
                }
                else if (!hasManifestAtRoot && successful) {
                    MessageBox.Show("The plugin zip is invalid. No manifest.json file could be found at the root.");
                }
            }
        }

        private void refreshAliasListBox()
        {
            aliasList.Items.Clear();

            if (aliases != null)
            {
                List<string> keys = aliases.ToObject<Dictionary<string, object>>().Keys.ToList();

                foreach (string key in keys)
                {
                    var listboxitem = new ListBoxItem();
                    string tabs = "\t";
                    if (key.Length < 6)
                    {
                        tabs = "\t\t";
                    }
                    listboxitem.Content = "A-" + key + tabs + aliases[key].ToString();
                    listboxitem.Tag = key;
                    aliasList.Items.Add(listboxitem);
                }
            }
        }

        private void refreshPluginListBox()
        {
            plugin_list.Items.Clear();

            if (((JArray)plugins.GetValue("plugins")).Count > 0)
            {
                foreach (string plugin in (JArray)plugins.GetValue("plugins"))
                {
                    var listboxitem = new ListBoxItem();
                    listboxitem.Content = plugin;
                    listboxitem.Tag = plugin;
                    plugin_list.Items.Add(listboxitem);
                }
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start("cmd", "/c start " + e.Uri.AbsoluteUri);
        }

        private void plugin_info(object sender, EventArgs e)
        {
            MessageBox.Show("No information available (Future Release)");
        }

        private void delete_plugin(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to remove this plugin?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes){
                if (plugin_list.SelectedItem != null)
                {
                    string selected_plugin = ((ListBoxItem)plugin_list.SelectedItem).Tag.ToString();

                    JObject plugin_settings = (JObject)JsonConvert.DeserializeObject(Properties.Settings.Default.Plugin_List);
                    JArray _plugins = (JArray)plugin_settings.GetValue("plugins");

                    //JObject.Parse(plugins.ToString()).Property(selected_plugin).Remove();

                    foreach (var plugin in _plugins)
                    {
                        if (plugin.ToString() == selected_plugin)
                        {
                            plugin.Remove();
                            break;
                        }
                    }

                    try
                    {
                        plugins = (JObject)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(plugin_settings));
                        Properties.Settings.Default.Plugin_List = JsonConvert.SerializeObject(plugin_settings);
                        Properties.Settings.Default.Save();

                        Directory.Delete(AppDomain.CurrentDomain.BaseDirectory + "/plugins/" + selected_plugin, true);

                        MessageBox.Show("The plugin has been removed!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Save Error\nError Details:\n\n" + ex);
                    }

                    refreshPluginListBox();
                }
            }
        }

        private void AddAlias(object sender, EventArgs e)
        {
                AliasModifier aliasModifier = new AliasModifier("", "");

                AliasModifier.aliasOptions options = new AliasModifier.aliasOptions();

                if (aliasModifier.ShowDialog() == true)
                {
                    options = aliasModifier.options;
                }

            if (options.uuid != null && options.alias != null)
            {
                if (aliases.ContainsKey(options.alias))
                    {
                        MessageBox.Show("The alias already exists. Duplicate aliases are not supported.");
                    }
                    else
                    {
                        aliases.Add(options.alias, options.uuid);
                        string _aliases = JsonConvert.SerializeObject(aliases);
                        Properties.Settings.Default.Aliases = _aliases;
                        Properties.Settings.Default.Save();
                        refreshAliasListBox();
                    }
                }
        }

        private void credits(object sender, EventArgs e)
        {
            MessageBox.Show("Created by @DogIcing\n\nLibraries/Nuget Packages:\n- Microsoft.CodeAnalysis.CSharp.Scripting 4.9.2\n- Microsoft.Web.WebView2 1.0.2478.35\n- Newtonsoft.Json 13.0.3\n- SharpVectors 1.8.4\n- System.Drawing.Common 8.0.7\n\nBeta Testers:\n@StarlightStudios,@Tri-Angle,@Help,@UN7X,@Rodrikk");
        }

        private void plugin_editor(object sender, EventArgs e)
        {
            PluginAuthor pluginAuthor = new PluginAuthor();
            pluginAuthor.Show();
        }

        private void modded_editor_change(object sender, EventArgs e)
        {
            switch (modded_editor_options.SelectedIndex)
            {
                case 0:
                    Properties.Settings.Default.Modded_Editor = true;
                    break;
                case 1:
                    Properties.Settings.Default.Modded_Editor = false;
                    break;
            }
            Properties.Settings.Default.Save();
        }

        private void ModifyAlias(object sender, EventArgs e)
        {
            if (aliasList.SelectedItem != null)
            {
                dynamic item = aliasList.Items[aliasList.SelectedIndex];
                string tag = item.Tag;

                AliasModifier aliasModifier = new AliasModifier(tag, aliases[tag].ToString());

                AliasModifier.aliasOptions options = new AliasModifier.aliasOptions();

                if (aliasModifier.ShowDialog() == true)
                {
                    options = aliasModifier.options;
                }

                if (options.uuid != null && options.alias != null)
                {
                    if (aliases.ContainsKey(options.alias) && options.original_alias != options.alias)
                    {
                        MessageBox.Show("The alias already exists. Duplicate aliases are not supported.");
                    }
                    else
                    {
                        string _aliases = JsonConvert.SerializeObject(aliases);
                        _aliases = _aliases.Replace("\"" + options.original_alias + "\":", "\"" + options.alias + "\":");
                        aliases = (JObject)JsonConvert.DeserializeObject(_aliases);
                        aliases[options.alias] = options.uuid;
                        _aliases = JsonConvert.SerializeObject(aliases);
                        Properties.Settings.Default.Aliases = _aliases;
                        Properties.Settings.Default.Save();
                        item.Tag = options.alias;
                        refreshAliasListBox();
                    }
                }
            }
        }

        private void DeleteAlias(object sender, EventArgs e)
        {
            if (aliasList.SelectedItem != null)
            {
                dynamic item = aliasList.Items[aliasList.SelectedIndex];
                string tag = item.Tag;
                aliases.Remove(tag);
                string _aliases = JsonConvert.SerializeObject(aliases);
                Properties.Settings.Default.Aliases = _aliases;
                Properties.Settings.Default.Save();
                refreshAliasListBox();
            }
        }

        private void resetHome(object sender, EventArgs e)
        {
            HS.Properties.Settings.Default.Projects_Created = 0;
            HS.Properties.Settings.Default.Projects_Played = 0;
            HS.Properties.Settings.Default.Seconds_Spent = 0;
            HS.Properties.Settings.Default.Users_Viewed = 0;
            HS.Properties.Settings.Default.Time_Saved = 0;
            HS.Properties.Settings.Default.Save();
            MessageBox.Show("Reset successful");
        }
    }
}
