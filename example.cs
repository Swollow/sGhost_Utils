exec("./sGhost_utils.cs");
datablock staticShapeData(testShape)
{
	shapeFile = "base/data/shapes/Hammer.dts";
};
sGhost_iniObjSet(testShape);
sGhost_ini();

//Creates a hammer over the nearest player's head when you click, but it is only visible to you
package swol_sGhost_example
{
	function armor::onTrigger(%db,%pl,%trig,%bool)
	{
		if(!%trig && %bool)
		{
			if(!%pl.getMountedImage(0))
			{
				if(%cl = %pl.client)
				{
					%i = clientGroup.getCount();
					%lowestDist = 9999;
					%pos = %pl.getPosition();
					while(%i-->=0)
					{
						if(!isObject(%cpl = clientGroup.getObject(%i).player))
							continue;
						if(%cpl == %pl)
							continue;
						if((%dist = vectorDist(%cpl.getPosition(),%pos)) < %lowestDist)
						{
							%lowestDist = %dist;
							%closest = %cpl;
						}
					}
					if(%closest)
					{
						%marker = sGhost_getObj(testShape,%cl);
						%marker.setTransform(vectorAdd(%closest.getPosition(),"0 0 3.5"));
						%marker.setNodeColor("ALL","1 0 0 1");
						%marker.schedule(2000,setTransform,"9999999999 9999999999 0");
					}
				}
			}
		}
		parent::onTrigger(%db,%pl,%trig,%bool);
	}
};
activatePackage(swol_sGhost_example);