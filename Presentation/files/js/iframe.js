// 2015-02-06 [LTL] - object for iframe
// 2015-05-08 [LTL] - ready callback

DM.Iframe = Class.create(DM.FrameBase, {
    initialize: function ($super, options) {
        "use strict";
        $super(options, 'iframe.html');
        var data = options.panel.data;
        this.div.observe('load', this.ready.bind(this));

        this.div.src = "getHtml.ashx?" + $H({ frame: this.frameId }).toQueryString();
    },
});

