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
// 2014-11-19 [LTL] - code improvements
// 2015-03-08 [LTL] - using data
// 2015-05-08 [LTL] - ready callback

DM.Memo = Class.create(DM.FrameBase, {
    initialize: function ($super, options) {
        "use strict";
        $super(options, 'div.memo');
        var data = options.panel.data;
        this.state = 0;
        this.delay = 0;
        this.tick = 0.1; 			// seconds
        this.length = 5 / this.tick; // seconds
        this.paused = false;
        this.div.select('.subject')[0].update(data.Subject);
        this.div.select('.body')[0].update(data.Body);

        this.timer = setInterval(this._callBack.bind(this), this.tick * 1000);
        this.ready.bind(this).delay(0);
    },

    pause: function ($super) {
        "use strict";
        $super();
        this.paused = true;
    },

    stop: function ($super) {
        "use strict";
        $super();
        clearInterval(this.timer);
        this.timer = null;
    },

    play: function ($super) {
        "use strict";
        $super();
        this.paused = false;
    },

    _callBack: function () {
        "use strict";
        if (this.paused || this.exiting)
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

// 12-08-13 [DPA] - client side scripting END
