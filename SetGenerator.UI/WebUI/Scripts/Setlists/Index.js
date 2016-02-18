/*!
 * Setlists/Index.js
 * Author: John Charlton
 * Date: 2015-05
 */
; (function ($) {

    var HIGHLIGHT_ROW_COLOUR = "#e3e8ff";
    var STRING_MY_SETLISTS = "My Setlists";
    var STRING_ALL_SETLISTS = "All Setlists";

    setlists.index = {
        init: function(options) {
            var _sortDescending = false;
            var _currentSortKey = "name";

            var config = {
                setlistId: "",
                currentUser: "",
                selectedOwnerSearch: ""
            };

            $.extend(config, options);

            var lists = {
                SetlistList: [],
                TableColumnList: []
            };

            loadConfig();

            ko.applyBindings(new SetlistViewModel());

            if (config.setlistId.length > 0) {
                $("#row_" + config.setlistId).trigger("click");
            }

            function loadConfig() {
                $.ajax({
                    type: "GET",
                    url: site.url + "Setlists/GetData/",
                    dataType: "json",
                    traditional: true,
                    async: false,
                    success: function(data) {
                        $.extend(lists, data);
                    }
                });
            }

            function Setlist(id, name, numsets, numsongs, owner, updateuser, updatedate) {
                var self = this;

                self.id = id;
                self.name = name;
                self.numsets = numsets;
                self.numsongs = numsongs;
                self.owner = owner;
                self.updateuser = updateuser;
                self.updatedate = updatedate;
            }

            function SetlistViewModel() {
                var tblSetlist = $("#tblSetlist");
                var ddlColumns = $("#ddlColumns");

                var self = this;

                self.setlists = ko.observableArray([]);
                self.ownerSearchList = ko.observableArray([STRING_ALL_SETLISTS, STRING_MY_SETLISTS]);
                self.selectedSetlist = ko.observable();
                self.nameSearch = ko.observable("");
                self.ownerSearch = ko.observable(config.selectedOwnerSearch.length > 0
                    ? config.selectedOwnerSearch
                    : STRING_MY_SETLISTS);
                self.numSetsList = ko.observable([]);
                self.selectedNumSets = ko.observable(3);
                self.selectedNumSongs = ko.observable(10);
                self.editFormHeader = ko.observable("");
                self.totalSetlists = ko.observable(0);

                createSetlistArray(lists.SetlistList);

                function createSetlistArray(list) {
                    self.setlists.removeAll();
                    $(list).each(function(index, value) {
                        pushSetlist(value);
                    });
                };

                ddlColumns.on("hidden.bs.dropdown", function () {
                    self.saveColumns();
                });

                self.columns = ko.computed(function() {
                    var arr = [];
                    $(lists.TableColumnList).each(function(index, value) {
                        arr.push({ id: value.Id, title: value.Header, sortKey: value.Data, dataMember: value.Data, isVisible: ko.observable(value.IsVisible), alwaysVisible: value.AlwaysVisible, isMemberColumn: value.IsMemberColumn });
                    });
                    return arr;
                });

                self.ownerSearchList = ko.computed(function () {
                    var arr = [];
                    arr.push(STRING_ALL_SETLISTS);
                    arr.push(STRING_MY_SETLISTS);
                    return arr;
                });

                function pushSetlist(value) {
                    self.setlists.push(new Setlist(value.Id, value.Name, value.NumSets, value.NumSongs, value.Owner, value.UserUpdate, value.DateUpdate));
                };

                self.sort = function(header) {
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
                            self.setlists.sort(function (a, b) {
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

                self.filteredSetlists = ko.computed(function() {
                    return ko.utils.arrayFilter(self.setlists(), function(sl) {
                        return (
                            (self.nameSearch().length === 0 || sl.name.toLowerCase().indexOf(self.nameSearch().toLowerCase()) !== -1)
                            && (self.ownerSearch() === STRING_ALL_SETLISTS || sl.owner.toLowerCase() === config.currentUser.toLowerCase())
                        );
                    });
                });

                self.setlistsTable = ko.computed(function () {
                    var filteredSetlists = self.filteredSetlists();
                    self.totalSetlists(filteredSetlists.length);

                    return filteredSetlists;
                });

                self.showEditDialog = function(row) {
                    var id = row.id;

                    if (id > 0) {
                        self.highlightRow(row);
                    }

                    self.showSetlistEditDialog(row);
                };

                self.deleteSelectedSetlist = function(row) {
                    deleteSetlist(row.id);
                };

                self.showDeleteDialog = function(row) {
                    self.highlightRow(row);
                    self.showSetlistDeleteDialog(row);
                };

                self.showSetlistDeleteDialog = function(row) {
                    var id = (typeof row.id !== "undefined" ? row.id: 0);
                    if (id <= 0) return;
                    var sl = self.getSetlist(id);

                    dialog.custom.showModal({
                        title: "<span class='glyphicon glyphicon-remove'></span> Delete Setlist?",
                        message: "<p>This will permanently delete the Setlist <i>" + sl.name + "</i>.</p>",
                        callback : function () {
                            return self.deleteSetlist(row.id);
                        },
                        width: 430
                    });
                };

                self.showSetlistEditDialog = function (row) {
                    var id = (typeof row.id !== "undefined" ? row.id : 0);
                    var title = "<span class='glyphicon glyphicon-{0}'></span>";
                    var message;

                    if (id > 0) {
                        title = title.replace("{0}", "pencil") + " Edit Setlist";
                        var setlist = self.getSetlist(id);
                        self.selectedSetlist(setlist);
                    } else {
                        title = title.replace("{0}", "cog") + " Generate Setlist";
                        self.selectedSetlist([]);
                    }

                    $.ajax({
                        type: "GET",
                        async: false,
                        url: site.url + "Setlists/GetSetlistEditView/" + id,
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
                            return self.saveSetlist(id > 0);
                        },
                        width: 300
                    });
                };

                self.getSetlist = function(setlistid) {
                    var setlist = null;

                    ko.utils.arrayForEach(self.setlists(), function(item) {
                        if (item.id == setlistid) {
                            setlist = item;
                        }
                    });

                    return setlist;
                };

                self.highlightRow = function(row) {
                    if (row == null) return;

                    var rows = tblSetlist.find(" tr:gt(0)");
                    rows.each(function() {
                        $(this).css("background-color", "#ffffff");
                    });

                    var r = tblSetlist.find("#row_" + row.id);
                    r.css("background-color", HIGHLIGHT_ROW_COLOUR);
                    tblSetlist.attr("tr:hover", HIGHLIGHT_ROW_COLOUR);
                };

                self.ownerSearch.subscribe(function() {
                    $.ajax({
                        type: "POST",
                        url: site.url + "Setlists/StoreSelectedOwnerSearch/",
                        data: { ownerSearch: self.ownerSearch() },
                        dataType: "json",
                        traditional: true
                    });
                });

                self.getSetlistDetailFromDialog = function(edit) {
                    var name = $.trim($("#txtName").val());
                    var numsets = parseInt($("#ddlNumSets").val());
                    var numsongs = parseInt($("#ddlNumSongs").val());
                    var id;

                    if(edit) id = self.selectedSetlist().id;
                    else id = 0;

                    return { Id: id, Name: name, NumSets: numsets, NumSongs: numsongs
                    };
                };

                self.getColumns = function () {
                    var arr =[];
                    $(self.columns()).each(function (index, value) {
                        if (!value.alwaysVisible) {
                            arr.push({
                                Id: value.id,
                                IsVisible: value.isVisible(),
                                IsMemberColumn: false
                            });
                        }
                    });
                    return arr;
                };

                self.showSets = function(set) {
                    var id = set.id;
                    window.location.href = site.url + "Setlists/" +id + "/Sets";
                }

                //---------------------------------------------- CONTROLLER (BEGIN) -------------------------------------------------------

                self.saveSetlist = function(edit) {
                    var setlistdetail = self.getSetlistDetailFromDialog(edit);
                    var jsonData = JSON.stringify(setlistdetail);
                    var url;
                    var result;

                    $("body").css("cursor", "wait");

                    if (!edit)
                        url = site.url + "Setlists/Generate/";
                    else
                        url = site.url + "Setlists/Save/";

                    $.ajax({
                        type: "POST",
                        async: false,
                        url: url,
                        data: { setListDetail: jsonData },
                        dataType: "json",
                        traditional: true,
                        failure: function() {
                            $("body").css("cursor", "default");
                            $("#validation-container").html("");
                        },
                        success: function(data) {
                            if (data.Success) {
                                lists.SetlistList = data.SetlistList;
                                createSetlistArray(lists.SetlistList);
                                self.selectedSetlist(self.getSetlist(data.SelectedId));
                                self.sort({ afterSave: true });
                                self.highlightRow(self.selectedSetlist());
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

                self.deleteSetlist = function(id) {
                    $("body").css("cursor", "wait");

                    $.ajax({
                        type: "POST",
                        url: site.url + "Setlists/Delete/",
                        data: { id: id },
                        dataType: "json",
                        traditional: true,
                        failure: function() {
                            $("body").css("cursor", "default");
                        },
                        success: function(data) {
                            if (data.Success) {
                                lists.SetlistList = data.SetlistList;
                                createSetlistArray(lists.SetlistList);
                                self.sort({ afterSave: true });
                            }
                            $("body").css("cursor", "default");
                        }
                    });

                    return true;
                };

                self.saveColumns = function() {
                    var jsonData = JSON.stringify(self.getColumns());
                    // after changes
                    var selfColumns = self.getColumns();
                    // before changes
                    var tableColumns = lists.TableColumnList.filter(function(value) { return !value.AlwaysVisible; });
                    var isDifference = false;

                    $(selfColumns).each(function (index, value) {
                        if (tableColumns[index].IsVisible !== value.IsVisible) {
                            isDifference = true;
                        }
                    });

                    if (isDifference) {
                        $.ajax({
                            type: "POST",
                            url: site.url + "Setlists/SaveColumns/",
                            data: { columns: jsonData },
                            dataType: "json",
                            traditional: true
                        });
                    }
                };

                //---------------------------------------------- CONTROLLER (END) -------------------------------------------------------
            };

            //---------------------------------------------- VIEW MODEL (END) -----------------------------------------------------

            ko.utils.stringStartsWith = function(string, startsWith) {
                string = string || "";
                if (startsWith.length > string.length) return false;
                return string.substring(0, startsWith.length) === startsWith;
            };
        }
    }
})(jQuery);