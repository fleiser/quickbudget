using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;

namespace QuickBudget_WPFSQLite
{
    /// <summary>
    /// Interaction logic for YesNoReCategorize.xaml
    /// </summary>
    public partial class YesNoReCategorize : MetroWindow
    {
        public YesNoReCategorize(string dialog, List<Category> categories)
        {
            InitializeComponent();
            ComboBoxCategory.ItemsSource = categories;
            checkBox.IsChecked = true;
            ComboBoxCategory.IsEnabled = true;
            ComboBoxCategory.SelectedIndex = 0;
            TextBlockDialog.Text = dialog;
            ButtonYes.Focus();
        }

        public bool IsSuccesful { get; set; } = false;
        public bool Recategorize { get; set; } = false;
        public Category Category { get; set; }
        public string Dialog { get; set; }


        private void ButtonNo_Click_1(object sender, RoutedEventArgs e)
        {
            IsSuccesful = false;
            Close();
        }

        private void ButtonYes_Click_1(object sender, RoutedEventArgs e)
        {
            IsSuccesful = true;
            Close();
        }

        private void checkBox_Checked(object sender, RoutedEventArgs e)
        {
            if (checkBox.IsChecked.Value)
            {
                ComboBoxCategory.IsEnabled = true;
                ComboBoxCategory.SelectedIndex = 0;
                Recategorize = true;
            }
            else
            {
                ComboBoxCategory.SelectedIndex = 0;
                ComboBoxCategory.IsEnabled = false;
                Recategorize = false;
            }
        }

        private void ComboBoxCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Category = (Category) ComboBoxCategory.SelectedItem;
        }

        private void checkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (checkBox.IsChecked.Value)
            {
                ComboBoxCategory.IsEnabled = true;
                ComboBoxCategory.SelectedIndex = 0;
                Recategorize = true;
            }
            else
            {
                ComboBoxCategory.IsEnabled = false;
                Recategorize = false;
            }
        }
    }
}

