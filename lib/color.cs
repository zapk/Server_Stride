function rgbToHex( %rgb )
{
	%r = _compToHex( 255 * getWord( %rgb, 0 ) );
	%g = _compToHex( 255 * getWord( %rgb, 1 ) );
	%b = _compToHex( 255 * getWord( %rgb, 2 ) );

	return %r @ %g @ %b;
}

function hexToRgb( %rgb )
{
	%r = _hexToComp( getSubStr( %rgb, 0, 2 ) ) / 255;
	%g = _hexToComp( getSubStr( %rgb, 2, 2 ) ) / 255;
	%b = _hexToComp( getSubStr( %rgb, 4, 2 ) ) / 255;

	return %r SPC %g SPC %b;
}

function _compToHex( %comp )
{
	%left = mFloor( %comp / 16 );
	%comp = mFloor( %comp - %left * 16 );

	%left = getSubStr( "0123456789ABCDEF", %left, 1 );
	%comp = getSubStr( "0123456789ABCDEF", %comp, 1 );

	return %left @ %comp;
}

function _hexToComp( %hex )
{
	%left = getSubStr( %hex, 0, 1 );
	%comp = getSubStr( %hex, 1, 1 );

	%left = striPos( "0123456789ABCDEF", %left );
	%comp = striPos( "0123456789ABCDEF", %comp );

	if ( %left < 0 || %comp < 0 )
	{
		return 0;
	}

	return %left * 16 + %comp;
}