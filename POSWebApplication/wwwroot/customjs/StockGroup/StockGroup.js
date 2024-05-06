function callAddStockGroupController() {
  $.ajax({
    url: "/StockGroup/AddStockGroupPartial",
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

function callEditStockGroupController(StkGrpId) {
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
    },
    error: function (data) {
      alert('error');
    }
  });

  scrollToDiv();
}

function callDeleteStockGroupController(StkGrpId) {
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
    },
    error: function (data) {
      alert('error');
    }
  });

  scrollToDiv();
}
