/*!
 * Instruments/Index.js
 * Author: John Charlton
 * Date: 2015-05
 */
; (function ($) {

    var HIGHLIGHT_ROW_COLOUR = "#e3e8ff";

    instruments.index = {
        init: function () {
            var _sortDescending = false;
            var _currentSortKey = "name";

            var lists = {
                InstrumentList: [],
                TableColumnList: []
            };

            loadConfig();

            ko.applyBindings(new InstrumentViewModel());

            function loadConfig() {
                $.ajax({
                    type: "GET",
                    url: site.url + "Instruments/GetData/",
                    dataType: "json",
                    traditional: true,
                    async: false,
                    success: function (data) {
                        $.extend(lists, data);
                    }
                });
            }

            function Instrument(id, name, abbreviation, issonginstrument) {
                var self = this;

                self.id = id;
                self.name = name;
                self.abbreviation = abbreviation;
                self.issonginstrument = issonginstrument;
            }

            function InstrumentViewModel() {
                var tblInstrument = $("#tblInstrument");
                var ddlColumns = $("#ddlColumns");

                var self = this;

                self.instruments = ko.observableArray([]);
                self.selectedInstrument = ko.observable();
                self.nameSearch = ko.observable("");
                self.editFormHeader = ko.observable("");
                self.totalInstruments = ko.observable(0);

                createInstrumentArray(lists.InstrumentList);

                function createInstrumentArray(list) {
                    self.instruments.removeAll();
                    $(list).each(function (index, value) {
                        pushInstrument(value);
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

                function pushInstrument(value) {
                    self.instruments.push(new Instrument(value.Id, value.Name, value.Abbreviation, value.IsSongInstrument));
                };

                self.selectedInstrument(self.instruments()[0]);

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
                            self.instruments.sort(function (a, b) {
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

                self.filteredInstruments = ko.computed(function () {
                    return ko.utils.arrayFilter(self.instruments(), function (g) {
                        return (
                            (self.nameSearch().length === 0 || g.name.toLowerCase().indexOf(self.nameSearch().toLowerCase()) !== -1)
                        );
                    });
                });

                self.instrumentsTable = ko.computed(function () {
                    var filteredInstruments = self.filteredInstruments();
                    self.totalInstruments(filteredInstruments.length);

                    return filteredInstruments;
                });

                self.showEditDialog = function (row) {
                    var id = row.id;

                    if (id > 0) {
                        self.highlightRow(row);
                    }

                    self.showInstrumentEditDialog(row);
                };

                self.deleteSelectedInstrument = function (row) {
                    deleteInstrument(row.id);
                };

                self.showDeleteDialog = function (row) {
                    self.highlightRow(row);
                    self.showInstrumentDeleteDialog(row);
                };

                self.showInstrumentDeleteDialog = function (row) {
                    var id = (typeof row.id !== "undefined" ? row.id : 0);
                    if (id <= 0) return;
                    var i = self.getInstrument(id);

                    dialog.custom.showModal({
                        title: "<span class='glyphicon glyphicon-remove'></span> Delete Instrument?",
                        message: "<p>This will permanently delete the instrument <i><b>" + i.name + "</b></i>.</p>",
                        callback: function () {
                            return self.deleteInstrument(row.id);
                        },
                        width: 400
                    });
                };

                self.showInstrumentEditDialog = function (row) {
                    var id = (typeof row.id !== "undefined" ? row.id : 0);
                    var title = "<span class='glyphicon glyphicon-pencil'></span>";
                    var message;

                    if (id > 0) {
                        title = title + " Edit Instrument";
                        var instrument = self.getInstrument(id);
                        self.selectedInstrument(instrument);
                    } else {
                        title = title + " Add Instrument";
                        self.selectedInstrument([]);
                    }

                    $.ajax({
                        type: "GET",
                        async: false,
                        url: site.url + "Instruments/GetInstrumentEditView/" + id,
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
                            return self.saveInstrument();
                        },
                        width: 400
                    });
                };

                self.getInstrument = function (instrumentid) {
                    var instrument = null;

                    ko.utils.arrayForEach(self.instruments(), function (item) {
                        if (item.id === instrumentid) {
                            instrument = item;
                        }
                    });

                    return instrument;
                };

                self.highlightRow = function (row) {
                    if (row == null) return;

                    var rows = tblInstrument.find("tr:gt(0)");
                    rows.each(function () {
                        $(this).css("background-color", "#ffffff");
                    });

                    var r = tblInstrument.find("#row_" + row.id);
                    r.css("background-color", HIGHLIGHT_ROW_COLOUR);
                    $("#tblInstrument").attr("tr:hover", HIGHLIGHT_ROW_COLOUR);
                };

                self.getInstrumentDetailFromDialog = function () {
                    var name = $.trim($("#txtName").val());
                    var abbreviation = $.trim($("#txtAbbreviation").val());

                    return {
                        Id: self.selectedInstrument().id,
                        Name: name,
                        Abbreviation: abbreviation
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

                self.saveInstrument = function () {
                    var instrumentdetail = self.getInstrumentDetailFromDialog();
                    var jsonData = JSON.stringify(instrumentdetail);
                    var result;

                    $("body").css("cursor", "wait");

                    $.ajax({
                        type: "POST",
                        async: false,
                        url: site.url + "Instruments/Save/",
                        data: { instrument: jsonData },
                        dataType: "json",
                        traditional: true,
                        failure: function () {
                            $("body").css("cursor", "default");
                            $("#validation-container").html("");
                        },
                        success: function (data) {
                            if (data.Success) {
                                lists.InstrumentList = data.InstrumentList;
                                createInstrumentArray(lists.InstrumentList);
                                self.selectedInstrument(self.getInstrument(data.SelectedId));
                                self.sort({ afterSave: true });
                                self.highlightRow(self.selectedInstrument());
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

                self.deleteInstrument = function (id) {
                    $("body").css("cursor", "wait");

                    $.ajax({
                        type: "POST",
                        url: site.url + "Instruments/Delete/",
                        data: { id: id },
                        dataType: "json",
                        traditional: true,
                        failure: function () {
                            $("body").css("cursor", "default");
                        },
                        success: function (data) {
                            if (data.Success) {
                                lists.InstrumentList = data.InstrumentList;
                                createInstrumentArray(lists.InstrumentList);
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
                            url: site.url + "Instruments/SaveColumns/",
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
        }
    }
})(jQuery);