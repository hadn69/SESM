(function( $ ) {

$.fn.showalert = function (message, alerttype) {
        var target = this;
        target.append('<div class="alert alert-' + alerttype.toString().toLowerCase() + ' fade in removealert"><a class="close" data-dismiss="alert">&times;</a><span>' + message + '</span></div>')
        /*$(".removealert").delay(10000).fadeOut(1000);*/
    }


$.fn.flashMessage = function (options) {
    var target = this;
    options = $.extend({ timeout: 5000, alert: 'info' }, options);

    if (!options.message) {
        setFlashMessageFromCookie(options);
    }


    // Get the first alert message read from the cookie
    function setFlashMessageFromCookie() {
        $.each(new Array('Success', 'Danger', 'Warning', 'Info'), function (i, alert) {
       
            var cookie = $.cookie("Flash." + alert);

            if (!jQuery.isEmptyObject(cookie)) {
                options.message = cookie;
                options.alert = alert;
                target.showalert(options.message, options.alert);
                deleteFlashMessageCookie(alert);
                return;
            }
        });
    }

    // Delete the named flash cookie
    function deleteFlashMessageCookie(alert) {
        //$.cookie("Flash." + alert, null, { path: '/' });
        $.removeCookie("Flash." + alert, { path: '/' });
    }
};


}(jQuery));