
$username = "me";

function ConsoleController($scope)
{
	$scope.consoleLines = [ {username:'System', message:'Welcome to Asteroid Outpost!', timestamp:new Date()} ];
	
	$scope.consoleSubmit = function()
	{
		if($scope.consoleInput.trim() != "")
		{
			$scope.addConsoleMessage($scope.consoleInput, true);
		}
		$scope.consoleInput = "";
	}
	
	$scope.addConsoleMessage = function(text, scrollToNew)
	{
		if(scrollToNew === undefined)
		{
			scrollToNew = $("#history").height() + $("#history").scrollTop() >= $("#history").prop("scrollHeight");
		}
		
		// Execute javascript on request
		if(text.toLowerCase().indexOf("/js ") == 0)
		{
			eval("(" + text.substr(4) + ")");
		}
		else
		{
			$scope.consoleLines.push({username:$username, message:text, timestamp:new Date() });
			
			if(scrollToNew)
			{
				setTimeout(function(){
					$("#history").scrollTop($("#history").prop("scrollHeight"))
				}, 0);
			}
		}
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
