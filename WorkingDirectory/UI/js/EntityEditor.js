

var EditorMode = true;
var currentSelection = null;

function UpdateEditor(newSelection)
{
	if(EditorMode)
	{
		if(newSelection == null)
		{
			if($("#editorPanel").is('*'))
			{
				$("#editorPanel").remove();
				return;
			}
		}
		
		var editorPanel = GetOrCreate('editorPanel', $('body'), '<div id="editorPanel" class="panel"></div>');
		
		var entityID = FirstProperty(newSelection);
		if(entityID != null)
		{
			var selection;
			if(entityID == FirstProperty(newSelection))
			{
				// We are looking at the same entity, process the differences
				selection = Diff(newSelection, currentSelection);
			}
			else
			{
				// This is a new selection, clear out the old data then re-populate
				//editorPanel.empty();
				selection = newSelection;
			}
			
			for(component in selection[entityID])
			{
				// For each component, create a section
				var componentHeader = GetOrCreate(entityID + '-' + component + '-header', editorPanel, '<h3 id="' + entityID + '-' + component + '-header">' + component + '</h3>');
				var componentBody = GetOrCreate(entityID + '-' + component + '-body', editorPanel, '<div id="' + entityID + '-' + component + '-body"></div>');
				
				for(dataPoint in selection[entityID][component])
				{
					if(dataPoint == "GUID")
					{
						// Ignore GUIDs, these don't need to be displayed
						continue;
					}
					
					var baseID = entityID + '-' + component + '-' + dataPoint;
					var dataLabel = GetOrCreate(baseID + '-label', componentBody, '<div id="' + baseID + '-label" class="editorLabel">' + dataPoint + '</div>');
					
					switch(typeof(selection[entityID][component][dataPoint]))
					{
					case "string":
						componentBody.append('<div><input type="text" id="' + baseID + '-text" value="' + selection[entityID][component][dataPoint] + '"></div>');
						break;
						
					case "number":
						CreateNumberEditor(componentBody, $("#" + baseID + "-label"), baseID, selection[entityID][component][dataPoint]);
						break;
						
					case "boolean":
						$("#" + baseID + "-label").append(': <input type="checkbox">');
						
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



function CreateNumberEditor(componentBody, componentLabel, baseID, value)
{
	componentLabel.append(': <input type="text" id="' + baseID + 'SliderText" style="border:0; color:#f6931f; font-weight:bold; background-color:transparent" value="' + value + '">');
	
	componentBody.append("<div id='" + baseID + "Slider'></div>");
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
	$("#" +  baseID + "Slider").slider({
		range: "min",
		value: value,
		min: 0,
		max: max,
		step: step,
		slide: function( event, ui ) {
			$("#" + event.target.id + "Text").val(ui.value);
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
