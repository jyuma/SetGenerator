﻿/*!
 * Songs/Index.js
 * Author: John Charlton
 * Date: 2015-05
 */

; (function ($) {

    var HIGHLIGHT_ROW_COLOUR = "#e3e8ff";
    var STRING_ALL = "<All>";
    var STRING_ALL_SINGERS = "All Singers";
    var STRING_NONE = "<None>";
    var LISTTYPE_ALIST = "A List";
    var LISTTYPE_DISABLED = "Shitcan";

    songs.index = {
        init: function () {

            var _memberNameList;

            var lists = {
                songList: [],
                memberArrayList: [],
                keyListFull: [],
                tableColumnList: []
            };

            loadConfig();
            loadMemberNameList();

            ko.applyBindings(new SongViewModel());

            $("#song-table-container").show();

            function loadConfig() {
                $.ajax({
                    type: "GET",
                    url: site.url + "Songs/GetData/",
                    dataType: "json",
                    traditional: true,
                    //async: false,
                    success: function (data) {
                        $.extend(lists, data);
                    }
                });
            }

            function SongKeyDetail(detail) {
                var self = this;

                self.id = detail.Id;
                self.nameid = detail.NameId;
                self.sharpflatnat = detail.SharpFlatNatural;
                self.sharpflatnatdesc = getSharpFlatNotation(detail.SharpFlatNatural);
                self.majminor = detail.MajorMinor;
                self.name = detail.Name + self.sharpflatnatdesc + (detail.MajorMinor === 1 ? "m" : "");
            }

            function Song(id, title, keydetail, singerid, composer, neverclose, neveropen, disabled, updateuser, updatedate, instrumentmemberdetails) {
                var self = this;

                self.id = id;
                self.title = title;
                self.keyid = keydetail.Id;
                self.keydetail = new SongKeyDetail(keydetail);
                self.singerid = singerid;
                self.composer = composer;
                self.neverclose = neverclose;
                self.neveropen = neveropen;
                self.disabled = disabled;
                self.singer = getValue(lists.memberArrayList, singerid, 'Display', 'Value');
                self.key = self.keydetail.name;
                self.updateuser = updateuser;
                self.updatedate = updatedate;
                self.memberInstruments = [];

                $(instrumentmemberdetails).each(function (index, value) {
                    var memberName = getValue(lists.memberArrayList, value.MemberId, "Display", "Value").toLowerCase();
                    self[memberName] = getValue(instrumentmemberdetails, value.InstrumentId, "InstrumentName", "InstrumentId");
                });

                self.memberInstrumentDetails = instrumentmemberdetails;
            }

            //---------------------------------------------- VIEW MODEL (BEGIN) ---------------------------------------------------

            function SongViewModel() {
                var self = this;

                self.songs = ko.observableArray([]);
                self.memberNameList = ko.observableArray(_memberNameList);
                self.showDisabled = ko.observable(false);
                self.selectedSong = ko.observable();
                self.memberSearch = ko.observable(STRING_ALL);
                self.keySearch = ko.observable('');
                self.titleSearch = ko.observable('');
                self.listTypeSearch = ko.observable(LISTTYPE_ALIST);

                createSongArray();

                $("#ddlColumns").on("hidden.bs.dropdown", function () {
                    self.saveColumns();
                });

                self.columns = ko.computed(function () {
                    var arr = [];
                    $(lists.tableColumnList).each(function (index, value) {
                        arr.push({ title: value.Header, sortKey: value.Data, dataMember: value.Data, isVisible: ko.observable(value.IsVisible), alwaysVisible: value.AlwaysVisible, isMember: value.IsMember });
                    });
                    return arr;
                });

                self.memberArrayList = ko.computed(function () {
                    var arr = [];
                    arr.push({ Value: 0, Display: STRING_NONE });
                    $(lists.memberArrayList).each(function (index, value) {
                        arr.push({ Value: value.Value, Display: value.Display });
                    });
                    return arr;
                });

                self.singerNameSearchList = ko.computed(function () {
                    var arr = [];
                    arr.push(STRING_ALL_SINGERS);
                    $(_memberNameList).each(function (index, value) {
                        arr.push(value);
                    });
                    return arr;
                });

                self.listTypeSearchList = ko.computed(function () {
                    var arr = [];
                    arr.push(LISTTYPE_ALIST);
                    arr.push(LISTTYPE_DISABLED);
                    return arr;
                });

                function createSongArray() {
                    self.songs.removeAll();
                    $(lists.songList).each(function (index, value) {
                        if (value.Disabled && self.showDisabled()) {
                            pushSong(value);
                        }
                        if (!value.Disabled && !self.showDisabled()) {
                            pushSong(value);
                        }
                    });
                };

                function pushSong(value) {
                    self.songs.push(new Song(value.Id, value.Title, value.KeyDetail, value.SingerId, value.Composer, value.NeverClose, value.NeverOpen, value.Disabled, value.UserUpdate, value.DateUpdate, value.SongMemberInstrumentDetails));
                };

                self.songsTable = self.songs;

                self.memberInstrumentHeaders = [
                    { title: "Member", sortKey: "member" },
                    { title: "Instrument", sortKey: "instrument" }
                ];

                self.sort = function (header) {
                    var sortKey = header.sortKey;

                    $(self.columns()).each(function (index, value) {
                        if (value.sortKey == sortKey) {
                            self.songs.sort(function (a, b) {
                                return a[sortKey] < b[sortKey] ? -1 : a[sortKey] > b[sortKey] ? 1 : a[sortKey] == b[sortKey] ? 0 : 0;
                            });
                        }
                    });
                };

                self.filteredSongs = ko.computed(function () {
                    return ko.utils.arrayFilter(self.songs(), function (s) {
                        return (
                                (self.memberSearch() === STRING_ALL_SINGERS || s.singer === self.memberSearch())
                                && ((self.titleSearch().length === 0 || s.title.toLowerCase().indexOf(self.titleSearch().toLowerCase()) !== -1))
                                && ((self.keySearch().length === 0 || s.key.toLowerCase().indexOf(self.keySearch().toLowerCase()) !== -1))
                        );
                    });
                });

                self.songsTable = ko.computed(function () {
                    return self.filteredSongs();
                });

                self.toggleDisabled = ko.computed(function () {
                    var listType = self.listTypeSearch();

                    if (listType === LISTTYPE_ALIST) {
                        self.showDisabled(false);
                    } else {
                        self.showDisabled(true);
                    }
                    createSongArray();
                });

                self.showEditDialog = function (s) {
                    self.highlightRow(s);
                    self.showSongEditDialog(s);
                };

                self.deleteSelectedSong = function (s) {
                    deleteSong(s.id);
                };

                self.showDeleteDialog = function (s) {
                    self.highlightRow(s);
                    self.showSongDeleteDialog(s);
                };

                self.showDisableDialog = function (s) {
                    self.highlightRow(s);
                    self.showSetDisabledDialog(s);
                };

                self.showSongDeleteDialog = function (row) {
                    var id = (typeof row.id !== "undefined" ? row.id : 0);
                    if (id <= 0) return;
                    var s = self.getSong(id);

                    dialog.custom.showModal({
                        title: "Delete Song?",
                        message: "This will permanently delete the song '" + s.title + "'.",
                        callback: function () {
                            return self.deleteSong(row.id);
                        },
                        width: 500
                    });
                };

                self.showSetDisabledDialog = function (row) {
                    var id = (typeof row.id !== "undefined" ? row.id : 0);
                    var s = self.getSong(id);
                    if (id <= 0) return;

                    var title = self.showDisabled() ? "Ressurrect It?" : "Shitcan It?";

                    var message = self.showDisabled()
                        ? "This will ressurrect the song '" + s.title + "' and place it back into the A List."
                        : "This will place the song '" + s.title + "' into the Shitcan.";
                    
                    dialog.custom.showModal({
                        title: title,
                        message: message,
                        callback: function () {
                            return self.setDisabled(row.id);
                        },
                        width: 500
                    });
                };

                self.showSongEditDialog = function (row) {
                    var id = (typeof row.id !== "undefined" ? row.id : 0);
                    var title;
                    var message;

                    if (id > 0) {
                        title = "Edit Song";
                        var song = self.getSong(id);
                        self.selectedSong(song);
                    } else {
                        title = "Add Song";
                        self.selectedSong([]);
                    }

                    $.ajax({
                            type: "GET",
                            async: false,
                            url: site.url + "Songs/GetSongEditView/" + id,
                            success: function(data) {
                                message = data;
                            }
                    });

                    dialog.custom.showModal({
                        title: title,
                        message: message,
                        focusElement: "txtTitle",
                        callback: function () {
                            $("#validation-container").html("");
                            $("#validation-container").hide();
                            return self.saveSong();
                        },
                        width: 400
                    });
                };

                self.getSong = function (songid) {
                    var song = null;

                    ko.utils.arrayForEach(self.songs(), function (item) {
                        if (item.id === songid) {
                            song = item;
                        }
                    });
                    return song;
                };

                self.highlightRow = function (row) {
                    if (row == null) return;
                        var id = row.id;
                        var table = $("#tblSong");
                        var rows = $("#tblSong tr:gt(0)");
                        rows.each(function() {
                            $(this).css("background-color", "#ffffff");
                    });

                    var r = table.find("#row_" +id);
                    r.css("background-color", HIGHLIGHT_ROW_COLOUR);
                    $("#tblSong").attr("tr:hover", HIGHLIGHT_ROW_COLOUR);
                };

                self.saveSong = function () {
                    var song = self.getSongFromDialog();
                    var jsonData = JSON.stringify(song);
                    var result;

                    $("body").css("cursor", "wait");

                    $.ajax({
                        type: "POST",
                        async: false,
                        url: site.url + "Songs/Save/",
                        data: { song: jsonData },
                        dataType: "json",
                        traditional: true,
                        failure: function () {
                            $("body").css("cursor", "default");
                            $("#validation-container").html("");
                        },
                        success: function (data) {
                            if (data.Success) {
                                lists.songList = data.SongList;
                                createSongArray();
                                self.selectedSong(self.getSong(data.SelectedId));
                                self.highlightRow(self.selectedSong());
                                result = true;
                            } else {
                                if (data.ErrorMessages.length > 0) {
                                    $("#validation-container").show();
                                    $("#validation-container").html("");
                                    var html = "<ul>";
                                    for (var i = 0; i < data.ErrorMessages.length; i++) {
                                        var message = data.ErrorMessages[i];
                                        html = html + "<li>" + message + "</li>";
                                    }
                                    html = html + "</ul>";
                                    $("#validation-container").append(html);
                                    result = false;
                                }
                            }
                            $("body").css("cursor", "default");
                        }
                    });

                    return result;
                };

                self.deleteSong = function (songid) {
                    $("body").css("cursor", "wait");

                    $.ajax({
                        type: "POST",
                        async: false,
                        url: site.url + "Songs/Delete/",
                        data: { id: songid },
                        dataType: "json",
                        traditional: true,
                        failure: function () {
                            $("body").css("cursor", "default");
                        },
                        success: function (data) {
                            if (data.Success) {
                                lists.songList = data.SongList;
                                createSongArray();
                            }
                            $("body").css("cursor", "default");
                        }
                    });
                    return true;
                };

                self.setDisabled = function (songid) {
                    $("body").css("cursor", "wait");
                    
                    $.ajax({
                        type: "POST",
                        async: false,
                        url: site.url + "Songs/SetDisabled/",
                        data: { id: songid, disabled: !self.showDisabled() },
                        dataType: "json",
                        traditional: true,
                        failure: function () {
                            $("body").css("cursor", "default");
                        },
                        success: function (data) {
                            if (data.Success) {
                                lists.songList = data.SongList;
                                createSongArray();
                            }
                            $("body").css("cursor", "default");
                        }
                    });
                    return true;
                };

                self.saveColumns = function () {
                    var jsonData = JSON.stringify(self.getColumns());
                    $.ajax({
                        type: "POST",
                        url: site.url + "Songs/SaveColumns/",
                        data: { columns: jsonData },
                        dataType: "json",
                        traditional: true
                    });
                };

                self.getColumns = function () {
                    var arr = [];
                    $(self.columns()).each(function (index, value) {
                        arr.push({ Header: value.title, Data: value.dataMember, IsVisible: value.isVisible() });
                    });
                    return arr;
                };

                self.getSongFromDialog = function () {
                    var title = $.trim($("#txtTitle").val());
                    var singerid = $("#ddlSinger").val();
                    var nameid = parseInt($("#ddlKey").val());
                    var sharpflatnat = parseInt($("#ddlSharpFlatNatural").val());
                    var majminor = parseInt($("#ddlMajorMinor").val());
                    var composer = $.trim($("#txtComposer").val());
                    var neverclose = $("#chkNeverClose").is(":checked");
                    var neveropen = $("#chkNeverOpen").is(":checked");
                    var disabled = (self.listTypeSearch() === LISTTYPE_DISABLED);
                    var meminstdetails = [];
                    var keyid = getKeyId(nameid, sharpflatnat, majminor);

                    $("#tblSongMemberIstrument tr").not("thead tr").each(function (index, value) {
                        var memberid = value.id.replace("row_", "");
                        var instrumentid = $(this).find("select[name=ddlMemberInstrument_" + memberid + "]").val();
                        meminstdetails.push({ MemberId: memberid, InstrumentId: instrumentid });
                    });
                    return { Id: self.selectedSong().id, Title: title, SingerId: singerid, KeyId: keyid, Composer: composer, NeverClose: neverclose, NeverOpen: neveropen, Disabled: disabled, SongMemberInstrumentDetails: meminstdetails };
                };
            };

            //---------------------------------------------- VIEW MODEL (END) -----------------------------------------------------

            //---------------------------------------------- GENERAL (BEGIN) ------------------------------------------------------

            function getKeyId(nameid, sharpflatnat, majminor) {
                var id = 0;
                $(lists.keyListFull).each(function (index, value) {
                    if (value.NameId === nameid && value.SharpFlatNatural === sharpflatnat && value.MajorMinor === majminor) {
                        id = value.Id;
                    }
                });
                return id;
            }

            function getSharpFlatNotation(sharpflatnat) {
                var desc = "";
                if (sharpflatnat > 0)
                    desc = sharpflatnat === 1 ? "#" : "b";
                return desc;
            }

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

            //---------------------------------------------- GENERAL (END) ------------------------------------------------------

            function loadMemberNameList() {
                var url = site.url + "Songs/GetMemberNameList/";

                $.ajax({
                    url: url,
                    type: "GET",
                    dataType: "json",
                    async: false,
                    cache: false,
                    success: function (result) {
                        _memberNameList = result;
                    }
                });
            }

            //function isSessionExpired() {
            //    var url = site.url + "GetCurrentSessionUser/";
            //    var user;
            //    $.ajax({
            //        url: url,
            //        data: null,
            //        type: "GET",
            //        dataType: "text",
            //        async: false,
            //        cache: false,
            //        success: function (result) {
            //            user = result;
            //        }
            //    });
            //    return (user.length === 0);
            //}

            //---------------------------------------------- CONTROLLER (END) -------------------------------------------------------
        }
    }
})(jQuery);