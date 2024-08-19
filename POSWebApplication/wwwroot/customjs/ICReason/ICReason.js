function callAddICReasonController() {
    $('#loader-wrapper').show();
    $.ajax({
        url: "/ICReason/AddICReasonPartial",
        type: "GET",
        success: function (data) {
            $('#defaultContainer').html(data);
            $('#inputStart').focus();
            $('#loader-wrapper').hide();
        },
        error: function () {
            redirectToLogIn();
        }
    });

    scrollToDiv();
}

function callEditICReasonController(icReasonId) {
    $('#loader-wrapper').show();
    var inputData = {
        icReasonId: icReasonId
    };

    $.ajax({
        url: "/ICReason/EditICReasonPartial",
        type: "GET",
        dataType: "html",
        data: inputData,
        success: function (data) {
            $('#defaultContainer').html(data);
            $('#loader-wrapper').hide();
        },
        error: function () {
            redirectToLogIn();
        }
    });

    scrollToDiv();
}

function callDeleteICReasonController(icReasonId) {
    $('#loader-wrapper').show();
    var inputData = {
        icReasonId: icReasonId
    };

    $.ajax({
        url: "/ICReason/DeleteICReasonPartial",
        type: "GET",
        dataType: "html",
        data: inputData,
        success: function (data) {
            $('#defaultContainer').html(data);
            $('#loader-wrapper').hide();
        },
        error: function () {
            redirectToLogIn();
        }
    });

    scrollToDiv();
}
