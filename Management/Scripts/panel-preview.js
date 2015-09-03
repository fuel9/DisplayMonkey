/*!
* DisplayMonkey source file
* http://displaymonkey.org
*
* Copyright (c) 2015 Fuel9 LLC and contributors
*
* Released under the MIT license:
* http://opensource.org/licenses/MIT
*/

var square = 500;
var scale = 1.0;
var url, url_data;

function resetCanvas() {
    $('#canvas')
        .css('background-color', '')
        .css('width', square + 'px')
        .css('height', square + 'px')
    ;
    $('#canvas img:first')
        .hide()
        .attr("src", "")
    ;
    $('#canvas div.panel, #canvas div.panel-high')
        .remove()
    ;
}

$(document).ready(function () {

    $('#canvas').width(square).height(square);

    url = $('#canvas img:first').attr("src").replace(/qqq/g, square);
    url_data = $('#canvas').attr('data-path') + '/';

    $('#CanvasId').change(function () {

        resetCanvas();

        if (this.value > 0)
            $.ajax({
                url: url_data + this.value, type: "POST", datatype: "json"
                , success: function (json) {
                    scale = json.Height > json.Width ? square / json.Height : square / json.Width;
                    $('#canvas')
                        .css('background-color', json.BackgroundColor)
                        .css('width', json.Width * scale + 'px')
                        .css('height', json.Height * scale + 'px')
                    ;
                    if (json.BackgroundImage) {
                        $('#canvas img:first')
                            .attr("src", url.replace("nnn", json.BackgroundImage))
                            .css('max-width', json.Width * scale + 'px')
                            .css('max-height', json.Height * scale + 'px')
                            .show()
                        ;
                    }
                    $.each(json.Panels, function (i, p) {
                        $('#canvas')
                            .append($('<div>')
                                .text(p.Name)
                                .addClass(p.PanelId == $('#PanelId').val() ? 'panel-high' : 'panel')
                                .css('top', p.Top * scale + 'px')
                                .css('left', p.Left * scale + 'px')
                                .css('width', p.Width * scale + 'px')
                                .css('height', p.Height * scale + 'px')
                            );
                    });
                    if (!$('#PanelId').val()) {
                        $('#canvas').append(
                            $('<div>').addClass('panel-high')
                        );
                    }
                }
            });
    });

    $('#Top, #Left, #Width, #Height').change(function () {
        $('#canvas div.panel-high')
            .css('top', $('#Top').val() * scale + 'px')
            .css('left', $('#Left').val() * scale + 'px')
            .css('width', $('#Width').val() * scale + 'px')
            .css('height', $('#Height').val() * scale + 'px')
        ;
    });

    $('#Name').change(function () {
        $('#canvas div.panel-high')
            .text($('#Name').val())
        ;
    });

    // init #canvas
    $('#CanvasId').change();
    $('#Width').change();
    $('#Name').change();
});
