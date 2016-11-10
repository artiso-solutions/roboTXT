using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using RoboticsTxt.Lib.Components.Sequencer;

namespace RoboterApp.Commands
{
    internal class SavePositionCommand : ICommand
    {
        private readonly IControllerSequencer sequencer;
        private readonly ObservableCollection<string> positionNames;

        public SavePositionCommand(IControllerSequencer sequencer, ObservableCollection<string> positionNames)
        {
            this.sequencer = sequencer;
            this.positionNames = positionNames;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter == null) throw new ArgumentNullException(nameof(parameter));

            this.sequencer.SaveCurrentPosition(parameter.ToString());

            this.positionNames.Clear();
            foreach (var positionName in this.sequencer.GetPositionNames())
            {
                this.positionNames.Add(positionName);
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}
