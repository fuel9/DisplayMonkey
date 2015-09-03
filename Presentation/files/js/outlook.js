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

DM.Outlook = Class.create(/*PeriodicalExecuter*/ DM.FrameBase, {
    initialize: function ($super, options) {
        "use strict";
        $super(options, 'div.outlook');
        var data = options.panel.data;

        this.div.select("div.busy")[0].hide();
        this.div.select("div.plan")[0].hide();
        this.div.select("div.free")[0].hide();
        this.div.select("div.progress")[0].show();

        this._callback();
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
                        throw new Error("Server error");

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
    },
});

// 14-11-18 [LTL] - outlook END
