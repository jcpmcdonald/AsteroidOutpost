
function ScenarioController($scope)
{
	$scope.AvailableScenarios = [];
	
	$scope.AddScenario = function($name)
	{
		$scope.AvailableScenarios.push({"name": $name});
		$scope.$apply();
		$(".fit").fitText(1.3);
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
