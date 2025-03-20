using System.Windows;
using System.Windows.Controls;

namespace AIUnittestExtension
{
    /// <summary>
    /// Interaction logic for LoadingControl.xaml
    /// </summary>
    public partial class LoadingControl : UserControl
    {
        public LoadingControl()
        {
            InitializeComponent();
            Hide();
        }
        public async Task ShowLoadingAsync(Func<Task> action)
        {
            Visibility = Visibility.Visible; // Show loading
            await action();
            Visibility = Visibility.Collapsed; // Hide after completion
        }
        public void Hide()
        {
            Visibility = Visibility.Collapsed;
        }
    }
}
