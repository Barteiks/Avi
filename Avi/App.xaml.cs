using Microsoft.Extensions.DependencyInjection;

namespace Avi
{
    public partial class App : Application
    {
        private readonly MainPage _mainPage;
        public App(MainPage mainPage)
        {
            InitializeComponent();
            _mainPage = mainPage;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            //return new Window(new AppShell());
#if WINDOWS
            // zwróć MainWindow z prawdziwym TitleBar
            return new Avi.MainWindow(_mainPage);

#else
            // reszta platform używa normalnego MainPage
            return new Window(new AppShell());
#endif
        }
    }
}