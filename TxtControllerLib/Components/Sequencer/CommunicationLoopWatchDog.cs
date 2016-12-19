using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace RoboticsTxt.Lib.Components.Sequencer
{
    internal class CommunicationLoopWatchDog
    {
        private readonly Subject<object> communicationLoopBlockSubject;

        private readonly IObservable<object> communicationLoopReactionEvents;

        private Task observerTask;
        private bool loopBlocking;
        private readonly object loopBlockingLock;

        private readonly CancellationTokenSource cancellationTokenSource;

        private IDisposable cycleTimeChangesSubscription;

        public CommunicationLoopWatchDog(IObservable<object> communicationLoopReactionEvents)
        {
            this.communicationLoopReactionEvents = communicationLoopReactionEvents;
            communicationLoopBlockSubject = new Subject<object>();
            loopBlockingLock = new object();
            cancellationTokenSource = new CancellationTokenSource();
        }

        public IObservable<object> CommunicationLoopBlockEvents => communicationLoopBlockSubject.AsObservable();

        public void StartWatching()
        {
            cycleTimeChangesSubscription = communicationLoopReactionEvents.Subscribe(OnCommunicationLoopCycleTimeChange);
            observerTask = Task.Run(() => WatchDogLoop(cancellationTokenSource.Token));
        }

        public void StopWatching()
        {
            cancellationTokenSource.Cancel();
            while (!observerTask.IsCompleted)
            {
                Task.Delay(TimeSpan.FromMilliseconds(100)).Wait();
            }

            cycleTimeChangesSubscription.Dispose();
        }

        private void OnCommunicationLoopCycleTimeChange(object parameter)
        {
            lock (loopBlockingLock)
            {
                loopBlocking = false;
            }
        }

        private void WatchDogLoop(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    Task.Delay(TimeSpan.FromSeconds(5), cancellationToken).Wait(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    continue;
                }
                
                lock (loopBlockingLock)
                {
                    if (loopBlocking)
                    {
                        Task.Run(() => communicationLoopBlockSubject.OnNext(null));
                    }
                    else
                    {
                        loopBlocking = true;
                    }
                }
            }
        }
    }
}