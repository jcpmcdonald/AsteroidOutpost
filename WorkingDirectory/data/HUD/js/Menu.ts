$(document).ready(function()
{
	$(".fit").fitText(1.3);
	
	$("a:not(.disabled)").click(
		function()
		{
			XNACall($(this).attr("call"));
			// if(typeof $(this).attr("href") !== 'undefined')
			// {
				// window.location = $(this).attr("href");
			// }
		}
	);
	
});
