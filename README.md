# sGhost_Utils
*04/01/2019*

*v1*


Selectivley ghost objects to only individual players

## Functions

#### sGhost_ini();
Initilize sGhost utils, only call this after you have initilized all your objSets, you must call this before any clients spawn
#### sGhost_iniObjSet(datablock);
Initilize an sGhost object set, this will create 64+ instances of the object meant to be designated for each individual player, you must call this before any clients spawn
#### sGhost_getObj(datablock,client);
Gets an sGhost object for a client, call this everytime you need to access an object of the type that is only intended for that single client to see,
DO NOT DELETE THESE OBJECTS, always move these objects far out of bounds
DO NOT SCOPE THESE TO ANY CLIENTS
DO NOT ADD OR REMOVE THESE FROM GHOSTALWAYSSET