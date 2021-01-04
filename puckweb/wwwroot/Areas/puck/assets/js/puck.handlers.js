//bindings
$(document).ajaxError(function (event, jqxhr, settings, thrownError) {
    //console.log("event:",event,"jqxhr:",jqxhr,"settings:",settings,"thrownError:",thrownError);
    if (jqxhr.status == 401) {
        msg(false, "You are no longer logged in. <a style=\"background:#fff;\" target=\"_target\" href=\"/puck/admin/in\">Login</a> again to continue working", true, undefined, 60000);
        $(".submitLoader").remove();
        $(".content_btns").removeAttr("disabled");
        cleft.find(".loader:visible").each(function () {
            var el = $(this);
            var expand = el.parent().find(".expand").removeClass("fa-chevron-down").addClass("fa-chevron-right").show();
            el.hide();
        });
    }
});
$(window).off("keyup.overlay").on("keyup.overlay", function (e) {
    if (e.keyCode == 27) {
        var removed = false;
        while (overlays.length && removed == false){
            var overlay = overlays.shift();
            if (!overlay.hasClass("removed")) {
                removed = true;
                overlay.addClass("removed");
                overlay.remove();
            }
        }
    }
});
//tabs
$(document).on("click", ".editor-field .nav-tabs li a", function (e) {
    e.preventDefault();
});
$(document).on("click", ".puck-dropdown a", function (e) {
    //e.preventDefault();
});
$(document).on("click", ".menutop a.content", function (e) {
    var el = $(this);
    el.find(".badge").remove();
    if (cright.find(".workflow-container").length > 0) {
        showWorkflowItems();
        if (latestWorkflowNotificationId)
            workflowNotificationId = latestWorkflowNotificationId;
    }
});
//handle tabs without needing to set hrefs and ids
$(document).off("click.tabs").on("click.tabs", ".editor-field .nav-tabs li", function () {
    var el = $(this);
    var tabsContainer = el.parent().parent();
    tabsContainer.find(".nav-tabs li a").removeClass("active");
    el.find("a").addClass("active");
    var index = el.index() + 1;
    tabsContainer.find(".tab-content>div").removeClass("active");
    tabsContainer.find(".tab-content>div:nth-child(" + index + ")").addClass("active");
    //console.log("index",el.index());
});

//$('a.settings').click(function (e) {
//    e.preventDefault();
//    if (!canChangeMainContent())
//        return false;
//    getSettings(function (data) {
//        cright.html(data);
//        afterDom();
//        //setup validation
//        wireForm(cright.find('form'), function (data) {
//            msg(true, "settings updated.");
//            window.scrollTo(0,0);
//            getVariants(function (data) {
//                languages = data;
//            });
//        }, function (data) {
//            msg(false, data.message);
//        });
//        setChangeTracker();
//    });
//});
$("html").on("click", ".left_settings li button", function (e) {
    //if (!canChangeMainContent())
    //    return false;
    //var el = $(this).parent();
    //var path = el.attr("data-path");
    //showSettings(path);
});
//root new content button
$(".create_default").show().click(function () { newContent(emptyGuid); location.hash = "#"; });
//task list
//$(".menutop .tasks").click(function (e) { e.preventDefault(); showTasks(); });
//users
//$(".menutop .users").click(function (e) { e.preventDefault(); showUsers(); });
//select state
$(".menutop li").click(function () {
    $(".menutop li").removeClass("selected");
    $(this).addClass("selected");
});
//republish entire site button
$(".republish_entire_site").click(function () { republishEntireSite(); });

//template tree expand
$(document).on("click", "ul.content.templates li.node i.expand", function () {
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
        getDrawTemplates(node.attr("data-path"), node);
        node.find("i.expand:first").removeClass("fa-chevron-right").addClass("fa-chevron-down");
    }
});
//content tree expand
$(document).on("click", "ul.content:not(.templates) li.node i.expand", function () {
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
        getDrawContent(node.attr("data-id"), node, true, function () {
            node.find(".loader").hide();
            node.find("i.expand:first").show();
        },true);
        node.find(".loader").show();
        node.find("i.expand:first").removeClass("fa-chevron-right").addClass("fa-chevron-down").hide();
    }
});
//node settings dropdown
$(document).on("click", "ul.content li.node i.menu", function (e) {
    //display dropdown
    var node = $(this).parents(".node:first");
    var left = node.position().left;
    var top = node.position().top+15;
    var dropdown = $("." + node.parents("ul.content:first").attr("data-dropdown"));
    dropdown
        .addClass("open")
        .css({ top: top + "px", left: left + "px" })
        .attr("data-context", node.attr("data-id"));
    e.stopPropagation();
    $("html").on("click", "", function () {
        dropdown.removeClass("open");
        $("html").off();
    });
    //filter menu items according to context -- ie COULD this option be shown in the current context
    //filter template drop down stuff
    var type = node.attr("data-type");
    if (type == "folder") {
        dropdown.find("a[data-action]").show();
        if (node.attr("data-path") == '/') {
            dropdown.find("a[data-action='template_delete']").hide();
            dropdown.find("a[data-action='template_move']").hide();
        }   
    } else if (type == "file") {
        dropdown.find("a[data-action]").show();
        dropdown.find("a[data-action='template_create']").hide();
        dropdown.find("a[data-action='template_new_folder']").hide();
    }
    //filter notify
    dropdown.find("a[data-action='notify']").parents("li").show();
    //filter translation item
    var totranslate = untranslated(node.attr("data-variants"));
    if (totranslate)
        dropdown.find("a[data-action='translate']").parents("li").show();
    else
        dropdown.find("a[data-action='translate']").parents("li").hide();
    //filter publish/unpublish
    if (publishedVariants(node.attr("data-id")) != false)
        dropdown.find("a[data-action='unpublish']").parents("li").show();
    else
        dropdown.find("a[data-action='unpublish']").parents("li").hide();

    if (unpublishedVariants(node.attr("data-id")) != false)
        dropdown.find("a[data-action='publish']").parents("li").show();
    else
        dropdown.find("a[data-action='publish']").parents("li").hide();

    if (publishedVariants(node.attr("data-id")) != false)
        dropdown.find("a[data-action='republish']").parents("li").show();
    else
        dropdown.find("a[data-action='republish']").parents("li").hide();


    //filter domain
    if (isRootItem(node.attr("data-parent_id"))) {
        dropdown.find("a[data-action='domain']").parents("li").show();
    } else {
        dropdown.find("a[data-action='domain']").parents("li").hide();
    }
    //filter move - disallow root move
    //if (isRootItem(node.attr("data-parent_id")))
    //    dropdown.find("a[data-action='move']").parents("li").hide();
    //else
    //    dropdown.find("a[data-action='move']").parents("li").show();
    //filter copy - disallow root copy
    if (isRootItem(node.attr("data-parent_id")))
        dropdown.find("a[data-action='copy']").parents("li").hide();
    else
        dropdown.find("a[data-action='copy']").parents("li").show();
    //filter sort - disallow when 0 children
    if (node.attr("data-has_children")=="false")
        dropdown.find("a[data-action='sort']").parents("li").hide();
    else
        dropdown.find("a[data-action='sort']").parents("li").show();

    //filter menu items according to permissions -- ie can user access option
    dropdown.find("a[data-action]").each(function () {
        var permission = $(this).attr("data-permission");
        if (!userRoles.contains(permission)) $(this).parents("li").hide();
    });
});

//menu items
$(document).on("click",".node-dropdown a,.template-dropdown a",function () {
    var el = $(this);
    var action = el.attr("data-action");
    var context = el.parents(".puck-dropdown").attr("data-context");
    var node = $(".node[data-id='" + context + "']");
    switch (action) {
        case "template_create":
            var path = node.attr("data-path");
            newTemplate(path);
            break;
        case "template_new_folder":
            var path = node.attr("data-path");
            newTemplateFolder(path);
            break;
        case "template_delete":
            var path = node.attr("data-path");
            var type = node.attr("data-type");
            if (type == "folder")
                deleteTemplateFolder(node.attr("data-name"), path);
            else 
                deleteTemplate(node.attr("data-name"),path);            
            break;
        case "template_move":
            var markup = $(".interfaces .template_tree_container.move").clone();
            var el = markup.find(".node:first");
            var ovarlayEl=overlay(markup,undefined,undefined,undefined,"Move Template");
            overlayEl.find(".msg").html("select new parent node for content <b>" + node.attr("data-name") + "</b>");
            getDrawTemplates(startPath, el);
            markup.on("click", ".node[data-type='folder']>div>span", function (e) {
                var dest_node = $(this).parents(".node:first");
                var from = node.attr("data-path");
                var to = dest_node.attr("data-path");
                if (!confirm("move " + from + " to " + to + " ?")) {
                    return;
                }
                var afterMove = function (d) {
                    if (d.success) {
                        $("ul.templates .node[data-path='" + from + "']").remove();
                        var tonode = $("ul.templates .node[data-path='" + to + "']");
                        //console.log({ el: tonode });
                        tonode.find(".expand:first").removeClass("fa-chevron-right").addClass("fa-chevron-down").css({ visibility: "visible" });
                        getDrawTemplates(to);
                    } else {
                        msg(false, d.message);
                    }
                    overlayClose();
                }
                if(node.attr("data-type")=="file")
                    setMoveTemplate(from, to, afterMove);
                else
                    setMoveTemplateFolder(from, to, afterMove);
            });
            break;
        case "delete":
            var doDelete = function (id, variant) {
                msg(0, "deleting");
                setDelete(id, function (data) {
                    if (data.success === true) {
                        msg(true, "content deleted");
                        if (variant == "" || variant == undefined) {
                            node.remove();
                        } else {
                            if (node.find("span.variant").length > 1)
                                node.find("span.variant[data-variant='" + variant + "']").remove();
                            //else
                            //node.remove();
                        }
                        getDrawContent(node.attr("data-parent_id"), undefined, undefined, function () {
                            highlightSelectedNode(node.attr("data-id"));
                        },true);
                        overlayClose();
                    } else {
                        msg(false, data.message);
                        overlayClose();
                    }
                }, variant);
            }
            var variants = node.attr("data-variants").split(",");
            if (variants.length > 1) {
                var dialog = dialogForVariants(variants);
                overlay(dialog, 400, 150,undefined,"Delete");
                dialog.find(".descendantscontainer").hide();
                dialog.find("button").click(function () {
                    if (confirm("are you sure you want to delete this content?")) { doDelete(node.attr("data-id"), dialog.find("select").val()); }
                });
            } else {
                if (confirm("are you sure you want to delete this content?")) { doDelete(node.attr("data-id")); }
            }
            break;
        case "republish":
            var doRePublish = function (id, variants, descendants, overlayEl) {
                setRePublish(id, variants, descendants, function (data) {
                    if (data.success === true) {
                        if (overlayEl) {
                            overlayEl.find("button").removeAttr("disabled");
                            overlayEl.find(".submitLoader").remove();
                        }
                        msg(true, "content re-published");
                        overlayClose();
                    } else {
                        if (overlayEl) {
                            overlayEl.find("button").removeAttr("disabled");
                            overlayEl.find(".submitLoader").remove();
                        }
                        msg(false, data.message);
                        overlayClose();
                    }
                });
            }
            var variants = allVariants(node.attr("data-id"));
            if (variants.length > 1 || true) {
                var dialog = dialogForVariants(variants,true);
                dialog.find(".descendantscontainer label").html("Re-publish descendants?");
                var overlayEl = overlay(dialog, 400, 250, undefined, "Re-Publish");
                overlayEl.find(".descendantscontainer").after("<div class=\"p-1 pt-3\">including descendants can take some time depending on the number of pages involved. You will not be able to publish any content you're editing while this is happening</div>");
                dialog.find("button").click(function () {
                    var button = $(this);
                    var selectedVariants = (dialog.find("select[name=variant]").val() || []).join(',');
                    if (!selectedVariants) {
                        msg(undefined, "cannot re-publish without selecting at least one variant");
                        return;
                    }
                    var descendantVariants = (dialog.find("select[name=descendants]").val() || []).join(',');
                    if (descendantVariants) {
                        dialog.find("select[name=descendants] option[value='']").removeAttr("selected");
                        descendantVariants = (dialog.find("select[name=descendants]").val() || []).join(',');
                    }
                    //console.log(descendantVariants);
                    button.after(spinningLoaderImg("submitLoader").css({ "float": "right", "margin-top": "10px" }));
                    button.attr("disabled", "disabled");
                    doRePublish(node.attr("data-id"), selectedVariants, descendantVariants, overlayEl);
                });
            } else {
                doRePublish(node.attr("data-id"), variants[0]);
            }
            break;
        case "publish":
            var doPublish = function (id, variants, descendants,overlayEl) {
                setPublish(id, variants, descendants, function (data) {
                    if (data.success === true) {
                        if (overlayEl) {
                            overlayEl.find("button").removeAttr("disabled");
                            overlayEl.find(".submitLoader").remove();
                        }
                        msg(true,"content published");
                        getDrawContent(node.attr("data-parent_id"), undefined, true, undefined, true);
                        var variantsArr = variants.split(",");
                        for (var i = 0; i < variantsArr.length; i++) {
                            var variant = variantsArr[i];
                            node.find(">.inner .variant[data-variant='" + variant + "']").addClass("published");
                        }
                        overlayClose();
                    } else {
                        if (overlayEl) {
                            overlayEl.find("button").removeAttr("disabled");
                            overlayEl.find(".submitLoader").remove();
                        }
                        msg(false, data.message);
                        overlayClose();
                    }
                });
            }
            var variants = unpublishedVariants(node.attr("data-id"));
            if (variants.length > 1||true ) {
                var dialog = dialogForVariants(variants,true);
                dialog.find(".descendantscontainer label").html("Publish descendants?");
                var overlayEl = overlay(dialog, 400, 250, undefined, "Publish");
                overlayEl.find(".descendantscontainer").after("<div class=\"p-1 pt-3\">including descendants can take some time depending on the number of pages involved. You will not be able to publish any content you're editing while this is happening</div>");
                dialog.find("button").click(function () {
                    var button = $(this);
                    var selectedVariants = (dialog.find("select[name=variant]").val() || []).join(',');
                    if (!selectedVariants) {
                        msg(undefined,"cannot publish without selecting at least one variant");
                        return;
                    }
                    var descendantVariants = (dialog.find("select[name=descendants]").val() || []).join(',');
                    if (descendantVariants) {
                        dialog.find("select[name=descendants] option[value='']").removeAttr("selected");
                        descendantVariants = (dialog.find("select[name=descendants]").val() || []).join(',');
                    }
                    //console.log(descendantVariants);
                    button.after(spinningLoaderImg("submitLoader").css({"float":"right", "margin-top":"10px"}));
                    button.attr("disabled","disabled");
                    doPublish(node.attr("data-id"), selectedVariants, descendantVariants, overlayEl);
                });
            } else {
                doPublish(node.attr("data-id"), variants[0]);
            }
            break;
        case "unpublish":
            var doUnpublish = function (id, variants, descendants,overlayEl) {
                setUnpublish(id, variants, descendants, function (data) {
                    if (data.success === true) {
                        if (overlayEl) {
                            overlayEl.find("button").removeAttr("disabled");
                            overlayEl.find(".submitLoader").remove();
                        }
                        msg(true, "content unpublished");
                        getDrawContent(node.attr("data-parent_id"), undefined, true,undefined,true);
                        var variantsArr = variants.split(",");
                        for (var i = 0; i < variantsArr.length; i++) {
                            var variant = variantsArr[i];
                            node.find(">.inner .variant[data-variant='" + variant + "']").removeClass("published");
                            publishedContent[id][variant] = undefined;
                        }
                        overlayClose();
                    } else {
                        if (overlayEl) {
                            overlayEl.find("button").removeAttr("disabled");
                            overlayEl.find(".submitLoader").remove();
                        }
                        msg(false, data.message);
                        overlayClose();
                    }
                });
            }
            var variants = publishedVariants(node.attr("data-id"));
            if (variants.length > 1 ) {
                var dialog = dialogForVariants(variants,true);
                var dCon = dialog.find(".descendantscontainer");
                dCon.find("label").html("Unpublish descendants");
                dCon.find("label").after("<p/>");
                var overlayEl = overlay(dialog, 400, 250, undefined, "Unpublish");
                
                dialog.find("button").click(function () {
                    var button = $(this);
                    var selectedVariants = (dialog.find("select[name='variant']").val() || []).join(',');
                    if (!selectedVariants) {
                        msg(undefined, "cannot unpublish without selecting at least one variant");
                        return;
                    }
                    var allSelected = dialog.find("select[name='variant'] option").length == dialog.find("select[name='variant'] option:selected").length;
                    //var descendantVariants = selectedVariants;
                    var descendantVariants = "";
                    if (allSelected) {
                        $(languages).each(function (i) {
                            descendantVariants += this.Key;
                            if (i < languages.length - 1)
                                descendantVariants += ",";
                        });
                    } else {
                        var selectedDescendants = (dialog.find("select[name='descendants']").val() || []).join(',');
                        if (selectedDescendants)
                            descendantVariants += /*"," +*/ selectedDescendants;
                        descendantVariants = descendantVariants.replace(",,", ",");
                    }
                    //console.log("variant:", variant, "descendantVariants:", descendantVariants);
                    button.attr("disabled", "disabled");

                    button.after(spinningLoaderImg("submitLoader").css({"float":"right","margin-top":"10px"}));

                    doUnpublish(node.attr("data-id"), selectedVariants, descendantVariants, overlayEl);
                });
                var updateVariant = function () {
                    var selectedVariants = dialog.find("select[name='variant']").val() || [];
                    if (selectedVariants.length == 0) {
                        dCon.find("select").show();
                        dCon.find("p").html("Select any language variants to unpublish for descendant content");
                        //dCon.hide();
                        //dCon.find("p").html('');
                    } else {
                        //dCon.show();
                        var friendlyNames = [];
                        for (var i = 0; i < selectedVariants.length; i++) {
                            friendlyNames.push(variantNames[selectedVariants[i]]);
                        }
                        var hasUnselectedDescendants = languages.length != selectedVariants.length;
                        //if (hasUnselectedDescendants) {
                        //    dCon.find("select").show();
                        //    dCon.find("p").html("Descendant content with language(s) - " + friendlyNames.join(", ") + " - will be unpublished, select any additional languages to unpublish for descendant content");
                        //} else {
                        //    dCon.find("select").hide();
                        //    dCon.find("p").html("Descendant content with language(s) - " + friendlyNames.join(", ") + " - will be unpublished");
                        //}

                        var allSelected = dialog.find("select[name='variant'] option").length == dialog.find("select[name='variant'] option:selected").length;
                        if (allSelected) {
                            dCon.find("select").hide();
                            dCon.find("p").html("All descendant content will be unpublished");
                        } else {
                            dCon.find("select").show();
                            dCon.find("p").html("Select any language variants to unpublish for descendant content");
                        }

                    }
                    //dCon.find("option").removeAttr("disabled");
                    for (var i = 0; i < selectedVariants.length; i++) {
                        var v = selectedVariants[i];
                        //dCon.find("option[value='" + v + "']").attr("disabled", "disabled").removeAttr("selected");
                    }
                    //dCon.find("p").html("Select any additional languages to unpublish for descendant content");
                }
                dialog.find("select[name='variant']").change(function () {
                    updateVariant();
                });
                updateVariant();
            } else {
                var descendantVariants = "";
                $(languages).each(function (i) {
                    descendantVariants += this.Key;
                    if (i < languages.length - 1)
                        descendantVariants += ",";
                });
                doUnpublish(node.attr("data-id"), variants[0], descendantVariants);
            }
            break;
        case "revert":
            revisionsFor(node.attr("data-variants"), node.attr("data-id"));
            break;
        case "cache":
            showCacheInfo(node.attr("data-path"));
            break;
        case "create":
            newContent(node.attr("data-id"), node.attr("data-type"));
            break;
        case "move":
            var markup = $(".interfaces .tree_container.move").clone();
            var el = markup.find(".node:first");

            el.attr({ "data-path": "/", "data-nodename": "root", "data-id": emptyGuid }).append(
                '<div class="inner"><span class="nodename">- root -&nbsp;</span></div>'
            );

            var overlayEl = overlay(markup, undefined, undefined, undefined, "Move Content");
            overlayEl.find(".msg").html("select new parent node for content <b>" + node.attr("data-nodename") + "</b>");
            getDrawContent(startId, el);
            var moving = false;
            markup.on("click", ".node span", function (e) {
                if (moving) return;
                var dest_node = $(this).parents(".node:first");
                var from = node.attr("data-path");
                var to = dest_node.attr("data-path");
                var fromId = node.attr("data-id");
                var toId = dest_node.attr("data-id");
                if (!confirm("move " + from + " to " + to + " ?")) {
                    return;
                }

                moving = true;
                var img = spinningLoaderImg("submitLoader")
                    .css({ position: "absolute", top: "17px", right: "60px" });
                    
                overlayEl.find(".overlay_close").before(img);

                setMove(fromId, toId, function (d) {
                    if (d.success) {
                        moving = false;
                        img.remove();
                        cleft.find(".node[data-id='" + fromId + "']").remove();
                        var tonode = cleft.find(".node[data-id='" + toId + "']");
                        //console.log({ el: tonode });
                        tonode.find(".expand:first").removeClass("fa-chevron-right").addClass("fa-chevron-down").css({ visibility: "visible" });
                        getDrawContent(toId, undefined, true, function () { },true);
                    } else {
                        moving = false;
                        img.remove();
                        msg(false, d.message);
                    }
                    overlayClose();
                });
            });
            break;
        case "copy":
            var markup = $(".interfaces .tree_container.copy").clone();
            var el = markup.find(".node:first");

            el.attr({ "data-path": "/", "data-nodename": "root", "data-id": emptyGuid }).append(
                '<div class="inner"><span class="nodename">- root -&nbsp;</span></div>'
            );

            var overlayEl = overlay(markup, undefined, undefined, undefined, "Copy Content");
            overlayEl.find(".msg").html("select new parent node for copied content <b>" + node.attr("data-nodename") + "</b>");
            getDrawContent(startId, el);
            var copying = false;
            markup.on("click", ".node span", function (e) {
                if (copying) return;
                var dest_node = $(this).parents(".node:first");
                var from = node.attr("data-path");
                var to = dest_node.attr("data-path");
                var fromId = node.attr("data-id");
                var toId = dest_node.attr("data-id");
                var nodeTitle = node.find("span:first").text();
                var includeDescendants = markup.find("input").is(":checked");
                //console.log("includeDescendants",includeDescendants);
                if (!confirm("copy " + nodeTitle + " to " + to + " ?")) {
                    return;
                }
                copying = true;
                var img = spinningLoaderImg("submitLoader")
                    .css({ position: "absolute", top: "17px", right: "60px" });
                    
                overlayEl.find(".overlay_close").before(img);
                setCopy(fromId, toId, includeDescendants, function (d) {
                    if (d.success) {
                        copying = false;
                        img.remove();
                        var tonode = cleft.find(".node[data-id='" + toId + "']");
                        //console.log({ el: tonode });
                        if (tonode.length == 0) return;
                        tonode.find(".expand:first").removeClass("fa-chevron-right").addClass("fa-chevron-down").css({ visibility: "visible" });
                        getDrawContent(toId, undefined, true, function () { }, true);
                        msg(true,"content copied");
                    } else {
                        copying = false;
                        img.remove();
                        msg(false, d.message);
                    }
                    overlayClose();
                });
            });
            break;
        case "sort":
            var markup = $(".interfaces .tree_container.sort").clone();
            var el = markup.find(".node:first");
            var overlayEl = overlay(markup, 400, undefined, undefined, "Sort Content");
            overlayEl.find(".msg").html("drag to sort children of content <b>" + node.attr("data-nodename") + "</b>");
            getDrawContent(node.attr("data-id"), el, false, function () {
                el.find("ul:first").sortable({ axis: "y" });
                el.find("li").each(function () {
                    initTouch(this);
                });
            });
            overlayEl.find("button").click(function (e) {
                var btn = $(this);
                btn.attr("disabled", "disabled");
                overlayEl.find("img").removeClass("d-none");
                var parent = el;
                var sortParentId = node.attr("data-id");
                var items = [];
                parent.find("li.node").each(function () {
                    items.push($(this).attr("data-id"));
                });
                sortNodes(sortParentId, items, function (res) {
                    if (res.success) {
                        msg(true,"sort complete")
                        getDrawContent(node.attr("data-id"),undefined,true,undefined,true);
                        overlayClose();
                    } else {
                        msg(false, res.message);
                        btn.removeAttr("disabled");
                        overlayEl.find("img").addClass("d-none");
                    }
                });
            });
            break;
        case "changetype":
            getChangeTypeDialog(node.attr("data-id"),function (markup) {
                var variants = node.attr("data-variants").split(",");
                var overlayEl = overlay(markup, 400, 250, undefined, "Change Type");
                overlayEl.find("button").click(function () {
                    var newType = overlayEl.find("select").val();
                    getChangeTypeMappingDialog(node.attr("data-id"), newType, function (mappingMarkup) {
                        overlayClose();
                        var overlayEl = overlay(mappingMarkup, 500, 250, undefined, "Change Type");
                        wireForm(overlayEl.find("form"), function (d) {
                            msg(true, "type changed");
                            displayMarkup(null, node.attr("data-type"), variants[0], undefined, node.attr("data-id"));
                            overlayClose();
                        }, function (d) {
                            msg(false, d.message);
                            overlayClose();
                        });
                    });
                });
                
            });
            break;
        case "timedpublish":
            timedPublish(node.attr("data-variants"),node.attr("data-id"));
            break;
        case "audit":
            showAudit(node.attr("data-id"),"","",1,20,cright);
            break;
        case "sync":
            sync(node.attr("data-id"));
            break;
        case "translate":
            getCreateDialog(function (data) {
                var overlayEl=overlay(data, 400, 250,undefined,"Translate");
                var type = overlayEl.find("select[name=type]");
                var variant = overlayEl.find("select[name=variant]");
                var fromVariant = variant.clone().attr("name", "fromVariant");
                fromVariant.append($("<option value=\"none\">None - blank form</option>"));
                overlayEl.find(".typecontainer label").html("Copy values from existing version").siblings().hide().after(fromVariant);
                overlayEl.find(".variantcontainer label").html("Language of new content");
                type.val(node.attr("data-type"));
                var variants = node.attr("data-variants").split(",");
                if (variants.length == 1) {
                    //$(".overlay_screen .typecontainer").hide();
                    //$(".overlay_screen").css({ height: "170px" });
                }
                fromVariant.find("option").each(function () {
                    var option = $(this);
                    var contains = false;
                    $(variants).each(function () {
                        if (this == option.val())
                            contains = true;
                    });
                    if (!contains && option.val()!="none") {
                        option.remove();
                    }
                });
                variant.find("option").each(function () {
                    //check value doesn't exist in variant list
                    var option = $(this);
                    var contains = false;
                    $(variants).each(function () {
                        if (this == option.val())
                            contains = true;
                    });
                    if (contains)
                        option.remove();
                });
                overlayEl.find("button").click(function () {
                    displayMarkup(null, node.attr("data-type"), variant.val(), fromVariant.val(),node.attr("data-id"));
                    overlayClose();
                });
            }, node.attr("data-type"));
            break;
        case "localisation":
            setLocalisation(node.attr("data-path"));
            break;
        case "domain":
            setDomainMapping(node.attr("data-path"));
            break;
        case "notify":
            setNotify(node.attr("data-path"));
            break;
    }
});
cleft.on("click", ".node .icon_search", function () {
    var el = $(this).parents(".node:first");
    searchRoot = el.attr("data-path");
    searchDialog(el.attr("data-path"));
});
cleft.find("ul.content").on("click", "li.node span.nodename", function () {
    //get markup
    if (!canChangeMainContent())
        return false;
    var node = $(this).parents(".node:first");
    if (node.data("disabled"))
        return;
    var path = node.attr("data-path");
    var rootPath = path.indexOf("/", 1) > -1 ? path.substr(0, path.indexOf("/", 1)) : path;
    var variants = node.attr("data-variants").split(",");
    variants.sort(function (a, b) {
        var aOrder = getVariantOrder(a, path);
        var bOrder = getVariantOrder(b, path);
        return aOrder - bOrder;
    });
    var firstVariant = variants[0];
    location.hash = "#content?id=" + node.attr("data-id") + "&variant=" + firstVariant;
    //displayMarkup(null, node.attr("data-type"), firstVariant, undefined, node.attr("data-id"));
});
$("button.search").click(function () {
    searchDialog("");
});

//set heights
var setAreaHeights = function () {
    var _h = $(window).height() - ($(".menutop").outerHeight() + 15) - ($(window).width()<768?$(".top .message").outerHeight():0);
    $(".leftarea").css({ height: _h, overflowY: "scroll" });
    $(".rightarea").css({ height: _h, overflowY: "scroll" });
    $(".leftToggle i").css({ top: (Math.round(_h / 2)) });
}
setAreaHeights();

var toggleMobileUI = function () {
    if ($(window).width() < 768) {
        $(".leftToggle").show();
        cleft.css({ position: "absolute" });
        $(".main.grid").on("click.mobileUi", ".rightarea", function (e) {
            if ($.contains($(".search_ops").get(0), e.target)) return;
            if ($.contains($(".overlay_screen").get(0), e.target)) return;
            cleft.hide();
        });
        $(".leftToggle").off().click(function (e) {
            e.stopPropagation();
            cleft.show();
        });
        $("body").addClass("mobile-ui");
    } else {
        cleft.show();
        $(".leftToggle").hide();
        cleft.css({ position: "relative" });
        $(".main.grid").off("click.mobileUi");
        $("body").removeClass("mobile-ui");
    }
}

$(window).resize(function () { setAreaHeights(); toggleMobileUI(); });

toggleMobileUI();
//extensions
$.validator.methods.date = function (value, element) {
    {
        if (value == '' || Globalize.parseDate(value, "dd/MM/yyyy HH:mm:ss") != null) {
            {
                return true;
            }
        }
        return false;
    }
}
String.prototype.isEmpty = function () {
    return this.replace(/\s/g, "").length == 0;
}
var isNullEmpty = function (s) {
    if (s == null || s == undefined) return true;
    return s.replace(/\s/g, "").length == 0;
}
var isInt = function (s) {
    return /^\d+$/.test(s);
}
var isFunction = function (functionToCheck) {
    var getType = {};
    return functionToCheck && getType.toString.call(functionToCheck) === '[object Function]';
}
Array.prototype.contains = function (v) {
    var contains = false;
    for (var i = 0; i < this.length; i++)
        if (this[i] == v)
            contains = true;
    return contains;
}
var location_hash = "";
var checkHash = function () {
    if (window.location.hash != location_hash) {
        $(document).trigger("puck.hash_change", {oldHash:location_hash,newHash:window.location.hash});
        location_hash = window.location.hash;        
    }
    setTimeout(checkHash, 500);
}
//checkHash(true);
$(window).on("hashchange", function (e) {
    handleHash(location.hash);
    //msg(false, "old hash " + obj.oldHash + "|| new hash " + obj.newHash + " " + Math.random());
});
$(document).on("puck.hash_change", function (e,obj) {
    handleHash(obj.newHash);
    //msg(false, "old hash " + obj.oldHash + "|| new hash " + obj.newHash + " " + Math.random());
});
var getHashValues = function (hash) {
    var h = hash.substring(hash.indexOf("?")+1);
    var kvp = h.split("&");
    var dict = [];
    for (var i = 0; i < kvp.length; i++) {
        var k = kvp[i].split("=")[0];
        var v = kvp[i].split("=")[1];
        dict[k] = v;
    }
    return dict;
}
var highlightSection = function (href,id) {
    $(".menutop li").removeClass("selected");
    var anchor;
    if (href)
        anchor = $(".menutop a[href^='" + href + "']");
    else if (id)
        anchor = $(".menutop a#" + id);
    else return;
    var el = anchor.parent();
    el.addClass("selected");
}
var showCustomSection = function (id) {
    var hash = location.hash;
    highlightSection(undefined,id);
    $(".left_item").hide();
    cleft.find(".left_item."+id).show();
    var dict = getHashValues(hash);
    cleft.find(".left_item."+id+" a").removeClass("current");
    cleft.find(".left_item."+id+" a[href='" + hash + "']").addClass("current");
    return dict;
}
var handleHash = function (hash) {
    if (/^#\/?content/.test(hash)) {
        highlightSection("#content");
        $(".left_item").hide();
        cleft.find(".left_content").show();
        var dict = getHashValues(hash);
        if (dict["id"] == undefined || dict["variant"] == undefined) {
            cleft.show();
            showWorkflowItems();
            return;
        }
        displayMarkup(null, null, dict["variant"], undefined, dict["id"]);
    } else if (/^#\/?settings/.test(hash)) {
        highlightSection("#settings");
        //if (!canChangeMainContent())
        //    return false;
        $(".left_item").hide();
        cleft.find(".left_settings").show();
        var dict = getHashValues(hash);
        var path = dict["path"];
        cleft.find(".left_settings a").removeClass("current");
        cleft.find(".left_settings a[href='" + hash + "']").addClass("current");
        showSettings(path);
        $(".menutop .settings").click();
    } else if (/^#\/?users/.test(hash)) {
        highlightSection("#users");
        $(".left_item").hide();
        cleft.find(".left_users").show();
        showUsers();
    } else if (/^#\/?developer/.test(hash)) {
        highlightSection("#developer");
        $(".left_item").hide();
        cleft.find(".left_developer").show();

        if (cleft.find(".left_developer ul.machines").length == 0) {
            logHelper.showMachines();
        }

        var dict = getHashValues(hash);
        var page = dict["page"];
        cleft.find(".left_developer a").removeClass("current");
        cleft.find(".left_developer a[href='" + hash + "']").addClass("current");

        if (page == "tasks")
            showTasks();
        else if (page == "views")
            showViews();
        else if (page == "logs") {
            var machine = dict["machine"]||"";
            var name = dict["name"]||"";
            logHelper.showLog(machine,name);
        }
    } else {
        if (window.puckCustomHashHandler) {
            try {
                puckCustomHashHandler(hash);
            } catch (ex) {
                console.error(ex);
            }
        }
    }
}
var loadCustomSections = function () {
    if (window.puckCustomSections && puckCustomSections.constructor === Array) {
        var customSectionStr = "";
        for (var i = 0; i < puckCustomSections.length; i++) {
            var customSection = puckCustomSections[i];
            customSectionStr += '<li><a title="' + customSection.title + '" class="' + customSection.id + '" id="' + customSection.id + '" href="' + customSection.hash + '">'
                + '<i class="' + customSection.iconClasses + '"></i></a></li >'
            if (customSection.leftItems && customSection.leftItems.constructor === Array) {
                var leftSection = $("<div/>").addClass("left_item").addClass(customSection.id).css("display","none").appendTo(".leftarea");
                leftSection.append('<ul data-dropdown="node-dropdown" class="'+customSection.id+' p-0"></ul>');
                for (var j = 0; j < customSection.leftItems.length; j++) {
                    var leftItem = customSection.leftItems[j];
                    var nodeStr = '<li class="node"><i class="' + leftItem.iconClasses + '"></i><a href="' + leftItem.hash + '" class="">' + leftItem.title + '</a></li>';
                    leftSection.find("ul").append(nodeStr);
                }
            }
        }
        $(".menutop ul li.last").after(customSectionStr);
    }
}
function getQueryString(name, url) {
    if (!url) url = window.location.href;
    name = name.replace(/[\[\]]/g, '\\$&');
    var regex = new RegExp('[?&]' + name + '(=([^&#]*)|&|#|$)'),
        results = regex.exec(url);
    if (!results) return null;
    if (!results[2]) return '';
    return decodeURIComponent(results[2].replace(/\+/g, ' '));
}
$(window).load(function () {
    setAreaHeights();
    loadCustomSections();
    showWorkflowNotifications();
    //TODO: maybe add a column resizer
    //cleft.parent().append(
    //    $("<div/>").html("<i style=\"font-size:18px;\" class=\"fas fa-arrows-alt-h\"/>").addClass("colsResizer").css({ position: "absolute", left: (cleft.width()-0+13) + "px", top: "10px" })
    //);
    var afterVariants = function (data) {
        languages = data;
        for (var i = 0; i < languages.length; i++) {
            languageSortDictionary[languages[i].Key] = i + 1;
        }
        var hash = getQueryString("hash");
        if (languages.length == 0) {
            onAfterDom(function () {
                msg(0, "take a moment to setup puck. at the very least, choose your languages!");
            });
            location.hash = "settings?path=/puck/settings/languages";
        } else if (!hash&&!location.hash) {
            location.hash = "#content";
        }
    };

    var hash = getQueryString("hash");
    //console.log("hashQs",hash);
    
    getVariants(afterVariants);

    if (!hash) return;

    setTimeout(function () {
        location.hash = hash;
        if (hash[0] != "#")
            hash = "#" + hash;
        history.replaceState('','',"/puck"+hash);
    }, 500);

});

function touchHandler(event) {
    var touch = event.changedTouches[0];

    var simulatedEvent = document.createEvent("MouseEvent");
    simulatedEvent.initMouseEvent({
        touchstart: "mousedown",
        touchmove: "mousemove",
        touchend: "mouseup"
    }[event.type], true, true, window, 1,
        touch.screenX, touch.screenY,
        touch.clientX, touch.clientY, false,
        false, false, false, 0, null);

    touch.target.dispatchEvent(simulatedEvent);
    event.preventDefault();
}
function initTouch(el) {
    el.addEventListener("touchstart", touchHandler, true);
    el.addEventListener("touchmove", touchHandler, true);
    el.addEventListener("touchend", touchHandler, true);
    el.addEventListener("touchcancel", touchHandler, true);
}

getUserLanguage(function (d) { defaultLanguage = d; });
getUserRoles(function (d) {
    userRoles = d; hideTopNav();
    $(document).ready(function () {
        handleHash(location.hash);
        //var index = location.href.indexOf("?");
        //var qs = location.href.substring(index);
        //if (index != -1 && qs != "") {
        //    //console.log("qs %o",qs);
        //    var rp = /\?action=([a-zA-Z0-1]+)&/;
        //    var action = rp.exec(qs)[1];
        //    var hash = "#" + action + "?" + qs.replace(rp, "");
        //    //console.log("hash %o",hash);
        //    handleHash(hash);
        //}
        if (!userRoles.contains("_republish")) $(".republish_entire_site").hide();
    });
});

var isArray = function (arg) {
    return Object.prototype.toString.call(arg) === '[object Array]';
};
var isFunction = function (arg) {
    return arg && {}.toString.call(arg) === '[object Function]';
};
var isObject = function (arg) {
    return typeof arg === 'object' && arg !== null && arg!==undefined;
}
initTree(true);