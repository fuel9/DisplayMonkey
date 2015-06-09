// 2012-08-13 [DPA] - client side scripting BEGIN
// 2014-10-25 [LTL] - use strict, code improvements
// 2015-02-15 [LTL] - SVG analog face
// 2015-03-08 [LTL] - using data
// 2015-05-08 [LTL] - ready callback
// 2015-06-09 [LTL] - refinements

DM.Clock = Class.create(DM.FrameBase, {
    initialize: function ($super, options) {
        "use strict";
        $super(options, 'div.clock');
        var data = options.panel.data;
        this.showDate = !!data.ShowDate;
        this.showTime = !!data.ShowTime;
        this.faceType = data.Type || 0;
        this.offsetMilliseconds = _canvas.time.diff(data.OffsetGmt ?
            _canvas.utcTime.add(data.OffsetGmt, 'm') :
            _canvas.locationTime);
        this.showSeconds = !!data.ShowSeconds;

        this.div.setStyle({ width: this.width + "px", height: this.height + "px" });

        this.label = null;
        var label = this.div.down('.clockLabel');
        if (data.Label && this.faceType) {
            this.label = label.update(data.Label);
        } else {
            label.parentNode.removeChild(label);
        }

        switch (this.faceType) {
            case 1: this._initAnalog(data); break;
            case 2: this._initDigital(data); break;
            default: this._initText(data); break;
        }

        //console.log(this.div.innerHTML);

        this.timer = setInterval(this._callBack.bind(this), 1000);
        this._callBack();
    },
	
    stop: function ($super) {
        "use strict";
        $super();
	    clearInterval(this.timer);
	    this.timer = null;
    },

    _setContainer: function(cls) {
        this.container = null;
        this.div.select('div[class!=clockLabel]').each(function (e) {
            if (e.getAttribute('class') == cls)
                this.container = e;
            else
                e.parentNode.removeChild(e);
        }, this);
        if (!this.label)
            this.container.style.height = "100%";
        return this.container;
    },

    _initText: function (data) {
	    "use strict";
	    this._setContainer('clockText');
	},

    _initAnalog: function (data) {
	    "use strict";
	    var supportSvg = document.implementation.hasFeature("http://www.w3.org/TR/SVG11/feature#BasicStructure", "1.1");
	    if (supportSvg) {
	        this._setContainer('svgAnalog');
	        this.container.setStyle({ height: "" + (this.div.getHeight() - this.container.offsetTop) + "px" });
	        this.elemHour = this.div.down('#hourHand');
	        this.elemMin = this.div.down('#minuteHand');
	        this.elemSec = this.div.down('#secondHand');
	        if (!this.showSeconds)
	            this.elemSec.setAttribute('visibility', 'hidden');
	    } else {
	        this._setContainer('bmpAnalog');
	        var w = this.container.getWidth(), h = this.div.getHeight() - this.container.offsetTop, s = w > h ? h : w, px = "" + s + "px ";
	        var face = this.container.down('.analogFace').setStyle({ backgroundSize: px + px, width: px, height: px });
	        this.elemHour = face.down('.analogHour').setStyle({ backgroundSize: px + px, width: px, height: px });
	        this.elemMin = face.down('.analogMin').setStyle({ backgroundSize: px + px, width: px, height: px });
	        this.elemSec = face.down('.analogSec').setStyle({ backgroundSize: px + px, width: px, height: px });
	        if (!this.showSeconds)
	            this.elemSec.setAttribute('visibility', 'hidden');
	    }
	    this.showDate = false;
	},

    _initDigital: function (data) {
        "use strict";
        //this._initText();
    },

    _callBack: function () {
        "use strict";
        if (!this.timer || this.exiting)
            return;

        var time = moment();
        if (this.offsetMilliseconds > 0)
            time.add('ms', this.offsetMilliseconds);
        else
            time.subtract('ms', this.offsetMilliseconds);

        switch (this.faceType) {
            case 0:
                var d = this.showDate ? time.format(_canvas.dateFormat) : "";
                var t = this.showTime ? time.format(_canvas.timeFormat) : "";
                /*this.div*/ this.container.innerHTML = d + (d != "" && t != "" ? "<br>" : "") + t;
                break;

            case 1:
                var sec = time.seconds();
                var min = time.minutes();
                var hrs = time.hours(); if (hrs > 12) hrs -= 12;
                this._rotateHand(this.elemHour, hrs * 30 + (min * 0.5));
                this._rotateHand(this.elemMin, min * 6);
                if (this.showSeconds)
                    this._rotateHand(this.elemSec, sec * 6);
                break;
        }

        this.ready();
    },

    _rotateHand: function (e, r) {
        "use strict";
        if (e.tagName === "g") {
            e.setAttribute("transform", "rotate(" + r + ", 100, 100)");
        } else {
            r = "rotate(" + r + "deg)";
            e.setStyle({
                "transform": r,
                "-moz-transform": r,
                "-webkit-transform": r,
                "-ms-transform": r,
                "-o-transform": r
            });
        }
    },
});

// 12-08-13 [DPA] - client side scripting END
