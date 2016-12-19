using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using log4net.Util;
using RoboticsTxt.Lib.Contracts;
using RoboticsTxt.Lib.Contracts.Configuration;
using RoboticsTxt.Lib.Contracts.Exceptions;
using RoboticsTxt.Lib.ControllerDriver;
using RoboticsTxt.Lib.Interfaces;
using RoboticsTxt.Lib.Messages;

namespace RoboticsTxt.Lib.Components.Communicator
{
    internal class ControllerCommunicator
    {
        private readonly IPAddress ipAddress;
        private readonly ControllerConfiguration controllerConfiguration;
        private readonly ILog logger;

        private readonly CommandProcessor commandProcessor;
        private readonly ResponseProcessor responseProcessor;

        private readonly ConcurrentQueue<IControllerCommand> commandQueue;

        private Task communicationLoopTask;
        private CancellationTokenSource cancellationTokenSource;
        private ManualResetEvent waitForRunningLoop;

        public ControllerCommunicator(IPAddress ipAddress, ControllerConfiguration controllerConfiguration)
        {
            this.ipAddress = ipAddress;
            this.controllerConfiguration = controllerConfiguration;
            this.logger = LogManager.GetLogger(typeof(ControllerCommunicator));

            this.commandProcessor = new CommandProcessor();
            this.responseProcessor = new ResponseProcessor();

            this.commandQueue = new ConcurrentQueue<IControllerCommand>();

            this.UniversalInputs = Enum.GetValues(typeof(DigitalInput)).OfType<DigitalInput>().Select(d => new DigitalInputInfo(d)).ToArray();
            this.MotorDistanceInfos = Enum.GetValues(typeof(Motor)).OfType<Motor>().Select(m => new MotorDistanceInfo(m)).ToArray();
            this.CommunicationInfo = new CommunicationInfo();
        }

        public void Start()
        {
            waitForRunningLoop = new ManualResetEvent(false);
            this.cancellationTokenSource = new CancellationTokenSource();
            var token = this.cancellationTokenSource.Token;

            this.communicationLoopTask = Task.Run(() => this.CommunicationLoop(token), this.cancellationTokenSource.Token);

            waitForRunningLoop.WaitOne();
            Task.Delay(10, this.cancellationTokenSource.Token).Wait(this.cancellationTokenSource.Token);
            if (this.communicationLoopTask.IsFaulted)
            {
                this.Stop();
                throw new CommunicationFailedException("Unable to connect to the controller", this.communicationLoopTask.Exception?.InnerException);
            }
        }

        public void Stop()
        {
            if (!this.cancellationTokenSource.IsCancellationRequested)
            {
                this.cancellationTokenSource.Cancel();
                if (!this.communicationLoopTask.IsCompleted)
                {
                    this.communicationLoopTask.Wait(TimeSpan.FromSeconds(3));
                }
                this.cancellationTokenSource.Dispose();
            }
        }

        public void QueueCommand(IControllerCommand command)
        {
            this.logger.DebugExt($"Queue command {command.GetType().Name}");
            this.commandQueue.Enqueue(command);
        }

        public DigitalInputInfo[] UniversalInputs { get; }

        public MotorDistanceInfo[] MotorDistanceInfos { get; }

        public CommunicationInfo CommunicationInfo { get; }

        private async Task CommunicationLoop(CancellationToken cancellationToken)
        {
            try
            {
                var driver = new TcpControllerDriver(ipAddress);
                var currentCommandMessage = new ExchangeDataCommandMessage();

                driver.StartCommunication();
                driver.SendCommand(new StartOnlineCommandMessage());


                var configMessage = new UpdateConfigCommandMessage { ConfigId = 0 };

                configMessage.MotorModes = this.controllerConfiguration.MotorModes ??
                                           new[] { MotorMode.O1O2, MotorMode.O1O2, MotorMode.O1O2, MotorMode.O1O2 };

                configMessage.CounterModes = this.controllerConfiguration.CounterModes ??
                                             new[] { CounterMode.Normal, CounterMode.Normal, CounterMode.Normal, CounterMode.Normal };

                configMessage.InputConfigurations = this.controllerConfiguration.InputConfigurations ??
                                                    Enumerable.Repeat(
                                                        new InputConfiguration
                                                        {
                                                            InputMode = InputMode.Resistance,
                                                            IsDigital = true
                                                        }, 8).ToArray();

                driver.SendCommand(configMessage);

                var delayTimeSpan = this.controllerConfiguration.CommunicationCycleTime;

                DateTime lastCycleStartTime;
                DateTime currentCycleStartTime = DateTime.Now;
                while (!cancellationToken.IsCancellationRequested || !this.commandQueue.IsEmpty)
                {
                    CommunicationInfo.UpdateLoopReactions();
                    lastCycleStartTime = currentCycleStartTime;
                    currentCycleStartTime = DateTime.Now;
                    var cycleRunTime = currentCycleStartTime - lastCycleStartTime;
                    CommunicationInfo.UpdateCommunicationLoopCycleTime(cycleRunTime);

                    IControllerCommand command;
                    while (this.commandQueue.TryDequeue(out command))
                    {
                        try
                        {
                            this.logger.DebugExt($"Process {command.GetType().Name}");
                            this.commandProcessor.ProcessControllerCommand(this, command, currentCommandMessage);
                        }
                        catch (Exception exception)
                        {
                            this.logger.Error("Failed to execute command", exception);
                            CommunicationInfo.UpdateCommunicationLoopExceptions(exception);
                        }
                    }

                    ExchangeDataResponseMessage response;
                    try
                    {
                        response = driver.SendCommand<ExchangeDataCommandMessage, ExchangeDataResponseMessage>(currentCommandMessage);
                    }
                    catch (Exception exception)
                    {
                        logger.Error("Failed to send request to controller", exception);
                        CommunicationInfo.UpdateControllerConnectionState(false);
                        CommunicationInfo.UpdateCommunicationLoopExceptions(exception);
                        continue;
                    }

                    CommunicationInfo.UpdateControllerConnectionState(true);

                    try
                    {
                        this.responseProcessor.ProcessResponse(response, this.UniversalInputs, this.MotorDistanceInfos);
                    }
                    catch (Exception exception)
                    {
                        logger.Error("Failed to process response", exception);
                        CommunicationInfo.UpdateCommunicationLoopExceptions(exception);
                        continue;
                    }

                    await Task.Delay(delayTimeSpan);
                    waitForRunningLoop.Set();
                }

                driver.SendCommand(new StopOnlineCommandMessage());
            }
            catch (SocketException)
            {
                waitForRunningLoop.Set();
                throw;
            }
        }
    }
}
