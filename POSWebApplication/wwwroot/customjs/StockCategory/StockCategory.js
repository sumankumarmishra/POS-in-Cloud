function callAddStockCategoryController() {

    $('#loader-wrapper').show();
    $.ajax({
        url: "/StockCategory/AddStockCategoryPartial",
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

function callEditStockCategoryController(CatgId) {

    $('#loader-wrapper').show();
    var inputData = {
        catgId: CatgId
    };

    $.ajax({
        url: "/StockCategory/EditStockCategoryPartial",
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

function callDeleteStockCategoryController(CatgId) {

    $('#loader-wrapper').show();
    var inputData = {
        catgId: CatgId
    };

    $.ajax({
        url: "/StockCategory/DeleteStockCategoryPartial",
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

