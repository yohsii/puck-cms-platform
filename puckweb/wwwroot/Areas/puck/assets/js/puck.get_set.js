var getCropSizes = function (f) {
    $.get("/puck/api/GetCropSizes", f);
}
var getCurrentModel = function (id, variant, f) {
    $.get("/puck/api/GetCurrentModel?id="+id+(variant?"&variant="+variant:""), f);
}
var getReferencedContent = function (id, variant, f) {
    $.post("/puck/api/GetReferencedContent?id=" + id+"&variant="+variant, f);
}
var cancelSync = function (key, f) {
    $.post("/puck/api/CancelSync?key=" + key, f);
}
var getCacheItem = function (key, f) {
    $.get("/puck/api/GetCacheItem?key=" + key, f);
}
var getSyncDialog = function (id, f) {
    $.get("/puck/api/SyncDialog?id="+id, f,"html");
}
var getRedirects = function (f) {
    $.get("/puck/api/redirects", f);
}
var addRedirect = function (from, to, type, f) {
    var postStr = "from="+from+"&to="+to+"&type="+type;
    $.ajax({
        url: "/puck/api/addRedirect",
        data: postStr,
        traditional: true,
        success: f,
        type: "POST",
        datatype: "json"
    });
}
var deleteRedirect = function (from,f) {
    var postStr = "from=" + from;
    $.ajax({
        url: "/puck/api/deleteRedirect",
        data: postStr,
        traditional: true,
        success: f,
        type: "POST",
        datatype: "json"
    });
}
var getLogMachines = function (f) {
    $.get("/puck/log/machines", f);
}
var getLogs = function (machine,f) {
    var url = "/puck/log/logs?";
    if (machine)
        url += "machine=" + machine;
    $.get(url, f);
}
var getLog = function (machine,name,f) {
    var url = "/puck/log/log?";
    if (machine)
        url += "machine=" + machine;
    if (name)
        url += "&name=" + name;
    $.get(url, f);
}
var setHasChildren = function (id, f) {
    $.get("/puck/api/setHasChildren?id=" + id, f);
};
var getContentModels = function (ids, f) {
    $.get("/puck/api/getModels?ids=" + ids, f);
};
var getRootsLocalisations = function (ids, f) {
    $.get("/puck/api/rootsLocalisations?ids=" + ids, f);
};
var getAllLocalisations = function (f) {
    $.get("/puck/api/allLocalisations", f);
};
var getModels = function (path, f) {
    $.get("/puck/api/models?p_path=" + path, f);
};
var sortNodes = function (id, items, f) {
    var items_str = "";
    $(items).each(function (i) {
        items_str += "items=" + this + "&";
    });
    items_str = items_str.substring(0, items_str.length - 1);
    $.ajax({
        url: "/puck/api/sort?parentId=" + id,
        data: items_str,
        traditional: true,
        success: f,
        type: "POST",
        datatype: "json"
    });
}
var getSearchTypes = function (root,f) {
    $.get("/puck/api/searchtypes?root=" + root, function (d) {
        f(d);
    });
}
var getMarkup = function (parentId, type, variant, f, fromVariant, contentId) {
    $.get("/puck/api/edit?" + (parentId == null ? "" : "parentId=" + parentId + "&")
        + (contentId == null ? "" : "contentId=" + contentId + "&")
        + "&p_variant=" + variant + "&p_type=" + (type==null?"":type) + /*"&p_path=" + path +*/ "&p_fromVariant=" + (fromVariant == undefined ? "" : fromVariant), f, "html");
}
var getPrepopulatedMarkup = function (type,id,f) {
    $.get("/puck/api/prepopulatedEdit?p_type="+(type==null?"":type)+"&id="+id, f, "html");
}
var getAuditMarkup = function (id, variant, username, page, pageSize, f) {
    $.get("/puck/api/auditMarkup?id=" + id + "&variant=" + (variant || "") + "&username=" + (username || "") + "&page=" + page + "&pageSize=" + pageSize, f, "html");
}
var getTimedPublishDialog = function (id,variant, f) {
    $.get("/puck/api/timedpublishdialog?id=" + id+"&variant="+variant, f, "html");
}
var getCreateDialog = function (f, t) {
    $.get("/puck/api/createdialog" + (t === undefined ? "" : "?type=" + t), f, "html");
}
var getChangeTypeDialog = function (id,f) {
    $.get("/puck/api/changetypedialog?id="+id,f,"html");
}
var getChangeTypeMappingDialog = function (id,newType, f) {
    $.get("/puck/api/changetypemappingdialog?id=" + id+"&newType="+newType, f, "html");
}
var getTemplateCreateDialog = function (f, p) {
    $.get("/puck/task/createtemplate" + (p === undefined ? "" : "?path=" + p), f, "html");
}
var getTemplateFolderCreateDialog = function (f, p) {
    $.get("/puck/task/createfolder" + (p === undefined ? "" : "?path=" + p), f, "html");
}
var getSettings = function (path, f) {
    if (path == undefined) path = "/puck/settings/edit";
    $.get(path, f, "html");
}
var getVariants = function (f) {
    $.get("/puck/api/variants", f);
}
var getVariantsForPath = function (path,f) {
    $.get("/puck/api/variantsfornode/?path="+path, f);
}
var getVariantsForId = function (id, f) {
    $.get("/puck/api/variantsfornodebyid/?id=" + id, f);
}
var setDeleteTemplate = function (p, f) {
    $.post("/puck/task/deletetemplate?path=" + p, f);
}
var setDeleteTemplateFolder = function (p, f) {
    $.post("/puck/task/deletetemplatefolder?path=" + p, f);
}
var setRePublish = function (id, variants, descendants, f) {
    var path = "/puck/api/republish?id=" + id;
    if (variants)
        path += "&variants=" + variants;
    if (descendants)
        path += "&descendants=" + descendants;
    $.post(path, f);
}
var setPublish = function (id, variants, descendants, f) {
    var path = "/puck/api/publish?id=" + id;
    if (variants)
        path += "&variants=" + variants;
    if (descendants)
        path += "&descendants=" + descendants;
    $.post(path, f);
}
var setUnpublish = function (id, variants, descendants, f) {
    var path = "/puck/api/unpublish?id=" + id;
    if (variants) {
        path += "&variants=" + variants;
        //if (!descendants)
        //    descendants = variant;
    }
    if (descendants)
        path += "&descendants=" + descendants;
    $.post(path, f);
}
var setDelete = function (id, f, variant) {
    var path = "/puck/api/delete?id=" + id;
    if (variant != undefined)
        path += "&variant=" + variant;
    $.get(path, f);
}
var setMoveTemplate = function (from, to, f) {
    var path = "/puck/task/movetemplate?from=" + from + "&to=" + to;
    $.post(path, f);
}
var setMoveTemplateFolder = function (from, to, f) {
    var path = "/puck/task/movetemplatefolder?from=" + from + "&to=" + to;
    $.post(path, f);
}
var setMove = function (from, to, f) {
    var path = "/puck/api/move?startId=" + from + "&destinationId=" + to;
    $.get(path, f);
}
var setCopy = function (id, parentId, includeDescendants, f) {
    var path = "/puck/api/copy?id=" + id + "&parentId=" + parentId + "&includeDescendants=" + includeDescendants;
    $.get(path, f);
}
var setDeleteRevision = function (id, f) {
    var path = "/puck/api/deleterevision?id=" + id;
    $.get(path, f);
}
var setRevert = function (id, f) {
    var path = "/puck/api/revert?id=" + id;
    $.get(path, f);
}
var getFieldGroups = function (t, f) {
    $.get("/puck/api/fieldgroups?type=" + t, f);
}
var getNotifyDialog = function (p, f) {
    $.get("/puck/api/NotifyDialog?p_path=" + p, f);
}
var getLocalisationDialog = function (p, f) {
    $.get("/puck/api/LocalisationDialog?p_path=" + p, f);
}
var getDomainMappingDialog = function (p, f) {
    $.get("/puck/api/DomainMappingDialog?p_path=" + p, f);
}
var getTasks = function (f) {
    $.get("/puck/task/index", f);
}
var getViews = function (f) {
    $.get("/puck/task/views", f);
}
var getTaskCreateDialog = function (f) {
    $.get("/puck/task/CreateTaskDialog", f);
}
var getTaskMarkup = function (f, type, id) {
    var type = isNullEmpty(type) ? "" : ("type=" + type);
    var id = isNullEmpty(id) ? "" : "id=" + id;
    if (!isNullEmpty(type))
        type += "&"
    $.get("/puck/task/Edit?" + type + id, f);
}
var getUsersJson = function (f) {
    $.get("/puck/admin/users", f);
}
var getUsers = function (f) {
    $.get("/puck/admin/index", f, "html");
}
var getUserMarkup = function (u, f) {
    $.get("/puck/admin/edit?username=" + u, f, "html");
}
var getUserGroupMarkup = function (g, f) {
    $.get("/puck/admin/editusergroup?group="+g, f, "html");
}
var setDeleteUser = function (u, f) {
    $.get("/puck/admin/delete?username=" + u, f);
}
var getRevisions = function (id, variant, f) {
    $.get("/puck/api/revisions?id=" + id + "&variant=" + variant, f, "html");
}
var getCompareMarkup = function (id, f) {
    $.get("/puck/api/compare?id=" + id, f, "html");
}
var getCacheInfo = function (path, f) {
    $.get("/puck/api/cacheinfo?p_path=" + path, f);
}
var getUserRoles = function (f) {
    $.get("/puck/api/userroles", f);
}
var getUserLanguage = function (f) {
    $.get("/puck/api/userlanguage", f);
}
var setCacheInfo = function (path, value, f) {
    $.post("/puck/api/cacheinfo?p_path=" + path + "&value=" + value, f);
}
var deleteParameters = function (key, f) {
    $.get("/puck/settings/DeleteParameters?key=" + key, function (data) {
        if (data.success) {
            f();
        } else {
            msg(false, data.message);
        }
    });
}
var getEditorParametersMarkup = function (f, settingsType, modelType, propertyName) {
    $.get("/puck/settings/EditParameters?settingsType=" + settingsType + "&modelType=" + modelType + "&propertyName=" + propertyName, f);
}
var getContent = function (path, f) {
    $.get("/puck/api/content?path=" + path, f);
}
var getContentByParentId = function (parentId, f, cast) {
    if (cast == undefined) cast = true;
    $.get("/puck/api/contentbyparentid?cast="+cast+"&parentid=" + parentId, f);
}
var getMinimumContentByParentId = function (parentId, f, fullIndexContent, filterIndexContent) {
    if (filterIndexContent == undefined)
        filterIndexContent = false;
    fullIndexContent = fullIndexContent || false;
    $.get("/puck/api/minimumcontentbyparentid?parentid=" + parentId + "&fullIndexContent=" + fullIndexContent + "&filterIndexContent=" + filterIndexContent, f);
}
var getTemplates = function (path, f) {
    $.get("/puck/task/templates?path=" + path, f);
}
var getPath = function (id, f) {
    $.get("/puck/api/getpath?id=" + id, f);
};
var getIdPath = function (id, f) {
    $.get("/puck/api/getidpath?id=" + id, f);
};
var getStartPaths = function (f) {
    $.get("/puck/api/startpaths", f);
};
var getStartIds = function (f) {
    $.get("/puck/api/startids", f);
};
var getSearchView = function (term, f, type, root) {
    $.get("/puck/api/searchview?q=" + term + "&type=" + type + "&root=" + root, f, "html");
}
var getSearch = function (term, f, type, root) {
    $.get("/puck/api/search?q=" + term + "&type=" + type + "&root=" + root, f);
}
var setRepublishEntireSite = function (f) {
    $.post("/puck/api/RepublishEntireSite", f);
}
var getRepublishEntireSiteStatus = function (f) {
    $.get("/puck/api/GetRepublishEntireSiteStatus", f);
}
