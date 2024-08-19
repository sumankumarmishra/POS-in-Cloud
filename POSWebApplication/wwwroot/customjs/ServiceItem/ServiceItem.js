function callAddServiceItemController() {
    $('#loader-wrapper').show();
    $.ajax({
        url: "/ServiceItem/AddServiceItemPartial",
        type: "GET",
        success: function (data) {
            $('#defaultContainer').html(data);
            $('#inputStart').focus();
            $('#loader-wrapper').hide();
        },
        error: function () {
            $('#loader-wrapper').hide();
            redirectToLogIn();
        }
    });

    scrollToDiv();
}

function callEditServiceItemController(SrvcId) {

    $('#loader-wrapper').show();

    var inputData = {
        srvcId: SrvcId
    };
    $.ajax({
        url: "/ServiceItem/EditServiceItemPartial",
        type: "GET",
        dataType: "html",
        data: inputData,
        success: function (data) {
            $('#defaultContainer').html(data);
            $('#loader-wrapper').hide();
        },
        error: function () {
            $('#loader-wrapper').hide();
            redirectToLogIn();
        }
    });

    scrollToDiv();
}

function callDeleteServiceItemController(SrvcId) {

    $('#loader-wrapper').show();

    var inputData = {
        srvcId: SrvcId
    };
    $.ajax({
        url: "/ServiceItem/DeleteServiceItemPartial",
        type: "GET",
        dataType: "html",
        data: inputData,
        success: function (data) {
            $('#defaultContainer').html(data);
            $('#loader-wrapper').hide();
        },
        error: function () {
            $('#loader-wrapper').hide();
            redirectToLogIn();
        }
    });

    scrollToDiv();
}


