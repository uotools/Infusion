﻿using Infusion.Desktop.Profiles;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Infusion.Desktop.Launcher
{
    internal sealed class LauncherViewModel : INotifyPropertyChanged
    {
        private Profile selectedProfile;
        private ObservableCollection<Profile> profiles = new ObservableCollection<Profile>
        {
            new Profile {Name = "new profile"}
        };
        private readonly Action<string> passwordSetter;

        private bool showPassword;
        public bool ShowPassword
        {
            get => showPassword;
            set
            {
                showPassword = value;
                OnPropertyChanged();
                OnPropertyChanged("HidePassword");
            }
        }
        public bool HidePassword => !showPassword;

        public ObservableCollection<Profile> Profiles
        {
            get => profiles;
            set
            {
                profiles = value;
                if (profiles.Any())
                {
                    SelectedProfile = profiles.First();
                }
            }
        }

        public LauncherViewModel(Action<string> passwordSetter)
        {
            this.passwordSetter = passwordSetter;
            SelectedProfile = Profiles.First();
        }

        public Profile SelectedProfile
        {
            get => selectedProfile;
            set
            {
                selectedProfile = value;
                OnPropertyChanged();
                OnSelectedClientTypeChanged();
                passwordSetter(selectedProfile.LauncherOptions.Password);
            }
        }

        public string SelectedProfileName
        {
            get => SelectedProfile.Name;
            set => SelectedProfile.Name = value;
        }

        public void NewProfile()
        {
            var profile = new Profile { Name = "new profile" };
            Profiles.Add(profile);
            SelectedProfile = profile;

            // ReSharper disable once ExplicitCallerInfoArgument
            OnPropertyChanged("CanDeleteSelectedProfile");
        }

        public bool CanDeleteSelectedProfile => Profiles.Count > 1;

        public bool ClassicClientOptionsVisible => SelectedProfile.LauncherOptions.ClientType == UltimaClientType.Classic;
        public bool OrionOptionsVisible => SelectedProfile.LauncherOptions.ClientType == UltimaClientType.Orion;

        public UltimaClientType SelectedClientType
        {
            get => SelectedProfile.LauncherOptions.ClientType;
            set
            {
                SelectedProfile.LauncherOptions.ClientType = value;
                OnSelectedClientTypeChanged();
            }
        }

        private void OnSelectedClientTypeChanged()
        {
            OnPropertyChanged("SelectedClientType");
            OnPropertyChanged("ClassicClientOptionsVisible");
            OnPropertyChanged("OrionOptionsVisible");
        }

        public void DeleteSelectedProfile()
        {
            if (Profiles.Count > 1)
            {
                var profileToRemove = SelectedProfile;
                SelectedProfile = Profiles.First(x => x != profileToRemove);
                Profiles.Remove(profileToRemove);
                ProfileRepositiory.DeleteProfile(profileToRemove);

            }
            // ReSharper disable once ExplicitCallerInfoArgument
            OnPropertyChanged("CanDeleteSelectedProfile");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}