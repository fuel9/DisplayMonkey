// 12-08-13 [DPA] - client side scripting BEGIN
// 14-10-25 [LTL] - use strict, code improvements
// 15-02-15 [LTL] - SVG analog face

var Clock = Class.create({
    initialize: function (options) {
        "use strict";
        this.div = options.div;
        this.showDate = this.div.readAttribute("data-show-date") === 'True';
        this.showTime = this.div.readAttribute("data-show-time") === 'True';
        this.faceType = this.div.readAttribute("data-face-type");
        this.panelWidth = this.div.readAttribute("data-panel-width");
        this.panelHeight = this.div.readAttribute("data-panel-height");
        this.useSvg = document.implementation.hasFeature("http://www.w3.org/TR/SVG11/feature#BasicStructure", "1.1");
        this.finishedLoading = false;

        switch (this.faceType) {
            case '1':
                this.useSvg ? this._initAnalogSvg() : this._initAnalog();
                break;
            case '2':
                this._initDigital();
                break;
            default:
                break;
        }

        if (!this.div)
            return;

        this.timer = setInterval(this._callBack.bind(this), 1000);
        this._callBack();
        this.finishedLoading = true;
    },
	
	stop: function() {
	    "use strict";
	    clearInterval(this.timer);
	    this.timer = null;
	},

	isReady: function () {
	    "use strict";
	    return !!this.finishedLoading;
	},

    _initAnalog: function () {
        "use strict";
        this.div.select('svg').each(function (e) {
            e.parentNode.removeChild(e);
        });
        var s = this.panelWidth > this.panelHeight ? this.panelHeight : this.panelWidth;
        var face = new Element('ul', { "class": "analogFace", style: "background-size: " + s + "px " + s + "px;" })
        face.insert(this.elemSec = new Element('li', { "class": "analogSec", style: "width:" + s + "px; height:" + s + "px; background-size:" + s + "px " + s + "px;" }));
        face.insert(this.elemMin = new Element('li', { "class": "analogMin", style: "width:" + s + "px; height:" + s + "px; background-size:" + s + "px " + s + "px;" }));
        face.insert(this.elemHour = new Element('li', { "class": "analogHour", style: "width:" + s + "px; height:" + s + "px; background-size:" + s + "px " + s + "px;" }));
        this.div.style.width = this.div.style.height = s + "px";
        this.div.insert(face);
        //console.log(this.div.innerHTML);
    },

    _initAnalogSvg: function () {
        "use strict";
        var s = this.panelWidth > this.panelHeight ? this.panelHeight : this.panelWidth;
        this.div.style.width = this.div.style.height = s + "px";
        this.elemSec = this.div.select('#secondHand')[0];
        this.elemMin = this.div.select('#minuteHand')[0];
        this.elemHour = this.div.select('#hourHand')[0];
        this.div.select('#svgAnalogFace')[0].setAttribute('visibility', 'visible');
    },

    _initDigital: function () {
        "use strict";
    },

    _callBack: function () {
        "use strict";
        if (!this.timer)
            return;

        var time = moment();
        if (_canvas.offsetMilliseconds > 0)
            time.add('ms', _canvas.offsetMilliseconds);
        else
            time.subtract('ms', _canvas.offsetMilliseconds);

        switch (this.faceType) {
            case '0':
                var d = this.showDate ? time.format(_canvas.dateFormat) : "";
                var t = this.showTime ? time.format(_canvas.timeFormat) : "";
                this.div.innerHTML = d + (d != "" && t != "" ? "<br>" : "") + t;
                break;

            case '1':
                var sec = time.seconds();
                var min = time.minutes();
                var hrs = time.hours(); if (hrs > 12) hrs -= 12;
                this._rotateHand(this.elemHour, hrs * 30 + (min * 0.5));
                this._rotateHand(this.elemMin, min * 6);
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

/*(function () {
    Element.addMethods('div', {
        __clock: {},

		startClock: function (e) {
			e.__clock = new Clock(e.id);
		},

		stopClock: function (e) {
		    if (e.__clock instanceof Clock) {
			    e.__clock.stop();
			}
		}
    });
})();*/

// 12-08-13 [DPA] - client side scripting END
