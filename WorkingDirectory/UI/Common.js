

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
