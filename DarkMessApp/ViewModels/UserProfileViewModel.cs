using System.ComponentModel;
using System.Diagnostics;
using DarkMessApp.Helpers;
using DarkMessApp.Utils.Models;

namespace DarkMessApp.ViewModels;

public class UserProfileViewModel : INotifyPropertyChanged
{
    public UserProfileModel ProfileModel => UserProfileStore.ProfileModel;
    
    public event PropertyChangedEventHandler? PropertyChanged;
}