![Build status Develop](https://artiso.visualstudio.com/_apis/public/build/definitions/4fbc1b09-9da4-496f-9c69-3df7222e1274/49/badge)

[![NuGet version](https://badge.fury.io/nu/Robotics.TXT.net.svg)](https://badge.fury.io/nu/Robotics.TXT.net)

# Robotics TXT .Net Library
Simple communication library for controlling the Fischertechnik ROBOTICS TXT controller from .Net.
The library is work in progress and does not support all functions of the ROBOTICS TXT controller yet.

## Install Robotics TXT .Net Library via NuGet

If you want to include Robotics TXT .Net Library in your project, you can install it directly via NuGet

To install Robotics TXT .Net Library, run the following command in the Package Manager Console

```
PM> Install-Package Robotics.TXT.net
```

## Core Components of Robotics TXT .Net Library
The main component to use is the `ControllerSequencer` which provides some high level functions to 
operate motors and work with inputs and outputs. Further information can be found in the documentation of this class.

## Sample Application Manual Robot Controller
As an example how the Robotics TXT .Net Library can be used there is a sample application included.
This sample app is a manual robot controller for the four-axis-roboter included in the [Fischertechnik bundle which can be found here](http://www.fischertechnik.de/desktopdefault.aspx/tabid-21/39_read-138/usetemplate-2_column_pano/).

To connect to your robot you have to edit the app.config of the sample app and change the `RoboAddress` to the hostname or ip address of your robot controller.
