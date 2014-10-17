// 14-10-11 [LTL] - YouTube script BEGIN

// 1. This code initiates load of the IFrame Player API code asynchronously.
var YtLib = {
    initialized: false,
    loaded: false,

    // 1. Delayed Player API loading.
    load: function (div) {
        if (!this.initialized) {
            var lib = new Element('script', {
                src: 'https:' + '//www.youtube.com/iframe_api'
            });
            $$('script').last().insert({ after: lib });
            this.initialized = true;
        }
    }
};

// 2. This code loads the IFrame Player API code asynchronously.
// must be a static funciton
(function (w) {
    w.onYouTubeIframeAPIReady = function () {
        YtLib.loaded = true;
    }
})(window);

// 3. This class creates a player.
YtLib.YtPlayer = Class.create({
    initialize: function (options) {
        YtLib.load();
        this.player = null;
        with ($(options.div)) {
            this.id = id;
            this.width = readAttribute('data-width') || parentNode.clientWidth;
            var aspect = readAttribute('data-aspect')|| 0;
            switch (Number(aspect)) {
                case 1: aspect = (16 / 9); break;
                case 2: aspect = (4 / 3); break;
                default: aspect = this.width / readAttribute('data-height'); break;
            }
            aspect = options.aspect || aspect || (16 / 9);
            this.height = Math.ceil(this.width / aspect);
            this.videoId = readAttribute('data-content');
            this.loop = readAttribute('data-loop') == '1';
            this.volume = readAttribute('data-volume') || 0;
            if (this.volume > 100) this.volume = 100;
        }
        this.show();
    },

    stop: function () {
        if (this.player) this.player.stopVideo();
    },

    pause: function () {
        if (this.player && this.player.pauseVideo != undefined) this.player.pauseVideo();
    },

    play: function () {
        if (this.player && this.player.playVideo != undefined) this.player.playVideo();
    },

    show: function () {
        if (!YtLib.loaded) {
            this.show.bind(this).delay(0.1);
            return;
        }
        try {
            this.player = new YT.Player(this.id, {
                width: this.width,
                height: this.height,
                videoId: this.videoId,
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
        event.target.setVolume(this.volume);
        event.target.playVideo();
    },

    // 5. The API calls this function when the player's state changes.
    onPlayerStateChange: function (event) {
        //if (event.data == YtLib.PlayerState.PLAYING) {
        //}
    },

    // 6. The API calls this function when error occurs.
    onPlayerError: function (event) {
        try {
            switch (event.data) {
                case 2: throw "Invalid parameter";
                case 5: throw "Content not supported by HTML5 player";
                case 100, 150: throw "Requested content not found or marked private";
                case 101: throw "Requested content is restricted by owner";
            }
            throw "Error has occured";
        }
        catch (e) {
            new ErrorReport({
                exception: e,
                data: event.data,
                source: "YtPlayer.onPlayerError"
            });
        }
    }
});

// 14-10-11 [LTL] - YouTube script END
