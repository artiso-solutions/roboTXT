using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace RoboticsTxt.Lib.Components.Communicator
{
    internal class CommunicationInfo
    {
        private readonly Subject<TimeSpan> communicationLoopTimeSubject;

        private readonly Subject<Exception> communicationLoopExceptionSubject;

        public CommunicationInfo()
        {
            communicationLoopTimeSubject = new Subject<TimeSpan>();
            communicationLoopExceptionSubject = new Subject<Exception>();

            LastCycleRunTime = TimeSpan.Zero;
        }

        public TimeSpan LastCycleRunTime { get; private set; }

        public IObservable<TimeSpan> CommunicationLoopCycleTimeChanges => communicationLoopTimeSubject.AsObservable();

        public IObservable<Exception> CommunicationLoopExceptions => communicationLoopExceptionSubject.AsObservable();

        public void UpdateCommunicationLoopCycleTime(TimeSpan cycleRunTime)
        {
            if ((cycleRunTime - LastCycleRunTime).Duration() < TimeSpan.FromMilliseconds(5))
            {
                return;
            }

            LastCycleRunTime = cycleRunTime;
            Task.Run(() => communicationLoopTimeSubject.OnNext(cycleRunTime));
        }

        public void UpdateCommunicationLoopExceptions(Exception loopException)
        {
            Task.Run(() => communicationLoopExceptionSubject.OnNext(loopException));
        }
    }
}