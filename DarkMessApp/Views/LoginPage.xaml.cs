using System.Diagnostics;
using DarkMessApp.Services;
using DarkMessApp.Utils.Models;

namespace DarkMessApp.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }
    private async void OnLogin(object sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(EmailText.Text) && !string.IsNullOrWhiteSpace(PasswordText.Text)) {
            var user = new UserModel("", EmailText.Text, PasswordText.Text);
            await ApiService.SendLogin(user);
            await WebSocketService.ConnectAsync(WebSocketService.ServerWSUri);
            Debug.WriteLine("Подключение успешно! Наверное.");
            if (WebSocketService.IsConnected() && Application.Current != null) Application.Current.MainPage = new MainChatPage();
        }
        else {
            EmailText.Text = string.Empty;
            PasswordText.Text = string.Empty;
        }
    }
    private void OnLabelTapped(object sender, EventArgs e)
    {
        if (Application.Current != null) Application.Current.MainPage = new RegistrationPage();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await WebSocketService.ConnectAsync(WebSocketService.ServerWSUri);
        if (WebSocketService.IsConnected() && Application.Current != null) Application.Current.MainPage = new MainChatPage();
    }
}