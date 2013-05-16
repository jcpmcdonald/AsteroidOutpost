
$username = "me";

function ConsoleController($scope)
{
	$scope.consoleLines = [ {username:'System', message:'Welcome to Asteroid Outpost!', timestamp:new Date()} ];
	
	$scope.addConsoleMessage = function()
	{
		// Execute javascript on request
		if($scope.consoleInput.toLowerCase().indexOf("/js ") == 0)
		{
			eval($scope.consoleInput.substr(4));
		}
		
		$scope.consoleLines.push({username:$username, message:$scope.consoleInput, timestamp:new Date() });
		$scope.consoleInput = "";
		
		setTimeout(function(){
			$("#history").scrollTop($("#history").prop("scrollHeight"))
		}, 0);
	}
}

