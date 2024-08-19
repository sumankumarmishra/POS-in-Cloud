function callAddStockGroupController() {
    $('#loader-wrapper').show();
    $.ajax({
        url: "/StockGroup/AddStockGroupPartial",
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

function callEditStockGroupController(StkGrpId) {
    $('#loader-wrapper').show();
    var inputData = {
        stkGrpId: StkGrpId
    };

    $.ajax({
        url: "/StockGroup/EditStockGroupPartial",
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

function callDeleteStockGroupController(StkGrpId) {
    $('#loader-wrapper').show();
    var inputData = {
        stkGrpId: StkGrpId
    };

    $.ajax({
        url: "/StockGroup/DeleteStockGroupPartial",
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
