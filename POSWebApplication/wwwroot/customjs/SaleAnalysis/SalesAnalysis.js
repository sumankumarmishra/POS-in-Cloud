/*All JS functions for Sale Analysis*/

function searchSales() {
  $('#saleAnalysisBody').empty();
  $('#loader-wrapper').show();

  var cmpyId = $('#filterCmpyId').val();
  var fromDate = $('#filterFromDte').val();
  var days = $('#filterNextDay').val();
  var mode = $('#filterMode').val();
  var topBottom = $('#filterTopBottom').val();

  var inputData = {
    cmpyId: cmpyId,
    fromDate: fromDate,
    days: days,
    mode: mode,
    topBottom: topBottom
  }

  $.ajax({
    type: 'POST',
    url: "/SalesAnalysis/Search",
    data: inputData,
    success: function (view) {
      $('#loader-wrapper').hide();
      $('#saleAnalysisBody').html(view);
    },
    error: function (data) {
      $('#loader-wrapper').hide();
      alert('Session Expired!');
      window.location.href = '/LogIn/Index';  // Redirect to login
    }
  });
}
