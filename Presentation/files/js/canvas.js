// 2012-08-13 [DPA] - client side scripting BEGIN
// 2014-10-06 [LTL] - reload when display hash changes
// 2014-10-11 [LTL] - error reporting
// 2014-10-16 [LTL] - YouTube support
// 2014-10-25 [LTL] - use strict, code improvements
// 2015-01-30 [LTL] - minor code improvements
// 2015-02-06 [LTL] - major overhaul
// 2015-03-08 [LTL] - using data for frames
// 2015-03-27 [LTL] - fixed YT Flash w/ FF & IE

// TODO: upgrade moment.js

var $j = jQuery.noConflict();

var ErrorReport = Class.create({
    initialize: function (options) {
        "use strict";
        this.info = {
            Error: ((options.exception instanceof Error ? options.exception.message : options.exception) || "unspecified"),
            Where: (options.source || "unspecified"),
            When: moment().format(),
            Data: (options.data || "none")
        };
        console.error("!Display Monkey error: " + JSON.stringify(this.info));
        this.length = options.length || 100;
        if (_canvas.showErrors) {
            this.show(this.length);
        }
    },

    show: function (length) {
        "use strict";
        var div = /*$('error') ||*/ new Element('div', { id: 'error' });
        div.update(div.innerHTML.concat(
            "<table>",
            "<tr><td>Error:</td><td>#{Error}</td></tr>",
            "<tr><td>Where:</td><td>#{Where}</td></tr>",
            "<tr><td>When:</td><td>#{When}</td></tr>",
            "<tr><td>Data:</td><td>#{Data}</td></tr>",
            "</table>"
            ).interpolate(this.info)
        );
        $(document.body).insert({ bottom: div });
        div.fade({ duration: 1, from: 0, to: 1 });
        function _remove() { div.remove(); }
        function _hide() {
            _remove.delay(2);
            div.fade({ duration: 1, from: 1, to: 0 });
        }
        length = length || this.length;
        _hide.delay(length < 1 ? 1 : length);
    },
});

var DM = {};
var _canvas = {};

DM.Canvas = Class.create({
	initialize: function (options) {
	    "use strict";
	    this.fullScreenActive = false;

	    this.locationTime = moment(options.locationTime);
	    this.utcTime = moment(options.utcTime);
	    this.displayId = options.displayId;
		this.hash = options.hash;
		this.dateFormat = (options.dateFormat || 'LL');
		this.timeFormat = (options.timeFormat || 'LT');
		this.latitude = (options.latitude || 0);
		this.longitude = (options.longitude || 0);
		this.woeid = (options.woeid || 0);
		this.culture = (options.culture || "");
		this.temperatureUnit = (options.temperatureUnit || 'c');
		this.showErrors = (options.showErrors || false);

		this.width = (options.width || 0);
		this.height = (options.height || 0);
		this.backImage = (options.backImage || 0);
		this.backColor = (options.backColor || 'transparent');
		this.initialIdleInterval = (options.initialIdleInterval || 0);
		this.supports_video = !!document.createElement('video').canPlayType;

		this.panels = [];
		this.fullPanel = {};
	},

	initPanels: function () {
	    "use strict";
	    this.fixScreenDiv();
	    Event.observe(window, "resize", this.fixScreenDiv);

	    var s = $('segments').style;
	    if (this.backImage > 0) {
	        s.backgroundImage = "url('getImage.ashx?content=" + this.backImage + "')";
	    }
	    if (this.backColor != '') {
	        s.backgroundColor = this.backColor;
	    }

	    $$('div[data-panel-id]').each(function (e) {
	        var pi = e.readAttribute('data-panel-id');
	        if (e.id === "full")
	            this.fullPanel = new DM.FullScreenPanelUpdater({
                    panelId: pi,
                    container: e.id,
		            evalScripts: false,
		            fadeLength: 2, // sec (default)
		            idleInterval: this.initialIdleInterval,
                });
	        else
	            this.panels.push(new DM.PanelUpdater({
                    panelId: pi,
                    container: e.id,
		            evalScripts: false,
		            fadeLength: 1, // sec (default)
                }));
	    }.bind(this));
	},

	fixScreenDiv: function () {
	    "use strict";
	    var body = $$('body')[0];
		body.style.backgroundColor = this.backColor;
		var s = $('screen').style;
		s.height = body.clientHeight + 'px';
		s.width = body.clientWidth + 'px';
		s.backgroundColor = this.backColor;
	},

    checkDisplayHash: function () {
        "use strict";
        new Ajax.Request("getDisplayHash.ashx", {
            method: 'get'
            , parameters: $H({display: this.displayId})
            , canvas: this
            , evalJSON: false

            , onSuccess: function (resp) {
                try {
                    var json = null;
                    if (resp.responseText.isJSON())
                        json = resp.responseText.evalJSON();
                    if (!json)
                        throw new Error("JSON expected"); // <-- shouldn't get here
                    if (json.Error)
                        throw new Error("Server error");

                    var c = resp.request.options.canvas;
                    var _displayId = json.DisplayId;
                    if (c.displayId != _displayId)
                        return;
                    var _hash = json.Hash;
                    if (c.hash != _hash) {
                        console.log("reload triggered in checkDisplayHash");
                        document.location.reload(true);
                    }
                }
                catch (e) {
                    new ErrorReport({ exception: e, data: resp.responseText, source: "checkDisplayHash::onSuccess" });
                }
            }

			, onFailure: function (resp) {
			    new ErrorReport({ exception: resp, source: "checkDisplayHash::onFailure" });
			}
        });
    },

    collectUsedContainers: function () {
        $$("div[id^='x_'][collect='1']").each(function (e) {
            e.remove();
        });
    },
});

DM.PanelUpdaterBase = Class.create(Ajax.Base, {
	initialize: function ($super, options) {
	    "use strict";
	    $super(options);

		this.panelId = (this.options.panelId || 0);
		this.frequency = (this.options.frequency || 1);
		this.containerId = this.options.container; // "div" + this.panelId;
		this.html = "";
		this.data = {};
		//this.hash = "";
		this.object = null;

		this.newFrameId = 0;
		this.previousType = this.currentType = -1;
		this.fadeLength = (this.options.fadeLength || 0);
		if (this.fadeLength < 0) this.fadeLength = 0;

		//this._onFrameExpire.bind(this);
		this._onGetNextFrame.bind(this);
		//this._onUpdateEnd.bind(this);
	},

	/*_onFrameExpire: function () {
	    "use strict";
	    this._onGetNextFrame();
	},                // <-- override*/

	_onGetNextFrame: function () {
	    "use strict";
	    // get next frame
	    var p = $H({
	        "frame":    this.newFrameId,
	        "panel":    this.panelId,
	        "display":  _canvas.displayId,
	        "culture":  _canvas.culture,
	    });
		new Ajax.Request("getNextFrame.ashx", {
			method: 'get'
		    , parameters: p
		    , panelUpdater: this
		    , evalJSON: false

		    , onSuccess: function (resp) {
		        var p = resp.request.options.panelUpdater;
		        try {
				    var json = null;
				    if (resp.responseText.isJSON())
				        json = resp.responseText.evalJSON();
				    if (!json)
				        throw new Error("JSON expected"); // <-- shouldn't get here
				    if (json.Error)
				        throw new Error("Server error");

                    // get duration first
				    p.data = $(json);
				    p.frequency = json.Duration;
				    if (p.frequency == null || p.frequency <= 0)
				        p.frequency = 1;

                    // now get frame id
				    p.newFrameId = json.FrameId;
				    if (p.newFrameId == null || !p.newFrameId) {
				        p.newFrameId = 0;
				    } else {
				        p.currentType = json.FrameType;
				        p.html = json.Html;
				    }
			    }
			    catch (e) {
			        new ErrorReport({ exception: e, data: resp.responseText, source: "_onGetNextFrame::onSuccess" });
			    }
		        finally {
		            p._onUpdateEnd();
		        }
		    }

		    , onFailure: function (resp) {
			    /*switch(resp.status)
			    {
			    case 404:
			    case 415:
			    default:
			    break;
			    }*/
		        //_updateBegin();
		        new ErrorReport({ exception: resp.toString(), source: "_onGetNextFrame::onFailure", data: resp });
		        resp.request.options.panelUpdater._onUpdateEnd();
		    }
		});
	},               // <-- get HTML via Ajax, then calls _onUpdateEnd

	/*_onUpdateEnd: function () {
	    "use strict";
	    this.previousType = this.currentType;

	    // queue _onFrameExpire
	    this.expire = this._onFrameExpire
            .bind(this)
            .delay(this.frequency + this.fadeLength)
	    ;
	},                  // <-- override*/

	_uninitFrame: function () {
	    "use strict";
	    try {
	        if (this.object && this.object.stop) {
	            this.object.stop();
	        }

	        // resume others
	        //if (this instanceof DM.FullScreenPanelUpdater) {
	        //    _canvas.panels.forEach(function (p) {
	        //        if (p.object && p.object.play) p.object.play();
	        //    });
	        //}
	    }
	    catch (e) {
	        new ErrorReport({ exception: e, source: "_uninitFrame" }); // <-- shouldn't get here
	    }
	    finally {
	        this.object = null;
	    }
	},                 // <-- stops and destroys current object; for full panel resumes objects in other panels

	_initFrame: function (panel) {
	    "use strict";
	    var obj = null;
        try {
            //// pause others
            //if (this instanceof DM.FullScreenPanelUpdater) {
            //    _canvas.panels.forEach(function (p) {
            //        if (p.object && p.object.pause) p.object.pause();
            //    });
            //}

            var div = null,
                width = panel.getAttribute('data-panel-width'),
                height = panel.getAttribute('data-panel-height')
            ;

            switch (this.currentType) {
                //Clock = 0,
                case 0:
                    if (div = panel.down('div.clock')) {
                        obj = new Clock({
                            div: div,
                            data: this.data,
                            panelId: panel.id,
                            width: width,
                            height: height
                        });
                    }
                    break;
                    
                //Html = 1,
                case 1:
                    if (div = panel.down('iframe.html')) {
                        obj = new Iframe({
                            div: div,
                            data: this.data,
                            panelId: panel.id,
                            width: width,
                            height: height
                        });
                    }
                    break;

                //Memo = 2,
                case 2:
                    if (div = panel.down('div.memo')) {
                        obj = new Memo({
                            div: div,
                            data: this.data,
                            panelId: panel.id,
                            width: width,
                            height: height
                        });
                    }
                    break;
                    
                ////News = 3,
                //Outlook = 4,
                case 4:
                    if (div = panel.down('div.outlook')) {
                        obj = new Outlook({
                            div: div,
                            data: this.data,
                            panelId: panel.id,
                            width: width,
                            height: height
                        });
                    }
                    break;
                    
                //Picture = 5,
                //Report = 6,
                case 5:
                case 6:
                    if (div = panel.down('div.picture, div.report')) {
                        obj = new Picture({
                            div: div,
                            data: this.data,
                            panelId: panel.id,
                            width: width,
                            height: height
                        });
                    }
                    break;
                    
                //Video = 7,
                case 7:
                    if ((div = panel.down('div.video')) && _canvas.supports_video) {
                        obj = new Video({
                            div: div,
                            data: this.data,
                            panelId: panel.id,
                            width: width,
                            height: height,
                            play: (this instanceof DM.FullScreenPanelUpdater || !_canvas.fullScreenActive)
                        });
                    }
                    break;
                    
                //Weather = 8,
                case 8:
                    if (div = panel.down('div.weather')) {
                        obj = new Weather({
                            div: div,
                            data: this.data,
                            panelId: panel.id,
                            width: width,
                            height: height
                        });
                    }
                    break;
                    
                //YouTube = 9
                case 9:
                    if (div = panel.down('div.youtube')) {
                        obj = new YtLib.YtPlayer({
                            div: div, 
                            data: this.data,
                            panelId: panel.id,
                            width: width,
                            height: height
                        });
                    }
                    break;
            }

            if (div != null)
                div.id = "frame" + this.data.FrameId;

            // immune to full frame
	        //if (this instanceof DM.PanelUpdater)
	        //    this.freezeOnFullScreen = (this.currentType != "WEATHER");
        }
	    catch (e) {
	        new ErrorReport({ exception: e, source: "_initFrame" }); // <-- shouldn't get here
	    }
	    finally {
	        return obj;
	    }
	},          // <-- for full panel pauses other panels' objects, depending on frame type creates new object and optionally plays it
});

DM.PanelUpdater = Class.create(DM.PanelUpdaterBase, {
	initialize: function ($super, options) {
	    "use strict";
	    $super(options);
		this.freezeOnFullScreen = (options.freezeOnFullScreen || true);
		this._onFrameExpire();
	},

	_onFrameExpire: function (/*$super*/) {
	    "use strict";
	    if (this.freezeOnFullScreen && _canvas.fullScreenActive) {
            // TODO: recalculate expire and frequency when caught up in fullscreen
	        this.expire = this._onFrameExpire.bind(this).delay(1);   // wait 1 sec
	    } else {
	        //$super();
	        this._onGetNextFrame();
        }
	},                // <-- if not behind full frame call _onGetNextFrame, otherwise queue _onFrameExpire again

	_onUpdateEnd: function (/*$super*/) {
	    "use strict";

	    // create new empty container
	    this.oldContainer = $(this.containerId);
        this.newContainer = this.oldContainer
            .clone(false)
            .setStyle({ opacity: 0 }) //display: 'none'
	    ;
        this.oldContainer.id = "x_" + this.oldContainer.id;
        this.oldContainer.insert({ after: this.newContainer });

	    // bail if no more frames
	    if (!this.newFrameId) {
	        this._uninitFrame();
	        this._fadeOut();
	        this.expire = this._onFrameExpire.bind(this).delay(60);  // wait 1 min
	        return;
	    }

	    // set html
	    this.newContainer.update(this.html);

	    // init new frame
	    this.newObj = this._initFrame(this.newContainer);

	    // cross-fade
	    this._crossFade();
	},

	_fadeOut: function () {
	    "use strict";

	    var afterFadeOut = function () {
	        this.oldContainer.remove();
	        //this.oldContainer.setAttribute("collect", "1");
	    };

	    if (this.fadeLength > 0) {
	        this.oldContainer.fade({
	            duration: this.fadeLength,
	            afterFinish: afterFadeOut.bind(this)
	        });
	    } else {
	        afterFadeOut.apply(this);
	    }
	},
	
	_crossFade: function () {
	    "use strict";

	    // 1. if previous object existed wait till new object is ready
	    if (this.object && this.newObj && this.newObj.isReady && !this.newObj.isReady()) {
	        this._crossFade.bind(this).delay(0.1);
	        return;
	    }

	    // 2. uninit old object first, then bind to new object
	    this._uninitFrame();
	    this.object = this.newObj;   // <-- already inited

	    // 3. fade in new container
	    var afterFadeIn = function () {
	        this.newContainer.setStyle({ opacity: 1 });	// display: ''
	    };

	    if (this.fadeLength > 0) {
	        this.newContainer.appear({
	            duration: this.fadeLength,
	            afterFinish: afterFadeIn.bind(this)
	        });
	    } else {
	        afterFadeIn.apply(this);
	    }

	    // 4. fade out old container
	    this._fadeOut();

	    // 5. queue next frame
	    //$super();
	    this.previousType = this.currentType;

	    // queue _onFrameExpire
	    this.expire = this._onFrameExpire
            .bind(this)
            .delay(this.frequency + this.fadeLength)
	    ;
	},
});

DM.FullScreenPanelUpdater = Class.create(DM.PanelUpdaterBase, {
	initialize: function ($super, options) {
	    "use strict";
	    $super(options);
	    this.idleInterval = (this.options.idleInterval || 0);
	    this.screen = $('screen');
		this._getIdleInterval();
		this._onFrameExpire();
	},

	_onFrameExpire: function (/*$super*/) {
	    "use strict";

	    // un-init old frame
	    this._uninitFrame();

		// resume other panels
		_canvas.panels.forEach(function (p) {
			if (p.object && p.object.play) 
				p.object.play();
		});
		
	    // create new screen
	    var oldContainer = $(this.containerId);
        this.newContainer = oldContainer
            .clone(false)
            .setStyle({ display: 'none' })
	    ;
        oldContainer.id = "x_" + oldContainer.id;
        oldContainer.insert({ after: this.newContainer });

	    var afterFadeOut = function () {
	        oldContainer.remove();
	        this.screen.setStyle({ opacity: 0 });	//display: 'none'
	        _canvas.fullScreenActive = false;
	    };

	    // fade out old container and remove it
	    if (this.fadeLength > 0) {
	        this.screen.fade({
	            duration: this.fadeLength,
	            afterFinish: afterFadeOut.bind(this)
	        });
	    } else {
	        afterFadeOut.apply(this);
	    }

        // queue next frame update
	    this.idler = this._onGetNextFrame.bind(this).delay(this.idleInterval);
	},

	_onUpdateEnd: function (/*$super*/) {
	    "use strict";
	    if (!this.newFrameId) {
	        this.expire = this._onFrameExpire.bind(this).delay(this.idleInterval);
	        return;
	    }

	    // set html
	    this.newContainer //= $(this.containerId)
			.update(this.html)
			.setStyle({ display: '' })
	    ;

	    // tell other panels to pause
		_canvas.fullScreenActive = true;
		_canvas.panels.forEach(function (p) {
			if (p.object && p.object.pause) 
				p.object.pause();
		});
		
	    // init new frame
	    this.object = this._initFrame(this.newContainer);

	    // fadeIn new frame
	    this.fadeIn();
	},

	fadeIn: function () {
	    "use strict";
		// 1. wait till new object is ready
	    if (this.object && this.object.isReady && !this.object.isReady()) {
	        this.fadeIn.bind(this).delay(0.1);
	        return;
	    }

	    // 2. fade in new container
	    var afterFadeIn = function () {
	        this.screen.setStyle({ opacity: 1 });	// display: 'block'
	    };

	    if (this.fadeLength > 0) {
	        this.screen.appear({
	            duration: this.fadeLength,
	            afterFinish: afterFadeIn.bind(this)
	        });
	    } else {
	        afterFadeIn.apply(this);
	    }

	    // 3. queue next frame
	    //$super();
	    this.previousType = this.currentType;

	    // queue _onFrameExpire
	    this.expire = this._onFrameExpire
            .bind(this)
            .delay(this.frequency + this.fadeLength)
	    ;
	},

    _getIdleInterval: function () {
	    "use strict";
	    var p = $H({ display: _canvas.displayId });
	    new Ajax.Request("getIdleInterval.ashx", {
	        method: 'get'
            , parameters: p
            , panelUpdater: this
            , evalJSON: false

            , onSuccess: function (resp) {
                try {
                    var json = null;
                    if (resp.responseText.isJSON())
                        json = resp.responseText.evalJSON();
                    if (!json)
                        throw new Error("JSON expected, received ".concat(resp.responseText)); // <-- shouldn't get here
                    if (json.Error)
                        throw new Error("Server error");

                    var p = resp.request.options.panelUpdater;
                    p.idleInterval = json.IdleInterval || p.fadeLength;
                }
                catch (e) {
                    new ErrorReport({ exception: e, data: resp.responseText, source: "onAfterUpdate::onSuccess" });
                }
            }

            , onFailure: function (resp) {
                new ErrorReport({ exception: resp, source: "onAfterUpdate::onFailure" });
            }
	    });

	    this.getInterval = this._getIdleInterval.bind(this).delay(60);     // check every 60 seconds
	},
});


// to prevent webkit issues this func needs to be a global object
function ticker() {
    "use strict";
    // refresh the window every midnight
    var now = new Date();
    if (now.getHours() === 0 && now.getMinutes() === 0) {
        console.log("reload triggered in ticker " + now.toString());
        document.location.reload(true);
        return;
    }
    _canvas.checkDisplayHash();
    _canvas.collectUsedContainers();
}

/**
 * Calculate a 32 bit FNV-1a hash
 * Found here: https://gist.github.com/vaiorabbit/5657561
 * Ref.: http://isthe.com/chongo/tech/comp/fnv/
 *
 * @param {string} str the input value
 * @param {boolean} [asString=false] set to true to return the hash value as 
 *     8-digit hex string instead of an integer
 * @param {integer} [seed] optionally pass the hash of the previous chunk
 * @returns {integer | string}
 */
function hashFnv32a(str, asString, seed) {
    "use strict";
    /*jshint bitwise:false */
    var i, l,
        hval = (seed === undefined) ? 0x811c9dc5 : seed;

    for (i = 0, l = str.length; i < l; i++) {
        hval ^= str.charCodeAt(i);
        hval += (hval << 1) + (hval << 4) + (hval << 7) + (hval << 8) + (hval << 24);
    }
    if (asString) {
        // Convert to 8 digit hex string
        return ("0000000" + (hval >>> 0).toString(16)).substr(-8);
    }
    return hval >>> 0;
}

/*(function () {
    "use strict";
    if (!document.createElement('video').stop)
        Element.addMethods('video', {
            stop: function (e) { e.pause(); }
        });
})();*/

document.observe("dom:loaded", function () {
    "use strict";
	try {
	    // periodic checker
		setInterval(ticker, 60000);

		// init panels
		_canvas.initPanels();
	}
    catch (e) {
        new ErrorReport({ exception: e, source: "dom:loaded" }); // <-- shouldn't get here
    }
});

// 12-08-13 [DPA] - client side scripting END
