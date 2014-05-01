"use strict";

var currentRow = -1;

function SelectRow(newRowID)
{

    if (currentRow === newRowID) {
        return;
    }

    if (currentRow !== -1) {
        var oldrow = document.getElementById("row_" + currentRow);
        oldrow.style.background = "transparent";
    }

    var newRow = document.getElementById("row_" + newRowID);
    newRow.style.background = "#AAF";

    currentRow = newRowID;
}

function IsSelected() {
    return currentRow === -1 ? false : true;
}

function GetSelectedRow() {
    return currentRow;
}


$(document).keydown(
	function(event)
	{
		switch(event.keyCode)
		{
		case KEY_ENTER:
		case KEY_ESC:
		case KEY_SPACE:
			history.back();
		}
	}
)
