namespace Cosmos.EventsProcessing.Core
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;

    public class Lockkey : IDisposable
    {

        private readonly object padlock;

        public Lockkey(object locker)
        {
            this.padlock = locker;
        }

        public void Dispose()
        {
            Monitor.Exit(this.padlock);
        }

        public static Lockkey GetLock(object lockObject, int defaultTimeoutInMilliseconds)
        {
            if (Monitor.TryEnter(lockObject, defaultTimeoutInMilliseconds))
            {
                return new Lockkey(lockObject);
            }
            else
            {
                throw new TimeoutException("Failed to acquire the lock on the object...");
            }
        }
    }
}
