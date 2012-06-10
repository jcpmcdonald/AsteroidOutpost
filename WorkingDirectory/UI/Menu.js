
var selectionMarkerWidth = 12;
var selectionMarkerHeight = 12;

var selectedItem = null;

function changeSelectionTo(menuItem)
{
	if(selectedItem != null)
	{
		selectedItem.removeClass("selected");
	}
	menuItem.addClass("selected");
	selectedItem = menuItem;
	
	// Move the selection arrow around
	var selectionMarker = $("#selectionMarker");
	selectionMarker.css("left", (menuItem.offset().left - selectionMarkerWidth) + "px").show();
	//selectionMarker.css("left", "10px");
	selectionMarker.css("top", (menuItem.offset().top + (menuItem.outerHeight() / 2) - (selectionMarkerHeight / 2)) + "px").show();
}

function selectionMade(menuItem)
{
	if (typeof xna !== 'undefined' &&
		typeof menuItem.attr("call") !== 'undefined')
	{
		eval(menuItem.attr("call"));
	}
	if(typeof menuItem.attr("href") !== 'undefined')
	{
		window.location = menuItem.attr("href");
	}
}



$(document).ready(function()
{
	// do stuff when DOM is ready
	
	changeSelectionTo($("h2:first"));
	$("#selectionMarker").addClass("followSelection");
	
	
	$("h2").click(
		function()
		{
			selectionMade($(this));
		}
	);
	
	$("h2").mouseenter(
		function()
		{
			changeSelectionTo($(this));
		}
	);
	
	
	window.onresize = function(event){
		changeSelectionTo(selectedItem);
	};
	
	
	$(document).keydown(
		function(event)
		{
			if(event.keyCode == KEY_UP)
			{
				prevItem = selectedItem.prevAll("h2:first");
				if(prevItem.length > 0)
				{
					changeSelectionTo(prevItem);
				}
			}
			else if(event.keyCode == KEY_DOWN)
			{
				nextItem = selectedItem.nextAll("h2:first");
				if(nextItem.length > 0)
				{
					changeSelectionTo(nextItem);
				}
			}
			else if(event.keyCode == KEY_ENTER)
			{
				selectionMade(selectedItem);
			}
		}
	);
});
