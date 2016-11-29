/*!
* DisplayMonkey source file
* http://displaymonkey.org
*
* Copyright (c) 2015 Fuel9 LLC and contributors
*
* Released under the MIT license:
* http://opensource.org/licenses/MIT
*/

// 2016-08-12 [LTL] - object for Power BI

DM.Powerbi = Class.create(DM.FrameBase, {
    initialize: function ($super, options) {
        "use strict";
        $super(options, 'div.powerbi');
        var data = options.panel.data;
        this.action = data.Action;
        this.iframe = this.div.down('iframe');
        this.iframe.src = data.TargetUrl;
        this.iframe.addEventListener('load', this._postActionLoaded.bind(this));
    },

    _postActionLoaded: function () {
        "use strict";
        if (this.exiting || this.updating)
            return;

        // get access token
        this.updating = true;
        new Ajax.Request("getAzureToken.ashx", {
            method: 'get',
            parameters: $H({
                frame: this.frameId,
                panel: this.panelId,
                display: _canvas.displayId,
                culture: _canvas.culture,
            }),
            evalJSON: false,
            frame: this,

            onSuccess: function (resp) {
                var frame = resp.request.options.frame;
                try {
                    if (frame.exiting)
                        return;

                    var json = null;
                    if (resp.responseText.isJSON())
                        json = resp.responseText.evalJSON();
                    if (!json)
                        throw new Error("JSON expected"); // <-- shouldn't get here

                    if (json.Error)
                        throw new Error(json.Error);

                    if (!json.accessToken)
                        throw new Error("Invalid access token data");

                    var m = JSON.stringify({
                        action: frame.action,
                        width: frame.width,
                        height: frame.height,
                        accessToken: json.accessToken,
                    });
                    frame.iframe.contentWindow.postMessage(m, "*");;
                }
                catch (e) {
                    new DM.ErrorReport({ exception: e, data: resp.responseText, source: "Powerbi::callBack::onSuccess" });
                }
                finally {
                    frame.updating = false;
                    //console.log(resp.responseText);
                    frame.ready();
                }
            },

            onFailure: function (resp) {
                var frame = resp.request.options.frame;
                new DM.ErrorReport({ exception: resp, source: "Powerbi::callBack::onFailure" });
                frame.updating = false;
                frame.ready();
            },
        });
    },

    uninit: function ($super) {
        "use strict";
        this.iframe.removeEventListener('load', this._postActionLoaded);
        $super();
    },
});

