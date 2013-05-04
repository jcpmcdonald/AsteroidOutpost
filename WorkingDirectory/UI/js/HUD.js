//"use strict";

var EditorMode = true;


function SelectionInfoController($scope)
{
	$scope.selectedUnits = [];
	
	$scope.selectedUnitsView = function(){
		if ($scope.selectedUnits == null || $scope.selectedUnits.length == 0){
			return 'none';
		}else if ($scope.selectedUnits.length == 1){
			return 'one';
		}else if ($scope.selectedUnits.length > 1){
			return 'many';
		}
	}
	
	
	$scope.hasHitPoints = function(){
		return ($scope.selectedUnits != null &&
				$scope.selectedUnits.length == 1 &&
				typeof $scope.selectedUnits[0].HitPoints != 'undefined');
	}
	
	$scope.isConstructing = function(){
		return ($scope.selectedUnits != null &&
				$scope.selectedUnits.length == 1 &&
				typeof $scope.selectedUnits[0].Constructable != 'undefined' &&
				$scope.selectedUnits[0].Constructable.IsConstructing == true &&
				$scope.selectedUnits[0].Constructable.IsBeingPlaced == false);
	}
}

function UpdateSelection(selection)
{
	if(typeof(selection) === "string")
	{
		var before = new Date();
		selection = eval("(" + selection + ")");
		var after = new Date();
		console.log("Eval = " + (after.getTime() - before.getTime()));
	}
	
	scopeOf('SelectionInfoController').selectedUnits = selection;
	scopeOf('SelectionInfoController').$apply();
}
	
	// if(selection != null)
	// {
		// if(typeof(selection) === "string")
		// {
			// var before = new Date();
			// selection = eval("(" + selection + ")");
			// var after = new Date();
			// console.log("Eval = " + (after.getTime() - before.getTime()));
		// }
		// 
		// 
		// for(entityID in selection)
		// {
			// if(selection[entityID].EntityName !== undefined)
			// {
				// $("#selectionTitle").text(selection[entityID].EntityName.Name);
			// }
			// if(selection[entityID].HitPoints !== undefined)
			// {
				// $("#health").text(Math.round(selection[entityID].HitPoints.Armour) + " / " + selection[entityID].HitPoints.TotalArmour);
			// }
			// //$("#selectionTitle").text(selection[entityID].EntityName.Name);
			// //$("#health").text(selection.health + " / " + selection.maxhealth);
			// //$("#level").text(selection.level);
			// //$("#team").text(selection.team);
			// 
			// // Only do this once (for now)
			// break;
		// }
		// 
	// }
	// else
	// {
		// $("#selectionTitle").html("&nbsp;");
		// $("#health").text("");
	// }
	// 
	// //UpdateEditor(selection);
// }

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
		UpdateSelection(
			[
				{
					"EntityName" : {
						"Name" : "Solar Station",
						"GUID" : "Flsp0Nra4Um1VieH6zRgNg=="
					},
					"PowerProducer" : {
						"MaxPower" : 70,
						"AvailablePower" : 19.27076,
						"PowerProductionRate" : 10,
						"ConductsPower" : true,
						"ProducesPower" : true,
						"PowerLinkPointRelative" : {
							"X" : -1,
							"Y" : -13
						},
						"GUID" : "bPWxGkDFFU+jB62HmRN1HA=="
					},
					"HitPoints" : {
						"Armour" : 250,
						"TotalArmour" : 250,
						"GUID" : "pHMJfGX6oUG6LcG1poDh+Q=="
					},
					"Constructable" : {
						"MineralsToConstruct" : 200,
						"MineralsLeftToConstruct" : 200,
						"IsBeingPlaced" : false,
						"IsConstructing" : true,
						"GUID" : "dQz6/7Jjfk+6Slcu4NZLHQ=="
					},
					"Position" : {
						"Center" : {
							"X" : 9924,
							"Y" : 9926
						},
						"Solid" : true,
						"Velocity" : {
							"X" : 0,
							"Y" : 0
						},
						"Radius" : 40,
						"GUID" : "gMDlDXhPi068aSaaz56Wkw=="
					},
					"Animator" : {
						"Scale" : 0.7,
						"Tint" : {
							"R" : 255,
							"G" : 255,
							"B" : 255,
							"A" : 255,
							"PackedValue" : 4294967295
						},
						"FrameAngle" : 0,
						"Angle" : 180,
						"GUID" : "HvZ6suwYMkKhN1nggqpiaQ=="
					}
				}
			]
		);
		
		
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
	
	
	// Add a call attribute to all the construction buttons dynamically
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

$username = "me";

function ConsoleController($scope)
{
	$scope.consoleLines = [ {username:'System', message:'Welcome', timestamp:new Date()} ];
	
	$scope.addConsoleMessage = function()
	{
		// Execute javascript on request
		if($scope.consoleInput.toLowerCase().indexOf("/js ") == 0)
		{
			eval($scope.consoleInput.substr(4));
		}
		
		$scope.consoleLines.push({username:$username, message:$scope.consoleInput, timestamp:new Date() });
		$scope.consoleInput = "";
		
		setTimeout(function(){$("#history").scrollTop($("#history").prop("scrollHeight"))}, 10);
	}
}

