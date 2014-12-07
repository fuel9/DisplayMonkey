// 12-08-13 [DPA] - client side scripting BEGIN
// 14-10-06 [LTL] - reload when display hash changes
// 14-10-11 [LTL] - error reporting
// 14-10-16 [LTL] - YouTube support
// 14-10-25 [LTL] - use strict, code improvements

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
        console.log("!Display Monkey error: " + JSON.stringify(this.info));
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

var _canvas = {};
var Canvas = Class.create({
	initialize: function (options) {
	    "use strict";
	    this.fullScreenActive = false;

		var serverTime = moment(options.localTime);
		this.offsetMilliseconds = moment().diff(serverTime);
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
	            this.fullPanel = initFullScreenPanel(pi);
	        else
	            this.panels.push(initPanel(pi, e.id));
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
                    var c = resp.request.options.canvas;
                    var _displayId = json["DisplayId"];
                    if (c.displayId != _displayId)
                        return;
                    var _hash = json["Hash"];
                    if (c.hash != _hash)
                        document.location.reload(true);
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
});

Ajax.PanelUpdaterBase = Class.create(Ajax.Base, {
	initialize: function ($super, options) {
	    "use strict";
	    $super(options);

		/*
		this.options.requestHeaders = (options.requestHeaders||{
		"Pragma":            "no-cache",
		"Pragma":            "no-cache",
		"Cache-Control":     "no-store, no-cache, must-revalidate, post-check=0, pre-check=0",
		"Expires":           new Date(0),
		"Last-Modified":     new Date(0), // January 1, 1970
		"If-Modified-Since": new Date(0)
		});
		*/

		this.panelId = (this.options.panelId || 0);
		this.frequency = (this.options.frequency || 1);
		this.updater = {};
		this.container = this.options.container; // "div" + this.panelId;
		//this.h_container = "h_" + this.container;
		//this.url = "";
		this.html = "";
		this.object = null;

		this.currentId = 0;
		this.previousType = this.currentType = "";
		this.onBeforeUpdate = (this.options.onBeforeUpdate || Prototype.emptyFunction);
		this.onAfterUpdate = (this.options.onAfterUpdate || Prototype.emptyFunction);
		this.onFade = (this.options.onFade || Prototype.emptyFunction);
		this.onBeforeIdle = (this.options.onBeforeIdle || Prototype.emptyFunction);
		this.fadeLength = (this.options.fadeLength || 0);
		if (this.fadeLength < 0) this.fadeLength = 0;

		this.options.onComplete = this._updateEnd.bind(this);
		this.options.onException = this._dispatchException.bind(this);
		this.onBeforeUpdate.bind(this);
		this.onAfterUpdate.bind(this);
		this.onFade.bind(this);
		this.onBeforeIdle.bind(this);
		this._onFrameExpire.bind(this);
		this._onGetNextFrame.bind(this);
	},

	start: function () {
	    "use strict";
	    this._onFrameExpire();
	},

	_hashUrl: function (url) {
	    "use strict";
	    var u = url.split('?'), p = $H();
		if (u.length > 1) p = p.merge(u[1].toQueryParams());
		p.set('ts', (new Date()).getTime());
		return u[0] + '?' + p.toQueryString();
	},

	_onFrameExpire: function () {
	    "use strict";
	    this._onGetNextFrame();
	},

	_onGetNextFrame: function () {
	    "use strict";
	    // get next frame
	    var p = $H({
	        "frame":    this.currentId,
	        "panel":    this.panelId,
	        "display":  _canvas.displayId,
	        "culture":  _canvas.culture,
	        "woeid":    _canvas.woeid,
		    "tempU":    _canvas.temperatureUnit,
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
				    //console.log($H(json).inspect());

                    // get duration first
				    p.frequency = json["Duration"];
				    if (p.frequency == null || p.frequency <= 0)
				        p.frequency = 1;

                    // now get frame id
				    p.currentId = json["FrameId"];
				    if (p.currentId == null || !p.currentId)
					    return p._updateEnd();

				    p.currentType = json["FrameType"];
				    p.html = json["Html"];
				    /*p.url = "getFrame.ashx?" + $H({
					    "frame": p.currentId,
					    "panel": p.panelId,
					    "type": p.currentType,
					    "display": _canvas.displayId,
		                "woeid": _canvas.woeid,
				        "temperatureUnit": _canvas.temperatureUnit
                    }).toQueryString();*/
				    //console.log(p.url);

				    p._updateBegin();
			    }
			    catch (e) {
			        new ErrorReport({ exception: e, data: resp.responseText, source: "_onGetNextFrame::onSuccess" });
			        p._updateEnd();
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
		        var p = resp.request.options.panelUpdater;
		        p._updateEnd();
		    }
		});
	},

	_updateBegin: function () {
	    "use strict";
	    try { this.onBeforeUpdate(this.currentType); }
		catch (e) {
		    new ErrorReport({ exception: e, source: "_updateBegin::onBeforeUpdate" });
		}
	    //this.updater = new Ajax.Updater(this.h_container, this._hashUrl(this.url), this.options);
	    this._updateEnd();
	},

    // Ajax.Updater callback
	_dispatchException: function (e) {
	    "use strict";
	    new ErrorReport({ exception: e, source: "_dispatchException" }); // <-- shouldn't get here
	},

    // Ajax.Updater callback
	_updateEnd: function (response) {
	    "use strict";
	    var needRedraw = (
			this.previousType != this.currentType ||
			$(this.container).innerHTML != this.html //$(this.h_container).innerHTML
		);

	    if (!needRedraw) {
	        //$(this.h_container).update("");
			this.expire = this._onFrameExpire.bind(this).delay(this.frequency);
			return;
		}

		// fade out first
		if (this.fadeLength > 0) {
			try { this.onFade(false, this.previousType, this.fadeLength); }
			catch (e) {
			    new ErrorReport({ exception: e, source: "_updateEnd::onFade" });
            }
			this.fader = this._fadeOutEnd.bind(this).delay(this.fadeLength);
		}
		else
			this._fadeOutEnd();
	},

	_fadeOutEnd: function () {
	    "use strict";
	    this._beginNewFrame();
	},

	_beginNewFrame: function () {
	    "use strict";

		// substitute html
		this.previousType = this.currentType;
		/*var html = $(this.h_container).innerHTML;
		$(this.h_container).update("");
		$(this.container).update(html);*/
		$(this.container).update(this.html);

		// 1. call after update
		try { this.onAfterUpdate(this.previousType); }
		catch (e) {
		    new ErrorReport({ exception: e, source: "_beginNewFrame::onAfterUpdate" });
        }

		// 2. fade in last
		if (this.fadeLength > 0) {
			try { this.onFade(true, this.currentType, this.fadeLength); }
			catch (e) {
			    new ErrorReport({ exception: e, source: "_beginNewFrame::onFade" });
            }
			this.fader = this._fadeInEnd.bind(this).delay(this.fadeLength);
		}
		else
			this._fadeInEnd();
	},

	_fadeInEnd: function () {
	    "use strict";
	    this.expire = this._onFrameExpire.bind(this).delay(this.frequency);
	},

	_initFrame: function (currentType) {
	    "use strict";
        try {
            // pause others
            if (this instanceof Ajax.FullScreenPanelUpdater) {
                _canvas.panels.forEach(function (p) {
                    if (p.object && p.object.pause) p.object.pause();
                    /*if (p.ytPlayer) p.ytPlayer.pause();
                    if (p.video) p.video.pause();
                    if (p.scroller) p.scroller.pause();*/
                });
            }

            var div = null;

            // start scroller
	        if (div = $(this.container).down('div#memo')) {
	            //this.scroller = new TextScroller(div);
	            this.object = new TextScroller(div);
            }

	        // start clock
	        else if (div = $(this.container).down('div#clock')) {
	            //this.clock = new Clock(div);
	            this.object = new Clock(div);
            }

	        // start video
	        else if ((div = $(this.container).down('video')) && _canvas.supports_video) {
	            //this.video = div;
	            this.object = div;
	            var a;
	            if (a = div.readAttribute('loop')) div.loop = a;
	            if (a = div.readAttribute('muted')) div.muted = a;
	            if (this instanceof Ajax.FullScreenPanelUpdater || !_canvas.fullScreenActive) {
	                div.play();
	            }
	            var vc = div.up('div#videoContainer');
	            if (vc) {
	                vc.style.backgroundColor = _canvas.backColor;
	            }
            }

            // start youtube
	        else if (div = $(this.container).down('div[id^=ytplayer]')) {
	            //this.ytPlayer = new YtLib.YtPlayer({ div: div });
	            this.object = new YtLib.YtPlayer({ div: div });
            }

            // start outlook
	        else if (div = $(this.container).down('div#outlook')) {
	            //this.outlook = new Outlook({
	            this.object = new Outlook({
	                div: div,
	                frameId: this.currentId,
	                panelId: this.panelId
	            });
	        }

            // immune to full frame
	        if (this instanceof Ajax.PanelUpdater)
	            this.freezeOnFullScreen = (currentType != "WEATHER");
        }
	    catch (e) {
	        new ErrorReport({ exception: e, source: "_initFrame" }); // <-- shouldn't get here
	    }
	},

	_uninitFrame: function (currentType) {
	    "use strict";
	    try {
	        // kill scroller if any
	        /*if (this.scroller) {
	            this.scroller.stop();
            }

	        // kill clock
	        if (this.clock) {
	            this.clock.stop();
            }

	        // kill video
	        if (this.video) {
                this.video.stop();
	        }

	        // kill youtube
	        if (this.ytPlayer) {
	            this.ytPlayer.stop();
	        }

	        // kill outlook
	        if (this.outlook) {
	            this.outlook.stop();
	        }*/

	        if (this.object && this.object.stop) {
	            this.object.stop()
	        }

	        // resume others
	        if (this instanceof Ajax.FullScreenPanelUpdater) {
	            _canvas.panels.forEach(function (p) {
	                if (p.object && p.object.play) p.object.play();
	                /*if (p.ytPlayer) p.ytPlayer.play();
	                if (p.video) p.video.play();
	                if (p.scroller) p.scroller.resume();*/
	            });
	        }

        }
	    catch (e) {
	        new ErrorReport({ exception: e, source: "_uninitFrame" }); // <-- shouldn't get here
	    }
	    finally {
	        /*this.scroller = null;
	        this.clock = null;
	        this.video = null;
	        this.ytPlayer = null;*/
	        this.object = null;
        }
	},
});

Ajax.PanelUpdater = Class.create(Ajax.PanelUpdaterBase, {
	initialize: function ($super, options) {
	    "use strict";
	    $super(options);
		this.freezeOnFullScreen = (options.freezeOnFullScreen || true);
		this.start();
	},

	_onFrameExpire: function ($super) {
	    "use strict";
	    if (this.freezeOnFullScreen && _canvas.fullScreenActive) {
            // TODO: recalculate expire and frequency when caught up in fullscreen
	        this.expire = this._onFrameExpire.bind(this).delay(this.frequency);
	    } else {
	        $super();
	    }
	},
});

Ajax.FullScreenPanelUpdater = Class.create(Ajax.PanelUpdaterBase, {
	initialize: function ($super, options) {
	    "use strict";
	    $super(options);
		this.idler = {};
		this.idleInterval = (this.options.idleInterval || 0);
		this.onBeforeIdle = (this.options.onBeforeIdle || Prototype.emptyFunction);
		this.onBeforeIdle.bind(this);
		this.start();
	},

	_onFrameExpire: function () {
	    "use strict";
	    if (this.fadeLength > 0) {
			try { this.onFade(false, this.previousType, this.fadeLength); }
			catch (e) {
			    new ErrorReport({ exception: e, source: "_onFrameExpire::onFade" });
            }

			// 2. start fader
			this.fader = this._fadeOutEnd.bind(this).delay(this.fadeLength);
		}
		else
			this._fadeOutEnd();
	},

	_fadeOutEnd: function () {
	    "use strict";
	    try { this.onBeforeIdle(this.currentType); }
		catch (e) {
		    new ErrorReport({ exception: e, source: "_fadeOutEnd::onBeforeIdle" });
        }
		this.idler = this._onGetNextFrame.bind(this).delay(this.idleInterval);
	},

	_updateEnd: function (response) {
	    "use strict";
	    var needRedraw = (
			this.currentId > 0
		);

		if (!needRedraw) {
			this.expire = this._onFrameExpire.bind(this).delay(this.frequency);
			return;
		}
		
		this._beginNewFrame();
	},
});


function initPanel (panelId, container) {
    "use strict";
    return new Ajax.PanelUpdater({
		method: 'get'
		, panelId: panelId
		, container: container
		, evalScripts: false
		, fadeLength: 0.2 // sec (default)

		, onBeforeUpdate: function (currentType) {
		    this._uninitFrame(currentType);
		}

		, onAfterUpdate: function (currentType) {
		    this._initFrame(currentType);
		}

		, onFade: function (appear, contentType, fadeLength) {
			if (appear)
				$(this.container).appear({ duration: fadeLength });
			else
				$(this.container).fade({ duration: fadeLength });
		}

		, onException: function (request, ex) {
			new ErrorReport({ exception: new Error(ex.description), source: "onException" }); // <-- shouldn't get here
		}
	});
}

function initFullScreenPanel (panelId) {
    "use strict";
    return new Ajax.FullScreenPanelUpdater({
		method: 'get'
		, panelId: panelId
		, container: 'full'
		, evalScripts: false
		, fadeLength: 1 // sec (default)
		, idleInterval: _canvas.initialIdleInterval

		 //, onBeforeUpdate: function (currentType) {
		 //}

		, onAfterUpdate: function (currentType) {

		    _canvas.fullScreenActive = true;
			$("screen").style.display = "block";

			this._initFrame(currentType);

			// obtain idle interval
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
						var p = resp.request.options.panelUpdater;
						p.idleInterval = json["IdleInterval"];
					}
					catch (e) {
					    new ErrorReport({ exception: e, data: resp.responseText, source: "onAfterUpdate::onSuccess" });
					}
				}

				, onFailure: function (resp) {
				    new ErrorReport({ exception: resp, source: "onAfterUpdate::onFailure" });
				}
			});
		}

		, onBeforeIdle: function (currentType) {
			$("screen").style.display = "none";
			_canvas.fullScreenActive = false;

			this._uninitFrame(currentType);
		}

		, onFade: function (appear, contentType, fadeLength) {
			if (appear)
				$(this.container).appear({ duration: fadeLength });
			else
				$(this.container).fade({ duration: fadeLength });
		}

		, onException: function (request, ex) {
			new ErrorReport({ exception: new Error(ex.description), source: "onException" }); // <-- shouldn't get here
		}
	});
}


// to prevent webkit issues this func needs to be a global object
function ticker() {
    "use strict";
    // refresh the window every midnight
    var now = new Date();
    if (0 == now.getHours() == now.getMinutes()) {
        document.location.reload(true);
        return;
    }
    _canvas.checkDisplayHash();
}

(function () {
    "use strict";
    if (!document.createElement('video').stop)
        Element.addMethods('video', {
            stop: function (e) { e.pause(); }
        });
})();

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
