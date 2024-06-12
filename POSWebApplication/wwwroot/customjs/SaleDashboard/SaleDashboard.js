/*All JS functions for SaleDashboard*/


function goBack() {
  window.history.back();
}

function searchSales() {
  $('#mainBody').empty();
  $('#loader-wrapper').show();

  var cmpyId = $('#filterCmpyId').val();
  var fromDate = $('#filterFromDte').val();
  var toDate = $('#filterToDte').val();

  var inputData = {
    cmpyId: cmpyId,
    fromDate: fromDate,
    toDate: toDate
  }

  $.ajax({
    type: 'POST',
    url: "/SaleDashboard/Search",
    data: inputData,
    success: function (view) {
      $('#loader-wrapper').hide();
      $('#mainBody').html(view);
    },
    error: function (data) {
      $('#loader-wrapper').hide();
      alert('Session Expired!');
      window.location.href = '/Home/Login';  // Redirect to login
    }
  });
}

