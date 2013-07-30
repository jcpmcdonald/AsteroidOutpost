

var graph, options;

var times;
//var drawTimes = [];
//var drawDelayTimes = [];

var timeIndex = 0;
// var plot1b = null;

// var recentMaxUpdate = 0;
// var recentMaxDraw = 0;
// var recentMaxDrawDelay = 0;
// var refreshCounter = 0;



$(document).ready(function()
{
	offset = 2 * Math.PI * 10000;
	
	options = {
		colors: ['#00A8F0', '#C0D800', '#CB4B4B', '#4DA74D', '#9440ED'],
		title: "Performance",
		xaxis: {
			title: "seconds",
		},
		yaxis: {
			title: "ms",
		},
		grid: {
			color: '#FFFFFF',
		},
	};
	
	if(!InXNA())
	{
		times = [];
		times[0] = [[0,10], [1,20], [2,15], [3,25]];
		times[1] = [[0,11], [1,25], [2,30], [3,28]];
		times[2] = [[0,13], [1,22], [2,12], [3,19]];
		
		// updateTimes = [[0,10], [1,20], [2,15], [3,25]];
		// drawTimes = [[0,11], [1,25], [2,30], [3,28]];
		// drawDelayTimes = [[0,13], [1,22], [2,12], [3,19]];
		timeIndex = 4;
		
		graph = Flotr.draw($('#performanceGraph').get(0), [times[0], times[1], times[2]], options);
	}
	else
	{
		$("#performanceGraphPanel").hide();
	}
	
	DefinePerformancePlots(["update", "draw", "draw delay"]);
	
	
	//style='background-color: #00A8F0; width: 10px; height: 10px'></div>");
	
	// plot1b = $.jqplot('performanceGraph',[drawDelayTimes, drawTimes, updateTimes],{
	// 	title: 'Performance',
	// 	stackSeries: true,
	// 	showMarker: true,
	// 	seriesDefaults: {
	// 		fill: true
	// 	},
	// 	grid: {
	// 		drawBorder: false,
	// 		shadow: false,
	// 		background: '#646464'
	// 	},
	// 	axes: {
	// 		color: 'White',
	// 		xaxis: {
	// 			pad: 1,
	// 			ticks: [0, 50, 100],
	// 			tickOptions:{ showLabel: false }
	// 		},
	// 		yaxis: {
	// 			pad: 1,
	// 			min: 0,
	// 			tickOptions:{ formatString: '%dms' }
	// 		}
	// 	},
	// 	series: [
	// 		{
	// 			label: "Draw Delay",
	// 			color: "red",
	// 		},
	// 		{
	// 			label: "Draw Time",
	// 			color: "blue",
	// 		},
	// 		{
	// 			label: "Update Time",
	// 			color: "orange",
	// 		}
	// 	],
    //     legend: {
    //         show: true,
    //         renderer: $.jqplot.EnhancedLegendRenderer,
    //         rendererOptions: {
    //             numberRows: 1
    //         },
    //         placement: 'outsideGrid',
    //         location: 's'
    //     },
	// });
});

function DefinePerformancePlots(labels)
{
	for(i = 0; i < labels.length; i++)
	{
		$("#performanceLegend").append("<span style='white-space:nowrap; float:left;'><span id='legend" + i + "' class='legendColor'></span><span class='legendLabel'>" + labels[i] + "</span></span>");
		$("#legend" + i).css("background-color", options.colors[i]);
	}
}

function RefreshPerformanceGraph(timestamp, newTimes /*updateTime, drawTime, drawDelay*/)
{
	
	if(!$("#performanceGraphPanel").is(":visible"))
	{
		$("#performanceGraphPanel").show("slide");
	}
	
	
	// updateTimes[timeIndex] = [timeIndex, updateTime];
	// drawTimes[timeIndex] = [timeIndex, drawTime];
	// drawDelayTimes[timeIndex] = [timeIndex, drawDelay];
	// 
	// // Make a little V where we are currently updating
	// updateTimes[timeIndex + 1] = [timeIndex + 1, 0];
	// drawTimes[timeIndex + 1] = [timeIndex + 1, 0];
	// drawDelayTimes[timeIndex + 1] = [timeIndex + 1, 0];
	
	if(typeof times == 'undefined')
	{
		times = [];
		for(i = 0; i < newTimes.length; i++)
		{
			times[i] = [];
		}
	}
	
	timeIndex++;
	
	//if(timeIndex > 180){
	//	timeIndex = 0;
	//}
	
	if(timeIndex > 180)
	{
		for(i = 0; i < newTimes.length; i++)
		{
			times[i].shift();
		}
	}
	
	for(i = 0; i < newTimes.length; i++)
	{
		times[i].push([timestamp, newTimes[i]]);
	}
	
	
	graph = Flotr.draw(document.getElementById('performanceGraph'), times, options);
	
	// // Record the max of the past few frames
	// if(updateTime > recentMaxUpdate){
		// recentMaxUpdate = updateTime;
	// }
	// if(drawTime > recentMaxDraw){
		// recentMaxDraw = drawTime;
	// }
	// if(drawDelay > recentMaxDrawDelay){
		// recentMaxDrawDelay = drawDelay;
	// }
	// refreshCounter = refreshCounter + 1;
	// 
	// if(refreshCounter % 4 == 0)
	// {
		// updateTimes[timesIndex] = [timesIndex, recentMaxUpdate];
		// drawTimes[timesIndex] = [timesIndex, recentMaxDraw];
		// drawDelayTimes[timesIndex] = [timesIndex, recentMaxDrawDelay];
		// 
		// // Make a little V where we are currently updating
		// updateTimes[timesIndex + 1] = [timesIndex + 1, 0];
		// drawTimes[timesIndex + 1] = [timesIndex + 1, 0];
		// drawDelayTimes[timesIndex + 1] = [timesIndex + 1, 0];
		// 
		// timesIndex++;
		// 
		// recentMaxUpdate = 0;
		// recentMaxDraw = 0;
		// recentMaxDrawDelay = 0;
		// 
		// if(timesIndex > 100){
			// timesIndex = 0;
		// }
	// }
	// 
	// 
	// if(refreshCounter % 4 == 0)
	// {
		// if(typeof plot1b != 'undefined'){
			// // This is the fastest and most efficient way I could find to update the series data
			// plot1b.series[0].data = drawDelayTimes;
			// plot1b.series[1].data = drawTimes;
			// plot1b.series[2].data = updateTimes;
			// 
			// plot1b.axes.yaxis.resetScale();
			// 
			// //var start = new Date().getTime();
			// // From testing, this can take like 10-20ms on my PC
			// plot1b.replot();
			// //console.log(new Date().getTime() - start);
		// }
	// }
	return 0;
}

