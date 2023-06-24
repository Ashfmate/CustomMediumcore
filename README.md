# CustomMediumcore
This plugin for a custom medium core is an improvement to the one by Miffyli where it is possible to select what is dropped from the player as if he is in mediumcore.
This uses a JSON file that stores the items as IDs and Names and a command is provided to add or delete items from the list

## Commands
There is only one command with three options, that command can be written as one of these {`/mediumcustom` , `/medcustom` , `/medcust` , `/mc`}

## Options
There are four options, one for adding an item into the list, one for deleting an item from the list (Both of these need a third parameter which will be the item ID), one for checking the item list (will give a grocery list of items and ID) and lastly the help option which will help with the options.
The commands could be written as such:
Adding: /mediumcustom add [ItemID]
Deleting: /mediumcustom del [ItemID]
Checking: /mediumcustom check
Help: /mediumcustom help

## InnerWorkings
The commands are simple, multiple corner cases where made but if you run into an issue, please make an issue about it and I will fix immediately (or a pull request would be nice)
The bulk of the work is done in two functions, the first one which is onGetData, this function is used in the NetGetData hook and its main purpose is to check the message if it is a death message then inside, check the player's difficulty then call the other function OnPlayerDeath.
This function will loop through the entire inventory of the player then check whether the item is in the list or not, if it is then it will create a new Item in the player's death position and delete the one that the player already has.

This is the overview of the plugin
