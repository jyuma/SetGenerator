/*!
 * Songs/Index.js
 * Author: John Charlton
 * Date: 2015-05
 */

; (function ($) {

    var HIGHLIGHT_ROW_COLOUR = "#e3e8ff";
    var STRING_ALL_SINGERS = "Singer";
    var STRING_ALL_GENRES = "Genre";
    var STRING_ALL_TEMPOS = "Tempo";
    var STRING_NONE = "<None>";
    var LISTTYPE_ALIST = "A List";
    var LISTTYPE_DISABLED = "Shitcan";

    songs.index = {
        init: function () {
            var _singerNameList;
            var _currentSortKey = "title";
            var _sortDescending = false;

            var lists = {
                SongList: [],
                MemberArrayList: [],
                SingerArrayList: [],
                KeyListFull: [],
                GenreArrayList: [],
                TempoArrayList: [],
                TableColumnList: []
            };

            loadConfig();
            loadSingerNameList();

            ko.applyBindings(new SongViewModel());

            function loadConfig() {
                $.ajax({
                    type: "GET",
                    url: site.url + "Songs/GetData/",
                    dataType: "json",
                    traditional: true,
                    async: false,
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

            function Song(id, title, keydetail, singerid, genreid, tempoid, composer, neverclose, neveropen, disabled, updateuser, updatedate, issetsong, instrumentmemberdetails) {
                var self = this;

                self.id = id;
                self.title = title;
                self.keyid = keydetail.Id;
                self.keydetail = new SongKeyDetail(keydetail);
                self.singerid = singerid;
                self.genreid = genreid;
                self.tempoid = tempoid;
                self.composer = composer;
                self.neverClose = ko.observable(neverclose);
                self.neverOpen = ko.observable(neveropen);
                self.neverclose = neverclose;
                self.neveropen = neveropen;
                self.disabled = disabled;
                self.singer = getValue(lists.SingerArrayList, singerid, "Display", "Value");
                self.genre = getValue(lists.GenreArrayList, genreid, "Display", "Value");
                self.tempo = getValue(lists.TempoArrayList, tempoid, "Display", "Value");
                self.key = self.keydetail.name;
                self.updateuser = updateuser;
                self.updatedate = updatedate;
                self.issetsong = issetsong;

                self.memberInstruments = [];

                $(instrumentmemberdetails).each(function (index, value) {
                    var memberName = getValue(lists.MemberArrayList, value.MemberId, "Display", "Value").toLowerCase();
                    self[memberName] = getValue(instrumentmemberdetails, value.InstrumentId, "InstrumentName", "InstrumentId");
                });

                self.memberInstrumentDetails = instrumentmemberdetails;
            }

            //---------------------------------------------- VIEW MODEL (BEGIN) ---------------------------------------------------

            function SongViewModel() {
                var tblSong = $("#tblSong");
                var ddlColumns = $("#ddlColumns");

                var self = this;

                self.songs = ko.observableArray([]);
                self.singerNameSearchList = ko.observableArray([]);
                self.showDisabled = ko.observable(false);
                self.selectedSong = ko.observable();
                self.singerSearch = ko.observable(STRING_ALL_SINGERS);
                self.genreSearch = ko.observable(STRING_ALL_GENRES);
                self.tempoSearch = ko.observable(STRING_ALL_TEMPOS);
                self.keySearch = ko.observable("");
                self.titleSearch = ko.observable("");
                self.listTypeSearch = ko.observable(LISTTYPE_ALIST);
                self.totalSongs = ko.observable(0);

                createSongArray();
                createSingerSearchListArray();

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

                self.memberArrayList = ko.computed(function () {
                    var arr = [];
                    arr.push({ Value: 0, Display: STRING_NONE });
                    $(lists.MemberArrayList).each(function (index, value) {
                        arr.push({ Value: value.Value, Display: value.Display });
                    });
                    return arr;
                });

                self.genreArrayList = ko.computed(function () {
                    var arr = [];
                    
                    $(lists.GenreArrayList).each(function (index, value) {
                        arr.push({ Value: value.Value, Display: value.Display });
                    });
                    return arr;
                });

                self.genreNameSearchList = ko.computed(function () {
                    var arr = [];
                    arr.push(STRING_ALL_GENRES);
                    $(lists.GenreArrayList).each(function (index, value) {
                        arr.push(value.Display);
                    });
                    return arr;
                });

                self.tempoArrayList = ko.computed(function () {
                    var arr = [];
                    
                    $(lists.TempoArrayList).each(function (index, value) {
                        arr.push({ Value: value.Value, Display: value.Display });
                    });
                    return arr;
                });

                self.tempoNameSearchList = ko.computed(function () {
                    var arr = [];
                    arr.push(STRING_ALL_TEMPOS);
                    $(lists.TempoArrayList).each(function (index, value) {
                        arr.push(value.Display);
                    });
                    return arr;
                });

                self.listTypeSearchList = ko.computed(function () {
                    var arr = [];
                    arr.push(LISTTYPE_ALIST);
                    arr.push(LISTTYPE_DISABLED);
                    return arr;
                });

                function pushSong(value) {
                    self.songs.push(new Song(value.Id, value.Title, value.KeyDetail, value.SingerId, value.GenreId, value.TempoId, value.Composer, value.NeverClose, value.NeverOpen, value.Disabled, value.UserUpdate, value.DateUpdate, value.IsSetSong, value.SongMemberInstrumentDetails));
                };

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
                            self.songs.sort(function (a, b) {
                                if (_sortDescending) {
                                    return a[sortKey].toString().toLowerCase() > b[sortKey].toString().toLowerCase()
                                        ? -1 : a[sortKey].toString().toLowerCase() < b[sortKey].toString().toLowerCase()
                                        || a.title.toLowerCase() > b.title.toLowerCase() ? 1 : 0;
                                } else {
                                    return a[sortKey].toString().toLowerCase() < b[sortKey].toString().toLowerCase()
                                        ? -1 : a[sortKey].toString().toLowerCase() > b[sortKey].toString().toLowerCase()
                                        || a.title.toLowerCase() > b.title.toLowerCase() ? 1 : 0;
                                }
                            });
                        }
                    });
                };

                function createSongArray() {
                    self.songs.removeAll();
                    $(lists.SongList).each(function (index, value) {
                        if (value.Disabled && self.showDisabled()) {
                            pushSong(value);
                        }
                        if (!value.Disabled && !self.showDisabled()) {
                            pushSong(value);
                        }
                    });
                };

                function createSingerSearchListArray() {
                    self.singerNameSearchList.removeAll();

                    self.singerNameSearchList.push(STRING_ALL_SINGERS);
                    $(lists.SingerArrayList).each(function (index, value) {
                        self.singerNameSearchList.push(value.Display);
                    });
                }

                self.memberInstrumentHeaders = [
                    { title: "Member", sortKey: "member" },
                    { title: "Instrument", sortKey: "instrument" }
                ];

                self.filteredSongs = ko.computed(function () {
                    return ko.utils.arrayFilter(self.songs(), function (s) {
                        return (
                                (self.singerSearch() === STRING_ALL_SINGERS || s.singer === self.singerSearch())
                                && (self.genreSearch() === STRING_ALL_GENRES || s.genre === self.genreSearch())
                                && (self.tempoSearch() === STRING_ALL_TEMPOS || s.tempo === self.tempoSearch())
                                && ((self.titleSearch().length === 0 || s.title.toLowerCase().indexOf(self.titleSearch().toLowerCase()) !== -1))
                                && ((self.keySearch().length === 0 || s.key.toLowerCase().indexOf(self.keySearch().toLowerCase()) !== -1))
                        );
                    });
                });

                self.songsTable = ko.computed(function () {
                    var filteredSongs = self.filteredSongs();
                    self.totalSongs(filteredSongs.length);

                    return filteredSongs;
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
                        title: "<span class='glyphicon glyphicon-remove'></span> Delete Song?",
                        message: "<p>This will permanently delete the song <i>" + s.title + "</i>.</p>",
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

                    var title = self.showDisabled()
                        ? "<span class='glyphicon glyphicon-check'></span> Ressurrect It?"
                        : "<span class='glyphicon glyphicon-trash'></span> Shitcan It?";

                    var message = self.showDisabled()
                        ? "<p>This will ressurrect the song <i>" + s.title + "</i> and place it back into the A List.</p>"
                        : "<p>This will move the song <i>" + s.title + "</i> into the Shitcan.</p>";
                    
                    dialog.custom.showModal({
                        title: title,
                        message: message,
                        callback: function () {
                            return self.setDisabled(row.id);
                        },
                        width: 480
                    });
                };

                self.showSongEditDialog = function (row) {
                    var id = (typeof row.id !== "undefined" ? row.id : 0);
                    var title = "<span class='glyphicon glyphicon-pencil'></span>";
                    var message;

                    if (id > 0) {
                        title = title + " Edit Song";
                        var song = self.getSong(id);
                        self.selectedSong(song);
                    } else {
                        title = title + " Add Song";
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

                        var rows = tblSong.find("tr:gt(0)");
                        rows.each(function() {
                            $(this).css("background-color", "#ffffff");
                    });

                        var r = tblSong.find("#row_" + row.id);
                    r.css("background-color", HIGHLIGHT_ROW_COLOUR);
                    tblSong.attr("tr:hover", HIGHLIGHT_ROW_COLOUR);
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
                                lists.SongList = data.SongList;
                                lists.SingerArrayList = data.SingerArrayList;
                                createSongArray();
                                self.selectedSong(self.getSong(data.SelectedId));
                                self.sort({ afterSave: true });
                                self.highlightRow(self.selectedSong());
                                if (self.isNewSinger) {
                                    createSingerSearchListArray();
                                }

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

                self.isNewSinger = ko.computed(function() {
                    return (lists.SingerArrayList.length !== _singerNameList.length);
                });

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
                                lists.SongList = data.SongList;
                                lists.SingerArrayList = data.SingerArrayList;
                                createSongArray();
                                if (self.isNewSinger) {
                                    createSingerSearchListArray();
                                }
                                self.sort({ afterSave: true });
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
                                lists.SongList = data.SongList;
                                createSongArray();
                                self.sort({ afterSave: true });
                            }
                            $("body").css("cursor", "default");
                        }
                    });
                    return true;
                };

                self.saveColumns = function () {
                    var jsonData = JSON.stringify(self.getColumns());
                    // after changes
                    var selfColumns = self.getColumns();
                    // before changes
                    var tableColumns = lists.TableColumnList.filter(function (value) { return !value.AlwaysVisible; });
                    var isDifference = false;

                    $(selfColumns).each(function (index, value) {
                        if (tableColumns[index].IsVisible !== value.IsVisible) {
                            isDifference = true;
                        }
                    });

                    if (isDifference) {
                        $.ajax({
                            type: "POST",
                            url: site.url + "Songs/SaveColumns/",
                            data: { columns: jsonData },
                            dataType: "json",
                            traditional: true
                        });
                    }
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

                self.getSongFromDialog = function () {
                    var title = $.trim($("#txtTitle").val());
                    var singerid = $("#ddlSinger").val();
                    var genreid = $("#ddlGenre").val();
                    var tempoid = $("#ddlTempo").val();
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
                    return { Id: self.selectedSong().id, Title: title, SingerId: singerid, KeyId: keyid, GenreId: genreid, TempoId: tempoid, Composer: composer, NeverClose: neverclose, NeverOpen: neveropen, Disabled: disabled, SongMemberInstrumentDetails: meminstdetails };
                };
            };

            //---------------------------------------------- VIEW MODEL (END) -----------------------------------------------------

            //---------------------------------------------- GENERAL (BEGIN) ------------------------------------------------------

            function getKeyId(nameid, sharpflatnat, majminor) {
                var id = 0;
                $(lists.KeyListFull).each(function (index, value) {
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

            function loadSingerNameList() {
                var url = site.url + "Songs/GetSingerNameList/";

                $.ajax({
                    url: url,
                    type: "GET",
                    dataType: "json",
                    async: false,
                    cache: false,
                    success: function (result) {
                        _singerNameList = result;
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