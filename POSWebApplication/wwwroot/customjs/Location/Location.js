function callAddLocationController() {
    $('#loader-wrapper').show();

    $.ajax({
        url: "/Location/AddLocationPartial",
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

function callEditLocationController(LocCde) {

    $('#loader-wrapper').show();
    var inputData = {
        locCde: LocCde
    };

    $.ajax({
        url: "/Location/EditLocationPartial",
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

function callDeleteLocationController(LocCde) {

    $('#loader-wrapper').show();
    var inputData = {
        locCde: LocCde
    };

    $.ajax({
        url: "/Location/DeleteLocationPartial",
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
