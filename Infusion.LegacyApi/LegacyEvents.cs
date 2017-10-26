﻿using System;
using Infusion.LegacyApi.Events;

namespace Infusion.LegacyApi
{
    public class LegacyEvents
    {
        private readonly ItemsObservers itemsObserver;
        private readonly QuestArrowObserver questArrowObserver;
        private readonly SpeechRequestObserver speechRequestObserver;
        private readonly SoundObserver soundObserver;

        internal LegacyEvents(ItemsObservers itemsObserver, JournalSource journalSource, SoundObserver soundObserver,
            QuestArrowObserver questArrowObserver, SpeechRequestObserver speechRequestObserver)
        {
            this.soundObserver = soundObserver;
            this.questArrowObserver = questArrowObserver;
            this.speechRequestObserver = speechRequestObserver;
            this.itemsObserver = itemsObserver;
            journalSource.NewMessageReceived += JournalSourceOnNewMessageReceived;
        }

        private void JournalSourceOnNewMessageReceived(object sender, JournalEntry journalEntry)
        {
            SpeechReceived.RaiseScriptEvent(this, journalEntry);
        }

        public event EventHandler<JournalEntry> SpeechReceived;
        public event EventHandler<SpeechRequestedEvent> SpeechRequested
        {
            add => speechRequestObserver.SpeechRequested += value;
            remove => speechRequestObserver.SpeechRequested -= value;
        }

        public event EventHandler<CommandRequestedEvent> CommandRequested
        {
            add => speechRequestObserver.CommandRequested += value;
            remove => speechRequestObserver.CommandRequested -= value;
        }

        public event EventHandler<QuestArrowEvent> QuestArrowChanged
        {
            add => questArrowObserver.QuestArrowChanged += value;
            remove => questArrowObserver.QuestArrowChanged -= value;
        }

        public event EventHandler<CurrentHealthUpdatedEvent> HealthUpdated
        {
            add => itemsObserver.CurrentHealthUpdated += value;
            remove => itemsObserver.CurrentHealthUpdated -= value;
        }

        public event EventHandler<ItemEnteredViewEvent> ItemEnteredView
        {
            add => itemsObserver.ItemEnteredView += value;
            remove => itemsObserver.ItemEnteredView -= value;
        }

        public event EventHandler<Mobile> MobileEnteredView
        {
            add => itemsObserver.MobileEnteredView += value;
            remove => itemsObserver.MobileEnteredView -= value;
        }

        public event EventHandler<Mobile> MobileLeftView
        {
            add => itemsObserver.MobileLeftView += value;
            remove => itemsObserver.MobileLeftView -= value;
        }

        public event EventHandler<Mobile> MobileDeleted
        {
            add => itemsObserver.MobileDeleted += value;
            remove => itemsObserver.MobileDeleted -= value;
        }

        public event EventHandler<ItemUseRequestedEvent> ItemUseRequested
        {
            add => itemsObserver.DoubleClickRequested += value;
            remove => itemsObserver.DoubleClickRequested -= value;
        }

        public event EventHandler<SoundEffectPlayedEvent> SoundEffectPlayed
        {
            add => soundObserver.SoundEffectPlayed += value;
            remove => soundObserver.SoundEffectPlayed -= value;
        }

        public event EventHandler<Skill> SkillRequested;

        internal void OnSkillRequested(Skill skill)
        {
            SkillRequested?.Invoke(this, skill);
        }

        internal void ResetEvents()
        {
            itemsObserver.ResetEvents();
            soundObserver.ResetEvents();
            questArrowObserver.ResetEvents();
            speechRequestObserver.ResetEvents();
        }
    }
}