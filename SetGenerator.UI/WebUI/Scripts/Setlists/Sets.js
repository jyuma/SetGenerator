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
                SongList: [],
                SpareList: [],
                MemberArrayList: [],
                GenreArrayList: [],
                TempoArrayList: [],
                SetNumberList: [],
                TableColumnList: []
            };

            loadConfig();

            ko.applyBindings(new SetsViewModel());

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

            function Song(id, setnumber, title, keydetail, singerid, composer, genreid, tempoid, neverclose, neveropen, disabled, instrumentmemberdetails) {
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
                var self = this;

                self.songs = ko.observableArray([]);
                self.selectedSong = ko.observable();
                self.selectedSetNumber = ko.observable(1);

                createSongArray(lists.SongList);

                $("#ddlColumns").on("hidden.bs.dropdown", function () {
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

                function createSongArray(list) {
                    self.songs.removeAll();

                    if (self.selectedSetNumber() > 0) {
                        $(list).each(function (index, value) {
                            if (value.SetNumber === self.selectedSetNumber()) {
                                pushSong(value);
                            }
                        });
                    }
                    else {
                        $(lists.SpareList).each(function (index, value) {
                            pushSong(value);
                        });
                    }
                };

                function pushSong(value) {
                    self.songs.push(new Song(value.Id, value.SetNumber, value.Title, value.KeyDetail, value.SingerId, value.Composer, value.GenreId, value.TempoId, value.NeverClose, value.NeverOpen, value.Disabled, value.SongMemberInstrumentDetails));
                };

                self.showSet = function (row) {
                    if (row.setnumber > 0) self.selectedSetNumber(row.setnumber);
                    else self.selectedSetNumber(0);
                    createSongArray(lists.SongList);

                };

                self.highlightRow = function (row) {
                    if (row == null) return;
                    var id = row.id;
                    var table = $("#tblSong");
                    var rows = $("#tblSong tr:gt(0)");
                    rows.each(function () {
                        $(this).css("background-color", "#ffffff");
                    });

                    var r = table.find("#row_" + id);
                    r.css("background-color", HIGHLIGHT_ROW_COLOUR);
                    $("#tblSong").attr("tr:hover", HIGHLIGHT_ROW_COLOUR);
                };

                //---------------------------------------------- CONTROLLER (BEGIN) -------------------------------------------------------

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

                //---------------------------------------------- CONTROLLER (END) -------------------------------------------------------

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

            //---------------------------------------------- DIALOG -------------------------------------------------------
        }
    }
})(jQuery);