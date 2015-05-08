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
        img.src = this._hashUrl(data.FrameType === 5 ?
            "getImage.ashx?" + $H({ content: data.ContentId, frame: this.frameId }).toQueryString() :
            "getReport.ashx?" + $H({ frame: this.frameId }).toQueryString()
        );
        img.observe('load', this.ready.bind(this));
    },

    _hashUrl: function (url) {
	    "use strict";
	    var u = url.split('?'), p = $H();
	    if (u.length > 1) p = p.merge(u[1].toQueryParams());
	    p.set('ts', (new Date()).getTime());
	    return u[0] + '?' + p.toQueryString();
	},
});

