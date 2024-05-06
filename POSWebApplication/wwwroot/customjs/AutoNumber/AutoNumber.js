function callEditAutoNumberController(AutoNoId) {

  var inputData = {
    autoNoId: AutoNoId
  };

  $.ajax({
    url: "/AutoNumber/EditAutoNumberPartial",
    type: "GET",
    dataType: "html",
    data: inputData
  }).done(function (data) {
    $('#defaultContainer').html(data);
  }).fail(function () {
    alert('error');
  });

  scrollToDiv();
}
