

function ContextButtonController($scope)
{
	$scope.buttons = [];
	
	
	$scope.SetContextPage = function(data)
	{
		if (typeof data === 'string')
		{
			data = eval("(" + data + ")");
		}
		$scope.buttons = data["ContextButtons"];
		$.each($scope.buttons, function(index, button){
			button.EnabledCSS = (typeof(button.Enabled) == 'undefined' || button.Enabled) ? "" : "disabled";
		});
		
		$scope.InitTooltips();
		//$scope.$apply();
	};
	
	$scope.AddButton = function(buttonData)
	{
		$scope.buttons.push(buttonData);
	};
	
	
	$scope.InitTooltips = function()
	{
		$( document ).tooltip({
			track: true,
			show: false,
			hide: false,
			items: ".contextButton:not(.disabled)",
			content: function(){
				var element = $(this);
				var index = element.attr("index");
				if(typeof $scope.buttons[index] != 'undefined')
				{
					var rv = "<h3 class='buttonHoverTitle'>" + $scope.buttons[index]["Name"] + "</h3>";
					for(dataPoint in $scope.buttons[index]["ImportantValues"])
					{
						//if(dataPoint.indexOf("$") != 0)
						//{
							rv += "<b>" + dataPoint + ":</b>&nbsp" + $scope.buttons[index]["ImportantValues"][dataPoint] + "<br/>";
						//}
					}
					return rv;
				}
				else
				{
					return "not found";
				}
			}
		});
	};
	
	
	$(document).ready(function()
	{
		$scope.InitTooltips();
	});
}


function SetContextPage($page)
{
	scopeOf("ContextButtonController").SetContextPage($page);
}




$(document).ready(function()
{
	if(!InXNA())
	{
		SetContextPage({
			"ContextButtons": [
			{
				"ImportantValues": {
					"Cost": "200 minerals",
					"HP": "50",
					"Production Rate": "5 pps",
					"Max Power": "80"
				},
				"Name": "Solar Station",
				"Image": "images/SolarStation.png",
				"Slot": 1
			},
			{
				"ImportantValues": {
					"Cost": "50 minerals",
					"HP": "50"
				},
				"Name": "Power Node",
				"Image": "images/PowerNode.png",
				"Slot": 2
			},
			{
				"ImportantValues": {
					"Cost": "150 minerals",
					"HP": "150",
					"Mining Rate": "5.76 mps"
				},
				"Name": "Laser Miner",
				"Image": "images/LaserMiner.png",
				"Slot": 3
			},
			{
				"ImportantValues": {
					"Cost": "150 minerals",
					"HP": "50",
					"Max Power": "200"
				},
				"Name": "Battery",
				"Image": "images/Battery.png",
				"Slot": 3
			},
			{
				"ImportantValues": {
					"Cost": "150 minerals",
					"HP": "150",
					"Damage": "15 dps",
					"Range": "150"
				},
				"Name": "Laser Tower",
				"Image": "images/LaserTower.png",
				"Slot": 4
			},
			{
				"ImportantValues": {
					"Cost": "200 minerals",
					"HP": "100",
					"Damage": "16.6 dps",
					"Range": "300"
				},
				"Name": "Missile Tower",
				"Image": "images/MissileTower.png",
				"Slot": 5
			}
			],
			"Name": "Main"
		});
	}
});
