(function ($) {
    $.fn.extend({
        jqsimplemenu: function () {
            return this.each(function () {
                //add class .drop-down to all of the menus having drop-down items
                var menu = $(this);
                var timeoutInterval;
                if (!menu.hasClass('menu')) menu.addClass('menu');
                $("> li", menu).each(function () {
                    if ($(this).find("ul:first").length > 0)
                        $(this).addClass('pull-down');
                });

                $("> li ul li ul", menu).each(function () {
                    $(this).parent().addClass('right-menu');
                });
                $("li", menu).mouseenter(function () {
                    var isTopLevel = false;
                    //if its top level then add animation 
                    isTopLevel = $(this).parent().attr('class') === 'menu';
                    if (isTopLevel) {
                        clearTimeout(timeoutInterval);
                        var w = $(this).outerWidth();
                        // if ($(this).hasClass('pull-down')) w += 10;
                        var h = $(this).outerHeight();
                        var box = $('<div/>').addClass('box');
                        $('> li', menu).removeClass('selected');
                        $('>li div.box', menu).remove();
                        $('>li ul', menu).css('display', 'none').slideUp(0);
                        $(this).prepend(box);
                        $(this).addClass('selected');
                        box.stop(true, false).animate({ width: w, height: h }, 100, function () {
                            if ($(this).parent().find('ul:first').length == 0) {
                                timeoutInterval = setTimeout(function () {
                                    box.stop(true, false).animate({ height: '+=5' }, 300, function () {
                                        box.parent().find('ul:first').css('display', 'block').css('top', box.height()).stop(true, false).slideDown(100);
                                    });
                                }, 10);
                            }
                            else {

                                timeoutInterval = setTimeout(function () {
                                    box.stop(true, false).animate({ height: '+=0' }, 0, function () {
                                        box.parent().find('ul:first').css('display', 'block').css('top', box.height()).stop(true, false).slideDown(100);
                                    });
                                }, 10);
                            }
                        });
                    }
                    else {
                        $(this).find('ul:first').css('display', 'block').stop(true, false).slideDown(100);
                    }

                }).mouseleave(function () {
                    isTopLevel = $(this).parent().attr('class') === 'menu';
                    if (isTopLevel) {
                        $(this).parent().find('div.box').remove();
                    }
                    $(this).find('ul').slideUp(100, function () {

                        $(this).css('display', 'none');
                    });
                });

                $('> li > ul li a', menu).hover(function () {
                    $(this).parent().addClass('menu-item-selected');
                }, function () {

                    $(this).parent().removeClass('menu-item-selected');
                });

            });
        }
    });
})(jQuery);

