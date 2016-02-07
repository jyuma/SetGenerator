/*!
 * Shared/_LayoutPartial.js
 * Author: John Charlton
 * Date: 2016-01
 */

; (function ($) {
    shared._logonpartial = {
        init: function (options) {
            var config = {
                controller: "",
                actionName: ""
            }

            $.extend(config, options);

            if (config.actionName === "Sets") {
                $("#ddlUserBand").attr("disabled", "disabled");
            } else {
                $("#ddlUserBand").removeAttr("disabled");
            };

            $("#ddlUserBand").change(function() {
                $.ajax({
                    type: "POST",
                    url: site.url + "Home/SetCurrentBand/",
                    data: { bandId: $("#ddlUserBand").val() },
                    dataType: "json",
                    traditional: true,
                    async: false,
                    success: function () {
                        window.location.href = site.url + config.controller;
                    }
                });
            });
        }
    }
})(jQuery);