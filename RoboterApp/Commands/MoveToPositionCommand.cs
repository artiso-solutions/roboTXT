﻿using System;
using System.Windows.Input;
using RoboticsTxt.Lib.Components.Sequencer;

namespace RoboterApp.Commands
{
    public class MoveToPositionCommand : ICommand
    {
        private readonly ControllerSequencer controllerSequencer;
        private readonly MainWindowViewModel mainWindowViewModel;

        public MoveToPositionCommand(ControllerSequencer controllerSequencer, MainWindowViewModel mainWindowViewModel)
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
            this.controllerSequencer.MoveToPositionAsync(positionName);
        }

        public event EventHandler CanExecuteChanged;
    }
}
