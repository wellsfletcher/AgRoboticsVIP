# AgRoboticsVIP

## Installation

The `Cylinders.csv` and `SunLocations.csv` need to be moved to the Application Support folder as of now for the tree to be imported properly. These can be found in the `Assets/Data` folder. When you run the project, you will receive an error message. View the error message to see where to put these files.

## Development Process

The `Assets/Scenes/LocomotionVR.unity` scene is the primary scene that gets run and built on the Oculus. However, the `Assets/Scenes/Main.unity` scene can be run locally on your computer and is great for developing non VR related features. Duplicate the `Assets/Scenes/Main.unity` scene and make changes to your copy in order to avoid merge conflicts.

## Controls

`Left Mouse Button` -> Remove branch

`Right Mouse Button` -> Rotate camera

`WASD` -> Move camera

`QE` -> Increase/decrease altitude

`ZX` -> Step through sun positions

`NM` -> Step through branch orders

`U` -> Undo branch removals

`H` -> Hide raycasts

`L` -> Toggle leaves

`R` -> Restart light metric

`1` -> Show sphere metric

`2` -> Hide sphere metric

`0` -> Export tree to clipboard
