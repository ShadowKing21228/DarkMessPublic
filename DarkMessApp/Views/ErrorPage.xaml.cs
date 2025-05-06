using System.Diagnostics;
using DarkMessApp.Utils;

namespace DarkMessApp.Views;

public partial class ErrorPage : ContentPage
{
    public ErrorPage()
    {
        InitializeComponent();
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        TryConnectTo();
    }

    private async void TryConnect(object sender, EventArgs e)
    {
        TryConnectTo();
    }
    private async void TryConnectTo() {
        var connection = await MessUtils.PingServer();
        if (connection && Application.Current != null) {
            Application.Current.MainPage = new LoginPage();
            Debug.WriteLine("Connected to DarkMess");
        }
        else if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet || Connectivity.Current.NetworkAccess != NetworkAccess.ConstrainedInternet) {
            MainLabel.Text = "Error: No Internet Connection";
            MainLabel.TextColor = Colors.Red;
            TryConnectButton.IsVisible = true;
        }
        else {
            MainLabel.Text = "Error: Server is not available";
            MainLabel.TextColor = Colors.Red;
            TryConnectButton.IsVisible = true;
            Debug.WriteLine("Server is not available");
        }
    }
}