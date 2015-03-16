// 2014-10-11 [LTL] - YouTube script BEGIN
// 2014-10-24 [LTL] - use strict
// 2015-02-06 [LTL] - added isReady method
// 2015-03-08 [LTL] - using data

// 1. This code initiates load of the IFrame Player API code asynchronously.
var YtLib = {
    initialized: false,
    loaded: false,

    // 1. Delayed Player API loading.
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
        this.finishedLoading = false;
        var div = $(options.div);
        this.id = div.id;
        this.width = options.width || div.parentNode.clientWidth;
        var aspect = options.data.Aspect || 0;
        switch (Number(aspect)) {
            case 1: aspect = (16 / 9); break;
            case 2: aspect = (4 / 3); break;
            default: aspect = this.width / options.height; break;
        }
        this.height = Math.ceil(this.width / aspect);
        this.videoId = options.data.YoutubeId;
        var quality = options.data.Quality || 0;
        switch (Number(quality)) {
            case 1: this.quality = "small"; break;     // 320x240
            case 2: this.quality = "medium"; break;    // 640x360 or 480x360
            case 3: this.quality = "large"; break;     // 853x480 or 640x480
            case 4: this.quality = "hd720"; break;     // 1280x720 or 960x720
            case 5: this.quality = "hd1080"; break;    // 1920x1080 or 1440x1080
            case 6: this.quality = "highres"; break;   // above 1080
            default: this.quality = "default"; break;
        }
        this.start = options.data.Start || 0;
        var rate = options.data.Rate || 0;
        switch (Number(rate)) {
            case 1: this.rate = 0.25; break;           // very slow
            case 2: this.rate = 0.5; break;            // slow
            case 3: this.rate = 1.5; break;            // fast
            case 4: this.rate = 2.0; break;            // very fast
            default: this.rate = 1.0; break;           // normal
        }
        this.loop = !!options.data.AutoLoop;
        this.volume = options.data.Volume || 0;
        if (this.volume > 100) this.volume = 100;
        this.show();
    },

    isReady: function () {
        "use strict";
        return !!this.finishedLoading;
    },

    stop: function () {
        "use strict";
        try {
            if (this.player) this.player.stopVideo(); 
        } catch (e) { }
    },

    pause: function () {
        "use strict";
        try {
            if (this.player) this.player.pauseVideo();
        } catch (e) { }
    },

    play: function () {
        "use strict";
        try {
            if (this.player) this.player.playVideo();
        } catch (e) { }
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
        this.finishedLoading = true;
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
