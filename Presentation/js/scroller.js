var TextScroller = Class.create(PeriodicalExecuter, {
    initialize: function ($super, id) {
        this.containerId = id;
        this.state = 0;
        this.delay = 0;
        this.tick = 0.1; 			// seconds
        this.length = 5 / this.tick; // seconds
        $super(this.callBack, this.tick);
    }

    , callBack: function (pe) {
        var div = $(this.containerId);
        if (!div) {
            pe.stop();
        } else {
            // freeze until full screen frame is up, unless it is itself scrolling
            if (_canvas.fullScreenActive && div != 'full')
                return;

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

