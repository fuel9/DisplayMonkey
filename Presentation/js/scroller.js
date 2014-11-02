// 12-08-13 [DPA] - client side scripting BEGIN
// 14-10-25 [LTL] - use strict, code improvements

var TextScroller = Class.create(PeriodicalExecuter, {
    initialize: function ($super, id) {
        "use strict";
        this.containerId = id;
        this.state = 0;
        this.delay = 0;
        this.tick = 0.1; 			// seconds
        this.length = 5 / this.tick; // seconds
        this.paused = false;
        $super(this.callBack, this.tick);
    }

    , pause: function () {
        this.paused = true;
    }

    , resume: function () {
        this.paused = false;
    }

    , callBack: function (pe) {
        "use strict";
        var div = $(this.containerId);
        if (!div) {
            pe.stop();
        } else {
            if (this.paused)
                return;

            // freeze until full screen frame is up, unless it is itself scrolling
            //if (_canvas.fullScreenActive && div != 'full')
            //    return;

            // no need to do anything if our text fits container completely
            var par = div.parentNode;
            if (div.offsetHeight <= par.clientHeight) {
                return;
            }

            // otherwise...
            // get bottoms of container and our text
            var pb = par.offsetTop + par.clientHeight
			, cb = par.offsetTop + div.offsetTop + div.offsetHeight;

            // first we show text up to X seconds before scrolling it
            if (this.state == 0) {
                if (this.delay < this.length) {
                    this.delay++;
                } else {
                    //alert('done state 0');
                    this.state++;
                    this.delay = 0;
                }
            }

            // next we scroll our text till the end
            if (this.state == 1) {
                if (cb > pb) {
                    div.style.top = div.offsetTop - 1 + "px";
                } else {
                    //alert('done state 1');
                    this.state++;
                    this.delay = 0;
                }
            }

            // next we wait before we jump back to the top
            if (this.state == 2) {
                if (this.delay < this.length) {
                    this.delay++;
                } else {
                    //alert('done state 2');
                    div.style.top = '0px';
                    this.state = 0;
                    this.delay = 0;
                }
            }
        }
    }
});

/*(function () {
    Element.addMethods('div', {
        __textScroller: {},

        startScroller: function (e) {
            e.__textScroller = new TextScroller(e.id);
        },

        stopScroller: function (e) {
            if (e.__textScroller instanceof TextScroller) {
                e.__textScroller.stop();
            }
        },

        pauseScroller: function (e) {
            if (e.__textScroller instanceof TextScroller) {
                e.__textScroller.pause();
            }
        },

        resumeScroller: function (e) {
            if (e.__textScroller instanceof TextScroller) {
                e.__textScroller.resume();
            }
        },
    });
})();*/

// 12-08-13 [DPA] - client side scripting END
