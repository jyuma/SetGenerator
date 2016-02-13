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

            //------------------------------------ initialize ---------------------------------------

            if (config.userId > 0) {
                var optBands = ddlUserBand.find("option");
                if (optBands.length === 0) {
                    var json = config.userBands.replace(/&quot;/g, "\"");
                    var bands = JSON.parse(json);

                    var ddlOptions = "";
                    $(bands).each(function (index, value) {
                        ddlOptions = ddlOptions + "<option value=" + value.Value;
                        if (value.Value.toString() === config.currentBandId) {
                            ddlOptions = ddlOptions + " selected";
                        }
                        ddlOptions = ddlOptions + ">" + value.Display + "</option>";
                    });
                    ddlUserBand.append(ddlOptions);
                }
            }

            //------------------------------------ initialize ---------------------------------------

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