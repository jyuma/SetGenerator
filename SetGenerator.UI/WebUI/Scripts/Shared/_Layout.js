/*!
 * Shared/_Layout.js
 * Author: John Charlton
 * Date: 2016-01
 */

; (function ($) {
    shared._layout = {
        setCurrentBand: function() {
            $.ajax({
                type: "POST",
                url: site.url + "Home/SetCurrentBand/",
                data: { bandId: $("#ddlUserBand").val() },
                dataType: "json",
                traditional: true,
                async: false,
                success: function() {
                    window.location.reload();
                }
            });
        }
    }
})(jQuery);