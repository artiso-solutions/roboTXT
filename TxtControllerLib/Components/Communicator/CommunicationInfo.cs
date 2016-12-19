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

        private readonly Subject<bool> controllerConnectionSubject;

        private readonly Subject<object> loopReactionSubject;

        public CommunicationInfo()
        {
            communicationLoopTimeSubject = new Subject<TimeSpan>();
            communicationLoopExceptionSubject = new Subject<Exception>();
            controllerConnectionSubject = new Subject<bool>();
            loopReactionSubject = new Subject<object>();

            LastCycleRunTime = TimeSpan.Zero;
            ConnectedToController = false;
        }

        public TimeSpan LastCycleRunTime { get; private set; }

        public bool ConnectedToController { get; private set; }

        public IObservable<TimeSpan> CommunicationLoopCycleTimeChanges => communicationLoopTimeSubject.AsObservable();

        public IObservable<Exception> CommunicationLoopExceptions => communicationLoopExceptionSubject.AsObservable();

        public IObservable<bool> ControllerConnectionStateChanges => controllerConnectionSubject.AsObservable();

        public IObservable<object> LoopReactionEvents => loopReactionSubject.AsObservable();

        public void UpdateCommunicationLoopCycleTime(TimeSpan cycleRunTime)
        {
            if ((cycleRunTime - LastCycleRunTime).Duration() < TimeSpan.FromMilliseconds(20))
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

        public void UpdateControllerConnectionState(bool newConnectionState)
        {
            if (ConnectedToController == newConnectionState)
            {
                return;
            }

            ConnectedToController = newConnectionState;
            Task.Run(() => controllerConnectionSubject.OnNext(newConnectionState));
        }

        public void UpdateLoopReactions()
        {
            Task.Run(() => loopReactionSubject.OnNext(null));
        }
    }
}