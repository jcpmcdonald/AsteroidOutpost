

function MissionController($scope)
{
	$scope.missions = [  ];
	
	$scope.AddMission = function($key, $description, $progress, $done)
	{
		$scope.missions.push({key:$key, description:$description, progress:$progress, done:$done});
		$scope.$apply();
	}
}

