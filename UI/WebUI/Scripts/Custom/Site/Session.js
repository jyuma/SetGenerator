/*!
 * Site/Session.js
 * Author: John Charlton
 * Date: 2015-05
 */

var session = (function ($) {
    var self = {
        isExpired: function () {
            var user;
            $.ajax({
                url: site.url + "Home/GetCurrentSessionUser/",
                data: null,
                type: "GET",
                dataType: "text",
                async: false,
                cache: false,
                success: function (result) {
                    user = result;
                }
            });
            return (0 === user.length);
        }
    };
    return {
        validate: function () {
            var isexpired = self.isExpired();
            if (isexpired) {
                dialog.custom.showInfo({ title: 'Session Ended', message: 'Your session has expired. Redirecting...', showCloseButton: false });
            }
            return !isexpired;
        }
    }
})(jQuery);