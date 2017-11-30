//   _____ _        _     _
//  / ____| |      (_)   | |
// | (___ | |_ _ __ _  __| | ___
//  \___ \| __| '__| |/ _` |/ _ \
//  ____) | |_| |  | | (_| |  __/
// |_____/ \__|_|  |_|\__,_|\___|
//
//
// Author: Zapk
// Version: 1.1.0 (November 2017)
// URL: https://github.com/zapk/Server_Stride

exec("./lib/color.cs"); // RGB -> HEX conversion methods.
exec("./lib/decals.cs"); // Port's decal system. Used for footprints.

if(isFile("Add-Ons/System_ReturnToBlockland/server.cs") && !$RTB::Hooks::ServerControl)
{
	exec("Add-Ons/System_ReturnToBlockland/hooks/serverControl.cs");
}

// No, this won't cause an error if RTB doesn't exist.
//    allGameScripts.cs.dso :
//    "function RTB_registerPref(%displayName, %category, %varName, %varType, %varCategory, %defaultValue, %a, %b)"
RTB_registerPref("Default Material", "Stride", "$Pref::Stride::DefaultMaterial", "string 32", "Server_Stride", "concrete", 0, 0);
RTB_registerPref("Default Footprints", "Stride", "$Pref::Stride::DefaultFootprints", "bool", "Server_Stride", false, 0, 0);

datablock StaticShapeData(FootprintDecal)
{
	shapeFile = "Add-Ons/Server_Stride/res/shapes/footprint.dts";
	doColorShift = false;

	decalSize = 1.0;
};

if(isPackage( "footstepPackage" ))
{
	deactivatePackage( "footstepPackage" );
}

package footstepPackage
{
	function gameConnection::spawnPlayer( %this )
	{
		parent::spawnPlayer( %this );
		%this.player.fs_velocityCheckTick();
	}

	function Armor::onTrigger( %this, %obj, %slot, %val )
	{
		parent::onTrigger( %this, %obj, %slot, %val );

		if ( %slot == 4 && !%this.canJet )
		{
			%val = false;
		}

		%obj.fs_trigger[ %slot ] = %val;
	}
};

activatePackage( "footstepPackage" );

function Player::getRightVector( %this )
{
	%vec = %this.getForwardVector();

	%x = getWord(%vec, 0);
	%y = getWord(%vec, 1);
	%z = getWord(%vec, 2);

	return %y SPC -%x SPC %z;
}

function Player::getLeftVector( %this )
{
	%vec = %this.getForwardVector();

	%x = getWord(%vec, 0);
	%y = getWord(%vec, 1);
	%z = getWord(%vec, 2);

	return -%y SPC %x SPC %z;
}

function getNumberStart( %str )
{
	%best = -1;

	for ( %i = 0 ; %i < 10 ; %i++ )
	{
		%pos = strPos( %str, %i );

		if ( %pos < 0 )
		{
			continue;
		}

		if ( %best == -1 || %pos < %best )
		{
			%best = %pos;
		}
	}

	return %best;
}

function loadFootstepSounds()
{
	%pattern = "Add-Ons/Server_Stride/res/steps/*.wav";
	%list = "generic 0";

	deleteVariables( "$FS::Sound*" );
	$FS::SoundNum = 0;

	for ( %file = findFirstFile( %pattern ) ; %file !$= "" ; %file = findNextFile( %pattern ) )
	{
		%base = fileBase( %file );
		%name = "footstepSound_" @ %base;

		if ( !isObject( %name ) )
		{
			datablock audioProfile( genericFootstepSound )
			{
				description = "audioClosest3D";
				fileName = %file;
				preload = true;
			};

			if ( !isObject( %obj = nameToID( "genericFootstepSound" ) ) )
			{
				continue;
			}

			%obj.setName( %name );
		}

		if ( ( %pos = getNumberStart( %base ) ) > 0 )
		{
			%pre = getSubStr( %base, 0, %pos );
			%post = getSubStr( %base, %pos, strLen( %base ) );

			if ( $FS::SoundCount[ %pre ] < 1 || !strLen( $FS::SoundCount[ %pre ] ) )
			{
				%list = %list SPC %pre SPC $FS::SoundNum;
			}

			if ( $FS::SoundCount[ %pre ] < %post )
			{
				$FS::SoundCount[ %pre ] = %post;
			}

			$FS::SoundName[ $FS::SoundNum ] = %pre;
			$FS::SoundIndex[ %pre ] = $FS::SoundNum;
			$FS::SoundNum++;
		}
	}

	registerOutputEvent( "fxDTSBrick", "setMaterial", "list" SPC %list TAB "bool" );
}

function fxDTSBrick::setMaterial( %this, %idx, %hasFootprints )
{
	%this.material = $FS::SoundName[ %idx ] TAB %hasFootprints;
}

function playFootstep( %pos, %material )
{
	if ( !strLen( %material ) || $FS::SoundCount[ %material ] < 1 )
	{
		return;
	}

	if ( !isObject( %sound = nameToID( "footstepSound_" @ %material @ getRandom( 1, $FS::SoundCount[ %material ] ) ) ) )
	{
		return;
	}

	serverPlay3D( %sound, %pos );
}

function Player::getFootstepSettings( %this )
{
	%pos = vectorAdd( %this.getPosition(), "0 0 1" );
	%feetPos = vectorSub( %pos, "0 0 1.1" );
	%mask = $TypeMasks::FxBrickObjectType | $TypeMasks::InteriorObjectType | $TypeMasks::TerrainObjectType | $TypeMasks::ShapeBaseObjectType;
	return determineObjectFootstepSettings( firstWord( containerRayCast( %pos, %feetPos, %mask, %this ) ) );
}

function Player::getFootLocation( %this )
{
	return vectorSub( %this.getPosition(), "0 0 0.1" );
}

function Player::fs_velocityCheckTick( %this )
{
	cancel( %this.fs_velocityCheckTick );

	if( %this.getDatablock().strideIgnore )
		return;

	if ( vectorLen( %this.getVelocity() ) >= 2.62 && !isEventPending( %this.fs_playTick ) )
	{
		%this.fs_playTick = %this.schedule( 100, "fs_playTick" );
	}

	%this.fs_velocityCheckTick = %this.schedule( 50, "fs_velocityCheckTick" );
}

function Player::fs_playTick( %this )
{
	cancel( %this.fs_playTick );

	if ( vectorLen( %this.getVelocity() ) < 2.62 )
	{
		return;
	}

	if ( %this.fs_trigger[ 3 ] || %this.fs_trigger[ 4 ] )
	{
		return;
	}

	%settings = %this.getFootstepSettings();

	%material = getField(%settings, 0);
	%hasFootprints = !!getField(%settings, 1);
	%color = getField(%settings, 2);

	if ( %material $= "" )
	{
		return;
	}

	playFootstep( %this.getFootLocation(), %material );

	if(%hasFootprints)
	{
		%cR = mClampF(getWord(%color, 0) - 0.075, 0, 1);
		%cG = mClampF(getWord(%color, 1) - 0.075, 0, 1);
		%cB = mClampF(getWord(%color, 2) - 0.075, 0, 1);

		%color = %cR SPC %cG SPC %cB SPC 1;

		%pos = vectorAdd( %this.getPosition(), "0 0 0.01" );

		%foot = !!%this.fs_currentFoot;

		if(%foot) // left foot
		{
			%pos = VectorAdd(%pos, VectorScale( %this.getLeftVector(), 0.25 ));
		}
		else // right foot
		{
			%pos = VectorAdd(%pos, VectorScale( %this.getRightVector(), 0.25 ));
		}

		%obj = spawnDecal("FootprintDecal", %pos, %this.getForwardVector());

		if(isObject(%obj))
		{
			%obj.setNodeColor("ALL", %color);
		}

		%this.fs_currentFoot = !%foot;
	}

	%this.fs_playTick = %this.schedule( 290, "fs_playTick" );
}

function determineObjectFootstepSettings( %obj )
{
	if ( !isObject( %obj ) )
	{
		return "";
	}

	%class = %obj.getClassName();

	if ( %class $= "fxDTSBrick" )
	{
		%colorID = %obj.colorID;
		%color = getColorIDTable(%colorID);

		if ( strLen( %obj.material ) )
		{
			return getField(%obj.material, 0) TAB !!getField(%obj.material, 1) TAB mFloor(%colorID);
		}

		if($Pref::Stride::MaterialSetting[ %colorID ] !$= "")
		{
			return getField($Pref::Stride::MaterialSetting[ %colorID ], 0) TAB !!getField($Pref::Stride::MaterialSetting[ %colorID ], 1) TAB getColorF(%color);
		}
	}
	else if ( %class $= "fxPlane" )
	{
		%color = %obj.color;
	}

	return getField($Pref::Stride::DefaultMaterial, 0) TAB !!getField($Pref::Stride::DefaultFootprints, 0) TAB getColorF(%color);
}

function serverCmdClearFootsteps(%this)
{
	if(!%this.isAdmin)
	{
		messageClient(%this, '', '\c7This is an admin-only command.');
		return;
	}

	deleteVariables("$Pref::Stride::MaterialSetting*");

	if(isFunction("RS_Log"))
	{
		// Server_Roleplay logging hook.
		RS_Log(%this.getPlayerName() SPC "(" @ %this.getBLID() @ ") used '/clearFootsteps'");
	}

	messageAll('MsgClearBricks', '\c3%1 \c2has cleared all footstep/footprint settings.', %this.getPlayerName());
}

function serverCmdSetFootstep(%this, %material, %hasFootprints)
{
	if(!%this.isAdmin)
	{
		messageClient(%this, '', '\c7This is an admin-only command.');
		return;
	}

	// Force to a boolean by setting hasFootprints to not-not hasFootprints.
	%hasFootprints = !!%hasFootprints;

	for(%i = 0; %i < $FS::SoundNum; %i++)
	{
		%name = $FS::SoundName[%i];
		if(%name $= %material)
		{
			%exists = true;
			break;
		}
	}

	if(!%exists)
	{
		messageClient(%this, '', '\c7No material named \c6%1 \c7found.', StripMLControlChars(%material));
		return;
	}

	%color = mFloor(%this.currentColor);

	$Pref::Stride::MaterialSetting[%color] = %material TAB %hasFootprints;

	messageClient(%this, '', '\c7You have assigned the material \c6%1 \c7to paint color <color:%2>%3\c7, with footprints \c6%4\c7.', StripMLControlChars(%material), rgbToHex(getColorIDTable(%color)), %color, %hasFootprints ? "Enabled" : "Disabled");

	if(isFunction("RS_Log"))
	{
		// Server_Roleplay logging hook.
		RS_Log(%this.getPlayerName() SPC "(" @ %this.getBLID() @ ") used '/setFootstep " @ %material @ "'");
	}
}

loadFootstepSounds();
