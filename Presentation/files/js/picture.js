// 2015-02-06 [LTL] - object for reports and pictures
// 2015-03-08 [LTL] - using data
// 2015-05-01 [LTL] - hash URL to avoid browser caching

var Picture = Class.create({
    initialize: function (options) {
        "use strict";
        this.finishedLoading = false;
        var div = $(options.div);
        var img = div.down('img');
        img.alt = options.data.Name;
        img.src = this._hashUrl(options.data.FrameType === 5 ?
            "getImage.ashx?" + $H({ content: options.data.ContentId, frame: options.data.FrameId }).toQueryString() :
            "getReport.ashx?" + $H({ frame: options.data.FrameId }).toQueryString()
        );
        img.observe('load', function () {
            this.finishedLoading = true;
            //console.log("picture " + options.div.id + " finished loading");
        }.bind(this));
    },

    isReady: function () {
        "use strict";
        return !!this.finishedLoading;
    },

    _hashUrl: function (url) {
	    "use strict";
	    var u = url.split('?'), p = $H();
	    if (u.length > 1) p = p.merge(u[1].toQueryParams());
	    p.set('ts', (new Date()).getTime());
	    return u[0] + '?' + p.toQueryString();
	},
});

