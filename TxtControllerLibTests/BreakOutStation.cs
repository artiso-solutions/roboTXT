using System;
using System.Net;
using System.Threading.Tasks;
using log4net;
using log4net.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RoboticsTxt.Lib.Components.Sequencer;
using RoboticsTxt.Lib.Contracts;
using RoboticsTxt.Lib.Contracts.Configuration;
using TxtControllerLibTests.Properties;

namespace TxtControllerLibTests
{
    [TestClass]
    public class BreakOutStation
    {
        private ILog logger;

        public BreakOutStation()
        {
            this.logger = LogManager.GetLogger(typeof(ControllerCommunicatorTest));
        }

        [TestMethod]
        public void BreakOutBothInterfaces()
        {
            ProcessWorkpiece(true, false).Wait();
        }

        private async Task ProcessWorkpiece(bool breakoutFirstInterface, bool breakoutSecondInterface)
        {
            using (var sequencer = new ControllerSequencer(IPAddress.Parse(Settings.Default.TestDeviceIpAddress), new ControllerConfiguration(), new ApplicationConfiguration()))
            {
                await sequencer.StartMotorStopWithDigitalInputAsync(Motor.Two, Speed.Fast, Direction.Right, DigitalInput.One, true);

                if (breakoutFirstInterface)
                {
                    await BreakoutInterface(sequencer);
                }

                await sequencer.StartMotorStopWithDigitalInputAsync(Motor.Two, Speed.Fast, Direction.Right, DigitalInput.Two, true);

                if (breakoutSecondInterface)
                {
                    await BreakoutInterface(sequencer);
                }

                await sequencer.StartMotorStopAfterTimeSpanAsync(Motor.Two, Speed.Maximal, Direction.Right, TimeSpan.FromSeconds(2));
            }
        }

        private async Task BreakoutInterface(IControllerSequencer sequencer)
        {
            logger.InfoExt("Breakout interface");
            await sequencer.StartMotorStopAfterTimeSpanAsync(Motor.One, Speed.Fast, Direction.Left, TimeSpan.FromMilliseconds(900));
            await sequencer.StartMotorStopWithDigitalInputAsync(Motor.One, Speed.Fast, Direction.Right, DigitalInput.Three, true);
        }
    }
}
