"use strict";


function InXNA()
{
	return typeof xna !== 'undefined';
}


$(document).ready(function ()
{
	if (!InXNA())
	{
		$("body").addClass("nebula");
	}
});


// Inject a new function to make working inside scopes a little nicer
window.scopeOf = function(selector){
    return angular.element('[ng-controller=' + selector + ']').scope();
};


function XNACall(param)
{
	if (typeof xna === 'undefined')
	{
		// We're not in XNA, abort
		return;
	}
	
	
	var callString;
	if (typeof param === 'string')
	{
		callString = param;
	}
	else if (typeof param !== 'undefined' && typeof param.attr("call") !== 'undefined')
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
	//x = nStr.split('.');
	//x1 = x[0];
	//x2 = x.length > 1 ? '.' + x[1] : '';
	var rgx = /(\d+)(\d{3})/;
	while (rgx.test(nStr)) {
		nStr = nStr.replace(rgx, '$1' + ',' + '$2');
	}
	return nStr;
}




function Diff(newValues, oldValues)
{
	if(oldValues == null)
	{
		return newValues;
	}
	
	var diff = null;
	for(var p in newValues)
	{
		if (oldValues.hasOwnProperty(p))
		{
			if(newValues[p] !== oldValues[p])
			{
				if(typeof(newValues[p]) == 'object')
				{
					var innerDiff = Diff(newValues[p], oldValues[p]);
					if(innerDiff != null)
					{
						if(diff == null)
						{
							diff = {};
						}
						diff[p] = innerDiff;
					}
				}
				else
				{
					if(diff == null)
					{
						diff = {};
					}
					diff[p] = newValues[p];
				}
			}
		}
		else
		{
			if(diff == null)
			{
				diff = {};
			}
			diff[p] = newValues[p];
		}
	}
	return diff;
}

