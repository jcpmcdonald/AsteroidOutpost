

function SelectionChanged(newSelection)
{
	$("#selectionTitle").text(newSelection["name"]);
	$("#health").text(newSelection["health"] + " / " + newSelection["maxhealth"]);
	$("#level").text(newSelection["level"]);
	$("#team").text(newSelection["team"]);
}

function SetResources(newResources)
{
	$("#minerals").text(FormatNumber(newResources["minerals"]));
}

function ShowGameMenu()
{
	$("#gameMenu").show();
}

function HideGameMenu()
{
	$("#gameMenu").hide();
	XNACall("hud.ResumeGame()");
}

function SetPaused(paused)
{
	if(paused)
	{
		$("#pauseDisplay").addClass("paused");
	}
	else
	{
		$("#pauseDisplay").removeClass("paused");
	}
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
	if(!InXNA())
	{
		$("body").addClass("nebula");
		SelectionChanged({
			"name": "Solar Station",
			"health": 110,
			"maxhealth": 150,
			"level": 1,
			"team": "Team1"
		});
		SetResources({"minerals": 1000});
	}
	
	
	// Add a call to all the construction buttons 
	$(".constructionButton").each( function(){
		$(this).attr("call", "hud.Build" + this.id + "()");
	});
	
	
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
	
});
