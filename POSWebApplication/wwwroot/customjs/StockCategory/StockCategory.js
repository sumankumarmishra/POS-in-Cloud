function callAddStockCategoryController() {
  $.ajax({
    url: "/StockCategory/AddStockCategoryPartial",
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

function callEditStockCategoryController(CatgId) {
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
    },
    error: function (data) {
      alert('error');
    }
  });

  scrollToDiv();
}

function callDeleteStockCategoryController(CatgId) {
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
    },
    error: function (data) {
      alert('error');
    }
  });

  scrollToDiv();
}

