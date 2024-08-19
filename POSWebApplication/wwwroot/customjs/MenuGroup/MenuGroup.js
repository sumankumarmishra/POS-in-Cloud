function callAddMenuGroupController() {
    $('#loader-wrapper').show();
    $.ajax({
        url: "/MenuGroup/AddMenuGroupPartial",
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

function callEditMenuGroupController(MnuGrpId) {
    $('#loader-wrapper').show();
    var inputData = {
        mnuGrpId: MnuGrpId
    };

    $.ajax({
        url: "/MenuGroup/EditMenuGroupPartial",
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

function callDeleteMenuGroupController(MnuGrpId) {
    $('#loader-wrapper').show();
    var inputData = {
        mnuGrpId: MnuGrpId
    };

    $.ajax({
        url: "/MenuGroup/DeleteMenuGroupPartial",
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
