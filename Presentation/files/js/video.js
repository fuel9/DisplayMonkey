/*!
* DisplayMonkey source file
* http://displaymonkey.org
*
* Copyright (c) 2015 Fuel9 LLC and contributors
*
* Released under the MIT license:
* http://opensource.org/licenses/MIT
*/

// 2015-02-06 [LTL] - object for reports and pictures
// 2015-03-08 [LTL] - using data
// 2015-05-08 [LTL] - ready callback
// 2015-07-29 [LTL] - RC12: performance and memory management improvements

DM.Video = Class.create(DM.FrameBase, {
    initialize: function ($super, options) {
        "use strict";
        $super(options, 'div.video');
        var data = options.panel.data;
        var frameId = this.frameId;

        if (!_canvas.supports_video) {
            new DM.ErrorReport({ exception: data.NoVideoSupport, data: data, source: "DM.Video::initialize" });
            return this.ready();
        }

        this.div.style.backgroundColor = _canvas.backColor;
        var video = this.video = this.div.down('video');
        video.loop = !!data.AutoLoop;
        video.muted = !!data.PlayMuted;
        video.update(data.NoVideoSupport);
        video.addEventListener('loadeddata', this.ready.bind(this));

        data.VideoAlternatives.each(function (va) {
            var e = new Element("source", {
                src: "getVideo.ashx?" + $H({content: va.ContentId, frame: frameId, hash: va.Hash}).toQueryString(),
                type: va.MimeType
            });
            video.insert({ top: e });
        });

        if (options.play) video.play();
    },

    stop: function ($super) {
        "use strict";
        $super();
        this.video.pause();
    },

    pause: function ($super) {
        "use strict";
        $super();
        this.stop();
    },

    play: function ($super) {
        "use strict";
        $super();
        this.video.play();
    },

    uninit: function ($super) {
        "use strict";
        this.video.removeEventListener('loadeddata', this.ready);
        $super();
    },
});

