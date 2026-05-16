using Microsoft.Maui.Controls;

namespace Avi
{
    public partial class MainWindow : Window
    {
        private readonly MainPage _mainPage;

        public MainWindow(MainPage mainPage)
        {
            InitializeComponent();
            _mainPage = mainPage;
            Page = mainPage;

            // Set the draggable region
        }

        private void SettingsButton_Clicked(object sender, EventArgs e)
        {
            _mainPage.ToggleSettings();
        }
    }
}