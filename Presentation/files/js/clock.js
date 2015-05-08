// 2012-08-13 [DPA] - client side scripting BEGIN
// 2014-10-25 [LTL] - use strict, code improvements
// 2015-02-15 [LTL] - SVG analog face
// 2015-03-08 [LTL] - using data
// 2015-05-08 [LTL] - ready callback

DM.Clock = Class.create(DM.FrameBase, {
    initialize: function ($super, options) {
        "use strict";
        $super(options, 'div.clock');
        var data = options.panel.data;
        this.showDate = !!data.ShowDate;
        this.showTime = !!data.ShowTime;
        this.faceType = data.Type || 0;
        var time = moment();
        this.offsetMilliseconds = time.diff(data.OffsetGmt ?
            _canvas.utcTime.add(data.OffsetGmt, 'm') :
            _canvas.locationTime);
        this.showSeconds = !!data.ShowSeconds;
        this.useSvg = document.implementation.hasFeature("http://www.w3.org/TR/SVG11/feature#BasicStructure", "1.1");

        switch (this.faceType) {
            case 1: this._initAnalog(); break;
            case 2: this._initDigital(); break;
            default: this._initText(); break;
        }

        var label = this.div.select('.clockLabel')[0];
        if (label.innerText === '')
            label.hide();

        this.timer = setInterval(this._callBack.bind(this), 1000);
        this.ready();
    },
	
    stop: function ($super) {
        "use strict";
        $super();
	    clearInterval(this.timer);
	    this.timer = null;
	},

	_initText: function () {
	    "use strict";
	    this.div.select('svg').each(function (e) {
	        e.parentNode.removeChild(e);
	    });
	    //console.log(this.div.innerHTML);
	},

	_initAnalog: function () {
	    "use strict";
	    var s = this.width > this.height ? this.height : this.width;
	    this.div.style.width = this.div.style.height = s + "px";
	    if (this.useSvg) {
	        this.elemHour = this.div.select('#hourHand')[0];
	        this.elemMin = this.div.select('#minuteHand')[0];
	        this.elemSec = this.div.select('#secondHand')[0];
	        if (!this.showSeconds)
	            this.elemSec.setAttribute('visibility', 'hidden');
	    } else {
	        this._initText();
	        var face = new Element('ul', { "class": "analogFace", style: "background-size: " + s + "px " + s + "px;" })
	        face.insert(this.elemHour = new Element('li', { "class": "analogHour", style: "width:" + s + "px; height:" + s + "px; background-size:" + s + "px " + s + "px;" }));
	        face.insert(this.elemMin = new Element('li', { "class": "analogMin", style: "width:" + s + "px; height:" + s + "px; background-size:" + s + "px " + s + "px;" }));
	        if (this.showSeconds)
	            face.insert(this.elemSec = new Element('li', { "class": "analogSec", style: "width:" + s + "px; height:" + s + "px; background-size:" + s + "px " + s + "px;" }));
	        this.div.insert(face);
	    }
	    //console.log(this.div.innerHTML);
	},

    _initDigital: function () {
        "use strict";
        this._initText();
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
                this.div.innerHTML = d + (d != "" && t != "" ? "<br>" : "") + t;
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
