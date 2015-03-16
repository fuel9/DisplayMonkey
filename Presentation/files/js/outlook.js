// 2014-11-18 [LTL] - outlook BEGIN
// 2015-02-06 [LTL] - added isReady method
// 2015-03-08 [LTL] - using data

var Outlook = Class.create(PeriodicalExecuter, {
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
        //this.div.hide();
        //$super(this._callback, 60);

        this.div.select("div.busy")[0].hide();
        this.div.select("div.plan")[0].hide();
        this.div.select("div.free")[0].hide();
        this.div.select("div.progress")[0].show();
        //this.div.show();

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
        new Ajax.Request("getOutlook.ashx", {
            method: 'get'
            , parameters: $H({
                frame: this.frameId,
                panel: this.panelId,
                display: _canvas.display,
                culture: _canvas.culture
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
                        throw new Error(json.Error);

                    var o = outlook.div,
                        free = o.select("div.free")[0],
                        busy = o.select("div.busy")[0],
                        plan = o.select("div.plan")[0]
                    ;
                    o.select("div.progress")[0].hide();
                    o.select(".mailbox").each(function (e) { e.update(json.mailbox); });
                    o.select(".status").each(function (e) { e.update(json.currentStatus); });
                    if (json.currentEvent === "") {
                        busy.hide();
                        free.show();
                    } else {
                        free.hide();
                        busy.select(".event")[0].update(json.currentEvent);
                        busy.show();
                    }

                    if (!json.events.showEvents) {
                        plan.update('');
                    }
                    else {
                        plan.update('<table><tr></tr></table>');
                        var rows = plan.down(1);
                        json.events.head.forEach(function (e) {
                            rows.insert(new Element('th', { 'class': e.cls }).update(e.name));
                        });
                        for (var i = 0, j = json.events.items.length; i < json.events.showEvents; i++) {
                            var r = rows.insert(new Element('tr')), k = 0;
                            json.events.head.forEach(function (e) {
                                r.insert(new Element('td', { class: e.cls }).update(
                                    !i && !j && !(k++) ? json.events.noEvents : i < j ? json.events.items[i][e.cls] : "&nbsp;"
                                    ));
                            });
                        }
                        plan.show();
                    }
                    o.show();
                }
                catch (e) {
                    new ErrorReport({ exception: e, data: resp.responseText, source: "Outlook::callBack::onSuccess" });
                }
                finally {
                    outlook.updating = false;
                    //console.log(resp.responseText);
                    outlook.finishedLoading = true;
                }
            }

            , onFailure: function (resp) {
                new ErrorReport({ exception: resp, source: "Outlook::callBack::onFailure" });
                resp.request.options.outlook.finishedLoading = true;
            }
        });
    },
});

// 14-11-18 [LTL] - outlook END
