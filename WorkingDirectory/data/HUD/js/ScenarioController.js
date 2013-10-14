
function ScenarioController($scope)
{
	$scope.AvailableScenarios = [];
	
	$scope.AddScenario = function($name)
	{
		$scope.AvailableScenarios.push({"name": $name});
		$scope.$apply();
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
