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
using MahApps.Metro.Controls;

namespace QuickBudget_WPFSQLite
{
    /// <summary>
    /// Interaction logic for ManageMasterCategory.xaml
    /// </summary>
    public partial class ManageMasterCategory : MetroWindow
    {
        public List<MasterCategory> MasterCategories;

        public ManageMasterCategory(List<MasterCategory> masterCategories)
        {
            MasterCategories = masterCategories;
            InitializeComponent();
            Load();
        }

        private void ButtonConfirm_OnClick(object sender, RoutedEventArgs e)
        {

            Close();
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Load()
        {
            DataGridCategories.ItemsSource = MasterCategories;
            DataGridCategories.Items.Refresh();
        }

        private void ButtonAdd_OnClick(object sender, RoutedEventArgs e)
        {
            var name = textBoxAdd.Text;
            if (string.IsNullOrWhiteSpace(name)) return;
            var masterCategory = new MasterCategory(0,name);
            MasterCategories.Add(masterCategory);
            Load();
        }

        private void TextBoxAdd_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var name = textBoxAdd.Text;
            ButtonAdd.IsEnabled = !string.IsNullOrWhiteSpace(name);
        }

        private void UIElement_OnLostFocus(object sender, RoutedEventArgs e)
        {
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button) sender;
            var id = (long) button.Tag;
            MasterCategories.RemoveAll(x => x.Id.Equals(id));
            Load();
        }
    }
}
