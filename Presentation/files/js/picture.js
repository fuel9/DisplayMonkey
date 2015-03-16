// 2015-02-06 [LTL] - object for reports and pictures
// 2015-03-08 [LTL] - using data

var Picture = Class.create({
    initialize: function (options) {
        "use strict";
        this.finishedLoading = false;
        var div = $(options.div);
        var img = div.down('img');
        img.alt = options.data.Name;
        img.src = options.data.FrameType === "PICTURE" ?
            "getImage.ashx?" + $H({ content: options.data.ContentId, frame: options.data.FrameId }).toQueryString() :
            "getReport.ashx?" + $H({ frame: options.data.FrameId }).toQueryString()
        ;
        img.observe('load', function () {
            this.finishedLoading = true;
            //console.log("picture " + options.div.id + " finished loading");
        }.bind(this));
    },

    isReady: function () {
        "use strict";
        return !!this.finishedLoading;
    },
});

