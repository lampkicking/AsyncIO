﻿using System;

namespace AsyncIO
{
    public abstract class CompletionPort : IDisposable
    {
        public static CompletionPort Create()
        {
            return new AsyncIO.DotNet.CompletionPort();
        }

        public abstract void Dispose();

        public abstract bool GetQueuedCompletionStatus(int timeout, out CompletionStatus completionStatus);

        public abstract bool GetMultipleQueuedCompletionStatus(int timeout, CompletionStatus[] completionStatuses,
            out int removed);

        public abstract void AssociateSocket(AsyncSocket socket, object state= null);

        public abstract void Signal(object state);
    }
}
