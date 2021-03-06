﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.Options
{
    public class UserSettings
    {
        public enum UserDisplaySetting { Avatar, Letter, Colour }

        public string Username { get { return UsernameProperty.Value; } set { UsernameProperty.Value = value; } }
        public event EventHandler UsernameChanged { add { UsernameProperty.Changed += value; } remove { UsernameProperty.Changed -= value; } }
        public event EventHandler UsernameChanging { add { UsernameProperty.Changing += value; } remove { UsernameProperty.Changing -= value; } }
        private readonly SettingProperty<string> UsernameProperty;
        public const string DefaultUsername = null;
        public string UserImageUrl { get { return UserImageUrlProperty.Value; } set { UserImageUrlProperty.Value = value; } }
        public event EventHandler UserImageUrlChanged { add { UserImageUrlProperty.Changed += value; } remove { UserImageUrlProperty.Changed -= value; } }
        public event EventHandler UserImageUrlChanging { add { UserImageUrlProperty.Changing += value; } remove { UserImageUrlProperty.Changing -= value; } }
        private readonly SettingProperty<string> UserImageUrlProperty;
        public const string DefaultImageUrl = null;
        public UserDisplaySetting UserCodeDisplay { get { return UserCodeDisplayProperty.Value; } set { UserCodeDisplayProperty.Value = value; } }
        public event EventHandler UserCodeDisplayChanged { add { UserCodeDisplayProperty.Changed += value; } remove { UserCodeDisplayProperty.Changed -= value; } }
        public event EventHandler UserCodeDisplayChanging { add { UserCodeDisplayProperty.Changing += value; } remove { UserCodeDisplayProperty.Changing -= value; } }
        private readonly SettingProperty<UserDisplaySetting> UserCodeDisplayProperty;
        public const UserDisplaySetting DefaultUserCodeDisplay = UserDisplaySetting.Avatar;
        public UserDisplaySetting UserTabDisplay { get { return UserTabDisplayProperty.Value; } set { UserTabDisplayProperty.Value = value; } }
        public event EventHandler UserTabDisplayChanged { add { UserTabDisplayProperty.Changed += value; } remove { UserTabDisplayProperty.Changed -= value; } }
        public event EventHandler UserTabDisplayChanging { add { UserTabDisplayProperty.Changing += value; } remove { UserTabDisplayProperty.Changing -= value; } }
        private readonly SettingProperty<UserDisplaySetting> UserTabDisplayProperty;
        public const UserDisplaySetting DefaultUserTabDisplay = UserDisplaySetting.Avatar;
        public UserSettings()
        {
            UsernameProperty = new SettingProperty<string>(this);
            UsernameProperty.Changed += (s, e) => TeamCodingPackage.Current.Logger.WriteInformation($"Changing setting {nameof(Username)}: {Username}");

            UserImageUrlProperty = new SettingProperty<string>(this);
            UserImageUrlProperty.Changed += (s, e) => TeamCodingPackage.Current.Logger.WriteInformation($"Changing setting {nameof(UserImageUrl)}: {UserImageUrl}");

            UserCodeDisplayProperty = new SettingProperty<UserDisplaySetting>(this);
            UserCodeDisplayProperty.Changed += (s, e) => TeamCodingPackage.Current.Logger.WriteInformation($"Changing setting {nameof(UserCodeDisplay)}: {UserCodeDisplay}");

            UserTabDisplayProperty = new SettingProperty<UserDisplaySetting>(this);
            UserTabDisplayProperty.Changed += (s, e) => TeamCodingPackage.Current.Logger.WriteInformation($"Changing setting {nameof(UserTabDisplay)}: {UserTabDisplay}");
        }
    }
}
