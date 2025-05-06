using DarkMessApp.Services;
using DarkMessApp.Utils.Models;

namespace DarkMessApp.Views;

public partial class RegistrationPage : ContentPage
{
    public RegistrationPage()
    {
        InitializeComponent();
    }
    private async void OnRegister(object sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(EmailText.Text) && !string.IsNullOrWhiteSpace(PasswordText.Text) && !string.IsNullOrWhiteSpace(UsernameText.Text)) {
            var user = new UserModel(UsernameText.Text, EmailText.Text, PasswordText.Text);
            await ApiService.SendRegistration(user);
            await WebSocketService.ConnectAsync(WebSocketService.ServerWSUri);
            if (WebSocketService.IsConnected() && Application.Current != null)
                Application.Current.MainPage = new MainChatPage();
        }
        else {
            UsernameText.Text = string.Empty;
            EmailText.Text = string.Empty;
            PasswordText.Text = string.Empty;
        }
    }
    private void OnLabelTapped(object sender, EventArgs e)
    {
        if (Application.Current != null) Application.Current.MainPage = new LoginPage();
    }
}