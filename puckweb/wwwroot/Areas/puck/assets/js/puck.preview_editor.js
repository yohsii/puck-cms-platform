var msg = function (success, str, shouldRemovePreviousMessages, container, timeout, cancel) {
    timeout = timeout || 5000;
    //container = container || cmsg;
    container = pobj.msgContainer;
    if (shouldRemovePreviousMessages) {
        container.find("div").remove();
    }
    var btnClass = "btn-light";
    if (success === false) { btnClass = "btn-danger"; }
    else if (success === true) { btnClass = "btn-success"; }
    var el = $("<div style='display:none;' class='btn " + btnClass + "'>" + str + "</div>");
    var remove = $("<div class='btn btnclose'>x</div>").click(function () { $(this).parent().remove(); /*setAreaHeights();*/ });
    el.append(remove);
    container.html(el);
    el.fadeIn(function () { /*setAreaHeights();*/ });
    if (cancel) {
        cancel(function () {
            el.fadeOut(function () { el.remove(); /*setAreaHeights();*/ });
        });
    } else {
        setTimeout(function () { el.fadeOut(function () { el.remove(); /*setAreaHeights();*/ }); }, timeout);
    }
}
overlay = function (el, width, height, top, title, isRightSided) {
    isRightSided = isRightSided || false;
    isRightSided = true;
    var overlayClass = isRightSided ? "right" : "left";
    top = top || "0px";
    //overlayClose(false,overlayClass,false);
    var cleftIsVisible = false;
    if (window.innerWidth < 768) {
        if (cleft.is(":visible")) {
            cleftIsVisible = true;
            //console.log("overlay width set from cleft");
            if (width != "100%")
                width = cleft.outerWidth();
            cleft.hide();
        }
    }
    if (width != "100%")
        if (width > window.innerWidth)
            width = window.innerWidth;
    var f = undefined;
    searchDialogClose();
    var outer = $(".interfaces .overlay_screen").clone().addClass("active").addClass(overlayClass).addClass("scrollContainer");
    outer.find(">h1:first").html(title || "")
    var left = (cright.offset().left - 30) < -10 ? -10 : (cright.offset().left - 30);
    if (isRightSided)
        outer.css({ right: "-14px", width: width, top: "0px", height: $(window).height() - 90 + "px" });
    else
        outer.css({ left: left + "px", width: "0px", top: "0px", height: $(window).height() - 90 + "px" });
    if (outer.position().top < $(".rightarea").scrollTop()) {
        outer.css({ top: $(".rightarea").scrollTop() });
    }
    var close = $('<i class="overlay_close fas fa-minus-circle"></i>');
    outer.data("removed", false);
    close.click(function () { overlayClose(cleftIsVisible, overlayClass, undefined, outer); });
    outer.append(close);
    var inner = outer.find(".inner");
    var clear = $("<div class='clearboth'/>");
    width = width || cright.width() - 10;

    inner.append(el).append(clear);
    cright.append(outer);
    if (!isRightSided)
        outer.animate({ width: width + (width.toString().indexOf("%") > -1 ? "" : "px") }, 200, function () { if (f) f(); afterDom(); });
    else afterDom();
    if ($(".overlay_screen.active").length == 1) {

    }
    overlays.unshift(outer);
    return outer;
}

var pobj = {};

pobj.getForm = function () {
    displayMarkup(undefined, undefined, pobj.variant, undefined, pobj.id, cright, undefined, false, function () {
        cright.find(".editor-label.col-sm-2").removeClass("col-sm-2");
        if (pobj.lastFocus) {
            pobj.focusForm(pobj.lastFocus);
        }
    });
}

pobj.focusForm = function (fieldName) {
    cright.find(".content_preview").hide();
    cright.find(".fieldwrapper").hide();
    cright.find(".fieldwrapper[data-fieldname='" + fieldName + "']").show();
    pobj.lastFocus = fieldName;
}

pobj.highlightIframe = function () {
    var iframe = $(pobj.iframe.contents());
    iframe.find("[data-puck-field]").each(function (i) {
        var el = $(this);
        var elName;
        el.hover(function () {
            el.css({ outline: "3px solid #ff8c8c" });
            var nameTop = el.offset().top-27;
            var nameLeft = el.offset().left-3;

            elName = $("<div/>").addClass("puck-field-name").css({
                position: "absolute",
                left: nameLeft,
                top: nameTop,
                background: "#000",
                color: "#fff",
                opacity:"0.8"
            }).html(el.attr("data-puck-field"));
            iframe.find("body").append(elName);
        }, function () {
                try {
                    el.css({ outline: "none" });
                    elName.remove();
                } catch (ex) { }
        });

    });   

}

pobj.bindHandlers = function () {
    var iframe = $(pobj.iframe.contents());
    iframe.find("[data-puck-field]").each(function (i) {
        var el = $(this);

        el.click(function () {
            if (cright.is(":visible")) {
                pobj.focusForm(el.attr("data-puck-field"));
            } else {
                pobj.showOverlaySpace();
                pobj.focusForm(el.attr("data-puck-field"));
            }

        });

    });
}
pobj.bindMenu = function () {
    crightOuter.find(".preview-editor-close").click(function () {
        crightOuter.hide();
    });
    crightOuter.find(".preview-editor-update").click(function () {
        var formEl = cright.find("form:first");
        if (formEl.length == 1) {
            var form = formEl.clone();
            form.attr("action","/puck/Preview/PreviewFromForm?p_type="+pobj.type);
            form.attr("target", "previewEditorIframe");
            $(".preview-editor-form-area").html("");
            $(".preview-editor-form-area").append(form);
            form.get(0).submit();
        }
    });
}

pobj.hideOverlaySpace = function () {
    crightOuter.hide();
}
pobj.showOverlaySpace = function () {
    crightOuter.removeClass("d-none");
    crightOuter.show();
    cright.show();
}

$(document).ready(function () {
    Array.prototype.contains = Array.prototype.includes;
    getUserLanguage(function (d) { defaultLanguage = d; });
    getUserRoles(function (d) {
        userRoles = d;
    });
    $("body").css("border-top","none");
    cleft = $(".preview-editor-cleft");
    cright = $(".preview-editor-cright");
    crightOuter = $(".preview-editor-cright-outer").css({
        height: window.innerHeight + "px",
        maxHeight: window.innerHeight + "px"
    });
    crightOuter.on("click", "#myTab li.nav-item", function (e) {
        debugger;
        var el = $(this);
        var index = el.index();
        el.parents("#myTab").find("li.nav-item").removeClass("active");
        el.addClass("active");
        el.parents(".puckImage:first").find(".tab-content .tab-pane").removeClass("active");
        el.parents(".puckImage:first").find(".tab-content .tab-pane:nth-child("+(index+1)+")").addClass("active");
    });

    pobj.iframe = $("iframe:first");
    pobj.msgContainer = $(".preview-editor-msg");
    pobj.id = $(".preview-editor-id").val();
    pobj.variant = $(".preview-editor-variant").val();
    pobj.type = $(".preview-editor-type").val();

    pobj.iframe.attr("src","/puck/preview/previewguid?id="+pobj.id+"&variant="+pobj.variant);
    var firstTime = true;
    pobj.iframe.load(function () {
        overlayClose();
        pobj.hideOverlaySpace();
        pobj.highlightIframe();
        pobj.bindHandlers();
        if (firstTime) {
            pobj.getForm();
            firstTime = false;
        }
        pobj.bindMenu();
    });

});


