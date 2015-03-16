// 2015-02-06 [LTL] - object for reports and pictures
// 2015-03-08 [LTL] - using data

var Video = Class.create({
    initialize: function (options) {
        "use strict";
        this.finishedLoading = false;
        var div = $(options.div);

        div.style.backgroundColor = _canvas.backColor;
        var video = this.video = div.select('video')[0];
        video.loop = !!options.data.AutoLoop;
        video.muted = !!options.data.PlayMuted;
        video.update(options.data.NoVideoSupport);
        options.data.VideoAlternatives.each(function (va) {
            var e = new Element("source", {
                src: "getVideo.ashx?" + $H({content: va.ContentId, frame: options.data.FrameId}).toQueryString(),
                type: va.MimeType
            });
            video.insert({ top: e });
        });
        if (options.play) video.play();

        div.observe('loadeddata', function () {
            this.finishedLoading = true;
            //console.log("video " + div.id + " finished loading");
        }.bind(this));
    },

    stop: function () {
        "use strict";
        this.video.pause();
    },

    pause: function () {
        "use strict";
        this.stop();
    },

    play: function () {
        "use strict";
        this.video.play();
    },

    isReady: function () {
        "use strict";
        return !!this.finishedLoading;
    },
});

