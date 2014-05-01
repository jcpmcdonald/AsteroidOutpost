/// <reference path="./libs/jquery.d.ts" />
/// <reference path="./libs/angular.d.ts" />
"use strict";

function InXNA()
{
	return typeof xna !== 'undefined';
}


$(document).ready(function ()
{
	if (!InXNA())
	{
		$("body").addClass("inXNA");

	}
});


// Inject a new function to make working inside scopes a little nicer
window.scopeOf = function(selector){
	return angular.element('[ng-controller=' + selector + ']').scope();
};


function XNACall(param)
{
	var callString;
	if (typeof param === 'string')
	{
		callString = param;
	}
	else if (typeof param !== 'undefined' && typeof param.attr("call") !== 'undefined')
	{
		if(!param.hasClass("disabled"))
		{
			callString = param.attr("call");
		}
		else
		{
			return;
		}
	}
	else
	{
		return;
	}
	
	
	if (typeof xna === 'undefined')
	{
		// We're not in XNA, abort
		console.log(callString + 'test');
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


// Prevent the backspace key from navigating back.
// Grabbed from http://stackoverflow.com/a/2768256/536974
$(document).unbind('keydown').bind('keydown', function (event) {
    var doPrevent = false;
    if (event.keyCode === 8) {
        var d = event.srcElement || event.target;
        if ((d.tagName.toUpperCase() === 'INPUT' && (d.type.toUpperCase() === 'TEXT' || d.type.toUpperCase() === 'PASSWORD')) 
             || d.tagName.toUpperCase() === 'TEXTAREA') {
            doPrevent = d.readOnly || d.disabled;
        }
        else {
            doPrevent = true;
        }
    }

    if (doPrevent) {
        event.preventDefault();
    }
});

