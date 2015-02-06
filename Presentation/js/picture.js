// 2015-02-06 [LTL] - object for reports and pictures

var Picture = Class.create({
    initialize: function (options) {
        "use strict";
        this.finishedLoading = false;
        $(options.div).down('img').observe('load', function () {
            this.finishedLoading = true;
            //console.log("picture " + options.div.id + " finished loading");
        }.bind(this));
    },

    isReady: function () {
        "use strict";
        return !!this.finishedLoading;
    },
});

