

var TEST_selectionData =
		{
			"name": "Solar Station",
			"health": 100,
			"maxhealth": 150,
			"level": 1,
			"team": "Team1"
		};


function selectionChanged(newSelection)
{
	$("#selectionTitle").text(newSelection["name"]);
	$("#health").text(newSelection["health"] + " / " + newSelection["maxhealth"]);
	$("#level").text(newSelection["level"]);
	$("#team").text(newSelection["team"]);
}


function timerTest()
{
	TEST_selectionData["health"] -= 1;
	selectionChanged(TEST_selectionData);
	//setTimeout("timerTest()", 100);
}



$(document).ready(function()
{
	
	$(".button").mousedown( function(event) {
		$(this).addClass("buttonPressed");
	});
	
	$(".button").mouseup( function(event) {
		$(this).removeClass("buttonPressed");
	});
	
	$(".button").mouseleave( function(event) {
		$(this).removeClass("buttonPressed");
	});
	
	//selectionChanged(TEST_selectionData);
	//setTimeout("timerTest()", 100);
});
