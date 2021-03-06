﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using TeamCoding.Documents;

namespace TeamCoding.VisualStudio.Models.ChangePersisters.FileBasedPersister
{
    /// <summary>
    /// Manages receiving remote IDE model changes.
    /// Used for debugging. Reads from the current directory, using protobuf.
    /// </summary>
    public abstract class FileBasedRemoteModelPersisterBase : RemoteModelPersisterBase
    {
        public const string ModelSyncFileFormat = "OpenDocs*.bin";
        protected abstract string PersistenceFolderPath { get; }
        private FileSystemWatcher FileWatcher;
        public FileBasedRemoteModelPersisterBase()
        {
            TeamCodingPackage.Current.Settings.SharedSettings.FileBasedPersisterPathChanging += SharedSettings_FileBasedPersisterPathChanging;
            TeamCodingPackage.Current.Settings.SharedSettings.FileBasedPersisterPathChanged += Settings_FileBasedPersisterPathChanged;

            CreateNewFileWatcher();
            UpdateFileWatcherPath();
        }

        private void CreateNewFileWatcher()
        {
            FileWatcher = new FileSystemWatcher()
            {
                Filter = "*.bin",
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.DirectoryName | NotifyFilters.FileName
            };
            FileWatcher.Created += FileWatcher_Changed;
            FileWatcher.Deleted += FileWatcher_Changed;
            FileWatcher.Changed += FileWatcher_Changed;
            FileWatcher.Renamed += FileWatcher_Changed;
        }
        private void SharedSettings_FileBasedPersisterPathChanging(object sender, EventArgs e)
        {
            SyncChanges();
        }
        private void Settings_FileBasedPersisterPathChanged(object sender, EventArgs e)
        {
            UpdateFileWatcherPath();
        }

        private void UpdateFileWatcherPath()
        {
            FileWatcher.EnableRaisingEvents = false;
            if (PersistenceFolderPath != null && Directory.Exists(PersistenceFolderPath))
            {
                FileWatcher.Path = PersistenceFolderPath;
                FileWatcher.EnableRaisingEvents = true;
                // Sync any changes since there could already be files in this new directory waiting
                SyncChanges();
            }
        }

        private readonly object FileWatcherChangedLock = new object();
        private void FileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            lock (FileWatcherChangedLock)
            {
                FileWatcher.EnableRaisingEvents = false;
                if (e.ChangeType != WatcherChangeTypes.Deleted)
                {
                    while (!IsFileReady(e.FullPath)) { System.Threading.Thread.Sleep(100); }
                }

                FileWatcher.EnableRaisingEvents = true;
            }
            SyncChanges();
        }
        private void SyncChanges()
        {
            ClearRemoteModels();
            if (PersistenceFolderPath == null || !Directory.Exists(PersistenceFolderPath))
            {
                OnRemoteModelReceived(null);
            }
            else
            {
                foreach (var modelSyncFile in Directory.GetFiles(PersistenceFolderPath, ModelSyncFileFormat))
                {
                    int waitCount = 100;
                    while (waitCount-- != 0 && !IsFileReady(modelSyncFile)) { System.Threading.Thread.Sleep(100); }
                    if (waitCount == 0)
                    {
                        TeamCodingPackage.Current.Logger.WriteError($"Could not get access to file {modelSyncFile}. Skipped.");
                        continue;
                    }
                    // If any file hasn't been modified in the last minute an a half, delete it (tidy up files left from crashes etc.)
                    if ((DateTime.UtcNow - File.GetLastWriteTimeUtc(modelSyncFile)).TotalSeconds > 90)
                    {
                        File.Delete(modelSyncFile);
                        continue;
                    }
                    using (var f = File.OpenRead(modelSyncFile))
                    {
                        OnRemoteModelReceived(ProtoBuf.Serializer.Deserialize<RemoteIDEModel>(f));
                    }
                }
            }
        }
        public override void Dispose()
        {
            base.Dispose();
            if (FileWatcher != null)
            {
                FileWatcher.EnableRaisingEvents = false;
                FileWatcher.Created -= FileWatcher_Changed;
                FileWatcher.Deleted -= FileWatcher_Changed;
                FileWatcher.Changed -= FileWatcher_Changed;
                FileWatcher.Renamed -= FileWatcher_Changed;

                lock (FileWatcherChangedLock)
                {
                    FileWatcher.Dispose();
                }
            }
        }
        private bool IsFileReady(string sFilename)
        {
            // If the file can be opened for exclusive access it means that the file
            // is no longer locked by another process.
            try
            {
                using (FileStream inputStream = File.Open(sFilename, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    return inputStream.Length > 0;

                }
            }
            catch (IOException)
            {
                return false;
            }
        }
    }
}
