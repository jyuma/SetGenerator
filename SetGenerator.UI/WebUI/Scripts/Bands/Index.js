/*!
 * Bands/Index.js
 * Author: John Charlton
 * Date: 2015-05
 */
; (function ($) {

    var HIGHLIGHT_ROW_COLOUR = "#e3e8ff";

    bands.index = {
        init: function (options) {
            var _sortDescending = false;
            var _currentSortKey = "name";

            var config = {
                bandId: "0"
            };

            $.extend(config, options);

            var lists = {
                BandList: [],
                DefaultSingerArrayList: [],
                TableColumnList: []
            };

            loadConfig();

            ko.applyBindings(new BandViewModel());

            // pre-select the band whose members were just being managed
            if (config.bandId > 0) { $("#row_" + config.bandId).trigger("click"); }

            function loadConfig() {
                $.ajax({
                    type: "GET",
                    url: site.url + "Bands/GetData/",
                    dataType: "json",
                    traditional: true,
                    async: false,
                    success: function (data) {
                        $.extend(lists, data);
                    }
                });
            }

            function Band(id, name, defaultsingerid, updateuser, updatedate) {
                var self = this;

                self.id = id;
                self.name = name;
                self.defaultsinger = getValue(lists.DefaultSingerArrayList, defaultsingerid, "Display", "Value");
                self.updateuser = updateuser;
                self.updatedate = updatedate;
            }

            function BandViewModel() {
                var tblBand = $("#tblBand");
                var ddlColumns = $("#ddlColumns");

                var self = this;

                self.bands = ko.observableArray([]);
                self.selectedBand = ko.observable();
                self.nameSearch = ko.observable("");
                self.editFormHeader = ko.observable("");
                self.totalBands = ko.observable(0);

                createBandArray(lists.BandList);

                function createBandArray(list) {
                    self.bands.removeAll();
                    $(list).each(function (index, value) {
                        pushBand(value);
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

                function pushBand(value) {
                    self.bands.push(new Band(value.Id, value.Name, value.DefaultSingerId, value.UserUpdate, value.DateUpdate));
                };

                self.selectedBand(self.bands()[0]);

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
                            self.bands.sort(function (a, b) {
                                if (_sortDescending) {
                                    return a[sortKey].toString().toLowerCase() > b[sortKey].toString().toLowerCase()
                                        ? -1 : a[sortKey].toString().toLowerCase() < b[sortKey].toString().toLowerCase()
                                        || a.name.toLowerCase() > b.name.toLowerCase() ? 1 : 0;
                                } else {
                                    return a[sortKey].toString().toLowerCase() < b[sortKey].toString().toLowerCase()
                                        ? -1 : a[sortKey].toString().toLowerCase() > b[sortKey].toString().toLowerCase()
                                        || a.name.toLowerCase() > b.name.toLowerCase() ? 1 : 0;
                                }
                            });
                        }
                    });
                };

                self.filteredBands = ko.computed(function () {
                    return ko.utils.arrayFilter(self.bands(), function (g) {
                        return (
                            (self.nameSearch().length === 0 || g.name.toLowerCase().indexOf(self.nameSearch().toLowerCase()) !== -1)
                        );
                    });
                });

                self.bandsTable = ko.computed(function () {
                    var filteredBands = self.filteredBands();
                    self.totalBands(filteredBands.length);

                    return filteredBands;
                });

                self.showEditDialog = function (row) {
                    var id = row.id;

                    if (id > 0) {
                        self.highlightRow(row);
                    }

                    self.showBandEditDialog(row);
                };

                self.deleteSelectedBand = function (row) {
                    deleteBand(row.id);
                };

                self.showDeleteDialog = function (row) {
                    self.highlightRow(row);
                    self.showBandDeleteDialog(row);
                };

                self.showBandDeleteDialog = function (row) {
                    var id = (typeof row.id !== "undefined" ? row.id : 0);
                    if (id <= 0) return;
                    var g = self.getBand(id);

                    dialog.custom.showModal({
                        title: "<span class='glyphicon glyphicon-remove'></span> Delete Band?",
                        message: "<p>This will permanently delete the band <i>" + g.name + "</i>.</p>",
                        callback: function () {
                            return self.deleteBand(row.id);
                        },
                        width: 500
                    });
                };

                self.showBandEditDialog = function (row) {
                    var id = (typeof row.id !== "undefined" ? row.id : 0);
                    var title = "<span class='glyphicon glyphicon-pencil'></span>";
                    var message;

                    if (id > 0) {
                        title = title + " Edit Band";
                        var band = self.getBand(id);
                        self.selectedBand(band);
                    } else {
                        title = title + " Add Band";
                        self.selectedBand([]);
                    }

                    $.ajax({
                        type: "GET",
                        async: false,
                        url: site.url + "Bands/GetBandEditView/" + id,
                        success: function (data) {
                            message = data;
                        }
                    });

                    dialog.custom.showModal({
                        title: title,
                        message: message,
                        focusElement: "txtName",
                        callback: function () {
                            $("#validation-container").html("");
                            $("#validation-container").hide();
                            return self.saveBand();
                        },
                        width: 400
                    });
                };

                self.getBand = function (bandid) {
                    var band = null;

                    ko.utils.arrayForEach(self.bands(), function (item) {
                        if (item.id === bandid) {
                            band = item;
                        }
                    });

                    return band;
                };

                self.highlightRow = function (row) {
                    if (row == null) return;

                    var rows = tblBand.find("tr:gt(0)");
                    rows.each(function () {
                        $(this).css("background-color", "#ffffff");
                    });

                    var r = tblBand.find("#row_" + row.id);
                    r.css("background-color", HIGHLIGHT_ROW_COLOUR);
                    $("#tblBand").attr("tr:hover", HIGHLIGHT_ROW_COLOUR);
                };

                self.getBandDetailFromDialog = function () {
                    var name = $.trim($("#txtName").val());
                    var defaultsingerid = $.trim($("#ddlMembers").val());

                    return {
                        Id: self.selectedBand().id, Name: name, DefaultSingerId: defaultsingerid
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

                self.showMembers = function (row) {
                    var id = row.id;
                    window.location.href = site.url + "Bands/" + id + "/Members";
                }

                //---------------------------------------------- CONTROLLER (BEGIN) -------------------------------------------------------

                self.saveBand = function () {
                    var banddetail = self.getBandDetailFromDialog();
                    var jsonData = JSON.stringify(banddetail);
                    var result;

                    $("body").css("cursor", "wait");

                    $.ajax({
                        type: "POST",
                        async: false,
                        url: site.url + "Bands/Save/",
                        data: { band: jsonData },
                        dataType: "json",
                        traditional: true,
                        failure: function () {
                            $("body").css("cursor", "default");
                            $("#validation-container").html("");
                        },
                        success: function (data) {
                            if (data.Success) {
                                lists.BandList = data.BandList;
                                createBandArray(lists.BandList);
                                self.selectedBand(self.getBand(data.SelectedId));
                                self.sort({ afterSave: true });
                                self.highlightRow(self.selectedBand());
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

                self.deleteBand = function (id) {
                    $("body").css("cursor", "wait");

                    $.ajax({
                        type: "POST",
                        url: site.url + "Bands/Delete/",
                        data: { id: id },
                        dataType: "json",
                        traditional: true,
                        failure: function () {
                            $("body").css("cursor", "default");
                        },
                        success: function (data) {
                            if (data.Success) {
                                lists.BandList = data.BandList;
                                createBandArray(lists.BandList);
                                self.sort({ afterSave: true });
                            }
                            $("body").css("cursor", "default");
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
                            url: site.url + "Bands/SaveColumns/",
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