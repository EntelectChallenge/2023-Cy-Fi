# How the State Tracker works

## The Tracker class
This stores all the state changes and will serialize/output them when needed.
The TrackChange method is used to add a new state change to the change list

## StateChange class
This is the base class that is used to represent state changes. There are a bunch of different types of state changes that we need to track, and we will need to implement a new state change type for each situation.


