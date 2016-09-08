﻿using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Timers;
using TeamCoding.Events;
using TeamCoding.Extensions;

namespace TeamCoding.VisualStudio
{
    [Export(typeof(IWpfTextViewConnectionListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal sealed class TeamCodingTextViewConnectionListener : IWpfTextViewConnectionListener
    {
        private readonly ITextDocumentFactoryService TextDocFactory;
        private readonly IClassifierAggregatorService TextClassifierService;
        private readonly DelayedEvent<CaretPositionChangedEventArgs> DelayedCaretPositionChanged = new DelayedEvent<CaretPositionChangedEventArgs>(500);

        [ImportingConstructor]
        public TeamCodingTextViewConnectionListener(ITextDocumentFactoryService textDocumentFactoryService, IClassifierAggregatorService textClassifierService)
        {
            TextDocFactory = textDocumentFactoryService;
            TextClassifierService = textClassifierService;
            TextDocFactory.TextDocumentDisposed += TextDocFactory_TextDocumentDisposed;
            DelayedCaretPositionChanged.Event += DelayedCaretPositionChanged_Event;
        }
        private async void DelayedCaretPositionChanged_Event(object sender, CaretPositionChangedEventArgs e)
        {
            await TeamCodingPackage.Current.LocalIdeModel.OnCaretPositionChanged(e);
        }
        public void SubjectBuffersConnected(IWpfTextView textView, ConnectionReason reason, Collection<ITextBuffer> subjectBuffers)
        {
            if (reason == ConnectionReason.TextViewLifetime)
            { // TextView opened
                if (TeamCodingPackage.Current.SourceControlRepo.GetRepoDocInfo(textView.GetTextDocumentFilePath()) == null) return;

                var test = AppDomain.CurrentDomain.GetAssemblies().OrderBy(a => a.FullName).ToArray();

                TeamCodingPackage.Current.LocalIdeModel.OnOpenedTextView(textView).Wait();
                textView.TextBuffer.Changed += TextBuffer_Changed;
                ITextDocument textDoc;

                TextDocFactory.TryGetTextDocument(textView.TextBuffer, out textDoc);
                if (textDoc != null)
                {
                    textDoc.FileActionOccurred += TextDoc_FileActionOccurred;
                    textView.Caret.PositionChanged += DelayedCaretPositionChanged.EventHandler;
                }
            }
        }
        private void TextDocFactory_TextDocumentDisposed(object sender, TextDocumentEventArgs e)
        {
            TeamCodingPackage.Current.LocalIdeModel.OnTextDocumentDisposed(e.TextDocument, e);
        }
        private void TextDoc_FileActionOccurred(object sender, TextDocumentFileActionEventArgs e)
        {
            if (e.FileActionType == FileActionTypes.ContentSavedToDisk || e.FileActionType == FileActionTypes.DocumentRenamed)
            {
                TeamCodingPackage.Current.LocalIdeModel.OnTextDocumentSaved(sender as ITextDocument, e);

                // If the file was the config file then try and load the settings
                if (e.FilePath.EndsWith(Options.Settings.TeamCodingConfigFileName))
                {
                    TeamCodingPackage.Current.Settings.LoadFromJsonFile();
                }
            }
        }
        private void TextBuffer_Changed(object sender, TextContentChangedEventArgs e)
        {
            TeamCodingPackage.Current.LocalIdeModel.OnTextBufferChanged(sender as ITextBuffer, e);
        }
        public void SubjectBuffersDisconnected(IWpfTextView textView, ConnectionReason reason, Collection<ITextBuffer> subjectBuffers)
        {
            if (reason == ConnectionReason.TextViewLifetime)
            { // TextView closed
                textView.TextBuffer.Changed -= TextBuffer_Changed;
                ITextDocument textDoc;

                TextDocFactory.TryGetTextDocument(textView.TextBuffer, out textDoc);
                if (textDoc != null)
                {
                    textDoc.FileActionOccurred -= TextDoc_FileActionOccurred;
                    textView.Caret.PositionChanged -= DelayedCaretPositionChanged.EventHandler;
                }
            }
        }
    }
}
