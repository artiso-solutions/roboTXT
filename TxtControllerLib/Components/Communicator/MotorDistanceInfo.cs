using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using log4net;
using RoboticsTxt.Lib.Contracts;

namespace RoboticsTxt.Lib.Components.Communicator
{
    internal class MotorDistanceInfo
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(MotorDistanceInfo));
        private readonly Subject<short> commandIdChangesSubject;
        private readonly Subject<int> distanceDifferencesSubject;

        private short currentCommandId;
        private short currentDistanceValue;

        public MotorDistanceInfo(Motor motor)
        {
            this.Motor = motor;
            this.IsTracking = true;

            distanceDifferencesSubject = new Subject<int>();
            commandIdChangesSubject = new Subject<short>();
        }

        public Motor Motor { get; }

        public bool IsTracking { get; set; }
        
        public IObservable<int> DistanceDifferences => distanceDifferencesSubject.AsObservable();

        public IObservable<short> CommandIdChanges => commandIdChangesSubject.AsObservable();

        public void SetCurrentDistanceValue(short distanceValue)
        {
            if (!this.IsTracking)
            {
                this.currentDistanceValue = distanceValue;
                return;
            }

            if ((distanceValue < currentDistanceValue))
            {
                currentDistanceValue = 0;
                logger.Debug($"Motor {Motor}: reset currentDistance");
            }

            if (currentDistanceValue == distanceValue)
            {
                return;
            }

            var difference = distanceValue - currentDistanceValue;

            logger.Debug($"Motor {Motor}: d={distanceValue:000} | diff={difference:000}");

            currentDistanceValue = distanceValue;
            Task.Run(() => this.distanceDifferencesSubject.OnNext(difference));
        }

        public void SetCurrentCommandId(short commandId)
        {
            if (currentCommandId == commandId)
                return;

            currentCommandId = commandId;

            Task.Run(() => commandIdChangesSubject.OnNext(commandId));

            this.logger.Debug($"Motor {Motor}: commandId changed to {commandId}");
        }
    }
}