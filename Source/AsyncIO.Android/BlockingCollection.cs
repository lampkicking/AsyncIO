using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace AsyncIO.Android
{
    internal class BlockingCollection<T>
    {
        private readonly Queue<T> _mQueue;

        public BlockingCollection()
        {
            _mQueue = new Queue<T>();
        }

        public void Add(T item)
        {
            lock (_mQueue)
            {
                _mQueue.Enqueue(item);
                if (_mQueue.Count == 1)
                {
                    Monitor.PulseAll(_mQueue);
                }
            }
        }

        public bool TryTake(out T item, int timeout)
        {
            var stopwatch = Stopwatch.StartNew();            

            lock (_mQueue)
            {
                while (_mQueue.Count == 0)
                {
                    var elapsed = stopwatch.ElapsedMilliseconds;
                    var timeoutLeft = timeout == -1 ? -1 :
                     (elapsed > timeout ? 0 : timeout - (int)elapsed);

                    if (timeoutLeft == 0)
                    {
                        item = default(T);                        
                        return false;
                    }

                    Monitor.Wait(_mQueue, timeoutLeft);
                }

                item = _mQueue.Dequeue();                
                return true;
            }         
        }
    }
}
