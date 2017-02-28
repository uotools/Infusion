﻿using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Threading;
using Windows.UI.Notifications;
using Infusion.Desktop.Profiles;
using Infusion.Proxy;
using Infusion.Proxy.LegacyApi;
using Infusion.Proxy.Logging;

namespace Infusion.Desktop
{
    internal class InfusionConsoleLogger : ILogger
    {
        private readonly ConsoleContent consoleContent;
        private readonly Dispatcher dispatcher;

        private readonly HashSet<string> ignoredMessages = new HashSet<string>
        {
            "Marden: Hej! Ty tam. Ano tebe myslim, Pipka. Chces lamu zadarmo? Ano?",
            "Brinley: Nice speaking to you Pipka",
            "Brinley: Well it was nice speaking to you Pipka but i must go about my business",
            "Cullin: Anna Del Tir ",
            "Keleman: Anna Del Tir "
        };

        public InfusionConsoleLogger(ConsoleContent consoleContent, Dispatcher dispatcher)
        {
            this.consoleContent = consoleContent;
            this.dispatcher = dispatcher;
        }

        public void Info(string message)
        {
            WriteLine(message, Brushes.Gray);
        }

        public void Speech(SpeechMessage message)
        {
            if (message.SpeakerId == 0 || message.SpeakerId == Injection.Me.PlayerId || message.IsName)
            {
                WriteLine(message.Text, Brushes.DarkGray);
            }
            else
            {
                WriteLine(message.Text, Brushes.White);
                ToastNotification(message.Text);
            }
        }

        public void Debug(string message)
        {
            WriteLine(message, Brushes.DimGray);
        }

        public void Critical(string message)
        {
            WriteLine(message, Brushes.Red);
            ToastAlertNotification(message);
        }

        public void Error(string message)
        {
            WriteLine(message, Brushes.DarkRed);
        }

        private void WriteLine(string message, Brush textBrush)
        {
            dispatcher.BeginInvoke((Action) (() => { consoleContent.Add($"{DateTime.Now} - {message}", textBrush); }));
        }

        private void ToastNotification(string message)
        {
            if (!ProfileRepositiory.SelectedProfile.Options.CanShowToastNotification ||
                !ProfileRepositiory.SelectedProfile.Options.ConversationToastNotificationEnabled ||
                IsIgnoredMessage(message))
            {
                return;
            }

            var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);

            var stringElements = toastXml.GetElementsByTagName("text");
            stringElements[0].AppendChild(toastXml.CreateTextNode(message));

            var audioElement = toastXml.CreateElement("audio");
            audioElement.SetAttribute("silent", "true");
            toastXml.SelectSingleNode("/toast")?.AppendChild(audioElement);

            var toast = new ToastNotification(toastXml);
            toast.Group = "Infusion";
            toast.ExpirationTime = DateTimeOffset.UtcNow.AddMinutes(10);

            var notifier = ToastNotificationManager.CreateToastNotifier("Infusion");
            notifier.Show(toast);
        }

        private void ToastAlertNotification(string message)
        {
            if (!ProfileRepositiory.SelectedProfile.Options.CanShowToastNotification || !ProfileRepositiory.SelectedProfile.Options.AlertToastNotificationEnabled ||
                IsIgnoredMessage(message))
            {
                return;
            }

            var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);

            var stringElements = toastXml.GetElementsByTagName("text");
            stringElements[0].AppendChild(toastXml.CreateTextNode("Infusion Alert"));
            stringElements[1].AppendChild(toastXml.CreateTextNode(message));

            var toast = new ToastNotification(toastXml);
            toast.Group = "Infusion";
            toast.ExpirationTime = DateTimeOffset.UtcNow.AddMinutes(10);

            var notifier = ToastNotificationManager.CreateToastNotifier("Infusion");
            notifier.Show(toast);
        }

        private bool IsIgnoredMessage(string message) => ignoredMessages.Contains(message);
    }
}