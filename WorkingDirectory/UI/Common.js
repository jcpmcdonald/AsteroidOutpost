

function InXNA()
{
	return typeof xna !== 'undefined';
}


function XNACall(param)
{
	if (typeof xna === 'undefined')
	{
		// We're not in XNA, abort
		return;
	}
	
	
	var callString;
	if(typeof param === 'string')
	{
		callString = param;
	}
	else if(typeof param !== 'undefined' && typeof param.attr("call") !== 'undefined')
	{
		callString = param.attr("call");
	}
	else
	{
		return;
	}
	
	eval(callString);
}


function FormatNumber(nStr)
{
	nStr += '';
	x = nStr.split('.');
	x1 = x[0];
	x2 = x.length > 1 ? '.' + x[1] : '';
	var rgx = /(\d+)(\d{3})/;
	while (rgx.test(x1)) {
		x1 = x1.replace(rgx, '$1' + ',' + '$2');
	}
	return x1 + x2;
}
