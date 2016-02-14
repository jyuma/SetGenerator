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
                actionName: "",
                userId: 0,
                currentBandId: "",
                userBands: ""
            }

            $.extend(config, options);

            var ddlUserBand = $("#ddlUserBand");

            if (config.actionName === "Sets") {
                ddlUserBand.attr("disabled", "disabled");
            } else {
                ddlUserBand.removeAttr("disabled");
            };

            //-------------------------------------- events -----------------------------------------

            ddlUserBand.change(function () {
                var bandId = ddlUserBand.val();

                $.ajax({
                    type: "POST",
                    url: site.url + "Home/SetCurrentBand/",
                    data: { bandId: bandId },
                    dataType: "json",
                    traditional: true,
                    async: false,
                    success: function () {
                        if (isNotSafariChromeFirefox()) {
                            window.location.reload();
                        } else {
                            window.location.href = site.url + config.controller;
                        }
                    }
                });
            });

            //-------------------------------------- events -----------------------------------------

            //-------------------------------------- private -----------------------------------------

            function isNotSafariChromeFirefox() {
                var myNav = navigator.userAgent.toLowerCase();
                var isFF = myNav.indexOf("firefox") !== -1;
                var isChrome = myNav.indexOf("chrome") !== -1;
                var isSafari = myNav.indexOf("safari") !== -1;

                return (!(isFF || isChrome || isSafari));
            }

            //-------------------------------------- private -----------------------------------------
        }
    }
})(jQuery);