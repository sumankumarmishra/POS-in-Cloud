/*All JS functions for YearlySales*/

function viewYearlySales() {
  $('#yearlySaleBody').empty();
  $('#loader-wrapper').show();

  var cmpyId = $('#filterCmpyId').val();
  var year = $('#filterYear').val();
  var exYear = $('#filterExYear').val();
  if (year == '' || year == null) {
    alert('Please Input Year!');
    return;
  }

  if (exYear == '' || exYear == null) {
    alert('Please Input For Next In-Year!');
    return;
  }

  var inputData = {
    cmpyId: cmpyId,
    year: year,
    exYear: exYear
  }

  $.ajax({
    type: 'POST',
    url: "/YearlySales/View",
    data: inputData,
    success: function (view) {
      $('#loader-wrapper').hide();
      $('#yearlySaleBody').html(view);
      showYearlyDonutChart(inputData);
      showYearlyBarChart(inputData);
    },
    error: function (data) {
      $('#loader-wrapper').hide();
      alert('Session Expired!');
      window.location.href = '/LogIn/Index';  // Redirect to login
    }
  });
}

function showYearlyDonutChart(inputData) {

  var donutChartCanvas = $('#donutChart').get(0).getContext('2d');
  $.ajax({
    url: '/YearlySales/GetDonutChartData',
    type: 'GET',
    data: inputData,
    dataType: 'json',
    success: function (data) {
      var donutOptions = {
        maintainAspectRatio: false,
        responsive: true,
      };
      new Chart(donutChartCanvas, {
        type: 'doughnut',
        data: data,
        options: donutOptions
      });
    },
    error: function (error) {
      console.error('Error fetching data:', error);
      alert('Session Expired!');
      window.location.href = '/LogIn/Index';  // Redirect to login
    }
  });

}

function showYearlyBarChart(inputData) {

  var barChartCanvas = $('#barChart').get(0).getContext('2d');

  $.ajax({
    url: '/YearlySales/GetBarChartData',
    type: 'GET',
    data: inputData,
    dataType: 'json',
    success: function (data) {
      var barChartOptions = {
        responsive: true,
        maintainAspectRatio: false,
        datasetFill: false,
        scales: { // Important
          xAxes: [{
            ticks: {
              beginAtZero: true
            }
          }],
          yAxes: [{
            ticks: {
              beginAtZero: true
            }
          }]
        }
      }

      new Chart(barChartCanvas, {
        type: 'horizontalBar',
        data: data,
        options: barChartOptions
      })
    },
    error: function (error) {
      console.error('Error fetching data:', error);
      alert('Session Expired!');
      window.location.href = '/LogIn/Index';  // Redirect to login
    }
  });
}
