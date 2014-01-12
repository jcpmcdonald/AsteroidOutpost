

var EditorMode = true;
var $currentSelection = null;

function UpdateEditor(newSelection)
{
	if(EditorMode)
	{
		
		if(newSelection == null || newSelection.length == 0)
		{
			$currentSelection = newSelection;
			
			// Remove the editor panel if we have nothing selected
			// if($("#editorPanel").is('*'))
			// {
				// $("#editorPanel").remove();
				// $("#editorToggleButton").remove();
				// return;
			// }
			if($("#editorPanel").attr("role") == "tablist")
			{
				$("#editorPanel").accordion("destroy");
			}
			$("#editorPanel").empty();
		}
		else
		{
			var entityID = newSelection[0].EntityID;
			var editorPanel;
			var selection;
			if($currentSelection != null && $currentSelection.length == 1 && entityID == $currentSelection[0].EntityID)
			{
				// We are looking at the same entity, process the differences
				selection = Diff(newSelection, $currentSelection);
				if(selection == null)
				{
					return;
				}
				else
				{
					selection = selection[0];
				}
			}
			else
			{
				// This is a new selection, clear out the old data then re-populate
				// $("#editorPanel").remove();
				// $("#editorToggleButton").remove();
				if($("#editorPanel").attr("role") == "tablist")
				{
					$("#editorPanel").accordion("destroy");
				}
				$("#editorPanel").empty();
				selection = newSelection[0];
			}
			
			editorPanel = GetOrCreate('editorPanel', $('body'), '<div id="editorPanel" class="panel"></div>');
			$currentSelection = newSelection;
			
			for(component in selection)
			{
				if(component == "EntityID")
				{
					// Ignore
					continue;
				}
				
				// For each component, create a section
				var componentHeader = GetOrCreate(entityID + '-' + component + '-header', editorPanel, '<h3 id="' + entityID + '-' + component + '-header">' + component + '</h3>');
				var componentBody = GetOrCreate(entityID + '-' + component + '-body', editorPanel, '<div id="' + entityID + '-' + component + '-body"></div>');
				
				for(dataPoint in selection[component])
				{
					if(dataPoint == "GUID")
					{
						// Ignore GUIDs, these don't need to be displayed
						continue;
					}
					
					var baseID = entityID + '-' + component + '-' + dataPoint;
					var dataLabel = GetOrCreate(baseID + '-label', componentBody, '<div id="' + baseID + '-label" class="editorLabel">' + dataPoint + '</div>');
					
					switch(typeof(selection[component][dataPoint]))
					{
					case "string":
						var textField = GetOrCreate(baseID + '-text', componentBody, '<input type="text" id="' + baseID + '-text">');
						CreateTextEditor(textField, selection[component][dataPoint], entityID, component, dataPoint);
						break;
						
					case "number":
						CreateNumberEditor(componentBody, dataLabel, baseID, selection[component][dataPoint], entityID, component, dataPoint);
						break;
						
					case "boolean":
						CreateCheckboxEditor(componentBody, dataLabel, baseID, selection[component][dataPoint], entityID, component, dataPoint);
						break;
					}
				}
			}
			
			editorPanel.accordion({ icons: false });
			//editorPanel.append('<input type="button" value="Apply">');
		}
	}
}

function EntityEditorOnShow()
{
	if($("#editorPanel").is(':visible') && $currentSelection != null)
	{
		$("#editorPanel").accordion("destroy");
		$("#editorPanel").accordion({ icons: false });
	}
}

function GetOrCreate(name, appendTo, value)
{
	var rv = $("#" + name);
	if(!rv.is('*'))
	{
		appendTo.append(value);
		rv = $("#" + name);
		if(!rv.is('*'))
		{
			console.log(name);
		}
	}
	return rv;
}


function CreateTextEditor(textField, value, entityID, component, dataPoint)
{
	textField.val(value);
	textField.on("input", function(e)
	{
		var updatedData = { "EntityID": entityID };
		updatedData[component] = {};
		updatedData[component][dataPoint] = $(this).val();
		console.log(JSON.stringify(updatedData));
		if(InXNA())
		{
			hud.EditEntity(JSON.stringify(updatedData));
		}
	});
}


function CreateNumberEditor(componentBody, dataLabel, baseID, value, entityID, component, dataPoint)
{
	var sliderText = GetOrCreate(baseID + '-slider-text', dataLabel, ': <input type="text" id="' + baseID + '-slider-text" style="border:0; color:#f6931f; font-weight:bold; background-color:transparent; width: 150px;">');
	CreateTextEditor(sliderText, value, entityID, component, dataPoint)
	
	var newSlider = false;
	var slider = $('#' + baseID + '-slider');
	if(!slider.is('*'))
	{
		componentBody.append("<div id='" + baseID + "-slider'></div>");
		newSlider = true;
	}
	
	var max = goodSliderMax(value);
	var step = goodSliderStep(max);
	
	
	if(newSlider)
	{
		sliderText.val(value);
		
		$("#" +  baseID + "-slider").slider({
			range: "min",
			value: value,
			min: 0,
			max: max,
			step: step,
			slide: function(event, ui) {
				$("#" + event.target.id + "-text").val(ui.value);
				var updatedData = { "EntityID": entityID };
				updatedData[component] = {};
				updatedData[component][dataPoint] = ui.value;
				//console.log(JSON.stringify(updatedData));
				if(InXNA())
				{
					hud.EditEntity(JSON.stringify(updatedData));
				}
			},
			stop: resizeSlider
		});
		
		//event.target
		// ui.value
		//$("#" +  baseID + "-slider").on("slide", function(event, ui){ console.log(ui); });
	}
	else
	{
		// Don't update the slider if the user is dragging it
		if(!$("#" +  baseID + "-slider > .ui-slider-handle").hasClass("ui-state-focus"))
		{
			sliderText.val(value);
			$("#" +  baseID + "-slider").slider("option", { value: value, max: max, step: step });
		}
	}
}

function goodSliderMax(currentValue)
{
	return Math.max(2, currentValue * 2);
}

function goodSliderStep(max)
{
	var step = max / 100;
	
	if(step < 0.15){
		step = 0.1;
	}else if(step < 0.25){
		step = 0.2;
	}else if(step < 0.8){
		step = 0.5;
	}else{
		step = 1;
	}
	return step;
}

function resizeSlider(event, ui)
{
	var max = goodSliderMax(ui.value);
	
	$(this).slider( "option", "max", max );
	$(this).slider( "option", "step", goodSliderStep(max) );
}



function CreateCheckboxEditor(componentBody, dataLabel, baseID, value, entityID, component, dataPoint)
{
	var checkbox = GetOrCreate(baseID + '-checkbox', dataLabel, ': <input type="checkbox" id="' + baseID + '-checkbox">');
	checkbox.prop('checked', value);
	checkbox.change(function()
	{
		var updatedData = { "EntityID": entityID };
		updatedData[component] = {};
		updatedData[component][dataPoint] = $(this).prop('checked');
		//console.log(JSON.stringify(updatedData));
		if(InXNA())
		{
			hud.EditEntity(JSON.stringify(updatedData));
		}
	});
}



function FirstProperty(obj)
{
	var rv;
	for(rv in obj)
	{
		return rv;
	}
}


