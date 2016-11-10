using System.Threading.Tasks;
using log4net;
using RoboticsTxt.Lib.Components.Sequencer;
using RoboticsTxt.Lib.Contracts;

namespace RoboterApp.Components
{
    public class SequenceCommandLogic
    {
        private readonly IControllerSequencer controllerSequencer;
        private readonly MotorPositionController backwardForwardPositionController;
        private readonly MotorPositionController upDownPositionController;
        private readonly MotorPositionController turnLeftRightPositionController;
        private readonly MotorPositionController openCloseClampPositionController;
        private readonly ILog logger;

        public SequenceCommandLogic(IControllerSequencer controllerSequencer,
                                    MotorPositionController backwardForwardPositionController,
                                    MotorPositionController upDownPositionController,
                                    MotorPositionController turnLeftRightPositionController,
                                    MotorPositionController openCloseClampPositionController)
        {
            this.controllerSequencer = controllerSequencer;
            this.backwardForwardPositionController = backwardForwardPositionController;
            this.upDownPositionController = upDownPositionController;
            this.turnLeftRightPositionController = turnLeftRightPositionController;
            this.openCloseClampPositionController = openCloseClampPositionController;

            this.logger = LogManager.GetLogger(typeof(SequenceCommandLogic));
        }

        public async Task RunPickUpBatchSequenceAsync(int batchIndex)
        {
            var waitingPositionName = $"Warteposition {batchIndex}";
            var pickUpPositionName = $"Abholposition {batchIndex}";
            var removalPositionName = $"Entnahmeposition {batchIndex}";

            var clampController = new ClampController(this.openCloseClampPositionController);

            this.logger.Info("Starting sequence...");

            await this.controllerSequencer.MoveToPositionAsync(waitingPositionName);
            this.logger.Info("Waiting position reached.");

            await this.openCloseClampPositionController.StartMotorAndMoveDistanceAsync(Speed.Fast, Direction.Right, 10, true);
            this.logger.Info("Clamp pre-closed.");

            await this.controllerSequencer.MoveToPositionAsync(pickUpPositionName);
            this.logger.Info("Pickup position reached.");

            await clampController.CloseClamp();
            this.logger.Info("Clamp closed.");

            await this.controllerSequencer.MoveToPositionAsync(removalPositionName);
            this.logger.Info("Removal position reached.");

            await this.controllerSequencer.MoveToPositionAsync("Übergabeposition");
            this.logger.Info("Delivery position reached.");

            await clampController.WaitForContainerRemoval();
            this.logger.Info("Container removed.");

            await this.openCloseClampPositionController.MoveMotorToReferenceAsync();
        }
    }
}
