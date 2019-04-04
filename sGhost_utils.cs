//Swollow's sGhost_Utils v1
//04/01/2019
//Selectivley ghost objects to only individual players
//
//Functions
//	sGhost_ini();						Initilize sGhost utils, only call this after you have initilized all your objSets,
//											you must call this before any clients spawn
//	sGhost_iniObjSet(datablock,e);		Initilize an sGhost object set, this will create 64+ instances of the object meant to be designated
//											for each individual player, you must call this before any clients spawn, e is a multiplier of
//											how many objects to create
//	sGhost_getObj(datablock,client);	Gets an sGhost object for a client, call this everytime you need to access an object of the type
//											that is only intended for that single client to see, DO NOT DELETE THESE OBJECTS, always
//											move these objects far out of bounds, DO NOT SCOPE THESE TO ANY CLIENTS, DO NOT ADD OR REMOVE
//											THESE FROM GHOSTALWAYSSET
//04/03/2019	1.02				Added E onto sGhost_iniObjSet, allows to create even more objects

$Swol_SGhostUtilsTmpFVersion = 1.02;
if($Swol_SGhostUtilsVersion !$= "" && $Swol_SGhostUtilsVersion >= $Swol_SGhostUtilsTmpFVersion)
	return;
$Swol_SGhostUtilsVersion = $Swol_SGhostUtilsTmpFVersion;

//front end functions

function sGhost_ini(%flag)
{
	if(!isObject(%t = swol_sGhostTmpQueue))
		return 0;
	if(!isObject(swol_sGhost))
	{
		if(!%flag)
		{
			cancel($Swol_SGhostUtilsTmpSched);
			$Swol_SGhostUtilsTmpSched = schedule(1,%t,sGhost_ini,1);
			return 1;
		}
		else
		{
			$Swol_SGhostUtilsTmpSched = "";
			return (new scriptObject(swol_sGhost)).ini();
		}
	}
	return 0;
}
function sGhost_iniObjSet(%db,%extra)
{
	if(!isObject(%db))
		return;
	if(!isObject(%t = swol_sGhostTmpQueue))
		%t = new simSet(swol_sGhostTmpQueue);
	if(%extra !$= "")
		%t.sE[nameToID(%db)] = %extra;
	%t.add(%db);
}
function sGhost_getObj(%db,%cl)
{
	if(!isObject(%s = swol_sGhost))
		return 0;
	return %s.getGhostObj(%db,%cl);
}

//backend, do not use any of these directly

function swol_sGhost::processQueue(%this)
{
	if(!%this.waitingToProcessQueue)
		return 0;
	if(!isObject(%t = swol_sGhostTmpQueue))
		return 0;
	%i = %t.getCount();
	while(%i-->=0)
	{
		%extra = %t.sE[nameToID(%db = %t.getObject(%i))];
		%this.iniGhostObjSet(%db,%extra);
	}
	%this.waitingToProcessQueue = "";
	swol_sGhostTmpQueue.delete();
	return 1;
}
function swol_sGhost::ini(%this)
{
	if($Pref::Server::MaxPlayers > 64)
		%this.maxPlayers = $Pref::Server::MaxPlayers;
	else
		%this.maxPlayers = 64;
	
	%this.version = $Swol_SGhostUtilsVersion;
	%this.waitingToProcessQueue = 1;
	%this.processQueue();
	return 1;
}
function swol_sGhost::destroy(%this,%skipClientClean)
{
	for(%i=0;(%d=%this.getTaggedField(%i))!$="";%i++)
	{
		if(getSubStr((%db = getField(%d,0)),0,1) !$= "s")
			continue;
		%db = getSubStr(%db,1,255);
		%set = getField(%d,1);
		%c = %set.getCount();
		while(%c-->=0)
			%set.getObject(%c).delete();
		%set.delete();
	}
	if(%skipClientClean)
		return 1;
	%i = (%g=clientGroup).getCount();
	while(%i-->=0)
	{
		%c = (%cl = %g.getObject(%i))._SWOL_SGHOST_USE_CNT;
		while(%c-->=0)
			%cl._SWOL_SGHOST_USE[%c] = "";
		%cl._SWOL_SGHOST_USE_CNT = "";
	}
	%this.delete();
	return 1;
}
function swol_sGhost::getGhostObj(%this,%db,%cl)
{
	%dbName = %db.getName();
	if(!isObject(%s = %this.s[%dbName]))
		return 0;

	%i = %cl._SWOL_SGHOST_USE_CNT;
	while(%i-->=0)
		if((%o = %cl._SWOL_SGHOST_USE[%i]).getDatablock() == %db)
			return %o;

	%i = %s.getCount();
	while(%i-->=0)
		if(!(%o = %s.getObject(%i))._SWOL_SGHOST_USED)
			break;
	
	%o.scopeToClient(%cl);
	%o._SWOL_SGHOST_USED = %cl;
	%cl._SWOL_SGHOST_USE[%cl._SWOL_SGHOST_USE_CNT|0] = %o;
	%cl._SWOL_SGHOST_USE_CNT++;
	return %o;
}
function swol_sGhost::iniGhostObjSet(%this,%db,%extra)
{
	%dbName = %db.getName();
	if(isObject(%this.s[%dbName]))
		return 0;

	%dbClass = %db.getClassName();
	switch$(%dbClass)
	{
		case "staticShapeData":
			%class = staticShape;
		case "playerData":
			%class = aiPlayer;
		default:
			return 0;
	}
	%s = %this.s[%dbName] = new simSet();
	missionCleanup.add(%s);
	if(%extra !$= "")
		%max = %this.maxPlayers*%extra;
	else %max = %this.maxPlayers;

	for(%i=0;%i<%max;%i++)
	{
		%o = new (%class)()
		{
			datablock = %dbName;
			position = "9999999999 9999999999 0";
			scale = "1 1 1";
			_SWOL_SGHOST_USED = 0;
		};
		%o.setScopeAlways();
		ghostAlwaysSet.remove(%o);
		%s.add(%o);
	}
	return 1;
}
package swol_sGhostUtils_v1
{
	function gameConnection::onDrop(%cl)
	{
		%i = %cl._SWOL_SGHOST_USE_CNT;
		while(%i-->=0)
			%cl._SWOL_SGHOST_USE[%i]._SWOL_SGHOST_USED = 0;

		return parent::onDrop(%cl);
	}
};
activatePackage(swol_sGhostUtils_v1);

package swol_sGhostUtils_v1_KILLER
{
	function gameConnection::spawnPlayer(%cl)
	{
		eval("function sGhost_ini(%x){error(\"sGhost_ini() - a player has already spawned\");}function sGhost_iniObjSet(%x){error(\"sGhost_iniObjSet(db) - a player has already spawned\");}function swol_sGhost::processQueue(%x){}");
		schedule(1,0,deactivatePackage,swol_sGhostUtils_v1_KILLER);
		return parent::spawnPlayer(%cl);
	}
};
activatePackage(swol_sGhostUtils_v1_KILLER);

$Swol_SGhostUtilsTmpFVersion = "";