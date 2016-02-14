﻿/*!
 * Setlists/Sets.js
 * Author: John Charlton
 * Date: 2015-05
 */

; (function ($) {

    var DEFAULT_BAND_ID = 1;
    var HIGHLIGHT_ROW_COLOUR = "#e3e8ff";

    setlists.sets = {
        init: function (options) {
            var config = {
                setlistId: 0
            }

            $.extend(config, options);

            var lists = {
                SetlistName: "",
                SetSongList: [],
                SpareList: [],
                MemberArrayList: [],
                GenreArrayList: [],
                TempoArrayList: [],
                SetNumberList: [],
                TableColumnList: []
            };

            loadConfig();

            ko.applyBindings(new SetsViewModel());

            $("#tblSetSong").tableDnD({ onDragClass: "myDragClass" });

            function loadConfig() {
                $.ajax({
                    type: "GET",
                    url: site.url + "Setlists/GetDataSets/",
                    data: { bandId: DEFAULT_BAND_ID, setlistId: config.setlistId },
                    dataType: "json",
                    traditional: true,
                    async: false,
                    success: function (data) {
                        $.extend(lists, data);
                    }
                });
            }

            function SetSong(id, setnumber, title, keydetail, singerid, composer, genreid, tempoid, neverclose, neveropen, disabled, instrumentmemberdetails) {
                var self = this;

                self.id = id;
                self.setnumber = setnumber;
                self.title = title;
                self.keyid = keydetail.Id;
                self.keydetail = new SongKeyDetail(keydetail);
                self.singerid = singerid;
                self.composer = composer;
                self.genreid = genreid;
                self.tempoid = tempoid;
                self.neverclose = neverclose;
                self.neveropen = neveropen;
                self.disabled = disabled;
                self.singer = getValue(lists.MemberArrayList, singerid, "Display", "Value");
                self.genre = getValue(lists.GenreArrayList, genreid, "Display", "Value");
                self.tempo = getValue(lists.TempoArrayList, tempoid, "Display", "Value");
                self.key = self.keydetail.name;
                self.memberInstruments = [];

                $(instrumentmemberdetails).each(function (index, value) {
                    var memberName = getValue(lists.MemberArrayList, value.MemberId, "Display", "Value").toLowerCase();
                    self[memberName] = getValue(instrumentmemberdetails, value.InstrumentId, "InstrumentAbbrev", "InstrumentId");
                });

                self.memberInstrumentDetails = instrumentmemberdetails;
            }

            function SongKeyDetail(detail) {
                var self = this;

                self.id = detail.Id;
                self.nameid = detail.NameId;
                self.sharpflatnat = detail.SharpFlatNatural;
                self.sharpflatnatdesc = GetSharpFlatNotation(detail.SharpFlatNatural);
                self.majminor = detail.MajorMinor;
                self.name = detail.Name + self.sharpflatnatdesc + (detail.MajorMinor == 1 ? "m" : "");
            }

            //---------------------------------------------- VIEW MODEL (BEGIN) -------------------------------------------------------

            function SetsViewModel() {
                var tblSetSong = $("#tblSetSong");
                var ddlColumns = $("#ddlColumns");

                var self = this;

                self.setSongs = ko.observableArray([]);
                self.selectedSetSong = ko.observable();
                self.selectedSetNumber = ko.observable(1);
                self.totalSongs = ko.observable(0);

                createSetSongArray(lists.SetSongList);

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

                self.setNumberList = ko.computed(function () {
                    var arr = [];
                    $(lists.SetNumberList).each(function (index, value) {
                        arr.push({ setnumber: value });
                    });
                    return arr;
                });

                function createSetSongArray(list) {
                    self.setSongs.removeAll();

                    if (self.selectedSetNumber() > 0) {
                        $(list).each(function (index, value) {
                            if (value.SetNumber === self.selectedSetNumber()) {
                                pushSetSong(value);
                            }
                        });
                    }
                    else {
                        $(lists.SpareList).each(function (index, value) {
                            pushSetSong(value);
                        });
                    }
                    var setSongs = self.setSongs();
                    self.totalSongs(setSongs.length);
                };

                function pushSetSong(value) {
                    self.setSongs.push(new SetSong(value.Id, value.SetNumber, value.Title, value.KeyDetail, value.SingerId, value.Composer, value.GenreId, value.TempoId, value.NeverClose, value.NeverOpen, value.Disabled, value.SongMemberInstrumentDetails));
                };

                self.showSet = function (row) {
                    if (row.setnumber > 0) {
                        self.selectedSetNumber(row.setnumber);
                    } else {
                        self.selectedSetNumber(0);
                    }

                    createSetSongArray(lists.SetSongList);

                    // only turn on DnD if not on Spares
                    if (row.setnumber > 0) {
                        tblSetSong.tableDnDUpdate();
                    }
                };

                self.isSequenceChanged = function () {
                    // get original order
                    var originalOrder = lists.SetSongList.filter(function(value) {
                        return (value.SetNumber === self.selectedSetNumber());
                    });

                    var originalIds = $.map(originalOrder, function (value) {
                        return value.Id;
                    });

                    // get new order
                    var newOrder = tblSetSong.find("tr").not("thead tr");
                    var newIds = [];
                    $(newOrder).each(function (index, value) {
                        var idx = value.id.lastIndexOf("_");
                        newIds.push(parseInt(value.id.substring(idx + 1, value.length)));
                    });

                    // see if the order has changed
                    var changedIds = $(originalIds).filter(function(index, value) {
                        return (value !== newIds[index]);
                    });

                    var result = (changedIds.length > 0);
                    return result;
                };

                self.setNewSongOrder = function (setnumber, data, event) {
                    if (setnumber === 0) return; // don't proceed if on Spares pages
                    if (event.target.localName !== "td") return;  // don't proceed if an anchor was clicked

                    var ischanged = self.isSequenceChanged();

                    if (ischanged) {
                        self.saveSetSongOrder();
                    }
                };

                self.highlightRow = function (row) {
                    if (row == null) return;

                    var rows = tblSetSong.find("tr:gt(0)");
                    rows.each(function () {
                        $(this).css("background-color", "#ffffff");
                    });

                    var r = tblSetSong.find("#row_" + row.id);
                    r.css("background-color", HIGHLIGHT_ROW_COLOUR);
                    tblSetSong.attr("tr:hover", HIGHLIGHT_ROW_COLOUR);
                };

                self.getSetSong = function (songid) {
                    var setsong;

                    ko.utils.arrayForEach(self.setSongs(), function (item) {
                        if (item.id === songid) {
                            setsong = item;
                        }
                    });
                    return setsong;
                };

                self.showMoveSongDialog = function (row) {
                    var message;
                    var songId = row.id;
                    var currentSet = self.selectedSetNumber();

                    $.ajax({
                        type: "GET",
                        async: false,
                        url: site.url + "Setlists/GetMoveSongView",
                        data: { totalSets: lists.SetNumberList.length, currentSet: currentSet },
                        success: function (data) {
                            message = data;
                        }
                    });

                    dialog.custom.showModal({
                        title: "Move From " + (currentSet > 0 ? "Set " + currentSet : "Spares"),
                        message: message,
                        callback: function () {
                            $("#validation-container").html("");
                            $("#validation-container").hide();
                            var setNumber = self.getSetNumberFromDialog();

                            if (setNumber === 0) {
                                return self.deleteSetSong(row.id); 
                            } else {
                                return self.moveSong(songId, setNumber);
                            }
                        },
                        width: 180
                    });
                };

                self.showMoveDialog = function (row) {
                    self.highlightRow(row);
                    self.showSongDeleteDialog(row);
                };

                self.showColumnSelectorDialog = function () {
                    
                    var div = $(document.createElement("div")).attr("id", "select-columns").addClass("panel");

                    div.append("<div class='row'><div class='col-sm-6'><span>Title</span><div class='col-sm-1 pull-right'><input type='checkbox' id='chkTitle' checked='checked' disabled='disabled' /></div></div></div>")
                        .append("<div class='row'><div class='col-sm-6'><span>Key</span><div class='col-sm-1 pull-right'><input type='checkbox' id='chkKey' checked='checked' /></div></div></div>")
                        .append("<div class='row'><div class='col-sm-6'><span>Singer</span><div class='col-sm-1 pull-right'><input type='checkbox' id='chkSinger' /></div></div></div>")
                        .append("<hr/><div class='row'><div class='col-sm-12'><label>Member Instrument (3 Max)</label></div></div>");
                        
                    $(lists.MemberArrayList).each(function(index, value) {
                        div.append("<div class='row'><div class='col-sm-6'><span>" + value.Display + "</span><div class='col-sm-1 pull-right'><input type='checkbox' onclick='enableMemberCheckboxes()' id='chkMember_" + value.Display + "' /></div></div></div>");
                    });

                    var html = div[0].outerHTML;

                    dialog.custom.showModal({
                        title: "Print Columns",
                        message: html,
                        callback: function () {
                            return self.downloadPDF();
                        },
                        width: 250
                    });
                }

                self.downloadPDF = function () {
                    var includeKey = $("#chkKey").is(":checked");
                    var includeSinger = $("#chkSinger").is(":checked");
                    var chkMembers = $("#select-columns").find(":checkbox[id^='chkMember_']:checked");
                    var member1 = "";
                    var member2 = "";
                    var member3 = "";

                    if (chkMembers.length > 0) {
                        member1 = chkMembers[0].id.substring(chkMembers[0].id.indexOf("_") + 1, chkMembers[0].id.length);
                    }

                    if (chkMembers.length > 1) {
                        member2 = chkMembers[1].id.substring(chkMembers[1].id.indexOf("_") + 1, chkMembers[1].id.length);
                    }

                    if (chkMembers.length > 2) {
                        member3 = chkMembers[2].id.substring(chkMembers[2].id.indexOf("_") + 1, chkMembers[2].id.length);
                    }

                    $(location).attr("href", site.url + "Setlists/Print/" +
                        "?setlistId=" + config.setlistId +
                        "&includeKey=" + includeKey +
                        "&includeSinger=" + includeSinger +
                        "&member1=" + member1 + 
                        "&member2=" + member2 +
                        "&member3=" + member3);

                    return true;
                }

                //---------------------------------------------- CONTROLLER (BEGIN) -------------------------------------------------------

                self.deleteSetSong = function (id) {
                    $("body").css("cursor", "wait");

                    $.ajax({
                        type: "POST",
                        url: site.url + "Setlists/DeleteSetSong/",
                        data: { setlistId: config.setlistId, songId: id },
                        dataType: "json",
                        traditional: true,
                        failure: function () {
                            $("body").css("cursor", "default");
                        },
                        success: function (data) {
                            if (data.Success) {
                                lists.SetSongList = data.SetSongList;
                                lists.SpareList = data.SpareList;
                                createSetSongArray(lists.SetSongList);
                                tblSetSong.tableDnDUpdate();
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
                            url: site.url + "Setlists/SaveColumnsSet/",
                            data: { columns: jsonData },
                            dataType: "json",
                            traditional: true
                        });
                    }
                };

                self.moveSong = function (songId, setNumber) {
                    var result;

                    $("body").css("cursor", "wait");

                    var url = site.url + "Setlists/MoveSong/";

                    $.ajax({
                        type: "POST",
                        async: false,
                        url: url,
                        data: { setlistId: config.setlistId, setNumber: setNumber, songId: songId },
                        dataType: "json",
                        traditional: true,
                        failure: function () {
                            $("body").css("cursor", "default");
                            $("#validation-container").html("");
                        },
                        success: function (data) {
                            if (data.Success) {
                                lists.SetSongList = data.SetSongList;
                                lists.SpareList = data.SpareList;
                                createSetSongArray(lists.SetSongList);
                                self.selectedSetSong(self.getSetSong(data.SelectedId));
                                self.highlightRow(self.selectedSetSong());
                                tblSetSong.tableDnDUpdate();
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

                self.saveSetSongOrder = function () {
                    var rows = tblSetSong.find("tr").not("thead tr");
                    var rowid = rows[0].id;
                    var setNumber = rowid.substring(rowid.indexOf("_") + 1,
                        rowid.lastIndexOf("_"));

                    var ids = [];
                    rows.each(function (index, value) {
                        var idx = value.id.lastIndexOf("_");
                        var id = value.id.substring(idx + 1, value.id.length);
                        ids.push(parseInt(id));
                    });

                    if (ids.length > 0) {
                        $.ajax({
                            type: "POST",
                            url: site.url + "Setlists/SaveSetSongOrder/",
                            data: { setlistId: config.setlistId, setNumber: setNumber, songIds: ids },
                            dataType: "json",
                            traditional: true,
                            success: function (data) {
                                if (data.Success) {
                                    lists.SetSongList = data.SetSongList;
                                    lists.SpareList = data.SpareList;
                                    createSetSongArray(lists.SetSongList);
                                    self.selectedSetSong(self.getSetSong(data.SelectedId));
                                    self.highlightRow(self.selectedSetSong());
                                    tblSetSong.tableDnDUpdate();
                                }
                            }
                        });
                    };
                }

                //---------------------------------------------- CONTROLLER (END) -------------------------------------------------------

                self.getSetNumberFromDialog = function () {
                    var setnum = parseInt($("#ddlLocation").val());
                    return setnum;
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
            }

            //---------------------------------------------- VIEW MODEL (END) -------------------------------------------------------

            function GetSharpFlatNotation(sharpflatnat) {
                var desc = "";
                if (sharpflatnat > 0)
                    desc = sharpflatnat === 1 ? "#" : "b";
                return desc;
            }

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