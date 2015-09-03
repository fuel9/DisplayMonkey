/*!
* DisplayMonkey source file
* http://displaymonkey.org
*
* Copyright (c) 2015 Fuel9 LLC and contributors
*
* Released under the MIT license:
* http://opensource.org/licenses/MIT
*/

// 2015-02-06 [LTL] - object for reports and pictures
// 2015-03-08 [LTL] - using data
// 2015-05-01 [LTL] - hash URL to avoid browser caching
// 2015-05-08 [LTL] - ready callback
// 2015-07-29 [LTL] - RC12: performance and memory management improvements

DM.Picture = Class.create(DM.FrameBase, {
    initialize: function ($super, options) {
        "use strict";
        $super(options, 'div.picture, div.report');
        var data = options.panel.data;
        var img = this.img = this.div.down('img');
        img.addEventListener('load', this.ready.bind(this));
        img.alt = data.Name;
        img.src = (data.FrameType === 5 ? 
            "getImage.ashx?" :
            "getReport.ashx?") + $H({ frame: this.frameId, hash: data.Hash }).toQueryString()
        ;
    },

    uninit: function ($super) {
        "use strict";
        this.img.removeEventListener('load', this.ready);
        $super();
    },
});

