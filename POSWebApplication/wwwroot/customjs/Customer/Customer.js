function callAddCustomerController() {
    $('#loader-wrapper').show();

    $.ajax({
        url: "/Customer/AddCustomerPartial",
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

function callEditCustomerController(ArId) {
    $('#loader-wrapper').show();

    var inputData = {
        arId: ArId
    };

    $.ajax({
        url: "/Customer/EditCustomerPartial",
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

function callDeleteCustomerController(ArId) {
    $('#loader-wrapper').show();

    var inputData = {
        arId: ArId
    };

    $.ajax({
        url: "/Customer/DeleteCustomerPartial",
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
