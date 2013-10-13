
function ScenarioController($scope)
{
	$scope.AvailableScenarios = [];
	
	$scope.AddScenario = function($name)
	{
		$scope.AvailableScenarios.push({"name": $name});
		$scope.$apply();
		
		$("a:not(.disabled)").click( function()
		{
			XNACall($(this).attr("call"));
		});
	}
}

var AddScenario = function(name)
{
	scopeOf("ScenarioController").AddScenario(name);
}



$(document).ready(function()
{
	if(!InXNA())
	{
		AddScenario("World1");
	}
});
