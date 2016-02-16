/*!
 * Gigs/Index.js
 * Author: John Charlton
 * Date: 2015-05
 */
; (function ($) {

    var HIGHLIGHT_ROW_COLOUR = "#e3e8ff";

    gigs.index = {
        init: function () {
            var _sortDescending = false;
            var _currentSortKey = "venue";

            var lists = {
                GigList: [],
                TableColumnList: []
            };

            loadConfig();

            ko.applyBindings(new GigViewModel());

            function loadConfig() {
                $.ajax({
                    type: "GET",
                    url: site.url + "Gigs/GetData/",
                    dataType: "json",
                    traditional: true,
                    async: false,
                    success: function (data) {
                        $.extend(lists, data);
                    }
                });
            }

            function Gig(id, venue, description, dategig, updateuser, updatedate) {
                var self = this;

                self.id = id;
                self.venue = venue;
                self.description = description;
                self.dategig = dategig;
                self.updateuser = updateuser;
                self.updatedate = updatedate;
            }

            function GigViewModel() {
                var tblGig = $("#tblGig");
                var ddlColumns = $("#ddlColumns");

                var self = this;

                self.gigs = ko.observableArray([]);
                self.selectedGig = ko.observable();
                self.venueSearch = ko.observable("");
                self.editFormHeader = ko.observable("");
                self.totalGigs = ko.observable(0);

                createGigArray(lists.GigList);

                function createGigArray(list) {
                    self.gigs.removeAll();
                    $(list).each(function (index, value) {
                        pushGig(value);
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

                function pushGig(value) {
                    self.gigs.push(new Gig(value.Id, value.Venue, value.Description, value.DateGig, value.UserUpdate, value.DateUpdate));
                };

                self.selectedGig(self.gigs()[0]);

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
                            self.gigs.sort(function (a, b) {
                                if (_sortDescending) {
                                    return a[sortKey].toString().toLowerCase() > b[sortKey].toString().toLowerCase()
                                        ? -1 : a[sortKey].toString().toLowerCase() < b[sortKey].toString().toLowerCase()
                                        || a.venue.toLowerCase() > b.venue.toLowerCase() ? 1 : 0;
                                } else {
                                    return a[sortKey].toString().toLowerCase() < b[sortKey].toString().toLowerCase()
                                        ? -1 : a[sortKey].toString().toLowerCase() > b[sortKey].toString().toLowerCase()
                                        || a.venue.toLowerCase() > b.venue.toLowerCase() ? 1 : 0;
                                }
                            });
                        }
                    });
                };

                self.filteredGigs = ko.computed(function () {
                    return ko.utils.arrayFilter(self.gigs(), function (g) {
                        return (
                            (self.venueSearch().length === 0 || g.venue.toLowerCase().indexOf(self.venueSearch().toLowerCase()) !== -1)
                        );
                    });
                });

                self.gigsTable = ko.computed(function () {
                    var filteredGigs = self.filteredGigs();
                    self.totalGigs(filteredGigs.length);

                    return filteredGigs;
                });

                self.showEditDialog = function (row) {
                    var id = row.id;

                    if (id > 0) {
                        self.highlightRow(row);
                    }

                    self.showGigEditDialog(row);
                };

                self.deleteSelectedGig = function (row) {
                    deleteGig(row.id);
                };

                self.showDeleteDialog = function (row) {
                    self.highlightRow(row);
                    self.showGigDeleteDialog(row);
                };

                self.showGigDeleteDialog = function (row) {
                    var id = (typeof row.id !== "undefined" ? row.id : 0);
                    if (id <= 0) return;
                    var g = self.getGig(id);

                    dialog.custom.showModal({
                        title: "<span style='color: #fff' class='glyphicon glyphicon-remove'></span> Delete Gig?",
                        message: "This will permanently delete the gig for venue '" + g.venue + "' on " + g.dategig + ".",
                        callback: function () {
                            return self.deleteGig(row.id);
                        },
                        width: 500
                    });
                };

                self.showGigEditDialog = function (row) {
                    var id = (typeof row.id !== "undefined" ? row.id : 0);
                    var title = "<span style='color: #fff' class='glyphicon glyphicon-pencil'></span>";
                    var message;

                    if (id > 0) {
                        title = title + " Edit Gig";
                        var gig = self.getGig(id);
                        self.selectedGig(gig);
                    } else {
                        title = title + " Add Gig";
                        self.selectedGig([]);
                    }

                    $.ajax({
                        type: "GET",
                        async: false,
                        url: site.url + "Gigs/GetGigEditView/" + id,
                        success: function (data) {
                            message = data;
                        }
                    });

                    dialog.custom.showModal({
                        title: title,
                        message: message,
                        focusElement: "txtVenue",
                        callback: function () {
                            $("#validation-container").html("");
                            $("#validation-container").hide();
                            return self.saveGig();
                        },
                        width: 400
                    });
                };

                self.getGig = function (gigid) {
                    var gig = null;

                    ko.utils.arrayForEach(self.gigs(), function (item) {
                        if (item.id === gigid) {
                            gig = item;
                        }
                    });

                    return gig;
                };

                self.highlightRow = function (row) {
                    if (row == null) return;

                    var rows = tblGig.find("tr:gt(0)");
                    rows.each(function () {
                        $(this).css("background-color", "#ffffff");
                    });

                    var r = tblGig.find("#row_" + row.id);
                    r.css("background-color", HIGHLIGHT_ROW_COLOUR);
                    $("#tblGig").attr("tr:hover", HIGHLIGHT_ROW_COLOUR);
                };

                self.getGigDetailFromDialog = function () {
                    var venue = $.trim($("#txtVenue").val());
                    var description = $.trim($("#txtDescription").val());
                    var dategig = $("#dtDateGig").val();

                    return {
                        Id: self.selectedGig().id, Venue: venue, Description: description, DateGig: dategig
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

                self.saveGig = function () {
                    var gigdetail = self.getGigDetailFromDialog();
                    var jsonData = JSON.stringify(gigdetail);
                    var result;

                    $("body").css("cursor", "wait");

                    $.ajax({
                        type: "POST",
                        async: false,
                        url: site.url + "Gigs/Save/",
                        data: { gig: jsonData },
                        dataType: "json",
                        traditional: true,
                        failure: function () {
                            $("body").css("cursor", "default");
                            $("#validation-container").html("");
                        },
                        success: function (data) {
                            if (data.Success) {
                                lists.GigList = data.GigList;
                                createGigArray(lists.GigList);
                                self.selectedGig(self.getGig(data.SelectedId));
                                self.sort({ afterSave: true });
                                self.highlightRow(self.selectedGig());
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

                self.deleteGig = function (id) {
                    $("body").css("cursor", "wait");

                    $.ajax({
                        type: "POST",
                        url: site.url + "Gigs/Delete/",
                        data: { id: id },
                        dataType: "json",
                        traditional: true,
                        failure: function () {
                            $("body").css("cursor", "default");
                        },
                        success: function (data) {
                            if (data.Success) {
                                lists.GigList = data.GigList;
                                createGigArray(lists.GigList);
                                self.sort({ afterSave: true });
                            }
                            $("body").css("cursor", "default");
                        }
                    });

                    return true;
                };

                self.saveColumns = function() {
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
                            url: site.url + "Gigs/SaveColumns/",
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