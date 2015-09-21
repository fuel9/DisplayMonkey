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
// 2014-10-25 [LTL] - use strict, code improvements
// 2015-02-15 [LTL] - SVG analog face
// 2015-03-08 [LTL] - using data
// 2015-05-08 [LTL] - ready callback
// 2015-06-09 [LTL] - refinements
// 2015-06-18 [LTL] - performance refinements for analog
// 2015-09-21 [LTL] - fixed custom location time

DM.Clock = Class.create(DM.FrameBase, {
    initialize: function ($super, options) {
        "use strict";
        $super(options, 'div.clock');
        var data = options.panel.data;
        this.showDate = !!data.ShowDate;
        this.showTime = !!data.ShowTime;
        this.faceType = data.Type || 0;
        if (data.LocationTime) {
            this.offsetMilliseconds = moment(data.LocationTime).diff(moment());
        } else {
            this.offsetMilliseconds = _canvas.time.diff(_canvas.locationTime);
        }
        this.showSeconds = !!data.ShowSeconds;

        this.div.setStyle({ width: this.width + "px", height: this.height + "px" });

        switch (this.faceType) {
            case 1: this._initAnalog(data); break;
            case 2: this._initDigital(data); break;
            default: this._initText(data); break;
        }

        //console.log(this.div.innerHTML);

        this.timer = setInterval(this._callBack.bind(this), 1000);
        this._callBack();
        this.ready.bind(this).delay(0);
    },
	
    stop: function ($super) {
        "use strict";
        $super();
	    clearInterval(this.timer);
	    this.timer = null;
    },

    _setContainer: function(cls, data) {
        "use strict";
        this.container = null;
        this.div.childElements().each(function (e) {
            if (e.getAttribute('class') == cls)
                this.container = e;
            else
                e.parentNode.removeChild(e);
        }, this);

        this.label = null;
        var label = this.container.down('.label'), face = this.container.down('.face');
        if (data.Label) {
            this.label = label.update(data.Label);
            face.style.height = "" + this.div.getHeight() - face.offsetTop + "px";
        } else {
            label.parentNode.removeChild(label);
            face.style.height = "" + this.div.getHeight() + "px";
        }
    },

    _initText: function (data) {
	    "use strict";
	    this._setContainer('text', data);
	    var elemDate = this.container.down('.date');
	    var elemTime = this.container.down('.time');
	    if (this.showDate) {
	        this.elemDate = elemDate;
	    } else {
	        elemDate.parentNode.removeChild(elemDate);
	    }
	    if (this.showTime) {
	        this.elemTime = elemTime;
	    } else {
	        elemTime.parentNode.removeChild(elemTime);
	    }
    },

    _initAnalog: function (data) {
	    "use strict";
	    this.prevH = 0; this.prevM = 0;
	    var supportSvg = document.implementation.hasFeature("http://www.w3.org/TR/SVG11/feature#BasicStructure", "1.1");
	    if (supportSvg) {
	        this._setContainer('svgAnalog', data);
	        this.elemHour = this.container.down('#hourHand');
	        this.elemMin = this.container.down('#minuteHand');
	        this.elemSec = this.container.down('#secondHand');
	        if (!this.showSeconds)
	            this.elemSec.setAttribute('visibility', 'hidden');
	    } else {
	        this._setContainer('bmpAnalog', data);
	        var face = this.container.down('.face');
	        var w = face.getWidth(), h = face.getHeight(), s = w > h ? h : w, px = "" + s + "px ";
	        face.setStyle({ backgroundSize: px + px, width: px, height: px });
	        this.elemHour = face.down('.hour').setStyle({ backgroundSize: px + px, width: px, height: px });
	        this.elemMin = face.down('.minute').setStyle({ backgroundSize: px + px, width: px, height: px });
	        this.elemSec = face.down('.second').setStyle({ backgroundSize: px + px, width: px, height: px });
	        if (!this.showSeconds)
	            this.elemSec.setAttribute('visibility', 'hidden');
	    }
	},

    _initDigital: function (data) {
        "use strict";
        throw new Error("Not implemented");
    },

    _callBack: function () {
        "use strict";
        if (!this.timer || this.exiting)
            return;

        var time = moment().add('ms', this.offsetMilliseconds);

        switch (this.faceType) {
            case 0:
                if (this.elemDate) {
                    this.elemDate.update(time.format(_canvas.dateFormat));
                }
                if (this.elemTime) {
                    this.elemTime.update(time.format(_canvas.timeFormat));
                }
                break;

            case 1:
                var sec = time.seconds(),
                    min = time.minutes(),
                    hrs = time.hours()
                    ;
                if (hrs > 12) hrs -= 12;
                if (this.prevH != hrs)
                    this._rotateHand(this.elemHour, hrs * 30 + (min * 0.5));
                if (this.prevM != min)
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
