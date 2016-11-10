using System;
using System.Threading.Tasks;
using System.Windows.Input;
using RoboticsTxt.Lib.Components.Sequencer;

namespace RoboterApp.Commands
{
    public class MoveToPositionCommand : ICommand
    {
        private readonly IControllerSequencer controllerSequencer;
        private readonly MainWindowViewModel mainWindowViewModel;

        public MoveToPositionCommand(IControllerSequencer controllerSequencer, MainWindowViewModel mainWindowViewModel)
        {
            this.controllerSequencer = controllerSequencer;
            this.mainWindowViewModel = mainWindowViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            var positionName = parameter.ToString();

            this.mainWindowViewModel.PositionName = positionName;
            Task.Run(() => this.controllerSequencer.MoveToPositionAsync(positionName));
        }

        public event EventHandler CanExecuteChanged;
    }
}
