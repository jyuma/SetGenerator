var site = (function ($) {
    var self = {
        getUrl: function () {
            var baseUrl = location.href;
            var rootUrl = baseUrl.substring(0, baseUrl.indexOf('/', 7));

            if (rootUrl.indexOf("localhost") > 0) {
                return rootUrl + "/SetGenerator.WebUI/";
            } else {
                return "/";
            }
        }
    };
    return {
        url: self.getUrl()
    }
})(jQuery);