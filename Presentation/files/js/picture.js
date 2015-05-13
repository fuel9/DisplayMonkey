// 2015-02-06 [LTL] - object for reports and pictures
// 2015-03-08 [LTL] - using data
// 2015-05-01 [LTL] - hash URL to avoid browser caching
// 2015-05-08 [LTL] - ready callback

DM.Picture = Class.create(DM.FrameBase, {
    initialize: function ($super, options) {
        "use strict";
        $super(options, 'div.picture, div.report');
        var data = options.panel.data;
        var img = this.div.down('img');
        img.alt = data.Name;
        img.src = (data.FrameType === 5 ? 
            "getImage.ashx?" :
            "getReport.ashx?") + $H({ frame: this.frameId, hash: data.Hash }).toQueryString()
        ;
        img.observe('load', function () {
            this.ready();
        }.bind(this));
    },
});

