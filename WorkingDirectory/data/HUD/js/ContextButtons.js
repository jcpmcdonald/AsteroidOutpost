

function ContextButtonController($scope)
{
	$scope.buttons = [];
	
	
	$scope.SetButtons = function(data)
	{
		if (typeof data === 'string')
		{
			data = eval(data);
		}
		$scope.buttons = data;
	};
	
	$scope.AddButton = function(buttonData)
	{
		$scope.buttons.push(buttonData);
	};
	
	
	$(document).ready(function()
	{
		// Add a call attribute to all the construction buttons dynamically
		// $(".constructionButton").each( function()
		// {
			// $(this).attr("call", "hud.Build" + this.id + "()");
		// });
		
		
		$( document ).tooltip({
			track: true,
			show: false,
			hide: false,
			items: ".contextButton",
			content: function(){
				var element = $(this);
				var index = element.attr("index");
				if(typeof $scope.buttons[index] != 'undefined')
				{
					var rv = "<h3 class='buttonHoverTitle'>" + $scope.buttons[index]["$Name"] + "</h3>";
					for(dataPoint in $scope.buttons[index])
					{
						if(dataPoint.indexOf("$") != 0)
						{
							rv += "<b>" + dataPoint + ":</b>&nbsp" + $scope.buttons[index][dataPoint] + "<br/>";
						}
					}
					return rv;
				}
				else
				{
					return "not found";
				}
			}
		});
		
	});
}


function SetContextButtons($buttonList)
{
	scopeOf("ContextButtonController").SetButtons($buttonList);
}




$(document).ready(function()
{
	if(!InXNA())
	{
		SetContextButtons([
			{
				"$Name" : "Solar Station",
				"$Image" : "images/SolarStation.png",
				"Cost" : "200 minerals",
				"HP" : 50,
				"Production Rate" : "5 power/sec",
				"Max Power" : "80 power",
			},
			{
				"$Name" : "Laser Miner",
				"$Image" : "images/LaserMiner.png",
				"Cost" : "150 minerals",
				"HP" : 150,
				"Mining Rate" : "5.76 minerals/sec",
			},
			{
				"$Name" : "Power Node",
				"$Image" : "images/PowerNode.png",
				"Cost" : "50 minerals",
				"HP" : 50,
			},
			{
				"$Name" : "Laser Tower",
				"$Image" : "images/LaserTower.png",
				"Cost" : "150 minerals",
				"HP" : 150,
				"Damage" : "15 dps",
				"Range" : 150,
			},
			{
				"$Name" : "Missile Tower",
				"$Image" : "images/MissileTower.png",
				"Cost" : "200 minerals",
				"HP" : 100,
				"Damage" : "16.6 dps",
				"Range" : 300,
			},
		]);
	}
});
