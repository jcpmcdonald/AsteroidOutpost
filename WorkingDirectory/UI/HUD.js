

var TEST_selectionData =
		{
			"name": "Solar Station",
			"health": 100,
			"maxhealth": 150,
			"level": 1,
			"team": "Team1"
		};


function selectionChanged(newSelection)
{
	$("#selectionTitle").text(newSelection["name"]);
	$("#health").text(newSelection["health"] + " / " + newSelection["maxhealth"]);
	$("#level").text(newSelection["level"]);
	$("#team").text(newSelection["team"]);
}


var mouseDownOverHUD = false;
function docMouseDown(event)
{
	if(typeof hud !== 'undefined')
	{
		hud.OnMouseDown(mouseDownOverHUD, event.which - 1);
	}
	mouseDownOverHUD = false;
}

var mouseUpOverHUD = false;
function docMouseUp(event)
{
	if(typeof hud !== 'undefined')
	{
		hud.OnMouseUp(mouseUpOverHUD, event.which - 1);
	}
	mouseUpOverHUD = false;
}


$(document).ready(function()
{
	
	$(".button").mousedown( function(event) {
		$(this).addClass("buttonPressed");
	});
	$(".button").mouseup( function(event) {
		$(this).removeClass("buttonPressed");
		XNACall($(this));
	});
	$(".button").mouseleave( function(event) {
		$(this).removeClass("buttonPressed");
	});
	
	
	$(document).mousedown( function(event) {
		docMouseDown(event);
	});
	$("body").mousedown( function(event) {
		mouseDownOverHUD = true;
	});
	
	
	$(document).mouseup( function(event) {
		docMouseUp(event);
	});
	$("body").mouseup( function(event) {
		mouseUpOverHUD = true;
	});
	
	//selectionChanged(TEST_selectionData);
});
