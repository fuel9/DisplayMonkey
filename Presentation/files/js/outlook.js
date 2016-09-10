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

        this.availableMinutes = 0;
        this.reserveMinutes = 0;
        this.allowReserve = data.AllowReserve || false;
        this.showEvents = data.ShowEvents || 0;

        this.summary = this.div.select(".summary")[0];
        this.events = this.div.select(".events")[0];
        this.actions = this.div.select(".actions")[0];
        this.progress = this.div.select(".progress")[0];

        this.bookingImpossible = "";
        this.bookingSent = "";

        if (this.summary) this.summary.hide();
        if (this.events) this.events.hide();
        if (this.progress) this.progress.show();

        if (this.actions) {
            this.actions
                .hide()
                .select(".book").each(function (a) {
                    $(a).observe('click', this._book.bind(this), true);
                }, this)
            ;
            if (this.allowReserve) {
                this.div.observe('click', function () {
                    if (this.actions.visible())
                        this.actions.hide();
                    else
                        this._showBook();
                }.bind(this), true);
            }
        }

        this.timer = setInterval(this._onTimer.bind(this), 60 * 1000);

        this._callback();
    },

    uninit: function ($super) {
        "use strict";
        if (this.timer) {
            clearInterval(this.timer);
            this.timer = 0;
        }
        $super();
    },


    _onTimer: function () {
        "use strict";
        if (this.availableMinutes > 0)
            this.availableMinutes--;
    },

    _showBook: function () {
        "use strict";
        if (this.allowReserve && this.actions) {
            var am = this.availableMinutes;
            if (!am) {
                this.actions.setAll(".message", this.bookingImpossible);
            }
            this.actions
                .show()
                .select(".book").each(function (c) {
                    if (am) {
                        var dm = $(c).getAttribute("data-minutes");
                        if (!dm || isNaN(dm)) {
                            if (!c.getAttribute("value") || c.getAttribute("data-value") == "*") {
                                var dur = moment.duration(am, "m");
                                c.writeAttribute("value", dur.hours().pad(0) + ":" + dur.minutes().pad(2));
                                c.writeAttribute("data-value", "*");
                            }
                            c.writeAttribute("data-minutes", null);
                            c.show();
                        } else {
                            if (am >= parseInt(dm))
                                c.show();
                            else
                                c.hide();
                        }
                    } else
                        c.hide();
                });
        }
    },

    _book: function (event) {
        "use strict";
        var e = $(event.target);
        if (e && e.hasClassName("book")) {
            this.reserveMinutes = e.getAttribute("data-minutes") || this.availableMinutes;
            if (this.reserveMinutes) {
                this._callback.bind(this).delay(0);
                this.actions.setAll(".message", this.bookingSent);
            }
            this.actions.select(".book").each(function (c) {
                $(c).hide();
            });
            Event.stop(event);
        }
    },

    _dismiss: function () {
        "use strict";
        if (this.actions.visible())
            this.actions.hide();
    },

    _update: function (json) {
        "use strict";

        if (this.allowReserve && this.actions) {
            this.availableMinutes = json.available.minutes || 0;
        }

        if (this.summary)
            this.summary
                .addClassName(json.currentEvent !== "" ? "busy" : "free")
                .setAll(".mailbox", json.mailbox)
                .setAll(".current.status", json.currentStatus)
                .setAll(".current.event", json.currentEvent)
                .show()
            ;

        if (this.events) {
            var items=this.events.select(".item"), i=items.length;
            if (i) {
                if (this.showEvents) {
                    for (; i<this.showEvents; i++) {
                        items[0].insert({ after: items[0].clone(true) });
                    }
                    for (; i>this.showEvents; i--) {
                        Element.remove(0);
                    }
                    var i=0;
                    this.events.select(".item").each(function (c) {
                        var ev, subj;
                        if (!i && !json.events.items.length) {
                            json.labels.forEach(function (l) {
                                if (l.key == "noEvents") subj = l.value;
                            });
                        }
                        else if (i < json.events.items.length) {
                            ev = json.events.items[i];
                            subj = ev.subject;
                            ev.flags.forEach(function (f) {
                                c.addClassName(f);
                            });
                        }
                        c   .setAll(".subject", subj)
                            .setAll(".starts", ev ? ev.starts : null)
                            .setAll(".ends", ev ? ev.ends : null)
                            .setAll(".sensitivity", ev ? ev.sensitivity : null)
                            .setAll(".showAs", ev ? ev.showAs : null)
                            .setAll(".duration", ev ? ev.duration : null)
                        ;
                        i++;
                    });
                    this.events.show();
                } else {
                    this.events.hide();
                }
            } else {
                this.events.hide();
            }
        }

        json.labels.forEach(function (l) {
            this.div.setAll(".label." + l.key, l.value);
            if (l.key == "bookingSent") this.bookingSent = l.value;
            if (l.key == "bookingImpossible") this.bookingImpossible = l.value;
        }, this);

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
