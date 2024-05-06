
function showBillDs(billHId) {
  $('#billHModal').show();

  $.ajax({
    url: "/Home/GetBillDList",
    type: "GET",
    dataType: "html",
    data: { billHId: billHId }
  }).done(function (data) {
    $('#billEditBodyId').html(data);
  }).fail(function () {
    alert('error');
  });
}

function search() {

  const allData = {
    fromDate: $('#filterFromDate').val(),
    toDate: $('#filterToDate').val(),
    shiftNo: $('#filterShiftNo').val()
  }

  console.log(allData);

  $.ajax({
    url: "/Home/Search",
    type: "POST",
    data: allData,
  }).done(function (data) {
    $('#mainTable').html(data);
  }).fail(function () {
    alert('error');
  });
}

function printBillSlip(billNo) {
  $.ajax({
    type: 'POST',
    url: '/Home/BillPrint',
    data: {
      billNo: billNo
    },
    success: function (response) {
      console.log('hi')
      var newWindow = window.open(''); // open new empty window for bill slip

      newWindow.document.write(response); // write return htmlcontent in the new empty window
    },
    error: function (error) {
      alert('Error occured.');
    }
  });
}

function closeBillHEditModal() {
  $('#billHModal').hide();
}
