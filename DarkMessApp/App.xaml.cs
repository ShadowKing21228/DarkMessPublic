using DarkMessApp.Views;

namespace DarkMessApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        //MainPage = new AppShell();
        MainPage = new ErrorPage();
        //MainPage = new NavigationPage(new MainChatPage());
    }
}