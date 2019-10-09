//bindings
$(document).ajaxError(function (event, jqxhr, settings, thrownError) {
    //console.log("event:",event,"jqxhr:",jqxhr,"settings:",settings,"thrownError:",thrownError);
    if (jqxhr.status == 401) {
        msg(false, "You are no longer logged in. <a style=\"background:#fff;\" target=\"_target\" href=\"/puck/admin/in\">Login</a> again to continue working",true,undefined,60000);
    }
});
//tabs
$(document).on("click", ".editor-field .nav-tabs li a", function (e) {
    e.preventDefault();
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
$(".create_default").show().click(function () { newContent(emptyGuid); });
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
    //filter domain
    if (isRootItem(node.attr("data-parent_id"))) {
        dropdown.find("a[data-action='domain']").parents("li").show();
    } else {
        dropdown.find("a[data-action='domain']").parents("li").hide();
    }
    //filter move - disallow root move
    if (isRootItem(node.attr("data-parent_id")))
        dropdown.find("a[data-action='move']").parents("li").hide();
    else
        dropdown.find("a[data-action='move']").parents("li").show();
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
                        console.log({ el: tonode });
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
                setDelete(id, function (data) {
                    if (data.success === true) {
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
        case "publish":
            var doPublish = function (id, variant, descendants) {
                setPublish(id, variant, descendants, function (data) {
                    if (data.success === true) {
                        msg(true,"content published");
                        getDrawContent(node.attr("data-parent_id"), undefined, true,undefined,true);
                        node.find(">.inner>.variant[data-variant='"+variant+"']").addClass("published");
                        overlayClose();
                    } else {
                        msg(false, data.message);
                        overlayClose();
                    }
                });
            }
            var variants = unpublishedVariants(node.attr("data-id"));
            if (variants.length > 1||true ) {
                var dialog = dialogForVariants(variants);
                dialog.find(".descendantscontainer label").html("Publish descendants?");
                overlay(dialog, 400, 250,undefined,"Publish");
                dialog.find("button").click(function () {
                    var descendantVariants = (dialog.find("select[name=descendants]").val() || []).join(',');
                    if (descendantVariants) {
                        dialog.find("select[name=descendants] option[value='']").removeAttr("selected");
                        descendantVariants = (dialog.find("select[name=descendants]").val() || []).join(',');
                    }
                    //console.log(descendantVariants);
                    doPublish(node.attr("data-id"), dialog.find("select[name=variant]").val(), descendantVariants);
                });
            } else {
                doPublish(node.attr("data-id"), variants[0]);
            }
            break;
        case "unpublish":
            var doUnpublish = function (id, variant, descendants) {
                setUnpublish(id, variant, descendants, function (data) {
                    if (data.success === true) {
                        msg(true, "content unpublished");
                        getDrawContent(node.attr("data-parent_id"), undefined, true,undefined,true);
                        node.find(">.inner>.variant[data-variant='" + variant + "']").removeClass("published");
                        publishedContent[id][variant] = undefined;
                        overlayClose();
                    } else {
                        msg(false, data.message);
                        overlayClose();
                    }
                });
            }
            var variants = publishedVariants(node.attr("data-id"));
            if (variants.length > 1 ) {
                var dialog = dialogForVariants(variants);
                var dCon = dialog.find(".descendantscontainer");
                dCon.find("label").html("Unpublish descendants?");
                dCon.find("label").after("<p/>");
                overlay(dialog, 400, 250, undefined, "Unpublish");
                dialog.find("button").click(function () {
                    var variant = dialog.find("select[name='variant']").val();
                    var descendantVariants = variant;
                    var selectedDescendants = (dialog.find("select[name='descendants']").val() || []).join(',');
                    if (selectedDescendants)
                        descendantVariants += "," + selectedDescendants;
                    descendantVariants = descendantVariants.replace(",,",",");
                    //console.log("variant:", variant, "descendantVariants:", descendantVariants);
                    doUnpublish(node.attr("data-id"), variant, descendantVariants);
                });
                var updateVariant = function () {
                    var variant = dialog.find("select[name='variant']").val();
                    dCon.find("p").html("Descendant content with language " + variantNames[variant] +" will be unpublished, select any additional languages to unpublish for descendant content");
                    dCon.find("option").removeAttr("disabled");
                    dCon.find("option[value='" + variant + "']").attr("disabled", "disabled").removeAttr("selected");
                }
                dialog.find("select[name='variant']").change(function () {
                    updateVariant();
                });
                updateVariant();
            } else {
                doUnpublish(node.attr("data-id"), variants[0]);
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
            var overlayEl = overlay(markup,undefined,undefined,undefined,"Move Content");
            overlayEl.find(".msg").html("select new parent node for content <b>" + node.attr("data-nodename") + "</b>");
            getDrawContent(startId, el);
            markup.on("click", ".node span", function (e) {
                var dest_node = $(this).parents(".node:first");
                var from = node.attr("data-path");
                var to = dest_node.attr("data-path");
                var fromId = node.attr("data-id");
                var toId = dest_node.attr("data-id");
                if (!confirm("move " + from + " to " + to + " ?")) {
                    return;
                }
                setMove(fromId, toId, function (d) {
                    if (d.success) {
                        cleft.find(".node[data-id='" + fromId + "']").remove();
                        var tonode = cleft.find(".node[data-id='" + toId + "']");
                        console.log({ el: tonode });
                        tonode.find(".expand:first").removeClass("fa-chevron-right").addClass("fa-chevron-down").css({ visibility: "visible" });
                        getDrawContent(toId);
                    } else {
                        msg(false, d.message);
                    }
                    overlayClose();
                });
            });
            break;
        case "copy":
            var markup = $(".interfaces .tree_container.copy").clone();
            var el = markup.find(".node:first");
            var overlayEl = overlay(markup, undefined, undefined, undefined, "Copy Content");
            overlayEl.find(".msg").html("select new parent node for copied content <b>" + node.attr("data-nodename") + "</b>");
            getDrawContent(startId, el);
            markup.on("click", ".node span", function (e) {
                var dest_node = $(this).parents(".node:first");
                var from = node.attr("data-path");
                var to = dest_node.attr("data-path");
                var fromId = node.attr("data-id");
                var toId = dest_node.attr("data-id");
                var nodeTitle = node.find("span:first").text();
                var includeDescendants = markup.find("input").is(":checked");
                console.log("includeDescendants",includeDescendants);
                if (!confirm("copy " + nodeTitle + " to " + to + " ?")) {
                    return;
                }
                setCopy(fromId, toId, includeDescendants, function (d) {
                    if (d.success) {
                        var tonode = cleft.find(".node[data-id='" + toId + "']");
                        console.log({ el: tonode });
                        if (tonode.length == 0) return;
                        tonode.find(".expand:first").removeClass("fa-chevron-right").addClass("fa-chevron-down").css({ visibility: "visible" });
                        getDrawContent(toId);
                    } else {
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
                el.find("ul:first").sortable({axis:"y"});
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
                        getDrawContent(node.attr("data-id"));
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
                        var overlayEl=overlay(mappingMarkup, 500, 250, undefined, "Change Type");
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
    } else {
        cleft.show();
        $(".leftToggle").hide();
        cleft.css({ position: "relative" });
        $(".main.grid").off("click.mobileUi");

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
        anchor = $(".menutop a[href='" + href + "']");
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
            cright.html("");
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
        showTasks();
    } else {
        if (window.puckCustomHashHandler)
            puckCustomHashHandler(hash);
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
    loadCustomSections();
    var hash = getQueryString("hash");
    //console.log("hashQs",hash);
    if (!hash) return;
    setTimeout(function () {
        location.hash = hash;
    }, 500);
    
});