// 2015-02-06 [LTL] - object for reports and pictures

var Video = Class.create({
    initialize: function (options) {
        "use strict";
        this.div = options.div;
        this.finishedLoading = false;

        var a;
        if (a = this.div.readAttribute('loop')) this.div.loop = a;
        if (a = this.div.readAttribute('muted')) this.div.muted = a;
        if (options.play) this.play();
        var vc = this.div.up('div.videoContainer');
        if (vc) {
            vc.style.backgroundColor = _canvas.backColor;
        }

        this.div.observe('loadeddata', function () {
            this.finishedLoading = true;
            //console.log("video " + vc.id + " finished loading");
        }.bind(this));
    },

    stop: function () {
        "use strict";
        this.div.pause();
    },

    pause: function () {
        "use strict";
        this.stop();
    },

    play: function () {
        "use strict";
        this.div.play();
    },

    isReady: function () {
        "use strict";
        return !!this.finishedLoading;
    },
});

