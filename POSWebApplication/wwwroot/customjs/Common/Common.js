// JQuery Only
function formatDecimal(id) {
  var inputElement = $("#" + id);
  var value = parseFloat(inputElement.val()).toFixed(2);
  inputElement.val(value);
}

function cleanDefaultContainer() {
  $("#defaultContainer").empty();
}

function scrollToDiv() {
  $("#defaultContainer").get(0).scrollIntoView({ behavior: "smooth" });
}
