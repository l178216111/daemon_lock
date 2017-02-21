var app=angular.module('myApp',[]);
app.config(function($httpProvider){
	$httpProvider.interceptors.push('myInterceptor');
});
app.factory('myInterceptor', ["$rootScope", function ($rootScope) {  
 var timestampMarker = {  
	 request: function (config) { 
		 $rootScope.loading = true;  
		 return config;  
	 },  
	 response: function (response) {  
		$rootScope.loading = false;  
		 return response;  
	 }  
 };  
 return timestampMarker;  
}]);
app.directive('datetime', function () {
		return {
				restrict: 'A',
				link: function($scope,element, attr) {
						//console.log($(element));
						$(element).datetimepicker({
							minView: 'month',
							autoclose: 1,
							format:'yyyy-mm-dd',
							todayHighlight: 1,
							startView: 2,
						});

				}
		};
})
app.directive('hightchar', function () {
                return {
						restrict: 'A',
						link: function($scope,element, attr) {
						$scope.highchart={
							'id':'',
							'options':''
						}
						$scope.highchart.id=attr.id;
						$scope.highchart.options= {
						chart: {
                                                                type: 'bar',
                                                                width: 1290,
                                                                height: 480
                                                        },
                                                        title: {
                                                                text: 'Engineer Mode Status'
                                                        },
                                                        credits : {
                                                                        enabled : false
                                                        },
                                                        xAxis:{
                                                                categories:[]
                                                        },
                                                        yAxis: {
                                                                min: 0,
                                                                reversedStacks : false,
								categories: ['8:00','9:00', '10:00', '11:00', '12:00','13:00','14:00','15:00','16:00','17:00','18:00','19:00','20:00','21:00','22:00','23:00','0:00','1:00','2:00','3:00','4:00','5:00','6:00','7:00','8:00','9:00'],

                                                                title: {
                                                                        text: ''
                                                                }
                                                        },
                                                        tooltip:{
                                                                        formatter:function() {
                                                                                        var a = parseInt(this.y * 3600);
                                                                                        var h = parseInt(a / 3600);
                                                                                        var m = parseInt((a - h * 3600) / 60);
                                                                                        var s = (a - h * 3600) % 60;
                                                                                        var c = "";
                                                                                        if (h > 0) {
                                                                                                        c += h.toString() + "hours";
                                                                                        }
                                                                                        if (m > 0) {
                                                                                                        c += m.toString() + "mins";
                                                                                        }
                                                                                        if (s > 0) {
                                                                                                        c += s.toString() + "sec"
                                                                                        }
                                                                                        var stack=this.series.userOptions.stack;
                                                                                        var name=this.series.name;
                                                                                        if (stack == 'mode'){
                                                                                        var info=[];
                                                                                        info=name.split(',');
                                                                                        return 'Equiment:' + this.x + '<br/>' +
                                                                                                'User:'+info[0]+'<br/>' +
                                                                                                'Scope:'+info[1]+'<br/>' +
                                                                                                'Part:'+info[2]+'<br/>' +
                                                                                                'Lot:'+info[3]+'<br/>' +
                                                                                                'Wafer:'+info[4]+'<br/>' +
                                                                                                'Pass:'+info[5]+'<br/>' +
                                                                                                'Last:'+c+ '</b>';
                                                                                        }else{
                                                                                        return 'Equiment:' + this.x + '<br/>' +
                                                                                                'Status:' + name + '<br/>' +
                                                                                                'Last:'+c+ '</b>';
                                                                                        }
                                                                        text: ''
                                                                }
                                                        },
                                                        tooltip:{
                                                                        formatter:function() {
                                                                                        var a = parseInt(this.y * 3600);
                                                                                        var h = parseInt(a / 3600);
                                                                                        var m = parseInt((a - h * 3600) / 60);
                                                                                        var s = (a - h * 3600) % 60;
                                                                                        var c = "";
                                                                                        if (h > 0) {
                                                                                                        c += h.toString() + "hours";
                                                                                        }
                                                                                        if (m > 0) {
                                                                                                        c += m.toString() + "mins";
                                                                                        }
                                                                                        if (s > 0) {
                                                                                                        c += s.toString() + "sec"
                                                                                        }
                                                                                        var stack=this.series.userOptions.stack;
                                                                                        var name=this.series.name;
                                                                                        if (stack == 'mode'){
                                                                                        var info=[];
                                                                                        info=name.split(',');
                                                                                        return 'Equiment:' + this.x + '<br/>' +
                                                                                                'User:'+info[0]+'<br/>' +
                                                                                                'Scope:'+info[1]+'<br/>' +
                                                                                                'Part:'+info[2]+'<br/>' +
                                                                                                'Lot:'+info[3]+'<br/>' +
                                                                                                'Wafer:'+info[4]+'<br/>' +
                                                                                                'Pass:'+info[5]+'<br/>' +
                                                                                                'Last:'+c+ '</b>';
                                                                                        }else{
                                                                                        return 'Equiment:' + this.x + '<br/>' +
                                                                                                'Status:' + name + '<br/>' +
                                                                                                'Last:'+c+ '</b>';
                                                                                        }

                                                                        }
                                                        },
                                                        legend: {
                                                                enabled: false
                                                        },
                                                        plotOptions: {
                                                                series: {
                                                                        stacking: 'normal'
                                                                },
                                                                column: {
                                                                        stacking: 'normal'
                                                                }

                                                        },
                                                        series:[]
                                                };

                     }
                };
})
app.controller('index',function($scope,$http){
		var chart_height;
		$scope.submit=function(){
			highchart($scope.idate);
		}
		$scope.volume=function(volume){
			if (volume == '+'){
				chart_height*=1.2;
				$scope.chart.setSize(1290,chart_height);	
			}else if (volume == '-'){
				chart_height*=0.8;
				$scope.chart.setSize(1290,chart_height);
			}
		}
		var highchart=function(time){
                                $http({
                                                method:"post",
                                                url:"/cgi-bin/Daemon_lock/index.pl",
                                                headers: {
                                                          'Content-Type':'application/x-www-form-urlencoded;charset=UTF-8'
                                                },
                                                transformRequest: function (data) {
                                                        return $.param(data);
                                                },
                                                data:{
                                                        'time':time,
                                                }
                                                })
                                .success(function (data,hearer,config,status) {
					$scope.highchart.options.xAxis.categories=data.xAxis;
					$scope.highchart.options.series=data.series;
//default every host height 80px;
					var chart_xAxis=data.xAxis.length;
					chart_height=480;
					if (chart_height < chart_xAxis *80){
						chart_height=chart_xAxis * 80;
					}
					$scope.highchart.options.chart.height=chart_height;
					$scope.chart=Highcharts.chart($scope.highchart.id,$scope.highchart.options);
                                })
	}
})
