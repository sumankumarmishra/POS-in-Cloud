function callAddLocationController() {
  $.ajax({
    url: "/Location/AddLocationPartial",
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

function callEditLocationController(LocCde) {
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
    },
    error: function (data) {
      alert('error');
    }
  });

  scrollToDiv();
}

function callDeleteLocationController(LocCde) {
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
    },
    error: function (data) {
      alert('error');
    }
  });

  scrollToDiv();
}
