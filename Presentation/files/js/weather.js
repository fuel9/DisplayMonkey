// 2015-03-08 [LTL] - weather BEGIN

/*********************************************************
Yahoo weather JSON sample: {
	"forecast" : [{
			"day" : "Mon",
			"date" : "9 Mar 2015",
			"low" : "49",
			"high" : "59",
			"text" : "Rain",
			"code" : "12"
		}, {
			"day" : "Tue",
			"date" : "10 Mar 2015",
			"low" : "56",
			"high" : "70",
			"text" : "Rain",
			"code" : "12"
		}, {
			"day" : "Wed",
			"date" : "11 Mar 2015",
			"low" : "52",
			"high" : "66",
			"text" : "Rain",
			"code" : "12"
		}, {
			"day" : "Thu",
			"date" : "12 Mar 2015",
			"low" : "57",
			"high" : "70",
			"text" : "Rain",
			"code" : "12"
		}, {
			"day" : "Fri",
			"date" : "13 Mar 2015",
			"low" : "54",
			"high" : "64",
			"text" : "Rain",
			"code" : "12"
		}
	],
	"location" : {
		"city" : "Lebanon",
		"region" : "TN",
		"country" : "United States"
	},
	"units" : {
		"temperature" : "F",
		"distance" : "mi",
		"pressure" : "in",
		"speed" : "mph"
	},
	"wind" : {
		"chill" : "57",
		"direction" : "140",
		"speed" : "5"
	},
	"atmosphere" : {
		"humidity" : "69",
		"visibility" : "10",
		"pressure" : "30.14",
		"rising" : "2"
	},
	"astronomy" : {
		"sunrise" : "7:05 am",
		"sunset" : "6:47 pm"
	},
	"condition" : {
		"text" : "Light Rain",
		"code" : "11",
		"temp" : "57",
		"date" : "Mon, 09 Mar 2015 5:51 pm CDT"
	}
}
*********************************************************/

var Weather = Class.create(PeriodicalExecuter, {
    initialize: function ($super, options) {
        "use strict";
        this.exiting = false;
        this.updating = false;
        this.finishedLoading = false;
        this.div = options.div;
        this.frameId = options.data.FrameId || 0;
        this.panelId = options.panelId || 0;
        if (!this.div || !this.panelId || !this.frameId)
            return;
        this.tempU = options.data.TemperatureUnit || _canvas.temperatureUnit;
        this.woeid = options.data.Woeid || _canvas.woeid;
        //this.div.hide();
        //$super(this._callback, 60);

        this._callback(null);
    },

    stop: function () {
        this.exiting = true;
    },

    isReady: function () {
        "use strict";
        return !!this.finishedLoading;
    },

    _callback: function (pe) {
        "use strict";
        if (this.exiting || this.updating)
            return;

        this.updating = true;
        new Ajax.Request("getYahooWeather.ashx", {
            method: 'get',
            parameters: $H({
                frame: this.frameId,
                panel: this.panelId,
                display: _canvas.displayId,
                woeid: this.woeid,
                tempU: this.tempU
            }),
            evalJSON: false,
            weather: this,

            onSuccess: function (resp) {
                var weather = resp.request.options.weather;
                try {
                    if (weather.exiting)
                        return;

                    var json = null;
                    if (resp.responseText.isJSON())
                        json = resp.responseText.evalJSON();
                    if (!json)
                        throw new Error("JSON expected"); // <-- shouldn't get here

                    if (json.Error)
                        throw new Error(json.Error);

                    weather.div.select('.condition_code').each(function(e) { e.src = "files/weather/" + json.condition.code + ".gif"; });
                    weather.div.select('.location_city').each(function(e) { e.update(json.location.city); });
                    weather.div.select('.location_region').each(function(e) { e.update(json.location.region); });
                    weather.div.select('.location_country').each(function(e) { e.update(json.location.country); });
                    weather.div.select('.condition_text').each(function(e) { e.update(json.condition.text); });
                    weather.div.select('.condition_temp').each(function(e) { e.update(json.condition.temp); });
                    weather.div.select('.units_temperature').each(function (e) { e.update(json.units.temperature); });
                }
                catch (e) {
                    new ErrorReport({ exception: e, data: resp.responseText, source: "Weather::callBack::onSuccess" });
                }
                finally {
                    weather.updating = false;
                    //console.log(resp.responseText);
                    weather.finishedLoading = true;
                }
            },

            onFailure: function (resp) {
                new ErrorReport({ exception: resp, source: "Weather::callBack::onFailure" });
                resp.request.options.weather.finishedLoading = true;
            },
        });
    },
});

// weather END
