var contentPicker = function (startId, title,f) {
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