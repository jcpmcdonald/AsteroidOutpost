
$username = "me";

function ConsoleController($scope)
{
	$scope.consoleLines = [ {username:'System', message:'Welcome to Asteroid Outpost!', timestamp:new Date()} ];
	
	$scope.addConsoleMessage = function()
	{
		// Execute javascript on request
		if($scope.consoleInput.toLowerCase().indexOf("/js ") == 0)
		{
			eval("(" + $scope.consoleInput.substr(4) + ")");
		}
		
		$scope.consoleLines.push({username:$username, message:$scope.consoleInput, timestamp:new Date() });
		$scope.consoleInput = "";
		
		setTimeout(function(){
			$("#history").scrollTop($("#history").prop("scrollHeight"))
		}, 0);
	}
}


var ToggleConsole = function(){ $("#console").toggle("slide", { direction:'up'}, 200); }

var SetConsoleVisible = function(visibility)
{
	if(visibility == true)
	{
		$("#console").show("slide", { direction:'up'}, 200);
	}
	else
	{
		$("#console").hide("slide", { direction:'up'}, 200);
	}
}



$(document).ready(function()
{
	$("#console").hide();
	
	if(!InXNA())
	{
		$(document).keydown(function(event)
		{
			if(event.keyCode === KEY_TILDE)
			{
				ToggleConsole();
			}
		});
	}
});
