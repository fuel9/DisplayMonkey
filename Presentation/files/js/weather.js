/*!
* DisplayMonkey source file
* http://displaymonkey.org
*
* Copyright (c) 2015 Fuel9 LLC and contributors
*
* Released under the MIT license:
* http://opensource.org/licenses/MIT
*/

// 2015-03-08 [LTL] - weather BEGIN
// 2015-05-08 [LTL] - ready callback
// 2016-03-27 [LTL] - fixed onSuccess

DM.Weather = Class.create(/*PeriodicalExecuter*/ DM.FrameBase, {
    initialize: function ($super, options) {
        "use strict";
        $super(options, 'div.weather');
        var data = options.panel.data;
        this.tempU = data.TemperatureUnit || _canvas.temperatureUnit;
        this.woeid = data.Woeid || _canvas.woeid;

        this.culture = _canvas.culture;
        this.location = data.Location;

        this._callback();
    },

    _callback: function () {
        "use strict";
        if (this.exiting || this.updating)
            return;

        this.updating = true;
        new Ajax.Request("getWeather.ashx", {
            method: 'get',
            parameters: $H({
                frame: this.frameId,
                panel: this.panelId,
                display: _canvas.displayId,
                woeid: this.woeid,
                tempU: this.tempU,
                culture: this.culture,
                location: this.location,
                latitude: _canvas.latitude,
                longitude: _canvas.longitude
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

                    if (json.current_observation) {
                        weather.div.select('.location_city').each(function (e) { e.update(json.current_observation.display_location.city); });
                        weather.div.select('.location_region').each(function (e) { e.update(json.current_observation.display_location.state); });
                        weather.div.select('.location_country').each(function (e) { e.update(json.current_observation.display_location.country); });
                        weather.div.select('.condition_code').each(function (e) { e.src = json.current_observation.icon_url; });
                        weather.div.select('.condition_text').each(function (e) { e.update(json.current_observation.weather); });
                        weather.div.select('.condition_temp').each(function (e) { e.update(json.current_observation.temp_c); });
                        weather.div.select('.wind_speed').each(function (e) { e.update(json.current_observation.wind_kph); });
                        weather.div.select('.wind_direction').each(function (e) { e.update(json.current_observation.wind_dir); });
                        weather.div.select('.units_temperature').each(function (e) { e.update("&#8451"); });
                        weather.div.select('.units_speed').each(function (e) { e.update("km/u"); });
                    }
                }
                catch (e) {
                    new DM.ErrorReport({ exception: e, data: resp.responseText, source: "Weather::callBack::onSuccess" });
                }
                finally {
                    weather.updating = false;
                    //console.log(resp.responseText);
                    weather.ready();
                }
            },

            onFailure: function (resp) {
                var weather = resp.request.options.weather;
                new DM.ErrorReport({ exception: resp, source: "Weather::callBack::onFailure" });
                weather.updating = false;
                weather.ready();
            },
        });
    },
});

// weather END
