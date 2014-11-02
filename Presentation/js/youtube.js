// 14-10-11 [LTL] - YouTube script BEGIN
// 14-10-24 [LTL] - use strict

// 1. This code initiates load of the IFrame Player API code asynchronously.
var YtLib = {
    initialized: false,
    loaded: false,

    // 1. Delayed Player API loading.
    load: function (div) {
        "use strict";
        if (!this.initialized) {
            var lib = new Element('script', {
                src: 'https:'.concat('//www.youtube.com/iframe_api')
            });
            $$('script').last().insert({ after: lib });
            this.initialized = true;
        }
    }
};

// 2. This code loads the IFrame Player API code asynchronously.
// must be a static funciton
(function (w) {
    "use strict";
    w.onYouTubeIframeAPIReady = function () {
        YtLib.loaded = true;
    }
})(window);

// 3. This class creates a player.
YtLib.YtPlayer = Class.create({
    initialize: function (options) {
        "use strict";
        YtLib.load();
        this.player = null;
        var div = $(options.div);
        this.id = div.id;
        this.width = div.readAttribute('data-width') || div.parentNode.clientWidth;
        var aspect = div.readAttribute('data-aspect')|| 0;
        switch (Number(aspect)) {
            case 1: aspect = (16 / 9); break;
            case 2: aspect = (4 / 3); break;
            default: aspect = this.width / div.readAttribute('data-height'); break;
        }
        aspect = options.aspect || aspect || (16 / 9);
        this.height = Math.ceil(this.width / aspect);
        this.videoId = div.readAttribute('data-content');
        var quality = div.readAttribute('data-quality') || 0;
        switch (Number(quality)) {
            case 1: this.quality = "small"; break;     // 320x240
            case 2: this.quality = "medium"; break;    // 640x360 or 480x360
            case 3: this.quality = "large"; break;     // 853x480 or 640x480
            case 4: this.quality = "hd720"; break;     // 1280x720 or 960x720
            case 5: this.quality = "hd1080"; break;    // 1920x1080 or 1440x1080
            case 6: this.quality = "highres"; break;   // above 1080
            default: this.quality = "default"; break;
        }
        this.start = div.readAttribute('data-start') || 0;
        var rate = div.readAttribute('data-rate') || 0;
        switch (Number(rate)) {
            case 1: this.rate = 0.25; break;           // very slow
            case 2: this.rate = 0.5; break;            // slow
            case 3: this.rate = 1.5; break;            // fast
            case 4: this.rate = 2.0; break;            // very fast
            default: this.rate = 1.0; break;           // normal
        }
        this.loop = div.readAttribute('data-loop') == '1';
        this.volume = div.readAttribute('data-volume') || 0;
        if (this.volume > 100) this.volume = 100;
        this.show();
    },

    stop: function () {
        "use strict";
        if (this.player &&
            this.player.getPlayerState() > 0)
            this.player.stopVideo();
    },

    pause: function () {
        "use strict";
        if (this.player &&
            this.player.pauseVideo != undefined)
            this.player.pauseVideo();
    },

    play: function () {
        "use strict";
        if (this.player &&
            this.player.playVideo != undefined)
            this.player.playVideo();
    },

    show: function () {
        "use strict";
        if (this.player) return;
        if (!YtLib.loaded) {
            this.show.bind(this).delay(0.1);
            return;
        }
        try {
            this.player = new YT.Player(this.id, {
                width: this.width,
                height: this.height,
                //videoId: this.videoId,
                playerVars: {
                    enablejsapi: 1,
                    controls: 0,
                    showinfo: 0,
                    modestbranding: 1,
                    rel: 0,
                    //origin: window.location,
                    wmode: 'opaque',
                    autoplay: 0,
                    loop: this.loop
                },
                events: {
                    'onReady': this.onPlayerReady.bind(this),
                    'onStateChange': this.onPlayerStateChange,
                    'onError': this.onPlayerError
                }
            });
        }
        catch (e) {
            new ErrorReport({exception: e, data: this.id, source: 'YtPlayer.create'});
        }
    },

    // 4. The API will call this function when the video player is ready.
    onPlayerReady: function (event) {
        "use strict";
        event.target.setVolume(this.volume);
        event.target.loadVideoById(this.videoId, this.start, this.quality);
        event.target.setPlaybackRate(this.rate);
    },

    // 5. The API calls this function when the player's state changes.
    onPlayerStateChange: function (event) {
        "use strict";
        //if (event.data == YtLib.PlayerState.PLAYING) {
        //}
    },

    // 6. The API calls this function when error occurs.
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
            new ErrorReport({
                exception: e,
                data: typeof event.data === 'undefined' ? event : event.data,
                source: "YtPlayer.onPlayerError"
            });
        }
    }
});

// 14-10-11 [LTL] - YouTube script END
