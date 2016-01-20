/*!
 * Home/Index.js
 * Author: John Charlton
 * Date: 2016-01
 */

; (function ($) {

    home.index = {
        init: function (options) {

            var config = {
                username: ""
            };

            $.extend(config, options);

            function signIn() {
                var username = $("#txtUserName").val();
                var password = $("#txtPassword").val();
                var rememberme = $("#chkRememberMe").is(":checked");
                var model = { Password: password, RememberMe: rememberme, UserName: username };

                $("#validation-container-signin").html("");

                $.post("Home/Login/", model, function (data) {
                    if (data.success) {
                        $("#signin-form").dialog("close");
                        window.location.reload(true);
                    } else {
                        if (data.messages.length > 0) {
                            $("#validation-container-signin").show();
                            $("#validation-container-signin").html("");
                            var html = "<ul>";
                            for (var i = 0; i < data.messages.length; i++) {
                                html = html + "<li>" + data.messages[i] + "</li>";
                            }
                            html = html + "</ul>";
                            $("#validation-container-signin").append(html);
                        }
                    }
                });
            }

            function registerUser() {
                var username = $("#txtRegisterUserName").val();
                var email = $("#txtEmail").val();
                var password = $("#txtRegisterPassword").val();
                var confirmpassword = $("#txtConfirmPassword").val();
                var model = { Password: password, ConfirmPassword: confirmpassword, EmailAddress: email, UserName: username };
                $("#validation-container-register").html("");

                $.post("/Home/Register/", model, function (data) {
                    if (data.success) {
                        if (data.EmailError.length > 0) {
                            alert("The user has been successfully registered but there was an error sending the cofirmation email.\n\n" + data.EmailError);
                        }
                        $("#register-form").dialog("close");
                        location.href = "/Home/";
                    } else {
                        if (data.messages.length > 0) {
                            $("#validation-container-register").show();
                            $("#validation-container-register").html("");
                            var html = "<ul>";
                            for (var i = 0; i < data.messages.length; i++) {
                                html = html + "<li>" + data.messages[i] + "</li>";
                            }
                            html = html + "</ul>";
                            $("#validation-container-register").append(html);
                        }
                    }
                });
            }

            function showSignInDialog() {
                $("#signin-form").dialog({
                    modal: true,
                    open: function () { $(".ui-dialog-titlebar").hide(); },
                    width: "auto",
                    height: "auto",
                    resizable: false
                });
                $("button").button();
                $("button").removeClass("ui-widget");
                $("#signin-form").closest(".ui-dialog")
                    .removeClass("ui-widget")
                    .removeClass("ui-widget-content")
                    .removeClass("ui-dialog-content")
                    .addClass("ui-dialog-custom");
                $("#validation-container-signin").hide();

                $("#chkRememberMe").attr("checked", "checked");
                var username = config.username;

                if (username != null) {
                    $("#txtUserName").val(username);
                    $("#txtPassword").focus();
                } else {
                    $("#txtUserName").focus();
                }
                $("#signin-form").keyup(function (e) {
                    if (e.keyCode === 13) {
                        signIn();
                    }
                });
            }

            function showRegisterDialog() {
                $("#register-form").dialog({
                    modal: true,
                    open: function () { $(".ui-dialog-titlebar").hide(); },
                    width: "auto",
                    height: "auto",
                    resizable: false
                });
                $("button").button();
                $("button").removeClass("ui-widget");
                $("#register-form").closest(".ui-dialog")
                    .removeClass("ui-widget")
                    .removeClass("ui-widget-content")
                    .removeClass("ui-dialog-content")
                    .addClass("ui-dialog-custom");
                $("#validation-container-register").hide();
                $("#txtUserName").focus();

                $("#register-form").keyup(function (e) {
                    if (e.keyCode === 13) {
                        registerUser();
                    }
                });
            }

            function createSignInForm() {
                $("#btnCancelSignin").click(function () {
                    $("#validation-container-signin").html("");
                    $("#signin-form").dialog("close");
                });

                $("#btnRegisterUser").click(function () {
                    $("#signin-form").dialog("close");
                    showRegisterDialog();
                });

                $("#btnSignIn").click(function () {
                    signIn();
                });

                $("#signin").click(function () {
                    showSignInDialog();
                });
            }

            function createRegisterForm() {
                $("#btnCancelRegister").click(function () {
                    $("#register-form").dialog("close");
                });
                $("#btnRegister").click(function () {
                    registerUser();
                });
            }

            createSignInForm();
            createRegisterForm();
        }
    }
})(jQuery);