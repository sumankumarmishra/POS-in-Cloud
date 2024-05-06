
function callAddCurrencyController() {
  $.ajax({
    url: "/Currency/AddCurrencyPartial",
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

function callEditCurrencyController(CurrId) {
  var inputData = {
    currId: CurrId
  };

  $.ajax({
    url: "/Currency/EditCurrencyPartial",
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

function callDeleteCurrencyController(CurrId) {
  var inputData = {
    currId: CurrId
  };

  $.ajax({
    url: "/Currency/DeleteCurrencyPartial",
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

