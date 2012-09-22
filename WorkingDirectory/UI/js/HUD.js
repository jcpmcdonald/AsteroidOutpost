//"use strict";

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


function MakeTimerPanel()
{
	$("body").append('<div id="timer" class="panel">0:00</div>');
}

function UpdateTimerPanel(value)
{
	$("#timer").text(value);
}


var missions;
function AddMission(key, description)
{
	//if (typeof missions === 'undefined')
	if (missions == undefined)
	{
		missions = new Array();
		$("body").append('<div id="missions" class="panel"></div>');
	}
	missions[key] = description;
	$("#missions").append('<div id="mission' + key + '" class="mission">' + description + '</div>');
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
		
		// Add some missions
		AddMission("buildMiners", "Build 3 miners near asteroids");
		AddMission("buildLaserTowers", "Build 2 laser towers");
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





/// #########################################################
/// #### Console
/// #########################################################

function addConsoleMessage(name, message)
{
	$("#history").append('<div class="consoleLine"><span class="consoleID">' +
							name +
							'</span><span class="consoleText">' +
							message +
							'</span><span class="consoleTime">' +
							(new Date()).toLocaleTimeString() +
							'</span></div>');
	$("#history").scrollTop($("#history").prop("scrollHeight"));
	
	// Execute javascript on request
	if(message.toLowerCase().indexOf("/js ") == 0)
	{
		eval(message.substr(4));
	}
}

$(document).ready(function()
{
	var username = "me";
	
	$("#consoleInput").keyup(function(event){
		if(event.which == 13)
		{
			var text = $("#consoleInput").val().trim();
			$("#consoleInput").val("");
			if(text == "")
			{
				return;
			}
			
			addConsoleMessage(username, text);
		}
	});
});

