# NPC System Fixes for Tea Shop Game

This document outlines the implemented fixes for the NPC interaction system in the tea shop game.

## Fixes Implemented

### 1. Player Interaction After Placing Game on Table
- Modified `TableController.cs` to reset player state after placing a game on a table
- Added a `SetPickedStatus` method to `Player.cs` to properly control the player's pickup state
- This ensures the player can pick up new items immediately after placing a game

### 2. Cup Placement on Tables
- Enhanced `PlaceCupOnTable` method in `NPC.cs` to better position cups on tables
- Added a reference to the current cup to track it after placing
- Reset player state properly after placing a cup to ensure continued interaction

### 3. Dirty Cup System
- Modified `NPC.DrinkBeverage` method to mark cups as dirty when NPCs finish drinking
- Changed the cup layer back to "Interactable" after NPCs finish, making them pickable again
- Enhanced `DirtyStatus.cs` to visually indicate when cups are dirty
- Added material change functionality to show dirty status on cups
- Added a dirty overlay visual option to the Tea_Cup class

### 4. Player Pickup of Dirty Cups
- Updated `Player.PickAndPut` method to handle dirty cups
- Added notification when player picks up dirty cups
- Ensure cups can be cleaned at the sink using the existing cleaning system

## How the System Works

1. **Player places drink for NPC**
   - Cup is positioned on the table
   - NPC drinks after a delay

2. **NPC finishes drink**
   - Cup is marked as dirty
   - Cup becomes interactable again
   - Visual indication shows the cup is dirty

3. **Player picks up dirty cup**
   - Message indicates the cup needs cleaning
   - Player can take cup to sink
   - Using the Water tag will clean the cup

## Testing the System

To test the implemented functionality:
1. Place a game on a table - you should be able to pick up other items immediately
2. Serve tea to NPCs - cups should stay properly positioned
3. Wait for NPCs to finish drinking - cups should become dirty
4. Pick up dirty cups - you should see a message about cleaning them
5. Take cups to sink - they should become clean again 