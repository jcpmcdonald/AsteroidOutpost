"use strict";

$(document).ready(function ()
{
	$("#creditsWrapper").css("top", $(window).height() + "px").css("top");
	$("#creditsWrapper").addClass("scroll");
	$("#creditsWrapper").css("top", -100 - $("#coreCredits").height());
	
	$(document).keydown(
		function (event)
		{
			switch (event.keyCode)
			{
			case KEY_ENTER:
			case KEY_ESC:
			case KEY_SPACE:
				history.back();
			}
		}
	);
	
	$(document).click(
		function ()
		{
			history.back();
		}
	);
});
