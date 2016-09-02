/*!
* DisplayMonkey source file
* http://displaymonkey.org
*
* Copyright (c) 2015 Fuel9 LLC and contributors
*
* Released under the MIT license:
* http://opensource.org/licenses/MIT
*/

// 2014-11-18 [LTL] - outlook BEGIN
// 2015-02-06 [LTL] - added isReady method
// 2015-03-08 [LTL] - using data
// 2015-05-08 [LTL] - ready callback
// 2016-09-04 [LTL] - more robust HTML and frame data implementation + reserve time

DM.Outlook = Class.create(/*PeriodicalExecuter*/ DM.FrameBase, {
    initialize: function ($super, options) {
        "use strict";
        $super(options, 'div.outlook');
        var data = options.panel.data;

        this.reserveMinutes = 0;
        this.allowReserve = data.allowReserve || true;

        this.summary = this.div.select(".summary")[0];
        this.events = this.div.select(".events")[0];
        this.actions = this.div.select(".actions")[0];
        this.progress = this.div.select(".progress")[0];

        if (this.summary) this.summary.hide();
        if (this.events) this.events.hide();
        if (this.progress) this.progress.show();

        if (this.actions) {
            this.actions
                .hide()
                .select(".reserve").each(function (a) {
                    $(a).observe('click', this._reserve.bind(this), true);
                }, this)
            ;
            if (this.allowReserve) {
                this.div.observe('click', function () {
                    if (this.visible())
                        this.hide();
                    else
                        this.show();
                }.bind(this.actions), true);
            }
        }

        this._callback();
    },

    _reserve: function(event) {
        "use strict";
        var a = $(event.target);
        if (a && a.hasClassName("reserve")) {
            this.reserveMinutes = a.getAttribute("data-minutes") || 0;
            //alert("frame: " + this.frameId + " reserve : " + this.reserveMinutes + " min");
            if (this.reserveMinutes)
                this._callback.bind(this).delay(1);
            this.actions.hide();
            Event.stop(event);
        }
    },

    _update: function (json) {
        "use strict";

        if (this.summary)
            this.summary
                .addClassName(json.currentEvent !== "" ? "busy" : "free")
                .setAll(".mailbox", json.mailbox)
                .setAll(".current.status", json.currentStatus)
                .setAll(".current.event", json.currentEvent)
                .show()
            ;

        if (this.events) {
            var item = this.events.select(".item")[0];
            if (item) {
                if (json.events.showEvents) {
                    for (var j = 0; j < json.events.showEvents; j++) {
                        var c = item.clone(true), i = null, subj = null;
                        if (!j && !json.events.items.length) {
                            subj = json.events.noEvents;
                        }
                        else if (j < json.events.items.length) {
                            var i = json.events.items[j];
                            subj = i.subject;
                            i.flags.forEach(function (f) {
                                c.addClassName(f);
                            });
                        }
                        c   .setAll(".subject", subj)
                            .setAll(".starts", i ? i.starts : null)
                            .setAll(".ends", i ? i.ends : null)
                            .setAll(".sensitivity", i ? i.sensitivity : null)
                            .setAll(".showAs", i ? i.showAs : null)
                            .setAll(".duration", i ? i.duration : null)
                        ;
                        item.insert({ before: c });
                    }
                    Element.remove(item);
                    this.events.show();
                } else {
                    this.events.hide();
                }
            } else {
                this.events.hide();
            }
        }

        json.events.labels.forEach(function (l) {
            this.setAll(".label." + l.key, l.value);
        }, this.div);

        if (this.progress)
            this.progress.hide();
    },

    _callback: function () {
        "use strict";
        if (this.exiting || this.updating)
            return;

        this.updating = true;
        new Ajax.Request("getOutlook.ashx", {
            method: 'get'
            , parameters: $H({
                frame: this.frameId,
                panel: this.panelId,
                display: _canvas.displayId,
                culture: _canvas.culture,
                reserveMinutes: this.reserveMinutes,
            })
            , evalJSON: false
            , outlook: this

            , onSuccess: function (resp) {
                var outlook = resp.request.options.outlook;
                try {
                    if (outlook.exiting)
                        return;

                    var json = null;
                    if (resp.responseText.isJSON())
                        json = resp.responseText.evalJSON();
                    if (!json)
                        throw new Error("JSON expected"); // <-- shouldn't get here

                    if (json.Error)
                        throw new Error("Server error");

                    outlook._update.call(outlook, json);
                }
                catch (e) {
                    new DM.ErrorReport({ exception: e, data: resp.responseText, source: "Outlook::callBack::onSuccess" });
                }
                finally {
                    outlook.updating = false;
                    //console.log(resp.responseText);
                    outlook.ready();
                }
            }

            , onFailure: function (resp) {
                var outlook = resp.request.options.outlook;
                new DM.ErrorReport({ exception: resp, source: "Outlook::callBack::onFailure" });
                outlook.updating = false;
                outlook.ready();
            }
        });

        this.reserveMinutes = 0;
    },
});

// 14-11-18 [LTL] - outlook END
