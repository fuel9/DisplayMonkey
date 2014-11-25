// 14-11-18 [LTL] - outlook BEGIN

var Outlook = Class.create(PeriodicalExecuter, {
    initialize: function ($super, options) {
        "use strict";
        this.exiting = false;
        this.div = options.div;
        this.frameId = options.frameId || 0;
        this.panelId = options.panelId || 0;
        if (!this.div || !this.panelId || !this.frameId)
            return;
        this.div.hide();
        $super(this._callback, 60);
        this._callback(null);
    }

    , stop: function () {
        this.exiting = true;
    }

    , _callback: function (pe) {
        "use strict";
        if (this.exiting)
            return;

        new Ajax.Request("getOutlook.ashx", {
            method: 'get'
            , parameters: $H({ frame: this.frameId, panel: this.panelId, display: _canvas.displayId })
            , evalJSON: false
            , outlook: this

            , onSuccess: function (resp) {
                try {
                    var outlook = resp.request.options.outlook;

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
                        free = o.select("div#free")[0],
                        busy = o.select("div#busy")[0],
                        plan = o.select("div#plan")[0];
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

                    plan.update('<table><tr></tr></table>');
                    var rows = plan.down(1);
                    json.events.head.forEach(function (e) {
                        rows.insert(new Element('th', { 'class': e.cls }).update(e.name));
                    });
                    if (!json.events.items.length) {
                        var r = rows.insert(new Element('tr'));
                        for (var i = 0; i < json.events.head.length; i++) {
                            var c = json.events.head[i].cls;
                            r.insert(new Element('td', { class: c }).update(i ? "" : json.events.noEvents));
                        }
                    } else {
                        json.events.items.forEach(function (e) {
                            var r = rows.insert(new Element('tr'));
                            for (var i = 0; i < json.events.head.length; i++) {
                                var c = json.events.head[i].cls;
                                r.insert(new Element('td', { class: c }).update(e[c]));
                            }
                        });
                    }
                    o.show();
                }
                catch (e) {
                    new ErrorReport({ exception: e, data: resp.responseText, source: "Outlook::callBack::onSuccess" });
                }
            }

            , onFailure: function (resp) {
                new ErrorReport({ exception: resp, source: "Outlook::callBack::onFailure" });
            }
        });
    }
});

// 14-11-18 [LTL] - outlook END
