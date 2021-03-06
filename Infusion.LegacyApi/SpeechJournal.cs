﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Infusion.LegacyApi
{
    public sealed class SpeechJournal : IEnumerable<JournalEntry>
    {
        private readonly Cancellation cancellation;
        private readonly Func<TimeSpan?> defaultTimeout;
        private readonly DiagnosticTrace trace;
        private readonly SpeechJournalSource source;
        private long journalEntryStartId;

        internal SpeechJournal(SpeechJournalSource source, Cancellation cancellation = null,
            Func<TimeSpan?> defaultTimeout = null, DiagnosticTrace trace = null)
        {
            this.source = source;
            this.cancellation = cancellation;
            this.defaultTimeout = defaultTimeout;
            this.trace = trace;
            journalEntryStartId = source.CurrentJournalEntryId;
            LastWaitEntryId = journalEntryStartId;
        }

        internal long LastWaitEntryId { get; private set; }

        public IEnumerator<JournalEntry> GetEnumerator()
        {
            return source.Where(line => line.Id >= journalEntryStartId).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return source.Where(line => line.Id >= journalEntryStartId).GetEnumerator();
        }

        public bool Contains(params string[] words)
        {
            return source.Where(line => line.Id >= journalEntryStartId)
                .Any(line => words.Any(w => line.Text.IndexOf(w, StringComparison.OrdinalIgnoreCase) >= 0));
        }

        public bool Contains(Player player, params string[] words) => Contains(player.PlayerId, words);
        public bool Contains(Mobile mobile, params string[] words) => Contains(mobile.Id, words);
        public bool Contains(Item item, params string[] words) => Contains(item.Id, words);

        public bool Contains(ObjectId speakerId, params string[] words)
        {
            return source.Where(line => line.Id >= journalEntryStartId)
                .Any(line =>
                    line.SpeakerId == speakerId &&
                    words.Any(w => line.Text.IndexOf(w, StringComparison.OrdinalIgnoreCase) >= 0));
        }

        public void Delete()
        {
#if DEBUG
            if (journalEntryStartId != source.CurrentJournalEntryId || LastWaitEntryId != journalEntryStartId)
                trace?.Log($"Delete (thread {System.Threading.Thread.CurrentThread.ManagedThreadId}) {journalEntryStartId} -> {source.CurrentJournalEntryId}, {LastWaitEntryId} -> {journalEntryStartId}");
#endif
            journalEntryStartId = source.CurrentJournalEntryId;
            LastWaitEntryId = journalEntryStartId;
        }

        public void WaitAny(params string[] words)
        {
            new SpeechJournalAwaiter(cancellation, source, this, defaultTimeout, trace)
                .When(words, () => { })
                .WaitAny();
        }

        public void WaitAny(TimeSpan timeout, params string[] words)
        {
            new SpeechJournalAwaiter(cancellation, source, this, defaultTimeout, trace)
                .When(words, () => { })
                .WaitAny(timeout);
        }

        public SpeechJournalAwaiter When(string awaitedWord1, Action whenAction)
        {
            return new SpeechJournalAwaiter(cancellation, source, this, defaultTimeout, trace)
                .When(awaitedWord1, whenAction);
        }

        public SpeechJournalAwaiter When(string awaitedWord1, string awaitedWord2, Action whenAction)
        {
            return new SpeechJournalAwaiter(cancellation, source, this, defaultTimeout, trace)
                .When(awaitedWord1, awaitedWord2, whenAction);
        }

        public SpeechJournalAwaiter When(string awaitedWord1, string awaitedWord2, string awaitedWord3, Action whenAction)
        {
            return new SpeechJournalAwaiter(cancellation, source, this, defaultTimeout, trace)
                .When(awaitedWord1, awaitedWord2, awaitedWord3, whenAction);
        }

        public SpeechJournalAwaiter When(string awaitedWord1, string awaitedWord2, string awaitedWord3, string awaitedWord4,
            Action whenAction)
        {
            return new SpeechJournalAwaiter(cancellation, source, this, defaultTimeout, trace)
                .When(awaitedWord1, awaitedWord2, awaitedWord3, awaitedWord4, whenAction);
        }

        public SpeechJournalAwaiter When(string awaitedWord1, string awaitedWord2, string awaitedWord3, string awaitedWord4,
            string awaitedWord5, Action whenAction)
        {
            return new SpeechJournalAwaiter(cancellation, source, this, defaultTimeout, trace)
                .When(awaitedWord1, awaitedWord2, awaitedWord3, awaitedWord4, awaitedWord5, whenAction);
        }

        public SpeechJournalAwaiter When(string[] awaitedWords, Action whenAction)
        {
            return new SpeechJournalAwaiter(cancellation, source, this, defaultTimeout, trace)
                .When(awaitedWords, whenAction);
        }

        public SpeechJournalAwaiter When(string[] awaitedWords, Action<JournalEntry> whenAction)
        {
            return new SpeechJournalAwaiter(cancellation, source, this, defaultTimeout, trace)
                .When(awaitedWords, whenAction);
        }

        public SpeechJournalAwaiter When(string awaitedWord1, Action<JournalEntry> whenAction)
        {
            return new SpeechJournalAwaiter(cancellation, source, this, defaultTimeout, trace)
                .When(awaitedWord1, whenAction);
        }

        public SpeechJournalAwaiter When(string awaitedWord1, string awaitedWord2, Action<JournalEntry> whenAction)
        {
            return new SpeechJournalAwaiter(cancellation, source, this, defaultTimeout, trace)
                .When(awaitedWord1, awaitedWord2, whenAction);
        }

        public SpeechJournalAwaiter When(string awaitedWord1, string awaitedWord2, string awaitedWord3,
            Action<JournalEntry> whenAction)
        {
            return new SpeechJournalAwaiter(cancellation, source, this, defaultTimeout, trace)
                .When(awaitedWord1, awaitedWord2, awaitedWord3, whenAction);
        }

        public SpeechJournalAwaiter When(string awaitedWord1, string awaitedWord2, string awaitedWord3, string awaitedWord4,
            Action<JournalEntry> whenAction)
        {
            return new SpeechJournalAwaiter(cancellation, source, this, defaultTimeout, trace)
                .When(awaitedWord1, awaitedWord2, awaitedWord3, awaitedWord4, whenAction);
        }

        public SpeechJournalAwaiter When(string awaitedWord1, string awaitedWord2, string awaitedWord3, string awaitedWord4,
            string awaitedWord5, Action<JournalEntry> whenAction)
        {
            return new SpeechJournalAwaiter(cancellation, source, this, defaultTimeout, trace)
                .When(awaitedWord1, awaitedWord2, awaitedWord3, awaitedWord4, awaitedWord5, whenAction);
        }

        internal void NotifyWait()
        {
            trace?.Log($"NotifyWait LastWaitEntryId {LastWaitEntryId} -> CurrentJournalEntryId {source.CurrentJournalEntryId}");
            LastWaitEntryId = source.CurrentJournalEntryId;
        }

        internal IEnumerable<JournalEntry> AfterLastAction() =>
            source.AfterLastAction().Where(line => line.Id >= LastWaitEntryId);

        public JournalEntry First(params string[] words)
        {
            return source.Where(line => line.Id >= journalEntryStartId)
                .FirstOrDefault(line => words.Any(w => line.Text.IndexOf(w, StringComparison.OrdinalIgnoreCase) >= 0));
        }

        public JournalEntry Last(params string[] words)
        {
            return source.Where(line => line.Id >= journalEntryStartId)
                .LastOrDefault(line => words.Any(w => line.Text.IndexOf(w, StringComparison.OrdinalIgnoreCase) >= 0));
        }
    }
}