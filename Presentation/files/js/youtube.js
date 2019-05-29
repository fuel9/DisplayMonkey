/*!
* DisplayMonkey source file
* http://displaymonkey.org
*
* Copyright (c) 2015 Fuel9 LLC and contributors
*
* Released under the MIT license:
* http://opensource.org/licenses/MIT
*/

// 2014-10-11 [LTL] - YouTube script BEGIN
// 2014-10-24 [LTL] - use strict
// 2015-02-06 [LTL] - added isReady method
// 2015-03-08 [LTL] - using data
// 2015-04-27 [LTL] - report ready in onPlayerStateChange
// 2015-05-08 [LTL] - ready callback
// 2015-06-18 [LTL] - RC10: tweak for http://stackoverflow.com/questions/17078094/youtube-iframe-player-api-onstatechange-not-firing
// 2015-07-29 [LTL] - RC12: performance and memory management improvements
// 2018-08-16 [LTL] - fixed looping, #60

// Create YT lib loader object
var YtLib = (function (w) {
    "use strict";
    w.onYouTubeIframeAPIReady = function () {
        YtLib.loaded = true;
    }
    return {
        initialized: false,
        loaded: false,
        load: function (div) {
            "use strict";
            if (!this.initialized) {
                this.initialized = true;
                var lib = new Element('script', {
                    src: 'https:'.concat('//www.youtube.com/iframe_api')
                });
                $$('script').last().insert({ after: lib });
            }
        }
    };
})(window);

DM.YtPlayer = Class.create(DM.FrameBase, {
    initialize: function ($super, options) {
        "use strict";
        $super(options, 'div.youtube');
        var data = options.panel.data;

        YtLib.load();
        this.player = null;
        var aspect = data.Aspect || 0;
        switch (Number(aspect)) {
            case 1: aspect = (16 / 9); break;
            case 2: aspect = (4 / 3); break;
            default: aspect = this.width / this.height; break;
        }
        this.height = Math.ceil(this.width / aspect);
        this.videoId = data.YoutubeId;
        var quality = data.Quality || 0;
        switch (Number(quality)) {
            case 1: this.quality = "small"; break;     // 320x240
            case 2: this.quality = "medium"; break;    // 640x360 or 480x360
            case 3: this.quality = "large"; break;     // 853x480 or 640x480
            case 4: this.quality = "hd720"; break;     // 1280x720 or 960x720
            case 5: this.quality = "hd1080"; break;    // 1920x1080 or 1440x1080
            case 6: this.quality = "highres"; break;   // above 1080
            default: this.quality = "default"; break;
        }
        this.start = data.Start || 0;
        var rate = data.Rate || 0;
        switch (Number(rate)) {
            case 1: this.rate = 0.25; break;           // very slow
            case 2: this.rate = 0.5; break;            // slow
            case 3: this.rate = 1.5; break;            // fast
            case 4: this.rate = 2.0; break;            // very fast
            default: this.rate = 1.0; break;           // normal
        }
        this.loop = !!data.AutoLoop;
        var v = data.Volume || 0;
        this.volume =
            v > 100 ? 100 :
            v < 0 ? 0 :
            v
        ;
        this.show();
    },

    stop: function ($super) {
        "use strict";
        $super();
        try {
            if (this.player) this.player.stopVideo(); 
        } catch (e) { }
    },

    pause: function ($super) {
        "use strict";
        $super();
        try {
            if (this.player) this.player.pauseVideo();
        } catch (e) { }
    },

    play: function ($super) {
        "use strict";
        $super();
        try {
            if (this.player) this.player.playVideo();
        } catch (e) { }
    },

    uninit: function ($super) {
        "use strict";
        try {
            if (this.player && typeof this.player.removeEventListener === "function") {
                this.player.removeEventListener('onStateChange', this.onPlayerStateChange);
                this.player.removeEventListener('onReady', this.onPlayerReady);
                this.player.removeEventListener('onError', this.onPlayerError);
            }
        }
        catch (e) { }
        $super();
    },

    show: function () {
        "use strict";
        if (this.player) return;
        if (!YtLib.loaded) {
            this.show.bind(this).delay(1);
            return;
        }
        try {
            this.player = new YT.Player(this.div.down('.ytplayer'), {
                width: this.width,
                height: this.height,
                //videoId: this.videoId,
                playerVars: {
                    enablejsapi: 1,
                    controls: 0,
                    showinfo: 0,
                    modestbranding: 1,
                    rel: 0,
                    origin: document.domain,
                    wmode: 'opaque',
                    autoplay: 0,
                    //loop: this.loop,
					playlist: this.videoId
                },
                events: {
                    'onReady': this.onPlayerReady.bind(this),
                    //'onStateChange': this.onPlayerStateChange.bind(this),
                    'onError': this.onPlayerError.bind(this),
                }
            });
        }
        catch (e) {
            new DM.ErrorReport({ exception: e, data: this.div.id, source: 'YtPlayer.create' });
            this.ready();
        }
    },

    // The API will call this function when the video player is ready.
    onPlayerReady: function (event) {
        "use strict";
        event.target.addEventListener('onStateChange', this.onPlayerStateChange.bind(this));
        event.target.setVolume(this.volume);
        event.target.loadVideoById(this.videoId, this.start, this.quality);
        event.target.setPlaybackRate(this.rate);
    },

    // The API calls this function when the player's state changes.
    onPlayerStateChange: function (event) {
        "use strict";
        if (event.data === YT.PlayerState.PLAYING) {
            //event.target.removeEventListener('onStateChange', this.onPlayerStateChange);
			this.ready();
        } else if (event.data === YT.PlayerState.ENDED && this.loop) {
			this.player.playVideo();
        }
    },

    // The API calls this function when error occurs.
    onPlayerError: function (event) {
        "use strict";
        try {
            switch (event.data) {
                case 2: throw "Invalid parameter";
                case 5: throw "Content not supported by HTML5 player";
                case 100, 150: throw "Requested content not found or marked private";
                case 101: throw "Requested content is restricted by owner";
            }
            throw "Error has occurred";
        }
        catch (e) {
            new DM.ErrorReport({
                exception: e,
                data: typeof event.data === 'undefined' ? event : event.data,
                source: "YtPlayer.onPlayerError"
            });
        }
        this.ready();
    }
});

// 14-10-11 [LTL] - YouTube script END
