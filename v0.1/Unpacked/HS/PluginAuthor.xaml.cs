using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
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
using System.Windows.Shapes;

namespace HS
{
    /// <summary>
    /// Interaction logic for PluginAuthor.xaml
    /// </summary>
    public partial class PluginAuthor : Window
    {
        public PluginAuthor()
        {
            InitializeComponent();
        }

        private static readonly string[] SuggestionValues = {
            "System",
            "System.Windows"
        };

        public void Test_Plugin(object sender, EventArgs e)
        {
            string _UUID = "";
            if(Regex.IsMatch(UUID.Text, @"(a|A)-.*"))
            {
                // Alias

                string _aliases = Properties.Settings.Default.Aliases;
                dynamic aliases = JsonConvert.DeserializeObject(_aliases);

                if (aliases.ContainsKey(Regex.Replace(UUID.Text, @"(a|A)-", "")))
                {
                    _UUID = aliases[Regex.Replace(UUID.Text, @"(a|A)-", "")];
                }
                else
                {
                    MessageBox.Show("Alias Does Not Exist");
                }
            }
            else
            {
                _UUID = UUID.Text;
            }
        }

        private string _currentInput = "";
        private string _currentSuggestion = "";
        private string _currentText = "";

        private int _selectionStart;
        private int _selectionLength;
        private void LoadSuggestions(object sender, TextChangedEventArgs e)
        {
            var input = codeBox.Text;
            if (input.Length > _currentInput.Length && input != _currentSuggestion)
            {
                _currentSuggestion = SuggestionValues.FirstOrDefault(x => x.ToLower().StartsWith(input.ToLower()));
                if (_currentSuggestion != null)
                {
                    _currentText = _currentSuggestion;
                    _selectionStart = input.Length;
                    _selectionLength = _currentSuggestion.Length - input.Length;

                    codeBox.Text = _currentText;
                    codeBox.Select(_selectionStart, _selectionLength);
                }
            }
            _currentInput = input;
        }
    }
}
