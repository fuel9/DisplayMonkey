/*!
* DisplayMonkey source file
* http://displaymonkey.org
*
* Copyright (c) 2015 Fuel9 LLC and contributors
*
* Released under the MIT license:
* http://opensource.org/licenses/MIT
*/

// 2015-02-06 [LTL] - object for iframe
// 2015-05-08 [LTL] - ready callback
// 2015-07-29 [LTL] - RC12: performance and memory management improvements

DM.Iframe = Class.create(DM.FrameBase, {
    initialize: function ($super, options) {
        "use strict";
        $super(options, 'iframe.html');
        var data = options.panel.data;
        this.div.addEventListener('load', this.ready.bind(this));
        this.div.src = "getHtml.ashx?" + $H({ frame: this.frameId, hash: data.Hash }).toQueryString();
    },

    uninit: function ($super) {
        "use strict";
        this.div.removeEventListener('load', this.ready);
        $super();
    },
});

