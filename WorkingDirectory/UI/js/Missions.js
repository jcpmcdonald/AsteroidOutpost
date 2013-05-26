

function MissionController($scope)
{
	$scope.missions = {};
	
	$scope.AddMission = function($key, $description, $done)
	{
		$scope.missions[$key] = {description:$description, done:$done};
		$scope.$apply();
	}
	
	$scope.UpdateMission = function($key, $description, $done)
	{
		$scope.missions[$key] = {description:$description, done:$done};
		$scope.$apply();
	}
}


function AddMision($key, $description, $done)
{
	scopeOf("MissionController").AddMission($key, $description, $done);
}
