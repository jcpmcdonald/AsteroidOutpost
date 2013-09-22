

function ContextButtonController($scope)
{
	$scope.buttons = [];
	
	
	$scope.SetContextPage = function(data)
	{
		if (typeof data === 'string')
		{
			data = eval("(" + data + ")");
		}
		data["ContextButtons"].sort(function(a,b){ return a.Slot - b.Slot })
		
		$scope.buttons = [];
		$.each(data["ContextButtons"], function(index, button){
			while(button.Slot > $scope.buttons.length)
			{
				$scope.buttons.push({"Enabled" : false, "EnabledCSS" : "blank disabled"});
			}
			button.EnabledCSS = (typeof(button.Enabled) == 'undefined' || button.Enabled) ? "" : "disabled";
			$scope.buttons.push(button);
		});
		
		while($scope.buttons.length < 9)
		{
			$scope.buttons.push({"Enabled" : false, "EnabledCSS" : "blank disabled"});
		}
		
		$scope.InitTooltips();
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
			items: ".contextButton:not(.disabled):not(.blank)",
			content: function(){
				var element = $(this);
				var index = element.attr("index");
				if(typeof $scope.buttons[index] != 'undefined' && $scope.buttons[index] != null)
				{
					var rv = "<h3 class='buttonHoverTitle'>" + $scope.buttons[index]["Name"];
					
					if(typeof $scope.buttons[index]["HotkeyName"] != 'undefined' && $scope.buttons[index]["HotkeyName"] != null)
					{
						rv += "  (" + $scope.buttons[index]["HotkeyName"] + ")";
					}
					rv += "</h3>";
					
					if(typeof $scope.buttons[index]["Description"] != 'undefined' && $scope.buttons[index]["Description"] != null)
					{
						rv += "<p><em>" + $scope.buttons[index]["Description"] + "</em></p>";
					}
				
					for(dataPoint in $scope.buttons[index]["ImportantValues"])
					{
						//if(dataPoint.indexOf("$") != 0)
						//{
							rv += "<b>" + dataPoint + ":</b>&nbsp" + $scope.buttons[index]["ImportantValues"][dataPoint] + "<br />";
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
					"Slot": 0,
					"Enabled": false,
				},
				{
					"ImportantValues": {
						"Cost": "50 minerals",
						"HP": "50"
					},
					"Name": "Power Node",
					"Image": "images/PowerNode.png",
					"Slot": 1
				},
				{
					"ImportantValues": {
						"Cost": "150 minerals",
						"HP": "150",
						"Mining Rate": "5.76 mps"
					},
					"Name": "Laser Miner",
					"Image": "images/LaserMiner.png",
					"Slot": 2
				},
				{
					"ImportantValues": {
						"Cost": "150 minerals",
						"HP": "50",
						"Max Power": "200"
					},
					"Name": "Battery",
					"Image": "images/Battery.png",
					"Slot": 3,
					"HotkeyName" : "B",
					"Description" : "Stores power that you can use at a later date",
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
					"Slot": 6
				}
			],
			"Name": "Main"
		});
	}
});
