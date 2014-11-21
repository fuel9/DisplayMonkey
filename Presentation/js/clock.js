// 12-08-13 [DPA] - client side scripting BEGIN
// 14-10-25 [LTL] - use strict, code improvements

var Clock = Class.create({
    initialize: function (div) {
        "use strict";
        this.div = div;
        this.showDate = this.div.readAttribute("data-show-date") === 'True';
        this.showTime = this.div.readAttribute("data-show-time") === 'True';
        this.faceType = this.div.readAttribute("data-face-type");
        this.panelWidth = this.div.readAttribute("data-panel-width");
        this.panelHeight = this.div.readAttribute("data-panel-height");
        switch (this.faceType) {
            case '1':
                this._initAnalog();
                break;
            case '2':
                this._initDigital();
                break;
            default:
                break;
        }

        if (!this.div)
            return;

        this._callBack();
        this.timer = setInterval(this._callBack.bind(this), 1000);
    }
	
	, stop: function() {
	    "use strict";
	    clearInterval(this.timer);
	    this.timer = null;
	}

    , _initAnalog: function () {
        "use strict";
        var s = this.panelWidth > this.panelHeight ? this.panelHeight : this.panelWidth;
        var face = new Element('ul', { "class": "analogFace", style: "background-size: " + s + "px " + s + "px;" })
        face.insert(this.elemSec = new Element('li', { "class": "analogSec", style: "width:" + s + "px; height:" + s + "px; background-size:" + s + "px " + s + "px;" }));
        face.insert(this.elemMin = new Element('li', { "class": "analogMin", style: "width:" + s + "px; height:" + s + "px; background-size:" + s + "px " + s + "px;" }));
        face.insert(this.elemHour = new Element('li', { "class": "analogHour", style: "width:" + s + "px; height:" + s + "px; background-size:" + s + "px " + s + "px;" }));
        this.div.style.width = this.div.style.height = s + "px";
        this.div.insert(face);
        //alert(this.div.innerHTML);
    }

    , _initDigital: function () {
        "use strict";
    }

    , _callBack: function () {
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
                this._rotateHand(this.elemSec, "rotate(" + (sec * 6) + "deg)");
                this._rotateHand(this.elemMin, "rotate(" + (min * 6) + "deg)");
                this._rotateHand(this.elemHour, "rotate(" + (hrs * 30 + (min / 2)) + "deg)");
                break;
        }
    }

    , _rotateHand: function (e, r) {
        "use strict";
        e.setStyle({ "transform": r, "-moz-transform": r, "-webkit-transform": r, "-ms-transform": r, "-o-transform": r });
    }
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
