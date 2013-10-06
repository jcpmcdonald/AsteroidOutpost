

function MissionController($scope)
{
	$scope.missions = [];
	
	$scope.AddMission = function($key, $description, $done)
	{
		$scope.missions.push({key:$key, description:$description, done:$done});
		$scope.$apply();
	}
	
	$scope.RemoveMission = function($key)
	{
		var removeIndex = $scope.MissionIndex($key);
		if(removeIndex >= 0)
		{
			$scope.missions.splice(removeIndex, 1);
			$scope.$apply();
		}
		else
		{
			console.log("During mission removal, failed to find matching mission");
		}
	}
	
	$scope.UpdateMission = function($key, $description, $done)
	{
		$scope.missions[$scope.MissionIndex($key)] = {key:$key, description:$description, done:$done};
		$scope.$apply();
	}
	
	
	$scope.MissionIndex = function($key)
	{
		var rv = -1;
		$.each($scope.missions, function(index, value)
		{
			if(value.key == $key)
			{
				rv = index;
			}
		});
		return rv;
	}
}


function AddMission($key, $description, $done)
{
	scopeOf("MissionController").AddMission($key, $description, $done);
}

function UpdateMission($key, $description, $done)
{
	scopeOf("MissionController").UpdateMission($key, $description, $done);
}

function RemoveMission($key)
{
	scopeOf("MissionController").RemoveMission($key);
}
