var pobj = {};
cleft = $(".preview-editor-cleft");
cright = $(".preview-editor-cright");

pobj.highlightIframe = function () {
    var iframe = $(pobj.iframe.contents());
    iframe.find("[data-puck-field]").each(function (i) {
        var el = $(this);
        var elName;
        el.hover(function () {
            el.css({ outline: "3px solid #ff8c8c" });
            var nameTop = el.offset().top-27;
            var nameLeft = el.offset().left-3;
            debugger;
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
                el.css({ outline: "none" });
                elName.remove();
        });

    });   

}

pobj.bindHandlers = function () {

}

pobj.hideOverlaySpace = function () {
    cright.hide();
}

$(document).ready(function () {
    $("body").css("border-top","none");
    pobj.iframe = $("iframe:first");
    pobj.id = $(".preview-editor-id").val();
    pobj.variant = $(".preview-editor-variant").val();

    pobj.iframe.attr("src","/puck/preview/previewguid?id="+pobj.id+"&variant="+pobj.variant);

    pobj.iframe.load(function () {
        overlayClose();
        pobj.highlightIframe();
    });

});


