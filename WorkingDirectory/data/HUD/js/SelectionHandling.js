
function SelectionInfoController($scope)
{
	$scope.selectedUnits = [];
	//$scope.hitPointsProgressBar = null;
	$scope.progressBars = [];
	
	
	
	$scope.$watch('selectedUnits', function()
	{
		setTimeout(function()
		{
			//console.log("Updating Editor!");
			//UpdateEditor($scope.selectedUnits);
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
	};
	
	
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
	};
	
	$scope.isConstructing = function()
	{
		return ($scope.selectedUnits != null &&
				$scope.selectedUnits.length == 1 &&
				typeof $scope.selectedUnits[0].Constructible != 'undefined' &&
				$scope.selectedUnits[0].Constructible.IsConstructing == true &&
				$scope.selectedUnits[0].Constructible.IsBeingPlaced == false);
	};
	
	$scope.has = function($component)
	{
		return ($scope.selectedUnits != null &&
				$scope.selectedUnits.length == 1 &&
				typeof $scope.selectedUnits[0][$component] != 'undefined');
	};
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
	
	return true;
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
	
	UpdateEditor(newSelection);		// TODO: Update only the changed pieces
	
	return true;
}


$(document).ready(function()
{
	scopeOf("SelectionInfoController").AddProgressBar("hitPointsProgressBar", "HitPoints", "Armour", "TotalArmour");
	scopeOf("SelectionInfoController").AddProgressBar("constructionProgressBar", "Constructible", "MineralsConstructed", "Cost");
	scopeOf("SelectionInfoController").AddProgressBar("powerLevelProgressBar", "PowerStorage", "AvailablePower", "MaxPower");
	scopeOf("SelectionInfoController").AddProgressBar("mineralsProgressBar", "Minable", "Minerals", "StartingMinerals");
});



$(document).ready(function()
{
	
	$("#editorPanel").hide();
	Position();
		
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
						"PowerProductionRate" : 10,
					},
					"PowerStorage" : {
						"MaxPower" : 70,
						"AvailablePower" : 19.27076,
					},
					"PowerNode" : {
						"PowerLinkPointRelative" : "-1, -13",
					},
					"HitPoints" : {
						"Armour" : 70,
						"TotalArmour" : 250,
						"GUID" : "pHMJfGX6oUG6LcG1poDh+Q=="
					},
					"Constructible" : {
						"Cost" : 200,
						"MineralsConstructed" : 20,
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
	}
});

