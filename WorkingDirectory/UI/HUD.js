"use strict";

function SelectionChanged(newSelection)
{
	$("#selectionTitle").text(newSelection.name);
	$("#health").text(newSelection.health + " / " + newSelection.maxhealth);
	$("#level").text(newSelection.level);
	$("#team").text(newSelection.team);
}

function SetResources(newResources)
{
	$("#minerals").text(FormatNumber(newResources.minerals));
}

function ShowGameMenu()
{
	$("#gameMenu").show();
}

//
// This will hide the in-game menu, but *not* unpause the game, because that could have been initiated by another player
//
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
	
	///
	/// This section attempts to immatate what XNA will be doing in-game. It makes it easier to debug
	///
	if(!InXNA())
	{
		// Show a dummy structure
		SelectionChanged({
			"name": "Solar Station",
			"health": 110,
			"maxhealth": 150,
			"level": 1,
			"team": "Team1"
		});
		
		// With some dummy minerals
		SetResources({"minerals": 1000});
		
		// Show the in-game menu when you press ESC
		$(document).keydown(function(event)
			{
				if(event.keyCode === KEY_ESC)
				{
					$("#gameMenu").toggle();
					SetPaused($("#gameMenu").is(":visible"));
				}
			}
		);
		
		// And don't forget to hide the "paused" notifier when we resume
		$("#btnResume").click( function (event){ SetPaused(false); });
		
		// Make the Main Menu button work (fakely)
		$("#btnMainMenu").click( function (event){ window.location = "MainMenu.html"; });
	}
	
	
	// Add a call to all the construction buttons 
	$(".constructionButton").each( function(){
		$(this).attr("call", "hud.Build" + this.id + "()");
	});
	
	
	// Make button presses look cool
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
	
	
	// Capture mouse up/down events so we can tell XNA if we've handled the action or not
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
