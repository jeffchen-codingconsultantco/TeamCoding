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
    [TextViewRole(PredefinedTextViewRoles.Document)]
    [ContentType("any")]
    internal sealed class TeamCodingTextViewConnectionListener : IWpfTextViewConnectionListener
    {
        private readonly ITextDocumentFactoryService TextDocFactory;
        private readonly IClassifierAggregatorService TextClassifierService;
        [ImportingConstructor]
        public TeamCodingTextViewConnectionListener(ITextDocumentFactoryService textDocumentFactoryService, IClassifierAggregatorService textClassifierService)
        {
            TextDocFactory = textDocumentFactoryService;
            TextClassifierService = textClassifierService;
            TextDocFactory.TextDocumentDisposed += TextDocFactory_TextDocumentDisposed;
        }
#pragma warning disable IDE1006 // Naming Styles
        public async void SubjectBuffersConnected(IWpfTextView textView, ConnectionReason reason, Collection<ITextBuffer> subjectBuffers)
#pragma warning restore IDE1006 // Naming Styles
        {
            if (reason == ConnectionReason.TextViewLifetime)
            { // TextView opened
                if (TeamCodingPackage.Current.SourceControlRepo.GetRepoDocInfo(textView.GetTextDocumentFilePath()) == null) return;

                await TeamCodingPackage.Current.LocalIdeModel.OnOpenedTextViewAsync(textView);
                textView.TextBuffer.Changed += TextBuffer_Changed;
                TextDocFactory.TryGetTextDocument(textView.TextBuffer, out var textDoc);
                if (textDoc != null)
                {
                    textDoc.FileActionOccurred += TextDoc_FileActionOccurred;
                    textView.Caret.PositionChanged += Caret_PositionChangedAsync;
                    textView.LayoutChanged += TextView_LayoutChangedAsync;
                }
            }
        }

        private async void TextView_LayoutChangedAsync(object sender, TextViewLayoutChangedEventArgs e)
        {
            var textView = sender as IWpfTextView;
            if (e.NewOrReformattedLines.Contains(textView.Caret.ContainingTextViewLine))
            {
                await TeamCodingPackage.Current.LocalIdeModel.OnCaretPositionChangedAsync(new CaretPositionChangedEventArgs(textView, textView.Caret.Position, textView.Caret.Position));
            }
        }

        private async void Caret_PositionChangedAsync(object sender, CaretPositionChangedEventArgs e)
        {
            await TeamCodingPackage.Current.LocalIdeModel.OnCaretPositionChangedAsync(e);
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
                TextDocFactory.TryGetTextDocument(textView.TextBuffer, out var textDoc);
                if (textDoc != null)
                {
                    textDoc.FileActionOccurred -= TextDoc_FileActionOccurred;
                    textView.Caret.PositionChanged -= Caret_PositionChangedAsync;
                    textView.LayoutChanged -= TextView_LayoutChangedAsync;
                }
            }
        }
    }
}
