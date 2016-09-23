﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.Options
{
    public class SharedSettings
    {
        public string FileBasedPersisterPath { get { return FileBasedPersisterPathProperty.Value; } set { FileBasedPersisterPathProperty.Value = value; } }
        public event EventHandler FileBasedPersisterPathChanged { add { FileBasedPersisterPathProperty.Changed += value; } remove { FileBasedPersisterPathProperty.Changed -= value; } }
        public event EventHandler FileBasedPersisterPathChanging { add { FileBasedPersisterPathProperty.Changing += value; } remove { FileBasedPersisterPathProperty.Changing -= value; } }
        public readonly SettingProperty<string> FileBasedPersisterPathProperty;
        public const string DefaultFileBasedPersisterPath = null;
        public string RedisServer { get { return RedisServerProperty.Value; } set { RedisServerProperty.Value = value; } }
        public event EventHandler RedisServerChanged { add { RedisServerProperty.Changed += value; } remove { RedisServerProperty.Changed -= value; } }
        public event EventHandler RedisServerChanging { add { RedisServerProperty.Changing += value; } remove { RedisServerProperty.Changing -= value; } }
        public readonly SettingProperty<string> RedisServerProperty;
        public const string DefaultRedisServer = null;
        public string SlackToken { get { return SlackTokenProperty.Value; } set { SlackTokenProperty.Value = value; } }
        public event EventHandler SlackTokenChanged { add { SlackTokenProperty.Changed += value; } remove { SlackTokenProperty.Changed -= value; } }
        public event EventHandler SlackTokenChanging { add { SlackTokenProperty.Changing += value; } remove { SlackTokenProperty.Changing -= value; } }
        public readonly SettingProperty<string> SlackTokenProperty;
        public const string DefaultSlackToken = null;
        public string SlackChannel { get { return SlackChannelProperty.Value; } set { SlackChannelProperty.Value = value; } }
        public event EventHandler SlackChannelChanged { add { SlackChannelProperty.Changed += value; } remove { SlackChannelProperty.Changed -= value; } }
        public event EventHandler SlackChannelChanging { add { SlackChannelProperty.Changing += value; } remove { SlackChannelProperty.Changing -= value; } }
        public readonly SettingProperty<string> SlackChannelProperty;
        public const string DefaultSlackChannel = null;
        public string SqlServerConnectionString { get { return SqlServerConnectionStringProperty.Value; } set { SqlServerConnectionStringProperty.Value = value; } }
        public event EventHandler SqlServerConnectionStringChanged { add { SqlServerConnectionStringProperty.Changed += value; } remove { SqlServerConnectionStringProperty.Changed -= value; } }
        public event EventHandler SqlServerConnectionStringChanging { add { SqlServerConnectionStringProperty.Changing += value; } remove { SqlServerConnectionStringProperty.Changing -= value; } }
        public readonly SettingProperty<string> SqlServerConnectionStringProperty;
        public const string DefaultSqlServerConnectionString = null;
        public string WinServiceIPAddress { get { return WinServiceIPAddressProperty.Value; } set { WinServiceIPAddressProperty.Value = value; } }
        public event EventHandler WinServiceIPAddressChanged { add { WinServiceIPAddressProperty.Changed += value; } remove { WinServiceIPAddressProperty.Changed -= value; } }
        public event EventHandler WinServiceIPAddressChanging { add { WinServiceIPAddressProperty.Changing += value; } remove { WinServiceIPAddressProperty.Changing -= value; } }
        public readonly SettingProperty<string> WinServiceIPAddressProperty; // TODO: Use IPAddress rather than string
        public const string DefaultWinServiceIPAddress = null;
        public SharedSettings()
        {
            FileBasedPersisterPathProperty = new SettingProperty<string>(this, (v) => System.IO.Directory.Exists(v));
            FileBasedPersisterPathProperty.Changed += (s, e) => TeamCodingPackage.Current.Logger.WriteInformation($"Changing setting {nameof(FileBasedPersisterPath)}: {FileBasedPersisterPath}");

            RedisServerProperty = new SettingProperty<string>(this);
            RedisServerProperty.Changed += (s, e) => TeamCodingPackage.Current.Logger.WriteInformation($"Changing setting {nameof(RedisServer)}: {RedisServer}");

            SlackTokenProperty = new SettingProperty<string>(this);
            SlackTokenProperty.Changed += (s, e) => TeamCodingPackage.Current.Logger.WriteInformation($"Changing setting {nameof(SlackToken)}: {SlackToken}");

            SlackChannelProperty = new SettingProperty<string>(this, (v) => v.StartsWith("#"));
            SlackChannelProperty.Changed += (s, e) => TeamCodingPackage.Current.Logger.WriteInformation($"Changing setting {nameof(SlackChannel)}: {SlackChannel}");

            SqlServerConnectionStringProperty = new SettingProperty<string>(this);
            SqlServerConnectionStringProperty.Changed += (s, e) => TeamCodingPackage.Current.Logger.WriteInformation($"Changing setting {nameof(SqlServerConnectionString)}: {SqlServerConnectionString}");

            WinServiceIPAddressProperty = new SettingProperty<string>(this, ValidateIPString);
            WinServiceIPAddressProperty.Changed += (s, e) => TeamCodingPackage.Current.Logger.WriteInformation($"Changing setting {nameof(WinServiceIPAddress)}: {WinServiceIPAddress}");
        }
        private bool ValidateIPString(string value)
        {
            System.Net.IPAddress tmpIP;
            int tmpInt;
            var split = value.Split(':');

            return split.Length == 2 && System.Net.IPAddress.TryParse(split[0], out tmpIP) && int.TryParse(split[1], out tmpInt);
        }
    }
}