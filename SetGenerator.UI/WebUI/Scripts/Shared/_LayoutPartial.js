/*!
 * Shared/_LayoutPartial.js
 * Author: John Charlton
 * Date: 2016-01
 */

; (function ($) {
    shared._layoutpartial = {
        init: function (options) {
            var config = {
                actionName: ""
            }

            $.extend(config, options);

            if (config.actionName === "Sets") {
                $("#ddlUserBand").attr("disabled", "disabled");
            } else {
                $("#ddlUserBand").removeAttr("disabled");
            }
        }
    }
})(jQuery);