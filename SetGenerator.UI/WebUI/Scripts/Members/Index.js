/*!
 * Members/Index.js
 * Author: John Charlton
 * Date: 2015-05
 */
; (function ($) {

    var HIGHLIGHT_ROW_COLOUR = "#e3e8ff";

    bands.members = {
        init: function () {
            var _sortDescending = false;
            var _currentSortKey = "alias";

            var lists = {
                MemberList: [],
                InstrumentArrayList: [],
                TableColumnList: []
            };

            loadConfig();

            ko.applyBindings(new MemberViewModel());

            function loadConfig() {
                $.ajax({
                    type: "GET",
                    url: site.url + "Members/GetData/",
                    dataType: "json",
                    traditional: true,
                    async: false,
                    success: function (data) {
                        $.extend(lists, data);
                    }
                });
            }

            function Member(id, firstname, lastname, alias, issongmemberinstrument, defaultinstrumentid, updateuser, updatedate) {
                var self = this;

                self.id = id;
                self.firstname = firstname;
                self.lastname = lastname;
                self.alias = alias;
                self.issongmemberinstrument = issongmemberinstrument;
                self.defaultinstrument = getValue(lists.InstrumentArrayList, defaultinstrumentid, "Display", "Value");
                self.updateuser = updateuser;
                self.updatedate = updatedate;
            }

            function MemberViewModel() {
                var tblMember = $("#tblMember");
                var ddlColumns = $("#ddlColumns");

                var self = this;

                self.members = ko.observableArray([]);
                self.selectedMember = ko.observable();
                self.firstnameSearch = ko.observable("");
                self.editFormHeader = ko.observable("");
                self.totalMembers = ko.observable(0);

                createMemberArray(lists.MemberList);

                function createMemberArray(list) {
                    self.members.removeAll();
                    $(list).each(function (index, value) {
                        pushMember(value);
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

                function pushMember(value) {
                    self.members.push(new Member(value.Id, value.FirstName, value.LastName, value.Alias, value.IsSongMemberInstrument, value.DefaultInstrumentId, value.UserUpdate, value.DateUpdate));
                };

                self.selectedMember(self.members()[0]);

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
                            self.members.sort(function (a, b) {
                                if (_sortDescending) {
                                    return a[sortKey].toString().toLowerCase() > b[sortKey].toString().toLowerCase()
                                        ? -1 : a[sortKey].toString().toLowerCase() < b[sortKey].toString().toLowerCase()
                                        || a.firstname.toLowerCase() > b.firstname.toLowerCase() ? 1 : 0;
                                } else {
                                    return a[sortKey].toString().toLowerCase() < b[sortKey].toString().toLowerCase()
                                        ? -1 : a[sortKey].toString().toLowerCase() > b[sortKey].toString().toLowerCase()
                                        || a.firstname.toLowerCase() > b.firstname.toLowerCase() ? 1 : 0;
                                }
                            });
                        }
                    });
                };

                self.filteredMembers = ko.computed(function () {
                    return ko.utils.arrayFilter(self.members(), function (g) {
                        return (
                            (self.firstnameSearch().length === 0 || g.firstname.toLowerCase().indexOf(self.firstnameSearch().toLowerCase()) !== -1)
                        );
                    });
                });

                self.membersTable = ko.computed(function () {
                    var filteredMembers = self.filteredMembers();
                    self.totalMembers(filteredMembers.length);

                    return filteredMembers;
                });

                self.showEditDialog = function (row) {
                    var id = row.id;

                    if (id > 0) {
                        self.highlightRow(row);
                    }

                    self.showMemberEditDialog(row);
                };

                self.deleteSelectedMember = function (row) {
                    deleteMember(row.id);
                };

                self.showDeleteDialog = function (row) {
                    self.highlightRow(row);
                    self.showMemberDeleteDialog(row);
                };

                self.showMemberDeleteDialog = function (row) {
                    var id = (typeof row.id !== "undefined" ? row.id : 0);
                    if (id <= 0) return;
                    var m = self.getMember(id);

                    dialog.custom.showModal({
                        title: "<span class='glyphicon glyphicon-remove'></span> Delete Member?",
                        message: "<p>This will permanently delete the member <i>" + m.firstname + m.lastname + "</i>.</p>",
                        callback: function () {
                            return self.deleteMember(row.id);
                        },
                        width: 500
                    });
                };

                self.showMemberEditDialog = function (row) {
                    var id = (typeof row.id !== "undefined" ? row.id : 0);
                    var title = "<span class='glyphicon glyphicon-pencil'></span>";
                    var message;

                    if (id > 0) {
                        title = title + " Edit Member";
                        var member = self.getMember(id);
                        self.selectedMember(member);
                    } else {
                        title = title + " Add Member";
                        self.selectedMember([]);
                    }

                    $.ajax({
                        type: "GET",
                        async: false,
                        url: site.url + "Members/GetMemberEditView/" + id,
                        success: function (data) {
                            message = data;
                        }
                    });

                    dialog.custom.showModal({
                        title: title,
                        message: message,
                        focusElement: "txtAlias",
                        callback: function () {
                            $("#validation-container").html("");
                            $("#validation-container").hide();
                            return self.saveMember();
                        },
                        width: 260
                    });
                };

                self.showMemberInstrumentEditDialog = function (row) {
                    var id = (typeof row.id !== "undefined" ? row.id : 0);
                    var title = "<span class='glyphicon glyphicon-pencil'></span>";
                    var message;

                    title = title + " Add/Remove Instruments";
                    var member = self.getMember(id);
                    self.selectedMember(member);

                    $.ajax({
                        type: "GET",
                        async: false,
                        url: site.url + "Members/GetMemberInstrumentEditView/" + id,
                        success: function (data) {
                            message = data;
                        }
                    });

                    dialog.custom.showModal({
                        title: title,
                        message: message,
                        callback: function () {
                            return self.saveMemberInstruments();
                        },
                        width: 500
                    });
                };

                self.getMember = function (memberid) {
                    var member = null;

                    ko.utils.arrayForEach(self.members(), function (item) {
                        if (item.id === memberid) {
                            member = item;
                        }
                    });

                    return member;
                };

                self.highlightRow = function (row) {
                    if (row == null) return;

                    var rows = tblMember.find("tr:gt(0)");
                    rows.each(function () {
                        $(this).css("background-color", "#ffffff");
                    });

                    var r = tblMember.find("#row_" + row.id);
                    r.css("background-color", HIGHLIGHT_ROW_COLOUR);
                    $("#tblMember").attr("tr:hover", HIGHLIGHT_ROW_COLOUR);
                };

                self.getMemberDetailFromDialog = function () {
                    var firstname = $.trim($("#txtFirstName").val());
                    var lastname = $.trim($("#txtLastName").val());
                    var alias = $.trim($("#txtAlias").val());
                    var defaultinstrumentid = $.trim($("#ddlMemberInstruments").val());

                    return {
                        Id: self.selectedMember().id, FirstName: firstname, LastName: lastname, Alias: alias, DefaultInstrumentId: defaultinstrumentid.length > 0 ? defaultinstrumentid: 0
                    };
                };

                self.getMemberInstrumentDetailFromDialog = function () {
                    var opts = $("#lstAssignedInstruments").find("option");
                    var ids = [];

                    opts.each(function (index, value) {
                        ids.push(value.value);
                    });

                    return {
                        MemberId: self.selectedMember().id, InstrumentIds: ids
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

                self.saveMember = function () {
                    var memberdetail = self.getMemberDetailFromDialog();
                    var jsonData = JSON.stringify(memberdetail);
                    var result;

                    $("body").css("cursor", "wait");

                    $.ajax({
                        type: "POST",
                        async: false,
                        url: site.url + "Members/Save/",
                        data: { member: jsonData },
                        dataType: "json",
                        traditional: true,
                        failure: function () {
                            $("body").css("cursor", "default");
                            $("#validation-container").html("");
                        },
                        success: function (data) {
                            if (data.Success) {
                                lists.MemberList = data.MemberList;
                                createMemberArray(lists.MemberList);
                                self.selectedMember(self.getMember(data.SelectedId));
                                self.sort({ afterSave: true });
                                self.highlightRow(self.selectedMember());
                                result = true;
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

                self.deleteMember = function (id) {
                    $("body").css("cursor", "wait");

                    $.ajax({
                        type: "POST",
                        url: site.url + "Members/Delete/",
                        data: { id: id },
                        dataType: "json",
                        traditional: true,
                        failure: function () {
                            $("body").css("cursor", "default");
                        },
                        success: function (data) {
                            if (data.Success) {
                                lists.MemberList = data.MemberList;
                                createMemberArray(lists.MemberList);
                                self.sort({ afterSave: true });
                            }
                            $("body").css("cursor", "default");
                        }
                    });

                    return true;
                };

                self.saveMemberInstruments = function () {
                    var memberInstrumentDetail = self.getMemberInstrumentDetailFromDialog();
                    var jsonData = JSON.stringify(memberInstrumentDetail);
                    var result;

                    $("body").css("cursor", "wait");

                    $.ajax({
                        type: "POST",
                        async: false,
                        url: site.url + "Members/SaveMemberInstruments/",
                        data: { memberInstrumentDetail: jsonData },
                        dataType: "json",
                        traditional: true,
                        failure: function () {
                            $("body").css("cursor", "default");
                        },
                        success: function (data) {
                            if (data.Success) {
                                lists.MemberList = data.MemberList;
                                createMemberArray(lists.MemberList);
                                self.selectedMember(self.getMember(data.SelectedId));
                                self.sort({ afterSave: true });
                                self.highlightRow(self.selectedMember());
                                result = true;
                            }
                            $("body").css("cursor", "default");
                        }
                    });

                    return result;
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
                            url: site.url + "Members/SaveColumns/",
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