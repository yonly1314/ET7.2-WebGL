using System;
using System.Threading;

namespace ET
{
    public class MainThreadSynchronizationContext: Singleton<MainThreadSynchronizationContext>, ISingletonUpdate
    {
        private readonly ThreadSynchronizationContext threadSynchronizationContext = new ThreadSynchronizationContext();

        public MainThreadSynchronizationContext()
        {
#if !UNITY_WEBGL
            SynchronizationContext.SetSynchronizationContext(this.threadSynchronizationContext);
#endif
        }

        public void Update()
        {
            this.threadSynchronizationContext.Update();
        }
#if !UNITY_WEBGL
        public void Post(SendOrPostCallback callback, object state)
        {
            this.Post(() => callback(state));
        }
#endif
        public void Post(Action action)
        {
            this.threadSynchronizationContext.Post(action);
        }
    }
}