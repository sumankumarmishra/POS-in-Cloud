function callStockBOMController(BomId) {
  var loadingScreen = document.getElementById('loadingTextId');
  loadingScreen.style.display = "block";

  var inputData = {
    bomId: BomId
  };

  $.ajax({
    url: "/StockBOM/StockBOMPartial",
    type: "GET",
    dataType: "html",
    data: inputData,
    success: function (data) {
      $('#defaultContainer').html(data);
      loadingScreen.style.display = "none";
    },
    error: function (data) {
      alert('error');
      loadingScreen.style.display = "none";
    }
  });

  scrollToDiv();
}

function callAddStockBOMController(BomId) {
  var inputData = {
    bomId: BomId
  };

  $.ajax({
    url: "/StockBOM/AddStockBOMPartial",
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

function callSelectItemController(BOMId, ItemId) {
  var inputData = {
    bomId: BOMId,
    itemId: ItemId
  };

  $.ajax({
    url: "/StockBOM/SelectItemPartial",
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

function callEditBOMController(BomId, StkBOMId) {
  var inputData = {
    bomId: BomId,
    stkBOMId: StkBOMId
  };

  var tr = StkBOMId;
  tr = "#" + tr;

  $.ajax({
    url: "/StockBOM/EditStockBOMPartial",
    type: "GET",
    dataType: "html",
    data: inputData,
    success: function (data) {
      $(tr).html(data);
    },
    error: function (data) {
      alert('error');
    }
  });

  scrollToDiv();
}

function callDeleteBOMController(BOMId, StkBOMId) {
  var inputData = {
    bomId: BOMId,
    stkBOMId: StkBOMId
  };

  $.ajax({
    url: "/StockBOM/DeleteStockBOMPartial",
    type: "GET",
    dataType: "html",
    data: inputData,
    success: function (data) {
      $('#defaultContainer').html(data);
    },
    error: function () {
      alert('error');
    }
  });

  scrollToDiv();
}

function addStockBOM() {
  var form = document.getElementById("addForm");
  var formData = new FormData(form);
  $.ajax({
    url: "/StockBOM/Create",
    type: "POST",
    contentType: "application/json",
    processData: false,
    contentType: false,
    data: formData,
    success: function (data) {
      $('#defaultContainer').html(data);
    },
    error: function (data) {
      alert('error');
    }
  });

  scrollToDiv();
}

function editStockBOM() {
  var form = document.getElementById("editForm");
  var formData = new FormData(form);
  $.ajax({
    url: "/StockBOM/Edit",
    type: "POST",
    contentType: "application/json",
    processData: false,
    contentType: false,
    data: formData,
    success: function (data) {
      $('#defaultContainer').html(data);
    },
    error: function (data) {
      alert('error');
    }
  });

  scrollToDiv();
}

function disableButtons() {
  var buttons = document.getElementsByClassName('btn-edit');

  for (var i = 0; i < buttons.length; i++) {
    var button = buttons[i];
    button.hidden = true;
  }

  document.getElementsByClassName('update-btn')[0].disabled = false;
  document.getElementsByClassName('cancel-btn')[0].hidden = false;
  document.getElementsByClassName('close-btn')[0].hidden = true;
  document.getElementsByClassName('add-btn')[0].disabled = true;
}

function addStockBOMItems() {
  const tBody = $('#stockBOMItemsListId');
  const newRow = $('<tr>').css({ 'font-size': '14px', 'text-align': 'center' });

  const columnStyles = {
    padding: '0px',
    height: '30px',
    width: '100%',
    border: 'none',
    color: '#31849B',
    textAlign: 'center'
  };

  const leftColumnStyles = {
    ...columnStyles,
    textAlign: 'left',
  };

  //input OrderId

  var inputOrder = $('<input>').attr('type', 'number').css(columnStyles);
  newRow.append($('<td>').css('padding', '0px').append(inputOrder));

  //select ItemId

  var selectItemId = $('<select>').css(leftColumnStyles);

  $.ajax({
    url: "/StockBOM/GetStocks",
    success: function (stocks) {
      var fragment = document.createDocumentFragment();
      stocks.forEach(stock => {
        $("<option>").val(stock.itemId).text(stock.itemId).appendTo(fragment);
      });
      selectItemId.append(fragment);
    },
    error: function () {
      alert('Error fetching stocks.');
    }
  });

  selectItemId.on('change', function () {
    $.ajax({
      url: "/StockPackage/GetStockUOMs",
      data: { itemId: this.value },
      success: function (uoms) {
        selectUOM.find("option").remove();
        uoms.forEach(uom => {
          $("<option>").val(uom.uomCde).text(uom.uomCde).appendTo(selectUOM);
        });
      },
      error: function () {
        alert('Error fetching UOMs.');
      }
    });
  });

  newRow.append($('<td>').css('padding', '0px').append(selectItemId));

  //select UOM

  var selectUOM = $('<select>').css(leftColumnStyles);

  $.ajax({
    url: "/StockPackage/GetStockUOMs",
    data: { itemId: selectItemId.val() },
    success: function (uoms) {
      if (uom != null) {
        uoms.forEach(uom => {
          $("<option>").val(uom.uomCde).text(uom.uomCde).appendTo(selectUOM);
        });
      }
    },
    error: function () {
      alert('Error fetching UOMs.');
    }
  });

  newRow.append($('<td>').css('padding', '0px').append(selectUOM));

  //input Qty

  var inputQty = $('<input>').attr('type', 'number').css(columnStyles).val(1);
  newRow.append($('<td>').css('padding', '0px').append(inputQty));

  newRow.on('keypress', function (event) {
    if (event.keyCode === 13) {
      addNewDetailsRow();
    }
  });

  newRow.on('keydown', function (event) {
    if (event.keyCode === 46) {
      newRow.remove();
    }
  });
  tBody.append(newRow);
  $('#spanId').text(itemId);
}

function editStockBOMItems(itemId) {
  clearStockBOMItems();
  const tBody = $('#stockBOMItemsListId');
  const modal = $('#stockBOMItemsModal');

  $.ajax({
    url: "/StockBOM/GetStockBOMList",
    type: "GET",
    data: { bomId: itemId },
    success: function (stockBOMs) {
      stockBOMs.forEach(function (item) {
        const newRow = $('<tr>').css({ 'font-size': '14px', 'text-align': 'center' });

        const columnStyles = {
          padding: '0px',
          height: '30px',
          width: '100%',
          border: 'none',
          color: '#31849B',
          textAlign: 'center'
        };

        const leftColumnStyles = {
          ...columnStyles,
          textAlign: 'left',
        };

        //input OrderId

        var inputOrder = $('<input>').attr('type', 'number').css(columnStyles).val(item.ordId);
        newRow.append($('<td>').css('padding', '0px').append(inputOrder));

        //select ItemId

        var selectItemId = $('<select>').css(leftColumnStyles);

        $.ajax({
          url: "/StockBOM/GetStocks",
          success: function (stocks) {
            var fragment = document.createDocumentFragment();
            stocks.forEach(stock => {
              $("<option>").val(stock.itemId).text(stock.itemId).appendTo(fragment);
            });
            selectItemId.append(fragment);
            selectItemId.val(item.itemId);
          },
          error: function () {
            alert('Error fetching stocks.');
          }
        });

        selectItemId.on('change', function () {
          $.ajax({
            url: "/StockPackage/GetStockUOMs",
            data: { itemId: this.value },
            success: function (uoms) {
              selectUOM.find("option").remove();
              uoms.forEach(uom => {
                $("<option>").val(uom.uomCde).text(uom.uomCde).appendTo(selectUOM);
              });
            },
            error: function () {
              alert('Error fetching UOMs.');
            }
          });
        });

        newRow.append($('<td>').css('padding', '0px').append(selectItemId));

        //select UOM

        var selectUOM = $('<select>').css(leftColumnStyles);

        $.ajax({
          url: "/StockPackage/GetStockUOMs",
          data: { itemId: item.itemId },
          success: function (uoms) {
            uoms.forEach(uom => {
              $("<option>").val(uom.uomCde).text(uom.uomCde).appendTo(selectUOM);
            });
            selectUOM.val(item.baseUnit);
          },
          error: function () {
            alert('Error fetching UOMs.');
          }
        });

        newRow.append($('<td>').css('padding', '0px').append(selectUOM));

        //input Qty

        var inputQty = $('<input>').attr('type', 'number').css(columnStyles).val(item.qty);
        newRow.append($('<td>').css('padding', '0px').append(inputQty));

        newRow.on('keypress', function (event) {
          if (event.keyCode === 13) {
            addNewDetailsRow();
          }
        });

        newRow.on('keydown', function (event) {
          if (event.keyCode === 46) {
            newRow.remove();
          }
        });
        tBody.append(newRow);
      });

      modal.show();
    },
    error: function (data) {
      alert('error');
    }
  });

  $('#spanId').text(itemId);

}
function clearStockBOMItems() {
  $('#stockBOMItemsListId').empty();
}

function closeStockBOMItemsModal() {
  $('#stockBOMItemsModal').hide();
}

