function callAddCustomerController() {
  $.ajax({
    url: "/Customer/AddCustomerPartial",
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

function callEditCustomerController(ArId) {
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
    },
    error: function (data) {
      alert('error');
    }
  });

  scrollToDiv();
}

function callDeleteCustomerController(ArId) {
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
    },
    error: function (data) {
      alert('error');
    }
  });

  scrollToDiv();
}
