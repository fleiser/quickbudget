using System.Windows;
using MahApps.Metro.Controls;

namespace QuickBudget_WPFSQLite
{
    /// <summary>
    /// Interaction logic for YesNo.xaml
    /// </summary>
    public partial class YesNo : MetroWindow
    {
        public YesNo(string dialog)
        {
            InitializeComponent();
            TextBlockDialog.Text = dialog;
            ButtonYes.Focus();
        }

        public bool IsSuccesful { get; set; } = false;
        public  string Dialog { get; set; }

        private void ButtonYes_Click(object sender, RoutedEventArgs e)
        {
            IsSuccesful = true;
            Close();
        }

        private void ButtonNo_Click(object sender, RoutedEventArgs e)
        {
            IsSuccesful = false;
            Close();
        }
    }
}
