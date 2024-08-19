
function callAddCurrencyController() {
    $('#loader-wrapper').show();
    $.ajax({
        url: "/Currency/AddCurrencyPartial",
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

function callEditCurrencyController(CurrId) {
    $('#loader-wrapper').show();
    var inputData = {
        currId: CurrId
    };

    $.ajax({
        url: "/Currency/EditCurrencyPartial",
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

function callDeleteCurrencyController(CurrId) {
    $('#loader-wrapper').show();
    var inputData = {
        currId: CurrId
    };

    $.ajax({
        url: "/Currency/DeleteCurrencyPartial",
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

