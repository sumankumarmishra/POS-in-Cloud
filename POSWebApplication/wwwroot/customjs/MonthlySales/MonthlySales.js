/*All JS functions for MonthlySales*/

function viewMonthlySales() {
  $('#monthlySaleBody').empty();
  $('#loader-wrapper').show();

  var cmpyId = $('#filterCmpyId').val();
  var catgCde = $('#filterCatg').val();
  var year = $('#filterYear').val();

  if (year == '' || year == null) {
    alert('Please Input Year!');
    return;
  }

  var inputData = {
    cmpyId: cmpyId,
    catgCde: catgCde,
    year: year
  }

  $.ajax({
    type: 'POST',
    url: "/MonthlySales/View",
    data: inputData,
    success: function (view) {
      $('#loader-wrapper').hide();
      $('#monthlySaleBody').html(view);
      showDonutChart(inputData);
      showBarChart(inputData);
    },
    error: function () {
      $('#loader-wrapper').hide();
      alert('Session Expired!');
      window.location.href = '/LogIn/Index';  // Redirect to login
    }
  });
}

function showDonutChart(inputData) {
  var donutChartCanvas = $('#donutChart').get(0).getContext('2d');
  $.ajax({
    url: '/MonthlySales/GetDonutChartData',
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

function showBarChart(inputData) {

  var barChartCanvas = $('#barChart').get(0).getContext('2d');

  $.ajax({
    url: '/MonthlySales/GetBarChartData',
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
