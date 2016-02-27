/*!
 * Genres/Index.js
 * Author: John Charlton
 * Date: 2015-05
 */
; (function ($) {

    var HIGHLIGHT_ROW_COLOUR = "#e3e8ff";

    genres.index = {
        init: function () {
            var _sortDescending = false;
            var _currentSortKey = "name";

            var lists = {
                GenreList: [],
                TableColumnList: []
            };

            loadConfig();

            ko.applyBindings(new GenreViewModel());

            function loadConfig() {
                $.ajax({
                    type: "GET",
                    url: site.url + "Genres/GetData/",
                    dataType: "json",
                    traditional: true,
                    async: false,
                    success: function (data) {
                        $.extend(lists, data);
                    }
                });
            }

            function Genre(id, name, abbreviation, issonggenre) {
                var self = this;

                self.id = id;
                self.name = name;
                self.abbreviation = abbreviation;
                self.issonggenre = issonggenre;
            }

            function GenreViewModel() {
                var tblGenre = $("#tblGenre");
                var ddlColumns = $("#ddlColumns");

                var self = this;

                self.genres = ko.observableArray([]);
                self.selectedGenre = ko.observable();
                self.nameSearch = ko.observable("");
                self.editFormHeader = ko.observable("");
                self.totalGenres = ko.observable(0);

                createGenreArray(lists.GenreList);

                function createGenreArray(list) {
                    self.genres.removeAll();
                    $(list).each(function (index, value) {
                        pushGenre(value);
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

                function pushGenre(value) {
                    self.genres.push(new Genre(value.Id, value.Name, value.Abbreviation, value.IsSongGenre));
                };

                self.selectedGenre(self.genres()[0]);

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
                            self.genres.sort(function (a, b) {
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

                self.filteredGenres = ko.computed(function () {
                    return ko.utils.arrayFilter(self.genres(), function (g) {
                        return (
                            (self.nameSearch().length === 0 || g.name.toLowerCase().indexOf(self.nameSearch().toLowerCase()) !== -1)
                        );
                    });
                });

                self.genresTable = ko.computed(function () {
                    var filteredGenres = self.filteredGenres();
                    self.totalGenres(filteredGenres.length);

                    return filteredGenres;
                });

                self.showEditDialog = function (row) {
                    var id = row.id;

                    if (id > 0) {
                        self.highlightRow(row);
                    }

                    self.showGenreEditDialog(row);
                };

                self.deleteSelectedGenre = function (row) {
                    deleteGenre(row.id);
                };

                self.showDeleteDialog = function (row) {
                    self.highlightRow(row);
                    self.showGenreDeleteDialog(row);
                };

                self.showGenreDeleteDialog = function (row) {
                    var id = (typeof row.id !== "undefined" ? row.id : 0);
                    if (id <= 0) return;
                    var i = self.getGenre(id);

                    dialog.custom.showModal({
                        title: "<span class='glyphicon glyphicon-remove'></span> Delete Genre?",
                        message: "<p>This will permanently delete the genre <i><b>" + i.name + "</b></i>.</p>",
                        callback: function () {
                            return self.deleteGenre(row.id);
                        },
                        width: 400
                    });
                };

                self.showGenreEditDialog = function (row) {
                    var id = (typeof row.id !== "undefined" ? row.id : 0);
                    var title = "<span class='glyphicon glyphicon-pencil'></span>";
                    var message;

                    if (id > 0) {
                        title = title + " Edit Genre";
                        var genre = self.getGenre(id);
                        self.selectedGenre(genre);
                    } else {
                        title = title + " Add Genre";
                        self.selectedGenre([]);
                    }

                    $.ajax({
                        type: "GET",
                        async: false,
                        url: site.url + "Genres/GetGenreEditView/" + id,
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
                            return self.saveGenre();
                        },
                        width: 400
                    });
                };

                self.getGenre = function (genreid) {
                    var genre = null;

                    ko.utils.arrayForEach(self.genres(), function (item) {
                        if (item.id === genreid) {
                            genre = item;
                        }
                    });

                    return genre;
                };

                self.highlightRow = function (row) {
                    if (row == null) return;

                    var rows = tblGenre.find("tr:gt(0)");
                    rows.each(function () {
                        $(this).css("background-color", "#ffffff");
                    });

                    var r = tblGenre.find("#row_" + row.id);
                    r.css("background-color", HIGHLIGHT_ROW_COLOUR);
                    $("#tblGenre").attr("tr:hover", HIGHLIGHT_ROW_COLOUR);
                };

                self.getGenreDetailFromDialog = function () {
                    var name = $.trim($("#txtName").val());
                    var abbreviation = $.trim($("#txtAbbreviation").val());

                    return {
                        Id: self.selectedGenre().id,
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

                self.saveGenre = function () {
                    var genredetail = self.getGenreDetailFromDialog();
                    var jsonData = JSON.stringify(genredetail);
                    var result;

                    $("body").css("cursor", "wait");

                    $.ajax({
                        type: "POST",
                        async: false,
                        url: site.url + "Genres/Save/",
                        data: { genre: jsonData },
                        dataType: "json",
                        traditional: true,
                        failure: function () {
                            $("body").css("cursor", "default");
                            $("#validation-container").html("");
                        },
                        success: function (data) {
                            if (data.Success) {
                                lists.GenreList = data.GenreList;
                                createGenreArray(lists.GenreList);
                                self.selectedGenre(self.getGenre(data.SelectedId));
                                self.sort({ afterSave: true });
                                self.highlightRow(self.selectedGenre());
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

                self.deleteGenre = function (id) {
                    $("body").css("cursor", "wait");

                    $.ajax({
                        type: "POST",
                        url: site.url + "Genres/Delete/",
                        data: { id: id },
                        dataType: "json",
                        traditional: true,
                        failure: function () {
                            $("body").css("cursor", "default");
                        },
                        success: function (data) {
                            if (data.Success) {
                                lists.GenreList = data.GenreList;
                                createGenreArray(lists.GenreList);
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
                            url: site.url + "Genres/SaveColumns/",
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