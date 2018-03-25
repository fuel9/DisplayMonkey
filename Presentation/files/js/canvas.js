/*!
* DisplayMonkey source file
* http://displaymonkey.org
*
* Copyright (c) 2015 Fuel9 LLC and contributors
*
* Released under the MIT license:
* http://opensource.org/licenses/MIT
*/

// 2012-08-13 [DPA] - client side scripting BEGIN
// 2014-10-06 [LTL] - reload when display hash changes
// 2014-10-11 [LTL] - error reporting
// 2014-10-16 [LTL] - YouTube support
// 2014-10-25 [LTL] - use strict, code improvements
// 2015-01-30 [LTL] - minor code improvements
// 2015-02-06 [LTL] - major overhaul
// 2015-03-08 [LTL] - using data for frames
// 2015-03-27 [LTL] - fixed YT Flash w/ FF & IE
// 2015-05-08 [LTL] - frame ready callback
// 2015-05-15 [LTL] - noscroll
// 2015-06-09 [LTL] - regular panel expiration no longer depends on fullscreen
// 2015-06-25 [LTL] - added readyTimeout
// 2015-07-28 [LTL] - minor improvements
// 2015-07-29 [LTL] - RC13: performance and memory management improvements
// 2015-09-03 [LTL] - RC15: added canvas recycleTime handling
// 2017-02-06 [LTL] - #11: added error CSS

var $j = {};
if (typeof jQuery === 'object') {
    $j = jQuery.noConflict();
}

var DM = {}, _canvas = {};

DM.ErrorReport = Class.create({
    initialize: function (options) {
        "use strict";
        this.info = {
            Error: ((options.exception instanceof Error ? options.exception.message : options.exception) || "unspecified"),
            Where: (options.source || "unspecified"),
            When: moment().format(),
            Data: (JSON.stringify(options.data) || "none").replace(/\\"|"/g, ''),
        };
        console.error("!Display Monkey error: " + JSON.stringify(this.info));
        this.length = options.length || _canvas.errorLength || 0;
        if (this.length) {
            this.show(this.length);
        }
    },

    show: function (length) {
        "use strict";
        var div = /*$('error') ||*/ new Element('div', { id: 'error' });
        div.update(div.innerHTML.concat(
            "<table>",
            "<tr><td class='errorMsg'>Error:</td><td>#{Error}</td></tr>",
            "<tr><td class='errorWhere'>Where:</td><td>#{Where}</td></tr>",
            "<tr><td class='errorWhen'>When:</td><td>#{When}</td></tr>",
            _canvas.errorInfo ? "<tr><td class='errorInfo'>Info:</td><td>#{Data}</td></tr>" : "",
            "</table>"
            ).interpolate(this.info)
        );
        $(document.body).insert({ bottom: div });
        var hide = function () {
            div.fade({
                duration: 1,
                from: 1,
                to: 0,
                afterFinish: function () { div.remove(); },
            });
        };
        length = length || this.length;
        hide.delay(length < 1 ? 1 : length);
    },
});

DM.Canvas = Class.create({
	initialize: function (options) {
	    "use strict";
	    this.fullScreenActive = false;

		this.time = moment();
		moment.locale(options.culture || "en");
	    this.locationTime = moment(options.locationTime);
	    this.utcTime = moment(options.utcTime);
	    this.displayId = options.displayId;
		this.hash = (options.hash || 0);
		this.dateFormat = (options.dateFormat || 'LL');
		this.timeFormat = (options.timeFormat || 'LT');
		this.latitude = (options.latitude || 0);
		this.longitude = (options.longitude || 0);
		this.woeid = (options.woeid || 0);
		this.culture = (options.culture || "");
		this.temperatureUnit = (options.temperatureUnit || 'c');
		this.noScroll = (options.noScroll || false);
		this.noCursor = (options.noCursor || false);
		this.readyTimeout = (options.readyTimeout || 0);
		this.fsIdleInterval = (options.initialIdleInterval || 0);   // !!!
		this.pollInterval = (options.pollInterval || 0);
		this.errorLength = (options.errorLength || 0);
		this.errorInfo = (options.errorInfo || true);
		this.width = (options.width || 0);
		this.height = (options.height || 0);
		this.backImage = (options.backImage || 0);
		this.backColor = (options.backColor || 'transparent');
		this.recycleTime = null;

		this.supports_video = !!document.createElement('video').canPlayType;
		this._initializedPanels = false;

		this.panels = [];
		this.fullPanel = {};

		window.addEventListener('message', this._postback.bind(this));
	},

	initPanels: function () {
	    "use strict";
	    if (this._initializedPanels)
	        return;
	    this._initializedPanels = true;

	    var body = $(document.body),
            segs = $('segments'),
            scr = $('screen')
	    ;

	    scr.setStyle({ opacity: 0 });

	    if (this.noScroll) {
	        body.addClassName("noscroll");
	    }

	    if (this.noCursor) {
	        body.addClassName("nocursor");
	    }

	    if (this.backImage > 0) {
	        segs.style.backgroundImage = "url('getImage.ashx?content=" + this.backImage + "')";
	    }

	    if (this.backColor != '') {
	        body.style.backgroundColor = this.backColor;
	        segs.style.backgroundColor = this.backColor;
	        scr.style.backgroundColor = this.backColor;
        }

	    this.fixScreenDiv();
	    Event.observe(window, "resize", this.fixScreenDiv);

	    $$('div[data-panel-id]').each(function (e) {
	        var pi = parseInt(e.readAttribute('data-panel-id'), 10),
	            fl = parseFloat(e.readAttribute('data-fade-length') || 0)
	        ;
	        if (e.id === "full") {
	            _canvas.fullPanel = new DM.FullScreenPanel({
	                panelId: pi,
	                container: e.id,
	                fadeLength: fl,
	            });
	        } else {
	            _canvas.panels.push(new DM.Panel({
	                panelId: pi,
	                container: e.id,
	                fadeLength: fl,
	            }));
	        }
	    });
	},

	pausePanels: function () {
	    _canvas.fullScreenActive = true;
	    _canvas.panels.forEach(function (p) {
	        try {
	            if (p.object && typeof p.object.pause === "function")
	                p.object.pause();
	        } catch (e) { }
	    });
	},

	resumePanels: function () {
	    _canvas.fullScreenActive = false;
	    _canvas.panels.forEach(function (p) {
	        try {
	            if (p.object && typeof p.object.play === "function")
	                p.object.play();
	        } catch (e) { }
	    });
	},

	fixScreenDiv: function () {
	    "use strict";
	    var body = $(document.body);
		var scr = $('screen').style;
		scr.height = body.clientHeight + 'px';
		scr.width = body.clientWidth + 'px';
	},

	_postback: function (evt) {
	    "use strict";
	    try {
	        if (evt.data) {
	            var msg = JSON.parse(evt.data);
	            if (msg.error) {
	                new DM.ErrorReport({ exception: msg.error, data: msg, source: "DM.Canvas::_postback" });
	            }
	        }
	    }
	    catch (e) { }
	},
});

DM.Canvas.Collector = function () {
    "use strict";

    // refresh the window every midnight
    if (_canvas.recycleTime) {
        var now = new Date();
        if (now.getHours() == _canvas.recycleTime.Hours && now.getMinutes() == _canvas.recycleTime.Minutes) {
            console.log("auto-recycle triggered at " + now.toString());
            document.location.reload(true);
            return;
        }
    }

    // force garbage collector
    if (typeof CollectGarbage === "function") {
        CollectGarbage();
    }
};

DM.Canvas.CheckDisplay = function () {
    "use strict";

    // first off, queue us again
    DM.Canvas.CheckDisplay.delay(_canvas.pollInterval);

    new Ajax.Request("getDisplayData.ashx", {
        method: 'get'
        , parameters: $H({ display: _canvas.displayId })
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

                if (_canvas.displayId != json.DisplayId)
                    return;

                if (_canvas.hash && _canvas.hash != json.Hash) {
                    console.log("recycle triggered in DM::Canvas::CheckDisplay::getDisplayData::onSuccess");
                    document.location.reload(true);
                    return;
                }

                _canvas.hash = json.Hash || _canvas.hash;
                _canvas.fsIdleInterval = json.IdleInterval || _canvas.fsIdleInterval;
                _canvas.recycleTime = json.RecycleTime;
                _canvas.initPanels();
            }
            catch (e) {
                new DM.ErrorReport({ exception: e, data: resp.responseText, source: "DM::Canvas::CheckDisplay::getDisplayData::onSuccess" });
            }
        }

        , onFailure: function (resp) {
            new DM.ErrorReport({ exception: resp, source: "DM::Canvas::CheckDisplay::getDisplayData::onFailure" });
        }
    });
};


DM.FrameBase = Class.create({
    initialize: function (options, element) {
        "use strict";
        this.onFrameReady = options.panel.onFrameReady.bind(options.panel);

        this.panelId = options.panel.panelId || 0;
        this.frameId = options.panel.newFrameId || 0;

        var panel = options.panel.newContainer;
        this.div = panel.down(element);
        this.div.id = "frame" + this.frameId;

        this.width = panel.getAttribute('data-panel-width') || panel.clientWidth;
        this.height = panel.getAttribute('data-panel-height') || panel.clientHeight;

        this.exiting = false;
        this.updating = false;

        var readyTimeoutCallback = function () {
            "use strict";
            this.ready();
        };

        this.readyTimer = readyTimeoutCallback
            .bind(this)
            .delay(_canvas.readyTimeout)
        ;
    },

    ready: function () {
        "use strict";
        this.readyTimeoutClear();
        var foo = this.onFrameReady; this.onFrameReady = null;
        if (typeof foo === "function") foo();
    },

    uninit: function () {
        "use strict";
        this.readyTimeoutClear();
        this.stop();
    },

    stop: function () {
        "use strict";
        this.exiting = true;
    },

    pause: function () {
        "use strict";
    },

    play: function () {
        "use strict";
    },

    readyTimeoutClear: function () {
        "use strict";
        if (this.readyTimer) {
            clearTimeout(this.readyTimer);
            this.readyTimer = 0;
        }
    },

    _hashUrl: function (url) {
        "use strict";
        var u = url.split('?'), p = $H();
        if (u.length > 1) p = p.merge(u[1].toQueryParams());
        p.set('ts', (new Date()).getTime());
        return u[0] + '?' + p.toQueryString();
    },
});

DM.FrameBase.Reclaim = function (id) {
    $$("#" + id).each(function (_x) {
        try {
            _x.select("iframe").each(function (_f) {
                _f.src = "about:blank";
            });
            _x.remove();
        }
        catch (e) { }
    });
};


DM.PanelBase = Class.create(Ajax.Base, {
	initialize: function ($super, options) {
	    "use strict";
	    $super(options);

		this.panelId = (this.options.panelId || 0);
		this.frameLength = (this.options.frameLength || 1);
		this.containerId = this.options.container;
		this.fadeLength = (this.options.fadeLength || 0);
		if (this.fadeLength < 0) this.fadeLength = 0;
		this.data = {
		    FrameId: this.newFrameId = 0,
		    FrameType: this.newType = -1,
		    Duration: 0,
		};

		this._onGetNextFrame.bind(this);
	},

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

				    p.newFrameId = json.FrameId || 0;
				    p.newType = json.FrameType;
				    var l = json.Duration || 0;
				    p.frameLength = p.newFrameId && l < 0 || !p.newFrameId ? 0 : l;
				    p.data = $(json);
                }
			    catch (e) {
			        new DM.ErrorReport({ exception: e, data: resp.responseText, source: "_onGetNextFrame::onSuccess" });
			    }
		        finally {
		            p._onUpdateEnd();
		        }
		    }

		    , onFailure: function (resp) {
		        new DM.ErrorReport({ exception: resp.toString(), source: "_onGetNextFrame::onFailure", data: resp });
		        resp.request.options.panelUpdater._onUpdateEnd();
		    }
		});
	},               // <-- get HTML via Ajax, then calls _onUpdateEnd

	_uninitFrame: function () {
	    "use strict";
	    try {
	        if (this.object) {
	            this.object.uninit();
	            delete this.object;
	        }
        }
	    catch (e) {
	        new DM.ErrorReport({ exception: e, source: "_uninitFrame" }); // <-- shouldn't get here
	    }
	    //finally {
	    //    this.object = null;
	    //}
	},                 // <-- stops and destroys current object; for full panel resumes objects in other panels

	_initFrame: function () {
	    "use strict";
	    var obj = null;
        try {
            switch (this.newType) {
                //Clock
                case 0:
                    obj = new DM.Clock({
                        panel: this
                    });
                    break;
                    
                //Html
                case 1:
                    obj = new DM.Iframe({
                        panel: this
                    });
                    break;

                //Memo
                case 2:
                    obj = new DM.Memo({
                        panel: this
                    });
                    break;
                    
                //News = 3
                //Outlook = 4
                case 4:
                    obj = new DM.Outlook({
                        panel: this
                    });
                    break;
                    
                //Picture = 5
                //Report = 6
                case 5:
                case 6:
                    obj = new DM.Picture({
                        panel: this
                    });
                    break;
                    
                //Video
                case 7:
                    obj = new DM.Video({
                        panel: this,
                        play: (this instanceof DM.FullScreenPanel || !_canvas.fullScreenActive)
                    });
                    break;
                    
                //Weather
                case 8:
                    obj = new DM.Weather({
                        panel: this
                    });
                    break;

                //YouTube
                case 9:
                    obj = new DM.YtPlayer({
                        panel: this
                    });
                    break;

                //Power BI
                case 10:
                    obj = new DM.Powerbi({
                        panel: this
                    });
                    break;
            }
        }
	    catch (e) {
	        new DM.ErrorReport({
	            exception: e,
	            source: "_initFrame",
	            data: { frameId: this.newFrameId, panel: this.panelId }
	        });
	    }
	    finally {
	        return obj;
	    }
	},          // <-- for full panel pauses other panels' objects, depending on frame type creates new object and optionally plays it
});

DM.Panel = Class.create(DM.PanelBase, {
    initialize: function ($super, options) {
        "use strict";
        $super(options);
        this.countdown = 0.0;
        this.step = 1.0;  // sec
        this._onFrameExpire();
    },

    _onFrameExpire: function (/*$super*/) {
        "use strict";
        if (_canvas.fullScreenActive || --this.countdown > 0) {
            this._onFrameExpire
                .bind(this)
                .delay(this.step)
            ;
        } else {
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
        this.oldContainer.id = "x_" + this.containerId;
        this.oldContainer.insert({ after: this.newContainer });

        // bail if no more frames
        if (!this.newFrameId) {
            this._uninitFrame();
            this._fadeOut();
            this._onFrameExpire
                .bind(this)
                .delay(_canvas.pollInterval)
            ;
            return;
        }

        // set html
        this.newContainer.update(this.data.Html);

        // init new frame
        this.newObj = this._initFrame(this.newContainer);
    },
	
    onFrameReady: function () {
        "use strict";

        // uninit old object first, then bind to new object
        this._uninitFrame();
        this.object = this.newObj;   // <-- already inited

        // fade in new container
        (function (con, l) {
            var done = function () {
                con.setStyle({ opacity: 1 });	// display: ''
            };

            // fade out old container and remove it
	        if (l)
	            con.appear({ duration: l, afterFinish: done });
	        else
	            done();
        })(this.newContainer, this.fadeLength);

	    // queue _onFrameExpire
	    this.countdown = (this.frameLength + this.fadeLength) / this.step;
	    this._onFrameExpire
            .bind(this)
            .delay(this.step)
	    ;

	    // fade out old container
	    this._fadeOut();
	},

	_fadeOut: function () {
	    "use strict";

	    if (/^x_/.exec(this.oldContainer.id)) {
	        var done = (function (id) {
	            return function () {
	                DM.FrameBase.Reclaim(id);
	            }
	        })(this.oldContainer.id);

	        if (this.fadeLength)
	            this.oldContainer.fade({ duration: this.fadeLength, afterFinish: done });
	        else
	            done();
	    }

	    this.oldContainer = {};
	},
});

DM.FullScreenPanel = Class.create(DM.PanelBase, {
	initialize: function ($super, options) {
	    "use strict";
	    $super(options);
	    this.screen = $('screen');
	    this.screen.setStyle({ display: 'none' });
	    this._onFrameExpire();
	},

	_onFrameExpire: function (/*$super*/) {
	    "use strict";

	    // blank frame?
	    if (!this.newFrameId) {
	        this._onGetNextFrame
                .bind(this)
                .delay(_canvas.fsIdleInterval)
	        ;
	        return;
	    }

	    // un-init old frame
	    this._uninitFrame();

	    // create new container
	    var oldContainer = $(this.containerId);
	    this.newContainer = oldContainer
            .clone(false)
            .setStyle({ display: '' })
	    ;
	    oldContainer.id = "x_" + this.containerId;
	    oldContainer.insert({ after: this.newContainer });

	    // fade out old container and remove it
	    (function (id, screen, l) {
	        var done = function () {
	            screen.setStyle({ opacity: 0, display: 'none' });
	            _canvas.resumePanels();
	            DM.FrameBase.Reclaim(id);
	        };

	        if (l)
	            screen.fade({ duration: l, afterFinish: done });
	        else
	            done();

	    })(oldContainer.id, this.screen, this.fadeLength);

	    oldContainer = {};

        // queue next frame update
	    this._onGetNextFrame
            .bind(this)
            .delay(_canvas.fsIdleInterval + this.fadeLength)    // !!! must add fade length to avoid cross-fader issues
	    ;
	},

	_onUpdateEnd: function (/*$super*/) {
	    "use strict";

	    // blank frame?
	    if (!this.newFrameId) {
	        if (this.object) {
	            this._onFrameExpire();  // clear previous frame
	        } else {
	            this._onGetNextFrame
                    .bind(this)
                    .delay(_canvas.fsIdleInterval)
	            ;
	        }
            return;
	    }

	    // set html
	    this.newContainer = (this.newContainer || $(this.containerId))
			.update(this.data.Html)
	        .setStyle({ display: '' })
	    ;

	    // init new frame
	    this.object = this._initFrame(this.newContainer);
	},

	onFrameReady: function () {
	    "use strict";

	    // tell other panels to pause
	    _canvas.pausePanels();

	    // fade in new container
	    (function (screen, l) {
	        var done = function () {
	            screen.setStyle({ opacity: 1 });
	        };

	        screen.setStyle({ display: '' });
	        if (l)
	            screen.appear({ duration: l, afterFinish: done });
	        else
	            done();

	    })(this.screen, this.fadeLength);


	    // queue _onFrameExpire
	    this._onFrameExpire
            .bind(this)
            .delay(this.frameLength + this.fadeLength)
	    ;
	},
});


/////////////////////////////////////////////////////////////////////////////////////////////////
document.observe("dom:loaded", function () {
    "use strict";
	try {
	    Element.addMethods({
	        setAll: function (ctx, filter, value, def) {
	            $(ctx).select(filter).each(function (_ea) { _ea.update(value || def || ""); });
	            return ctx;
	        },
	    });
	    Number.prototype.pad = function (size) {
	        var s = String(this);
	        while (s.length < (size || 0)) { s = "0" + s; }
	        return s;
	    };
	    setInterval(DM.Canvas.Collector, 60000);
	    DM.Canvas.CheckDisplay();
	}
    catch (e) {
        new DM.ErrorReport({ exception: e, source: "dom:loaded", length: 60 }); // <-- shouldn't get here
    }
});
/////////////////////////////////////////////////////////////////////////////////////////////////

// 12-08-13 [DPA] - client side scripting END
