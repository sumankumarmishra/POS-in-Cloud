function callAddMenuGroupController() {
  $.ajax({
    url: "/MenuGroup/AddMenuGroupPartial",
    type: "GET",
    success: function (data) {
      $('#defaultContainer').html(data);
    },
    error: function (data) {
      alert('error');
    }
  });

  scrollToDiv();
}

function callEditMenuGroupController(MnuGrpId) {
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
    },
    error: function (data) {
      alert('error');
    }
  });

  scrollToDiv();
}

function callDeleteMenuGroupController(MnuGrpId) {
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
    },
    error: function (data) {
      alert('error');
    }
  });

  scrollToDiv();
}
