var __spinner;
var __urlProvider;

$(document).ajaxStart(function () {
    if (Spinner) {
        __spinner = new Spinner().spin();
        document.getElementById('spinholder').appendChild(__spinner.el);
    }
});

$(document).ajaxComplete(function () {
    if (__spinner) __spinner.stop();
});

function _getProvider() {
    $('.account').hide();
    $('input[type=submit]').hide();

    var p = $('select#Provider')[0];
    if (p.selectedIndex) {
        var a = $('select#AccountId');
        var o = $('input#oldAccountId')[0];
        a.find("option:gt(0)").remove();
        $.ajax({
            type: 'GET',
            url: __urlProvider + '/' + p.value,
            success: function (payload) {
                if (payload.success) {
                    $('.account').show();
                    $(payload.data).each(function (i, c) {
                        var selected = (o.value == c.AccountId) ? ' selected=""' : '';
                        a.append('<option value="' + c.AccountId + '"' + selected + '>' + c.Name + '</option>');
                    });
                } else {
                    alert(payload.data);
                }
                if (p.selectedIndex && a[0].selectedIndex) {
                    $('input[type=submit]').show();
                }
            },
            error: function (jqXHR, textStatus, errorThrown) {
                alert(errorThrown);
            }
        });
    }
}



$(document).ready(function () {

    $('select#AccountId').change(function () {
        $('input[type=submit]').hide();
        if ($(this).val() != '') {
            $('input[type=submit]').show();
        }
    });

    $('select#Provider').change(function () {
        _getProvider();
    }).change();

});
