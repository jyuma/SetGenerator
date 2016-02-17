/*!
 * Dialog/Dialog.js
 * Author: John Charlton
 * Date: 2015-05
 */

; (function ($) {

    dialog.custom = {

        showModal: function (options) {
            var config = {
                title: "Title",
                message: "Message...",
                cancelButtonOnly: false,
                submitText: "OK",
                cancelText: "Cancel",
                width: 0,
                focusElement: "",
                callback: function () { }
            };

            $.extend(config, options);

            var html = "<div id=\"dialog\" class=\"modal fade\">";
            html = html + "<div class=\"modal-dialog\">";
            html = html + "<div class=\"modal-content\">";
            html = html + "<div class=\"modal-header\">";
            html = html + "<h4 class=\"modal-title\">" + config.title + "</h4></div>";
            html = html + "<div class=\"modal-body\"><span>" + config.message + "</span></div>";
            html = html + "<div class=\"modal-footer\">";

            if (config.cancelButtonOnly) {
                html = html + "<button type=\"button\" class=\"btn btn-primary\" data-dismiss=\"modal\">" + config.cancelText + "</button>";
            } else {
                html = html + "<button type=\"button\" class=\"cancel-button btn btn-default\" data-dismiss=\"modal\">" + config.cancelText + "</button>";
                html = html + "<button type=\"button\" class=\"submit-button btn btn-primary\" data-dismiss=\"modal\">" + config.submitText + "</button>";
            }

            html = html + "</div></div></div></div>";

            $("body").append(html);

            var canClose = true;

            $(".submit-button").click(function (e) {
                canClose = config.callback();
            });

            $(".cancel-button").click(function (e) {
                canClose = true;
            });

            $("#dialog").on("hide.bs.modal", function (e) {
                if (!canClose) {
                    e.preventDefault();
                    e.stopImmediatePropagation();
                    return false;
                }
            });

            $("#dialog").on("hidden.bs.modal", function (e) {
                $(this).remove();
            });

            $("#dialog").on("shown.bs.modal", function () {
                var e = $("#" + config.focusElement);
                e.focus();
            });

            if (config.width > 0) {
                $(".modal-dialog").css("width", config.width);
            }

            $("#dialog").modal("show");
        }
    }
})(jQuery);