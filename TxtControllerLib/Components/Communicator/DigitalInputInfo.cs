using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using log4net;
using RoboticsTxt.Lib.Contracts;

namespace RoboticsTxt.Lib.Components.Communicator
{
    internal class DigitalInputInfo
    {
        private readonly Subject<bool> stateChangesSubject;

        private readonly ILog logger;

        public DigitalInputInfo(DigitalInput digitalInput)
        {
            DigitalInput = digitalInput;
            this.stateChangesSubject = new Subject<bool>();

            this.logger = LogManager.GetLogger(typeof(DigitalInputInfo));
        }

        public DigitalInput DigitalInput { get; }

        public bool CurrentState { get; private set; }

        public IObservable<bool> StateChanges => this.stateChangesSubject.AsObservable();

        public void SetNewState(bool newState)
        {
            if (this.CurrentState == newState)
                return;

            this.CurrentState = newState;
            this.logger.Debug($"I{this.DigitalInput}: state changed to {newState}");
            Task.Run(() => this.stateChangesSubject.OnNext(newState));
        }
    }
}