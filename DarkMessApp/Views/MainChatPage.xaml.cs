using System.Diagnostics;
using DarkMessApp.Helpers;
using DarkMessApp.Services;
using DarkMessApp.Utils;
using DarkMessApp.Utils.Models;

namespace DarkMessApp.Views;

public partial class MainChatPage : ContentPage
{
    private MainPageViewModel MainPageViewModel;
    private bool _isMenuVisible = false;
    private double _lastWidth;
    private double _lastColumnWidth;
    private int currentChatId;
    private ChatModel currentChat;
    private enum ActiveOverlay {
        None,
        Menu,
        Profile,
        ChatCreate,
        ToastNotification,
        ChatSettings
    }

    private ActiveOverlay _currentOverlay = ActiveOverlay.None;

    public MainChatPage()
    {
        InitializeComponent();
        MainPageViewModel = new MainPageViewModel();
        BindingContext = MainPageViewModel;
        if (Content is Grid mainGrid && mainGrid.ColumnDefinitions.Count > 1)
        {
            _lastColumnWidth = mainGrid.ColumnDefinitions[1].Width.Value;
        }

        MessageCollectionView.Scrolled += (sender, e) =>
        {
            var firstVisible = e.FirstVisibleItemIndex;
            var lastVisible = e.LastVisibleItemIndex;

            for (int i = firstVisible; firstVisible > 0 && i <= lastVisible; i++)
            {
                if (MainPageViewModel.MessageVM.Messages[i] is MessageModel msg && !msg.IsRead && !msg.IsMyMessage)
                {
                    MainPageViewModel.MessageVM.MarkAsRead(msg.MessageId);
                }
            }
        };
        SideMenu.HorizontalOptions = LayoutOptions.Start;
        SideMenu.TranslationX = -300;
        ProfileWindow.HorizontalOptions = LayoutOptions.Center;
        ProfileWindow.VerticalOptions = LayoutOptions.Center;
        ChatSettingsWindow.HorizontalOptions = LayoutOptions.Center;
        ChatSettingsWindow.VerticalOptions = LayoutOptions.Center;
    }
    
    private async void OnChatSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is ChatModel selectedChat) {
            currentChatId = selectedChat.ChatId;
            currentChat =  selectedChat;
            ChatHeaderGrid.IsVisible = true;
            ChatHeader.Text = selectedChat.Name;
            ChatSendGrid.IsVisible = true;
            await WebSocketService.ChatInit(selectedChat.ChatId);
        }
        var previousSelectedItem = e.PreviousSelection.FirstOrDefault();
        if (previousSelectedItem != null && ChatListView.ItemTemplate.CreateContent() is ViewCell previousViewCell) {
            if (previousViewCell.View is Frame previousFrame)
                previousFrame.BorderColor = Color.FromArgb("#262626");
        }
        
        var selectedItem = e.CurrentSelection.FirstOrDefault();
        if (selectedItem != null && ChatListView.ItemTemplate.CreateContent() is ViewCell currentViewCell) {
            if (currentViewCell.View is Frame selectedFrame)
                selectedFrame.BorderColor = Color.FromArgb("#7B2CBF");
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Debug.WriteLine("Прогрузка окна чата");
        await WebSocketService.DataInit("chat_list");
        await WebSocketService.DataInit("user_profile");
    }

    protected override bool OnBackButtonPressed()
    {
        if (_currentOverlay == ActiveOverlay.Menu) {
            HideMenu();
        }
        else {
            if (_currentOverlay == ActiveOverlay.Profile) HideProfile();
            ShowMenu();
        }

        return base.OnBackButtonPressed();
    }

    private async void OnSendMessageClicked(object sender, EventArgs e)
    {
        if (ChatListView.SelectedItem is ChatModel chat && !string.IsNullOrWhiteSpace(MessageEntry.Text))
        {
            await WebSocketService.SendChatMessage(MessageEntry.Text, chat.ChatId);
        }
        else Debug.WriteLine(MessageEntry.Text + ": Ну, не подходит");

        MessageEntry.Text = string.Empty;
    }

    private void OnEntryCompleted(object sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(MessageEntry.Text))
        {
            OnSendMessageClicked(null, null);
        }
    }

    private void ToggleMenu(object sender, EventArgs e)
    {
        if (_currentOverlay == ActiveOverlay.Menu) {
            HideMenu();
        }
        else {
            if (_currentOverlay == ActiveOverlay.Profile) HideProfile();
            ShowMenu();
        }
    }

    private void ShowProfileButton_Clicked(object sender, EventArgs e)
    {
        if (_currentOverlay == ActiveOverlay.Profile) {
            HideProfile();
        }
        else {
            if (_currentOverlay == ActiveOverlay.Menu) HideMenu();
            ShowProfile();
        }
    }

    private void ShowMenu()
    {
        _currentOverlay = ActiveOverlay.Menu;
        GlobalOverlay.IsVisible = true;
        _ = GlobalOverlay.FadeTo(0.5, 200);
        _ = SideMenu.TranslateTo(0, 0, 300, Easing.SinOut);
    }

    private void HideMenu()
    {
        _currentOverlay = ActiveOverlay.None;
        _ = SideMenu.TranslateTo(-300, 0, 250, Easing.SinIn);
        _ = GlobalOverlay.FadeTo(0, 200);
        GlobalOverlay.IsVisible = false;
    }

    private void ShowProfile()
    {
        _currentOverlay = ActiveOverlay.Profile;
        GlobalOverlay.IsVisible = true;
        _ = GlobalOverlay.FadeTo(0.5, 200);
        ProfileWindow.IsVisible = true;
        _ = ProfileWindow.FadeTo(1, 200);
    }

    private void HideProfile()
    {
        _currentOverlay = ActiveOverlay.None;
        _ = ProfileWindow.FadeTo(0, 200);
        ProfileWindow.IsVisible = false;
        _ = GlobalOverlay.FadeTo(0, 200);
        GlobalOverlay.IsVisible = false;
    }

    private void OnGlobalOverlayTapped(object sender, EventArgs e)
    {
        switch (_currentOverlay) {
            case ActiveOverlay.Menu:
                HideMenu();
                break;
            case ActiveOverlay.Profile:
                HideProfile();
                break;
            case ActiveOverlay.ChatCreate:
                HideCreateChatWindow();
                break;
            case ActiveOverlay.ToastNotification:
                HideToastNotification();
                break;
            case ActiveOverlay.ChatSettings:
                HideChatSettings();
                break;
        }
    }

    private void HideToastNotification() {
        _ = ToastNotification.FadeTo(0, 200);
        ToastNotification.IsVisible = false;
    }

    private async void EditButton_Clicked(object sender, EventArgs e) {
        // Запрашиваем новые данные
        string newUsername = await DisplayPromptAsync(
            "Редактирование",
            "Введите новое имя:",
            initialValue: UserProfileStore.ProfileModel.Username);

        string newEmail = await DisplayPromptAsync(
            "Редактирование",
            "Введите новый email:",
            initialValue: UserProfileStore.ProfileModel.Email,
            keyboard: Keyboard.Email);

        string newPassword = await DisplayPromptAsync(
            "Редактирование",
            "Введите новый пароль:",
            initialValue: UserProfileStore.ProfileModel.Password);

        if (!string.IsNullOrWhiteSpace(newUsername))
        {
            UserProfileStore.ProfileModel.Username = newUsername;
            UsernameLabel.Text = newUsername;
            await WebSocketService.SendUsernameChange(newUsername);
        }

        if (!string.IsNullOrWhiteSpace(newEmail))
        {
            UserProfileStore.ProfileModel.Email = newEmail;
            EmailLabel.Text = newEmail;
            await WebSocketService.SendEmailChange(newEmail);
        }

        if (!string.IsNullOrWhiteSpace(newPassword))
        {
            UserProfileStore.ProfileModel.Password = newPassword;
            PasswordLabel.Text = newEmail;
            await WebSocketService.SendPasswordChange(newEmail);
        }
    }

    private void OnCreateChatWindow(object sender, EventArgs e) {
        HideMenu();
        _currentOverlay = ActiveOverlay.ChatCreate;
        GlobalOverlay.IsVisible = true;
        _ = GlobalOverlay.FadeTo(0.5, 200);
        CreateChatWindow.IsVisible = true;
        _ = CreateChatWindow.FadeTo(1, 200);
    }

    private void HideCreateChatWindow() {
        _currentOverlay = ActiveOverlay.None;
        _ = CreateChatWindow.FadeTo(0, 200);
        CreateChatWindow.IsVisible = false;
        _ = GlobalOverlay.FadeTo(0, 200);
        GlobalOverlay.IsVisible = false;
    }

    private void HideCreateChatWindowClicked(object sender, EventArgs e) {
        _currentOverlay = ActiveOverlay.Menu;
        _ = CreateChatWindow.FadeTo(0, 200);
        CreateChatWindow.IsVisible = false;
        _ = GlobalOverlay.FadeTo(0, 200);
        GlobalOverlay.IsVisible = false;
        ShowMenu();
    }

    private async void OnMessageTapped(object sender, EventArgs e)
    {
        if (sender is Frame frame && frame.BindingContext is MessageModel message) {
            var result = await DisplayActionSheet("Сообщение", "Отмена", null, "Копировать текст");
            if (result == "Копировать текст") {
                await Clipboard.SetTextAsync(message.Content);
                await DisplayAlert("Скопировано", "Текст сообщения скопирован", "OK");
            }
        }
    }
    private async void ShowToastNotification(string message) {
        ToastNotification.Text = message;
        ToastNotification.IsVisible = true;
        
        await ToastNotification.TranslateTo(0, 50, 200, Easing.SinInOut);
        await Task.Delay(2000);
        
        await ToastNotification.TranslateTo(0, -100, 200, Easing.SinInOut);
        ToastNotification.IsVisible = false;
    }

    private async void OnCreatePrivateChat(object sender, EventArgs e) {
        if (!string.IsNullOrWhiteSpace(UsernameEntry.Text)) {
            await WebSocketService.SendCreateChat(UsernameEntry.Text);
            UsernameEntry.Text = string.Empty;
            HideCreateChatWindow();
        }
    }
    
    private async void OnAddMemberClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NewMemberEntry.Text)) return;
        
            var currentChat = MainPageViewModel.ChatVM.Chats.FirstOrDefault(c => c.ChatId == currentChatId);
            
            await WebSocketService.SendChatMemberAdd(
                currentChat.ChatId, 
                NewMemberEntry.Text);
            
            NewMemberEntry.Text = string.Empty;
            ShowToastNotification("Запрос на добавление отправлен");
        
    }

    private async void OnSaveChatSettings(object sender, EventArgs e)
    {
        var newName = ChatNameEntry.Text;
        if (string.IsNullOrWhiteSpace(newName))
        {
            ShowToastNotification("Введите название чата");
            return;
        }
    
        try
        {
            var currentChat = MainPageViewModel.ChatVM.Chats
                .FirstOrDefault(c => c.ChatId == currentChatId);
            if (currentChat == null) 
            {
                ShowToastNotification("Чат не выбран");
                return;
            }
            await WebSocketService.SendChatNameChange(
                currentChat.ChatId, 
                newName);
            
            currentChat.Name = newName;
            ChatHeader.Text = newName;
            ShowToastNotification("Название чата обновлено");
            HideChatSettings();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ошибка при обновлении названия: {ex}");
            ShowToastNotification("Ошибка сохранения");
        }
    }
    
    private async Task LoadChatMembers()
    {
        try {
            await WebSocketService.SendChatMemberInit(currentChatId);
        }
        catch (Exception ex) {
            Debug.WriteLine($"Ошибка при загрузке участников: {ex}");
            ShowToastNotification("Ошибка загрузки участников");
        }
    }
    private async void OnChatSettingsClicked(object sender, EventArgs e)
    {
        _currentOverlay = ActiveOverlay.ChatSettings;
        GlobalOverlay.IsVisible = true;
        _ = GlobalOverlay.FadeTo(0.5, 200);
        ChatSettingsWindow.IsVisible = true;
        _ = ChatSettingsWindow.FadeTo(1, 200);
        ChatNameEntry.Text = currentChat.Name;
        await LoadChatMembers();
    }
    private void HideChatSettings()
    {
        _currentOverlay = ActiveOverlay.None;
        _ = ChatSettingsWindow.FadeTo(0, 200);
        ChatSettingsWindow.IsVisible = false;
        _ = GlobalOverlay.FadeTo(0, 200);
        GlobalOverlay.IsVisible = false;
    }
    private void OnCancelChatSettings(object sender, EventArgs e)
    {
        HideChatSettings();
    }
    private void AppExit(object sender, EventArgs e) {
        try {
            _ = WebSocketService.DisconnectAsync();
            JwtTokenHandler.RemoveToken();
            if (Application.Current != null) Application.Current.MainPage = new LoginPage(); 
        }
        catch (Exception exception) {
            Debug.WriteLine(exception);
        }
    }
}