﻿using ProtoBuf;
using System;
using System.Collections.Generic;
using TeamCoding.Documents;
using TeamCoding.IdentityManagement;

namespace TeamCoding.VisualStudio.Models
{
    /// <summary>
    /// Represents an IDE being used remotely
    /// </summary>
    [ProtoContract]
    public class RemoteIDEModel
    {
        [ProtoMember(1)]
        public readonly string Id;
        [ProtoMember(2)]
        public readonly UserIdentity IDEUserIdentity;
        [ProtoIgnore]
        private List<DocumentRepoMetaData> _OpenFiles;
        [ProtoMember(3)]
        public List<DocumentRepoMetaData> OpenFiles
        {
            get { return _OpenFiles ?? (_OpenFiles = new List<DocumentRepoMetaData>()); }
            private set { _OpenFiles = value; }
        }

        public RemoteIDEModel() { } // For protobuf
        public RemoteIDEModel(LocalIDEModel localModel)
        {
            Id = LocalIDEModel.Id;
            IDEUserIdentity = TeamCodingPackage.Current.IdentityProvider.GetIdentity();
            OpenFiles = new List<DocumentRepoMetaData>(localModel.OpenDocs());
        }
    }
}
