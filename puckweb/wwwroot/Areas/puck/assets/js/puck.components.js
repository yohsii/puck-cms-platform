var imagePicker = function (startId, title, f) {

    var showFolderContent = function (id, name,overlayEl, f) {
        var cont = overlayEl;
        var itemContainer = overlayEl.find(".itemContainer");
        if (cont.length == 0) return;
        itemContainer.html("");
        if (name != undefined) {
            var pathEl = $("<span data-id='" + id + "'/>").html(name + (name == "/" ? "" : "/"));
            cont.find(".path").append(pathEl);
        }
        getMinimumContentByParentId(id, function (res) {
            //console.log(res);
            for (guid in res.published) {
                var itemGroup = res.published[guid];
                var vCount = 0;
                for (variant in itemGroup) {
                    var item = itemGroup[variant];
                    if (vCount > 0 && item.Type != "ImageVM")
                        break;
                    draw(item, res.children, itemContainer);
                    vCount++;
                }
            }
            var items = itemContainer.find(".item");
            items.sort(function (a, b) {
                return parseInt($(a).attr("data-sortorder")) - parseInt($(b).attr("data-sortorder"));
            });
            itemContainer.append(items);
            if (f) f();
        }, true);
    }

    var draw = function (model, children, itemContainer) {
        var el =
            $('<div class="item col-4">'
                +'<div class="actionContainer">'
                    +'<i class="fas fa-pen view"></i>'
                    +'<i class="fas fa-trash remove"></i>'
                +'</div >'
                +'<div class="iconContainer">'
                    +'<i class="fas fa-file-alt"></i>'
                +'</div>'
                +'<div class="imageContainer">'
                    +'<img src="" />'
                +'</div>'
                +'<div class="metaContainer">'
                    +'<span class="nodename"></span>'
                    +'<span class="separator">-</span> <span class="variant"></span>'
                +'</div>'
            +'</div>');

        el.attr({ "data-id": model.Id, "data-variant": model.Variant, "data-nodename": model.NodeName, "data-sortorder": model.SortOrder });
        el.find(".variant").html(model.Variant);
        el.find(".nodename").html(model.NodeName);
        if (model.Type == "ImageVM") {
            el.find(".iconContainer").hide();
            el.find("img").attr({alt:model.Image.Description, src: model.Image.Path }).css({ width: "100%" });
        } else {
            el.find(".imageContainer").hide();
            el.find(".variant").hide();
            el.find(".separator").hide();
        }
        if (children.includes(model.Id)) {
            el.find(".metaContainer").addClass("folder");
            el.find(".iconContainer i").removeClass("fa-file-alt").addClass("fa-folder");
        }

        itemContainer.append(el);
        //console.log(model);
    }

    var el =
        $('<div class="imagePickerOverlayContainer">'
            + '<div class="searchContainer">'
                + '<input value="" type="text" class="searchBox" />'
                + '<button type="button" class="btn btn-light">search</button>'
            + '</div>'
            + '<div class="path"></div>'
            + '<div class="itemContainer row"></div>'
        + '</div>');
    overlayEl = overlay(el, 400, undefined, undefined, "Image Picker", true);
    showFolderContent(startId, "/", overlayEl, function () {
        if (overlayEl.find(".item").length == 0) {
            overlayEl.find("div.path").after($("<p/>").css({ "margin-top": "20px" }).html("there is no content to select."));
        }
    });
    overlayEl.off("click.imgPickerMeta").on("click.imgPickerMeta", ".metaContainer.folder", function (e) {
        var el = $(this);
        var item = el.parents(".item");
        showFolderContent(item.attr("data-id"), item.attr("data-nodename"),overlayEl);
    });
    overlayEl.off("click.imgPickerPath").on("click.imgPickerPath", ".imagePickerOverlayContainer .path span", function (e) {
        var el = $(this);
        el.nextAll("span").remove();
        var id = el.attr("data-id");
        showFolderContent(id, undefined,overlayEl);
    });
    overlayEl.off("click.searchButton").on("click.searchButton", ".imagePickerOverlayContainer .searchContainer button", function (e) {
        var input = overlayEl.find(".searchContainer input");
        var searchTerm = input.val();
        getSearch(searchTerm, function (res) {
            var cont = overlayEl;
            var itemContainer = overlayEl.find(".itemContainer");
            itemContainer.html("");
            //console.log(res);
            var pathEl = $("<span class='searchTerm'/>").html("search:\"" + searchTerm + "\" ");
            var clear = $("<button class='btn btn-link'>clear</button>").click(function () {
                cont.find(".path").html("");
                showFolderContent(startId, "/", overlayEl);
            }).appendTo(pathEl);
            cont.find(".path").html(pathEl);
            if (res.length == 0) {
                itemContainer.html($("<div class='col-12 zeroResults'/>").html("0 results"));
            } else {
                for (var i = 0; i < res.length; i++) {
                    draw(res[i], [], itemContainer);
                }
            }
        }, "ImageVM", "");
    });
    overlayEl.on("click", ".imageContainer img", function (e) {
        var el = $(this).parents(".item");
        if (f)
            f(el,el.attr("data-id"),el.attr("data-variant"),overlayEl);
    });
    
}
var contentPicker = function (startId, title, f) {
    startId = startId || '00000000-0000-0000-0000-000000000000';
    title = title || "Content Picker";
    var markup = '<ul class="contentTree nomenu">'
        + '<li class="node" data-children_path="/"></li>'
        + '</ul>';

    var tree = $(markup);
    overlayEl = overlay(tree, 400, undefined, undefined, title, true);
    overlayEl.addClass("contentPicker");
    el = overlayEl.find(".node:first");
    getDrawContent(startId, el, false, function () {
        if (overlayEl.find(".node").length == 1) {
            overlayEl.find("ul.contentTree").before($("<p/>").html("there is no content to select."));
        }
    }, false);
    overlayEl.on("click", ".node span", function (e) {
        var clicked = $(this);
        var node = clicked.parents(".node:first");
        var isVariantSelection = clicked.hasClass("variant");

        var variant;
        if (!isVariantSelection) {
            //changed to only allow variant selection
            variant = node.find(".variant:first").attr("data-variant");
            isVariantSelection = true;
        }
        else
            variant = clicked.attr("data-variant");
        id = node.attr("data-id"); 
        if(f)
            f(node, id, variant,overlayEl);
    });
    overlayEl.on("click", "ul.contentTree li.node i.expand", function () {
        //get children content
        var node = $(this).parents(".node:first");
        var descendants = node.find("ul:first");
        if (descendants.length > 0) {//show
            if (descendants.first().is(":hidden")) {
                node.find("i.expand:first").removeClass("fa-chevron-right").addClass("fa-chevron-down");
                descendants.show();
            } else {//hide
                node.find("i.expand:first").removeClass("fa-chevron-down").addClass("fa-chevron-right");
                descendants.hide();
            }
        } else {
            getDrawContent(node.attr("data-id"), node, false, function () {
                node.find(".loader").hide();
                node.find("i.expand:first").show();
            }, false);
            node.find(".loader").show();
            node.find("i.expand:first").removeClass("fa-chevron-right").addClass("fa-chevron-down").hide();
        }
    });
}