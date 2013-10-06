//"use strict";


function SetResources(newResources)
{
	$("#minerals").text(FormatNumber(newResources));
}



function SetPaused(paused)
{
	if(paused)
	{
		$("#pauseDisplay").addClass("paused");
		$("#modalOverlay").fadeIn(200);
	}
	else
	{
		$("#pauseDisplay").removeClass("paused");
		$(".modalPanel").hide();
		$("#modalOverlay").fadeOut(200);
	}
}


function ShowModalGameMenu()
{
	$("#modalOverlay").fadeIn(200);
	$("#gameMenu").show();
}

function GameOver(win)
{
	$("#modalOverlay").fadeIn(200);
	if(win)
	{
		$("#gameWin").show();
	}
	else
	{
		$("#gameLost").show();
	}
}

function ShowModalDialog(text)
{
	$("#modalOverlay").fadeIn(200);
	$("#dialog .modalText").html(text);
	$("#dialog").show();
}




function MakeTimerPanel()
{
	$("body").append('<div id="timer" class="panel">0:00</div>');
}

function UpdateTimerPanel(value)
{
	$("#timer").text(value);
}



var mouseDownOverHUD = false;
function docMouseDown(event)
{
	if(typeof hud !== 'undefined')
	{
		// Send event to XNA
		hud.OnMouseDown(mouseDownOverHUD, event.which - 1);
	}
	mouseDownOverHUD = false;
}

var mouseUpOverHUD = false;
function docMouseUp(event)
{
	if(typeof hud !== 'undefined')
	{
		// Send event to XNA
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
		
		// With some dummy minerals
		SetResources(1000);
		
		
		// Show the in-game menu when you press ESC
		$(document).keydown(function(event)
		{
			if(event.keyCode === KEY_ESC)
			{
				var paused = $("#gameMenu").is(":visible");
				SetPaused(!paused);
				if(!paused)
				{
					$("#gameMenu").show();
				}
			}
		});
		
		
		$(".btnResume").click( function (event){ SetPaused(false); });
		
		// Make the Main Menu button work (fakely)
		$(".btnMainMenu").click( function (event){ window.location = "Singleplayer.html"; });
		
		// Add some missions
		AddMission("buildMiners", "Build 3 miners near asteroids", true);
		AddMission("buildLaserTowers", "Build 2 laser towers", false);
	}

});


$(document).ready(function()
{
	
	// Make button presses look cool
	// $(".button").mousedown( function(event)
	// {
		// $(this).addClass("buttonPressed");
	// });
	
	// $(".button").mouseup( function(event)
	// {
		// //$(this).removeClass("buttonPressed");
		// if(!$(this).hasClass("disabled"))
		// {
			// XNACall($(this));
		// }
	// });
	
	// $(".button").mouseleave( function(event)
	// {
		// $(this).removeClass("buttonPressed");
	// });
	
	
	
	// Capture mouse up/down events so we can tell XNA if we've handled the action or not
	$(document).mousedown( function(event)
	{
		docMouseDown(event);
	});
	
	$("body").mousedown( function(event)
	{
		mouseDownOverHUD = true;
	});
	
	$(document).mouseup( function(event)
	{
		docMouseUp(event);
	});
	
	$("body").mouseup( function(event)
	{
		mouseUpOverHUD = true;
	});
});


var Position = function()
{
	// $("#editorPanel").position({
		// of: $(body),
		// my: "right bottom",
		// at: "right bottom"
	// });
	
	
	if($("#editorPanel").is(':visible'))
	{
		// $("#console").position({
			// of: $("#editorPanel"),
			// my: "right bottom",
			// at: "left bottom"
		// });
		
		$("#editorToggleButton").position({
			of: $("#editorPanel"),
			my: "right center",
			at: "left center"
		});
	}
	else
	{
		// $("#console").position({
			// of: $(window),
			// my: "right bottom",
			// at: "right bottom"
		// });
		
		$("#editorToggleButton").position({
			of: $(window),
			my: "right center",
			at: "right center"
		});
	}
};



$(document).ready(function()
{
	
	$(window).resize(Position);
	Position();
	
	$("#editorToggleButton").click(function(){
		$("#editorToggleButton").hide("slide", {direction:'right'}, 60, function(){
			$("#editorPanel").toggle("slide", { direction:'right'}, 500, function(){
				EntityEditorOnShow();
				Position();
				$("#editorToggleButton").show("slide", {direction:'right'}, 60, function(){ Position(); } );
			});
		});
	});
});




