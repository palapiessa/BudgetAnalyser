﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine
{
    /// <summary>
    /// A logging event subscriber that will log the occurance of all published application-wide events to the give <see cref="ILogger"/>.
    /// </summary>
    [AutoRegisterWithIoC]
    [UsedImplicitly]
    public class LoggingApplicationHookSubscriber : IApplicationHookSubscriber, IDisposable
    {
        private readonly ILogger logger;
        private bool isDisposed;
        private IEnumerable<IApplicationHookEventPublisher> myPublishers;

        public LoggingApplicationHookSubscriber([NotNull] ILogger logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            this.logger = logger;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.isDisposed)
            {
                if (disposing)
                {
                    this.isDisposed = true;
                    this.logger.LogInfo(l => "LoggingApplicationHookSubscriber is being disposed.");
                    if (this.myPublishers != null)
                    {
                        foreach (IApplicationHookEventPublisher publisher in this.myPublishers)
                        {
                            publisher.ApplicationEvent -= OnEventOccurred;
                        }
                    }
                }
            }
        }

        public void Subscribe([NotNull] IEnumerable<IApplicationHookEventPublisher> publishers)
        {
            if (this.isDisposed) throw new ObjectDisposedException(GetType().Name);

            if (publishers == null)
            {
                throw new ArgumentNullException("publishers");
            }

            this.myPublishers = publishers.ToList();
            foreach (IApplicationHookEventPublisher publisher in this.myPublishers)
            {
                publisher.ApplicationEvent += OnEventOccurred;
            }
        }

        protected virtual void PerformAction(object sender, ApplicationHookEventArgs args)
        {
            this.logger.LogInfo(l => l.Format("Application Hook Event Occurred. Sender: [{0}], Type: {1}, Origin: {2}", sender, args.Category, args.Origin));
        }

        private void OnEventOccurred(object sender, ApplicationHookEventArgs args)
        {
            if (this.isDisposed)
            {
                return;
            }

            PerformAction(sender, args);
        }
    }
}