using SharpVectors.Dom.Svg;
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
    /// Interaction logic for AliasModifier.xaml
    /// </summary>
    public partial class AliasModifier : Window
    {

        public aliasOptions options = new aliasOptions();

        public bool readyToSave = false;
        public AliasModifier(string alias, string uuid)
        {
            InitializeComponent();
            options.original_alias = alias;
            alias_input.Text = alias;
            uuid_input.Text = uuid;
        }

        private void save(object sender, EventArgs e)
        {
            options.uuid = uuid_input.Text;
            options.alias = alias_input.Text;
            this.DialogResult = true;
            this.Close();
        }

        private void cancel(object sender, EventArgs e)
        {
            this.Close();
        }

        public class aliasOptions
        {
            public string alias;
            public string uuid;
            public string original_alias;
        }
    }
}
