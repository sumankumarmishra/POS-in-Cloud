function callAddServiceItemController() {
  $.ajax({
    url: "/ServiceItem/AddServiceItemPartial",
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

function callEditServiceItemController(SrvcId) {
  var inputData = {
    srvcId: SrvcId
  };
  $.ajax({
    url: "/ServiceItem/EditServiceItemPartial",
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

function callDeleteServiceItemController(SrvcId) {
  var inputData = {
    srvcId: SrvcId
  };
  $.ajax({
    url: "/ServiceItem/DeleteServiceItemPartial",
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
