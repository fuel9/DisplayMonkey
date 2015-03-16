// 2012-08-13 [DPA] - client side scripting BEGIN
// 2014-10-25 [LTL] - use strict, code improvements
// 2014-11-19 [LTL] - code improvements
// 2015-03-08 [LTL] - using data

var Memo = Class.create(PeriodicalExecuter, {
    initialize: function ($super, options) {
        "use strict";
        this.finishedLoading = false;
        this.div = options.div;
        this.state = 0;
        this.delay = 0;
        this.tick = 0.1; 			// seconds
        this.length = 5 / this.tick; // seconds
        this.paused = false;
        this.div.select('.subject')[0].update(options.data.Subject);
        this.div.select('.body')[0].update(options.data.Body);
        this.finishedLoading = true;

        if (this.div) {
            $super(this._callBack, this.tick);
        }
    },

    isReady: function () {
        "use strict";
        return !!this.finishedLoading;
    },

    pause: function () {
        this.paused = true;
    },

    stop: function () {
        this.pause();
    },

    play: function () {
        this.paused = false;
    },

    _callBack: function (pe) {
        "use strict";
        if (this.paused)
            return;

        // no need to do anything if our text fits container completely
        var par = this.div.parentNode;
        if (this.div.offsetHeight <= par.clientHeight) {
            return;
        }

        // otherwise...
        // get bottoms of container and our text
        var pb = par.offsetTop + par.clientHeight,
		    cb = par.offsetTop + this.div.offsetTop + this.div.offsetHeight
        ;

        // first we show text up to X seconds before scrolling it
        if (this.state == 0) {
            if (this.delay < this.length) {
                this.delay++;
            } else {
                //console.log('done state 0');
                this.state++;
                this.delay = 0;
            }
        }

        // next we scroll our text till the end
        if (this.state == 1) {
            if (cb > pb) {
                this.div.style.top = this.div.offsetTop - 1 + "px";
            } else {
                //console.log('done state 1');
                this.state++;
                this.delay = 0;
            }
        }

        // next we wait before we jump back to the top
        if (this.state == 2) {
            if (this.delay < this.length) {
                this.delay++;
            } else {
                //console.log('done state 2');
                this.div.style.top = '0px';
                this.state = 0;
                this.delay = 0;
            }
        }
    },
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
