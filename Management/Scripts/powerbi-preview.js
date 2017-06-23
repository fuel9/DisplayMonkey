/*!
* DisplayMonkey source file
* http://displaymonkey.org
*
* Copyright (c) 2017 Fuel9 LLC and contributors
*
* Released under the MIT license:
* http://opensource.org/licenses/MIT
*/

var __urlToken;

$(document).ready(function () {
    $("#powerbi_preview").on('load', _postActionLoaded);
});

function _postActionLoaded () {
    $.ajax({
        type: "POST",
        url: __urlToken,
        data: { accountId: $('#AccountId').val() },
        datatype: "json",
        success: function (payload) {
            if (payload.success) {
                var ifr = $("#powerbi_preview")[0];
                var m = JSON.stringify({
                    action: $("#Type").val() === "PowerbiType_Report" ? "loadReport" : "loadTile",
                    width: ifr.clientWidth,
                    height: ifr.clientHeight,
                    accessToken: payload.data,
                });
                ifr.contentWindow.postMessage(m, "*");
            } else {
                alert(payload.data);
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            alert(errorThrown);
        }
    });
}
