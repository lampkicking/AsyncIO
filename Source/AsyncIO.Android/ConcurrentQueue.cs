using System.Collections.Generic;

namespace AsyncIO.Android
{
    internal class ConcurrentQueue<T>
    {
        private readonly Queue<T> _mQueue;

        public ConcurrentQueue()
        {
            _mQueue = new Queue<T>();
        }

        internal void Enqueue(T state)
        {
            lock (_mQueue)
            {
                _mQueue.Enqueue(state);
            }
        }

        internal bool TryDequeue(out T state)
        {
            lock (_mQueue)
            {
                if (_mQueue.Count > 0)
                {
                    state = _mQueue.Dequeue();
                    return true;    
                }
                state = default (T);
                return false;
            }
        }
    }
}
