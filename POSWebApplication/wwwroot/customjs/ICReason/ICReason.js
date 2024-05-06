function callAddICReasonController() {
  $.ajax({
    url: "/ICReason/AddICReasonPartial",
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

function callEditICReasonController(icReasonId) {
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
    },
    error: function (data) {
      alert('error');
    }
  });

  scrollToDiv();
}

function callDeleteICReasonController(icReasonId) {
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
    },
    error: function (data) {
      alert('error');
    }
  });

  scrollToDiv();
}
