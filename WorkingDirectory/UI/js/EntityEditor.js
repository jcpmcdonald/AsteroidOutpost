

var EditorMode = true;
var currentSelection = null;

function UpdateEditor(newSelection)
{
	if(EditorMode)
	{
		if(newSelection == null)
		{
			currentSelection = newSelection;
			if($("#editorPanel").is('*'))
			{
				$("#editorPanel").remove();
				return;
			}
		}
		
		var entityID = FirstProperty(newSelection);
		if(entityID != null)
		{
			var editorPanel;
			var selection;
			if(entityID === FirstProperty(currentSelection))
			{
				// We are looking at the same entity, process the differences
				selection = Diff(newSelection[entityID], currentSelection[entityID]);
			}
			else
			{
				// This is a new selection, clear out the old data then re-populate
				$("#editorPanel").remove();
				selection = newSelection[entityID];
			}
			
			editorPanel = GetOrCreate('editorPanel', $('body'), '<div id="editorPanel" class="panel"></div>');
			currentSelection = newSelection;
			
			for(component in selection)
			{
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
						textField.val(selection[component][dataPoint]);
						//value="' + selection[entityID][component][dataPoint] + '"
						//componentBody.append('<div><input type="text" id="' + baseID + '-text" value="' + selection[entityID][component][dataPoint] + '"></div>');
						break;
						
					case "number":
						CreateNumberEditor(componentBody, dataLabel, baseID, selection[component][dataPoint]);
						break;
						
					case "boolean":
						var checkbox = GetOrCreate(baseID + '-checkbox', dataLabel, ': <input type="checkbox" id="' + baseID + '-checkbox">');
						checkbox.prop('checked', selection[component][dataPoint]);
						//$("#" + baseID + "-label").append(': <input type="checkbox">');
						
						break;
					}
				}
			}
			
			editorPanel.accordion({ icons: false });
			//editorPanel.append('<input type="button" value="Apply">');
		}
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



function CreateNumberEditor(componentBody, dataLabel, baseID, value)
{
	var sliderText = GetOrCreate(baseID + '-slider-text', dataLabel, ': <input type="text" id="' + baseID + '-slider-text" style="border:0; color:#f6931f; font-weight:bold; background-color:transparent; width: 150px;">');
	sliderText.val(value);
	
	var newSlider = false;
	var slider = $('#' + baseID + '-slider');
	if(!slider.is('*'))
	{
		componentBody.append("<div id='" + baseID + "-slider'></div>");
		newSlider = true;
	}
	
	var max = value * 2;
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
	
	if(newSlider)
	{
		$("#" +  baseID + "-slider").slider({
			range: "min",
			value: value,
			min: 0,
			max: max,
			step: step,
			slide: function( event, ui ) {
				$("#" + event.target.id + "-text").val(ui.value);
			}
		});
	}
	else
	{
		$("#" +  baseID + "-slider").slider("option", { value: value, max: max, step: step });
	}
}


function FirstProperty(obj)
{
	var rv;
	for(rv in obj)
	{
		return rv;
	}
}
