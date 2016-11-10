# Controlling Motors
There are two high level components available for controlling the digital outputs for the motors connected to the fischertechnik TXT controller
* [ControllerSequencer]()
* [MotorPositionController]()

as well as a few pre-defined values like numbered motors and inputs.

# Getting input values
There is one high level component for accessing input values present on the fischertechnik TXT controller
* [ControllerSequencer]()

as well as several options to connect inputs with movement operations.

## Using the ControllerSequencer
Each hardware controller is accessed via an individual "ControllerSequencer".

In order to establish a connection to the hardware first instantiate the "ControllerSequencer".  
The constructor accepts an ip address or a resolvable hostname.  
Disposing the instance will close the connection.

After instatiating, the sequencer provides basic high level operations like
* void StartMotor(Motor, Speed, Direction)
* void StopMotor(Motor)
* bool GetDigitalInputState(DigitalInput)
	
as well as complex operations like
* async Task\<bool> StartMotorStopWithDigitalInputAsync(Motor, Speed, Direction, DigitalInput, ExpectedInputState, Timeout)
* async Task StartMotorStopAfterTimeSpanAsync(Motor, Speed, Direction, Timespan)
* IObservable\<bool> GetDigitalInputStateChanges(DigitalInput)

Also provided is an operation to configure motors with distance counters to be [trackable]().  
Using these motors it is possible to [save and load positions]()

## Using the MotorPositionController
This component is only for motors with an associated distance counter.

It cannot be directly instatiated using the constructor.  
Instead use "ConfigureMotorPositionController" on an active "ControllerSequencer" to configure a motor to use position tracking.  
In order to configure a motor a "MotorConfiguration" must be provided.

Calling state-changing operations on a configured motor using the "ControllerSequencer" will result in an "InvalidOperationException".

After successfully configurating a motor the "MotorPositionController" will provide high level movement operations respecting the configured distance limits.

It is also possible to move to specific positions.
These positions can be [saved and loaded]().

## Saving and loading positions
Using the "ControllerSequencer" and configuring motors with distance tracking it is possible to save and load positions.  
Only the position of tracked motors will be saved (can be disabled for specific motors).  
To load and save positions call the corresponding operations on the "ControllerSequencer".  
Positions are saved to a JSON formatted file giving each position a name.
The position file is named based on the configured ApplicationName for the sequencer.
