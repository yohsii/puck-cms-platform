var pobj = {};
cleft = $(".preview-editor-cleft");
cright = $(".preview-editor-cright");

pobj.highlightIframe = function () {
    

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


