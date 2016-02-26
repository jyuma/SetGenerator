/*!
 * Users/Index.js
 * Author: John Charlton
 * Date: 2015-05
 */
; (function ($) {

    var HIGHLIGHT_ROW_COLOUR = "#e3e8ff";

    users.index = {
        init: function (options) {
            var _sortDescending = false;
            var _currentSortKey = "username";

            var config = {
                userId: "0"
            };

            $.extend(config, options);

            var lists = {
                UserList: [],
                DefaultBandArrayList: [],
                TableColumnList: []
            };

            loadConfig();

            ko.applyBindings(new UserViewModel());

            // pre-select the user whose bands were just being managed
            if (config.userId > 0) { $("#row_" + config.userId).trigger("click"); }

            function loadConfig() {
                $.ajax({
                    type: "GET",
                    url: site.url + "Users/GetData/",
                    dataType: "json",
                    traditional: true,
                    async: false,
                    success: function (data) {
                        $.extend(lists, data);
                    }
                });
            }

            function User(id, username, email, dateregistered, ipaddress, browserinfo, isdisabled, defaultbandid) {
                var self = this;

                self.id = id;
                self.username = username;
                self.email = email;
                self.dateregistered = dateregistered;
                self.ipaddress = ipaddress;
                self.browserinfo = browserinfo;
                self.isdisabled = isdisabled;
                self.isDisabled = ko.observable(isdisabled);
                self.defaultband = getValue(lists.DefaultBandArrayList, defaultbandid, "Display", "Value");
            }

            function UserViewModel() {
                var tblUser = $("#tblUser");
                var ddlColumns = $("#ddlColumns");

                var self = this;

                self.users = ko.observableArray([]);
                self.selectedUser = ko.observable();
                self.userNameSearch = ko.observable("");
                self.editFormHeader = ko.observable("");
                self.totalUsers = ko.observable(0);

                createUserArray(lists.UserList);

                function createUserArray(list) {
                    self.users.removeAll();
                    $(list).each(function (index, value) {
                        pushUser(value);
                    });
                };

                ddlColumns.on("hidden.bs.dropdown", function () {
                    self.saveColumns();
                });

                self.columns = ko.computed(function () {
                    var arr = [];
                    $(lists.TableColumnList).each(function (index, value) {
                        arr.push({ id: value.Id, title: value.Header, sortKey: value.Data, dataMember: value.Data, isVisible: ko.observable(value.IsVisible), alwaysVisible: value.AlwaysVisible, isMemberColumn: value.IsMemberColumn });
                    });
                    return arr;
                });

                function pushUser(value) {
                    self.users.push(new User(value.Id, value.UserName, value.Email, value.DateRegistered, value.IPAddress, value.BrowserInfo, value.IsDisabled, value.DefaultBandId));
                };

                self.selectedUser(self.users()[0]);

                self.sort = function (header) {
                    var afterSave = typeof header.afterSave != "undefined" ? header.afterSave : false;
                    var sortKey;

                    if (!afterSave) {
                        sortKey = header.sortKey;

                        if (sortKey !== _currentSortKey) {
                            _sortDescending = false;
                        } else {
                            _sortDescending = !_sortDescending;
                        }
                        _currentSortKey = sortKey;
                    } else {
                        sortKey = _currentSortKey;
                    }

                    $(self.columns()).each(function (index, value) {
                        if (value.sortKey === sortKey) {
                            self.users.sort(function (a, b) {
                                if (_sortDescending) {
                                    return a[sortKey].toString().toLowerCase() > b[sortKey].toString().toLowerCase()
                                        ? -1 : a[sortKey].toString().toLowerCase() < b[sortKey].toString().toLowerCase()
                                        || a.username.toLowerCase() > b.username.toLowerCase() ? 1 : 0;
                                } else {
                                    return a[sortKey].toString().toLowerCase() < b[sortKey].toString().toLowerCase()
                                        ? -1 : a[sortKey].toString().toLowerCase() > b[sortKey].toString().toLowerCase()
                                        || a.username.toLowerCase() > b.username.toLowerCase() ? 1 : 0;
                                }
                            });
                        }
                    });
                };

                self.filteredUsers = ko.computed(function () {
                    return ko.utils.arrayFilter(self.users(), function (u) {
                        return (
                            (self.userNameSearch().length === 0 || u.username.toLowerCase().indexOf(self.userNameSearch().toLowerCase()) !== -1)
                        );
                    });
                });

                self.usersTable = ko.computed(function () {
                    var filteredUsers = self.filteredUsers();
                    self.totalUsers(filteredUsers.length);

                    return filteredUsers;
                });

                self.showEditDialog = function (row) {
                    var id = row.id;

                    if (id > 0) {
                        self.highlightRow(row);
                    }

                    self.showUserEditDialog(row);
                };

                self.deleteSelectedUser = function (row) {
                    deleteUser(row.id);
                };

                self.showDeleteDialog = function (row) {
                    self.highlightRow(row);
                    self.showUserDeleteDialog(row);
                };

                self.showUserDeleteDialog = function (row) {
                    var id = (typeof row.id !== "undefined" ? row.id : 0);
                    if (id <= 0) return;
                    var u = self.getUser(id);

                    dialog.custom.showModal({
                        title: "<span class='glyphicon glyphicon-remove'></span> Delete User?",
                        message: "<p>This will permanently delete the user <i>" + u.username + "</i>.</p>",
                        callback: function () {
                            return self.deleteUser(row.id);
                        },
                        width: 500
                    });
                };

                self.showUserEditDialog = function (row) {
                    var id = (typeof row.id !== "undefined" ? row.id : 0);
                    var title = "<span class='glyphicon glyphicon-pencil'></span>";
                    var message;

                    if (id > 0) {
                        title = title + " Edit User";
                        var user = self.getUser(id);
                        self.selectedUser(user);
                    } else {
                        title = title + " Add User";
                        self.selectedUser([]);
                    }

                    $.ajax({
                        type: "GET",
                        async: false,
                        url: site.url + "Users/GetUserEditView/" + id,
                        success: function (data) {
                            message = data;
                        }
                    });

                    dialog.custom.showModal({
                        title: title,
                        message: message,
                        focusElement: "txtUserName",
                        callback: function () {
                            $("#validation-container").html("");
                            $("#validation-container").hide();
                            return self.saveUser();
                        },
                        width: 400
                    });
                };

                self.showUserBandEditDialog = function (row) {
                    var id = (typeof row.id !== "undefined" ? row.id : 0);
                    var title = "<span class='glyphicon glyphicon-pencil'></span>";
                    var message;

                    title = title + " Add/Remove Bands";
                    var user = self.getUser(id);
                    self.selectedUser(user);

                    $.ajax({
                        type: "GET",
                        async: false,
                        url: site.url + "Users/GetUserBandEditView/" + id,
                        success: function (data) {
                            message = data;
                        }
                    });

                    dialog.custom.showModal({
                        title: title,
                        message: message,
                        callback: function () {
                            return self.saveUserBands();
                        },
                        width: 500
                    });
                };

                self.getUser = function (userid) {
                    var user = null;

                    ko.utils.arrayForEach(self.users(), function (item) {
                        if (item.id === userid) {
                            user = item;
                        }
                    });

                    return user;
                };

                self.highlightRow = function (row) {
                    if (row == null) return;

                    var rows = tblUser.find("tr:gt(0)");
                    rows.each(function () {
                        $(this).css("background-color", "#ffffff");
                    });

                    var r = tblUser.find("#row_" + row.id);
                    r.css("background-color", HIGHLIGHT_ROW_COLOUR);
                    $("#tblUser").attr("tr:hover", HIGHLIGHT_ROW_COLOUR);
                };

                self.getUserDetailFromDialog = function () {
                    var username = $.trim($("#txtUserName").val());
                    var email = $.trim($("#txtEmail").val());
                    var password = $.trim($("#txtPassword").val());
                    var isdisabled = $("#chkDisabled").is(":checked");
                    var defaultbandid = $.trim($("#ddlUserBands").val());

                    return {
                        Id: self.selectedUser().id,
                        UserName: username,
                        Password: password,
                        Email: email,
                        IsDisabled: isdisabled,
                        DefaultBandId: (defaultbandid.length > 0) ? defaultbandid : 0
                    };
                };

                self.getUserBandDetailFromDialog = function () {
                    var opts = $("#lstAssignedBands").find("option");
                    var ids = [];

                    opts.each(function (index, value) {
                        ids.push(value.value);
                    });

                    return {
                        UserId: self.selectedUser().id, BandIds: ids
                    };
                };

                self.getColumns = function () {
                    var arr = [];
                    $(self.columns()).each(function (index, value) {
                        if (!value.alwaysVisible) {
                            arr.push({
                                Id: value.id,
                                IsVisible: value.isVisible(),
                                IsMemberColumn: value.isMemberColumn
                            });
                        }
                    });
                    return arr;
                };

                //---------------------------------------------- CONTROLLER (BEGIN) -------------------------------------------------------

                self.saveUser = function () {
                    var userdetail = self.getUserDetailFromDialog();
                    var jsonData = JSON.stringify(userdetail);
                    var result;

                    $("body").css("cursor", "wait");

                    $.ajax({
                        type: "POST",
                        async: false,
                        url: site.url + "Users/Save/",
                        data: { user: jsonData },
                        dataType: "json",
                        traditional: true,
                        failure: function () {
                            $("body").css("cursor", "default");
                            $("#validation-container").html("");
                        },
                        success: function (data) {
                            if (data.Success) {
                                lists.UserList = data.UserList;
                                createUserArray(lists.UserList);
                                self.selectedUser(self.getUser(data.SelectedId));
                                self.sort({ afterSave: true });
                                self.highlightRow(self.selectedUser());
                                result = true;
                                window.location.href = site.url + "Users";

                            } else {
                                if (data.ErrorMessages.length > 0) {
                                    $("#validation-container").show();
                                    $("#validation-container").html("");
                                    $("body").css("cursor", "default");
                                    var html = "<ul>";
                                    for (var i = 0; i < data.ErrorMessages.length; i++) {
                                        var message = data.ErrorMessages[i];
                                        html = html + "<li>" + message + "</li>";
                                    }
                                    html = html + "</ul>";
                                    $("#validation-container").append(html);
                                }
                                result = false;
                            }
                            $("body").css("cursor", "default");
                        }
                    });

                    return result;
                };

                self.saveUserBands = function () {
                    var userBandDetail = self.getUserBandDetailFromDialog();
                    var jsonData = JSON.stringify(userBandDetail);
                    var result;

                    $("body").css("cursor", "wait");

                    $.ajax({
                        type: "POST",
                        async: false,
                        url: site.url + "Users/SaveUserBands/",
                        data: { userBandDetail: jsonData },
                        dataType: "json",
                        traditional: true,
                        failure: function () {
                            $("body").css("cursor", "default");
                        },
                        success: function (data) {
                            if (data.Success) {
                                lists.UserList = data.UserList;
                                lists.DefaultBandArrayList = data.DefaultBandArrayList;
                                createUserArray(lists.UserList);
                                self.selectedUser(self.getUser(data.SelectedId));
                                self.sort({ afterSave: true });
                                self.highlightRow(self.selectedUser());
                                window.location.href = site.url + "Users";
                                result = true;
                            }
                            $("body").css("cursor", "default");
                        }
                    });

                    return result;
                };

                self.deleteUser = function (id) {
                    $("body").css("cursor", "wait");

                    $.ajax({
                        type: "POST",
                        url: site.url + "Users/Delete/",
                        data: { id: id },
                        dataType: "json",
                        traditional: true,
                        failure: function () {
                            $("body").css("cursor", "default");
                        },
                        success: function (data) {
                            if (data.Success) {
                                lists.UserList = data.UserList;
                                createUserArray(lists.UserList);
                                self.sort({ afterSave: true });
                            }
                            $("body").css("cursor", "default");
                            window.location.href = site.url + "Users";
                        }
                    });

                    return true;
                };

                self.saveColumns = function () {
                    var jsonData = JSON.stringify(self.getColumns());
                    var selfColumns = self.getColumns();        // after changes
                    var tableColumns = lists.TableColumnList.filter(function (value) { return !value.AlwaysVisible });   // before changes
                    var isDifference = false;

                    $(selfColumns).each(function (index, value) {
                        var isvisible = tableColumns[index].IsVisible;
                        if (isvisible !== value.IsVisible) {
                            isDifference = true;
                        }
                    });

                    if (isDifference) {
                        $.ajax({
                            type: "POST",
                            url: site.url + "Users/SaveColumns/",
                            data: { columns: jsonData },
                            dataType: "json",
                            traditional: true
                        });
                    }
                };

                //---------------------------------------------- CONTROLLER (END) -------------------------------------------------------
            };

            //---------------------------------------------- VIEW MODEL (END) -----------------------------------------------------

            ko.utils.stringStartsWith = function (string, startsWith) {
                string = string || "";
                if (startsWith.length > string.length) return false;
                return string.substring(0, startsWith.length) === startsWith;
            };

            function getValue(list, id, dataMember, valueMember) {
                var name = "";
                $(list).each(function (index, item) {
                    if (item[valueMember] === id) {
                        name = item[dataMember];
                        return name;
                    }
                });
                return name;
            }
        }
    }
})(jQuery);