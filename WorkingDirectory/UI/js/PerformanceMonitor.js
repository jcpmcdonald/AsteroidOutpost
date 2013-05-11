



var updateTimes = [[0,100]]; //[[0,10], [1,20], [2,15], [3,25]];
var drawTimes = [[0,0]]; //[[0,11], [1,25], [2,30], [3,28]];
var timesIndex = 0;
var plot1b = null;

var recentMaxUpdate = 0;
var recentMaxDraw = 0;
var refreshCounter = 0;



$(document).ready(function(){
	
	plot1b = $.jqplot('performanceGraph',[drawTimes, updateTimes],{
		title: 'Performance',
		stackSeries: true,
		showMarker: true,
		seriesDefaults: {
			fill: true
		},
		grid: {
			drawBorder: false,
			shadow: false,
			background: '#646464'
		},
		axes: {
			color: 'White',
			xaxis: {
				pad: 1,
				ticks: [0, 50, 100],
				tickOptions:{ showLabel: false }
			},
			yaxis: {
				pad: 1,
				min: 0,
				tickOptions:{ formatString: '%dms' }
			}
		},
		series: [
			{
				label: "Draw Time",
			},
			{
				label: "Update Time",
			}
		],
        legend: {
            show: true,
            renderer: $.jqplot.EnhancedLegendRenderer,
            rendererOptions: {
                numberRows: 1
            },
            placement: 'outsideGrid',
            location: 's'
        },
	});
});

function RefreshPerformanceGraph(updateTime, drawTime)
{
	// Record the max of the past few frames
	if(updateTime > recentMaxUpdate){
		recentMaxUpdate = updateTime;
	}
	if(drawTime > recentMaxDraw){
		recentMaxDraw = drawTime;
	}
	refreshCounter = refreshCounter + 1;
	
	if(refreshCounter % 4 == 0)
	{
		updateTimes[timesIndex] = [timesIndex, recentMaxUpdate];
		drawTimes[timesIndex] = [timesIndex, recentMaxDraw];
		
		// Make a little V where we are currently updating
		updateTimes[timesIndex + 1] = [timesIndex + 1, 0];
		drawTimes[timesIndex + 1] = [timesIndex + 1, 0];
		
		timesIndex++;
		
		recentMaxUpdate = 0;
		recentMaxDraw = 0;
		
		if(timesIndex > 100){
			timesIndex = 0;
		}
	}
	
	
	if(refreshCounter % 4 == 0)
	{
		if(typeof plot1b != 'undefined'){
			// This is the fastest and most efficient way I could find to update the series data
			plot1b.series[0].data = drawTimes;
			plot1b.series[1].data = updateTimes;
			
			plot1b.axes.yaxis.resetScale();
			
			//var start = new Date().getTime();
			// From testing, this can take like 10-20ms on my PC
			plot1b.replot();
			//console.log(new Date().getTime() - start);
		}
	}
}

