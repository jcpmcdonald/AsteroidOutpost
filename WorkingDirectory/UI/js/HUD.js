//"use strict";



function SelectionInfoController($scope)
{
	$scope.selectedUnits = [];
	//$scope.hitPointsProgressBar = null;
	$scope.progressBars = [];
	
	
	
	$scope.$watch('selectedUnits', function()
	{
		setTimeout(function()
		{
			UpdateEditor($scope.selectedUnits);
			//$scope.hitPointsProgressBar = null;
			$.each($scope.progressBars, function(index, progressBar)
			{
				progressBar.progressbar = null;
			});
			//console.log("selectedUnitsUpdated!");
		}, 0);
	});
	
	
	$scope.AddProgressBar = function($divID, $component, $value, $max, $invert)
	{
		$invert = typeof $invert !== 'undefined' ? $invert : false;
		
		// Grab the div
		//var div = $("#" + $name);
		var newProgressBar = {divID:$divID, component:$component, value:$value, max:$max};
		$scope.progressBars.push( newProgressBar );
		
		
		$scope.$watch('selectedUnits[0].' + newProgressBar.component + "." + newProgressBar.value, function()
		{
			setTimeout(function()
			{
				if($scope.has(newProgressBar.component))
				{
					if(newProgressBar.progressbar == null || !$("#" + newProgressBar.divID).is(':data(progressbar)'))
					{
						newProgressBar.progressbar = $("#" + newProgressBar.divID);
						newProgressBar.progressbar.progressbar();
					}
					newProgressBar.progressbar.progressbar( "option", "max", $scope.selectedUnits[0][newProgressBar.component][newProgressBar.max]);
					if(!$invert)
					{
						newProgressBar.progressbar.progressbar( "option", "value", $scope.selectedUnits[0][newProgressBar.component][newProgressBar.value]);
					}
					else
					{
						newProgressBar.progressbar.progressbar( "option", "value", $scope.selectedUnits[0][newProgressBar.component][newProgressBar.max] - $scope.selectedUnits[0][newProgressBar.component][newProgressBar.value]);
					}
				}
			}, 0);
		});
	}
	
	
	$scope.selectedUnitsView = function()
	{
		if ($scope.selectedUnits == null || $scope.selectedUnits.length == 0)
		{
			return 'none';
		}
		else if ($scope.selectedUnits.length == 1)
		{
			return 'one';
		}
		else if ($scope.selectedUnits.length > 1)
		{
			return 'many';
		}
	}
	
	$scope.isConstructing = function()
	{
		return ($scope.selectedUnits != null &&
				$scope.selectedUnits.length == 1 &&
				typeof $scope.selectedUnits[0].Constructable != 'undefined' &&
				$scope.selectedUnits[0].Constructable.IsConstructing == true &&
				$scope.selectedUnits[0].Constructable.IsBeingPlaced == false);
	}
	
	$scope.has = function($component)
	{
		return ($scope.selectedUnits != null &&
				$scope.selectedUnits.length == 1 &&
				typeof $scope.selectedUnits[0][$component] != 'undefined');
	}
}

function SetSelection(newSelection)
{
	if(typeof(newSelection) === "string")
	{
		//var before = new Date();
		newSelection = eval("(" + newSelection + ")");
		//var after = new Date();
		//console.log("Eval = " + (after.getTime() - before.getTime()));
	}
	
	scopeOf('SelectionInfoController').selectedUnits = newSelection;
	scopeOf('SelectionInfoController').$apply();
}

function UpdateSelection(newSelection)
{
	if(typeof(newSelection) === "string")
	{
		//var before = new Date();
		newSelection = eval("(" + newSelection + ")");
		//var after = new Date();
		//console.log("Eval = " + (after.getTime() - before.getTime()));
	}
	
	var difference = $.extend(scopeOf('SelectionInfoController').selectedUnits, newSelection);
	scopeOf('SelectionInfoController').$apply();
}


$(document).ready(function()
{
	scopeOf("SelectionInfoController").AddProgressBar("hitPointsProgressBar", "HitPoints", "Armour", "TotalArmour");
	scopeOf("SelectionInfoController").AddProgressBar("constructionProgressBar", "Constructable", "MineralsLeftToConstruct", "MineralsToConstruct", true);
	scopeOf("SelectionInfoController").AddProgressBar("powerLevelProgressBar", "PowerProducer", "AvailablePower", "MaxPower");
	scopeOf("SelectionInfoController").AddProgressBar("mineralsProgressBar", "Minable", "Minerals", "StartingMinerals");
});



function SetResources(newResources)
{
	$("#minerals").text(FormatNumber(newResources.minerals));
}



function SetPaused(paused)
{
	if(paused)
	{
		$("#pauseDisplay").addClass("paused");
		$("#gameMenu").show();
		$("#modalOverlay").fadeIn(200);
	}
	else
	{
		$("#pauseDisplay").removeClass("paused");
		$("#gameMenu").hide();
		$("#modalOverlay").fadeOut(200);
	}
}


function GameOver(win)
{
	$("#modalOverlay").fadeIn(200);
	$("#gameLost").show();
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
					"EntityID": 10,
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
						"Armour" : 70,
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
				var paused = $("#gameMenu").is(":visible");
				SetPaused(!paused);
			}
		});
		
		
		$("#btnResume").click( function (event){ SetPaused(false); });
		
		// Make the Main Menu button work (fakely)
		$(".btnMainMenu").click( function (event){ window.location = "MainMenu.html"; });
		
		// Add some missions
		scopeOf("MissionController").AddMission("buildMiners", "Build 3 miners near asteroids", "0/3", false);
		scopeOf("MissionController").AddMission("buildLaserTowers", "Build 2 laser towers", "", false);
	}

});


$(document).ready(function()
{
	// Add a call attribute to all the construction buttons dynamically
	$(".constructionButton").each( function()
	{
		$(this).attr("call", "hud.Build" + this.id + "()");
	});
	
	
	// Make button presses look cool
	$(".button").mousedown( function(event)
	{
		$(this).addClass("buttonPressed");
	});
	
	$(".button").mouseup( function(event)
	{
		$(this).removeClass("buttonPressed");
		XNACall($(this));
	});
	
	$(".button").mouseleave( function(event)
	{
		$(this).removeClass("buttonPressed");
	});
	
	
	
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






