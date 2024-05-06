// js functions for Stock
function callAddStockController() {
  var loadingScreen = document.getElementById('loadingTextId');
  loadingScreen.style.display = "block";

  $.ajax({
    url: "/Stock/AddStockPartial",
    type: "GET",
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

function callEditStockController(ItemId) {
  var loadingScreen = document.getElementById('loadingTextId');
  loadingScreen.style.display = "block";

  var inputData = {
    itemId: ItemId
  };

  $.ajax({
    url: "/Stock/EditStockPartial",
    type: "GET",
    dataType: "html",
    data: inputData,
    success: function (data) {
      $('#defaultContainer').html(data);
      scrollToDiv();
      loadingScreen.style.display = "none";
    },
    error: function (data) {
      alert('error');
      loadingScreen.style.display = "none";
    }
  });
}

function callDeleteStockController(ItemId) {
  var loadingScreen = document.getElementById('loadingTextId');
  loadingScreen.style.display = "block";

  var inputData = {
    itemId: ItemId
  };

  $.ajax({
    url: "/Stock/DeleteStockPartial",
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

function addStockUOMItem() {
  const tBody = $('#stockUOMItemsListId');


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

  const newRow = $('<tr>').css({ 'font-size': '14px', 'text-align': 'center' });

  //input UOMCode

  var inputUOMCode = $('<input>').css(leftColumnStyles);
  newRow.append($('<td>').css('padding', '0px').append(inputUOMCode));

  //input Rate

  var inputRate = $('<input>').attr('type', 'number').css(columnStyles).val(1);
  newRow.append($('<td>').css('padding', '0px').append(inputRate));

  //input UnitCost

  var inputUnitCost = $('<input>').attr('type', 'number').css(columnStyles).val(0);
  newRow.append($('<td>').css('padding', '0px').append(inputUnitCost));

  //input Price

  var inputPrice = $('<input>').attr('type', 'number').css(columnStyles).val(0);
  newRow.append($('<td>').css('padding', '0px').append(inputPrice));

  newRow.on('keypress', function (event) {
    if (event.keyCode === 13) {
      addStockUOMItem();
    }
  });

  newRow.on('keydown', function (event) {
    if (event.keyCode === 46) {
      newRow.remove();
    }
  });
  tBody.append(newRow);
}

function editStockUOMItems(itemId) {
  clearStockUOMItems();
  const tBody = $('#stockUOMItemsListId');
  const modal = $('#stockUOMItemsModal');

  $.ajax({
    url: "/StockUOM/GetStockUOMsList",
    type: "GET",
    data: { itemId: itemId },
    success: function (stockUOMs) {

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

      stockUOMs.forEach(function (uom) {

        console.log(uom);

        const newRow = $('<tr>').css({ 'font-size': '14px', 'text-align': 'center' });

        //input UOMCode

        var inputUOMCode = $('<input>').css(leftColumnStyles).val(uom.uomCde);
        newRow.append($('<td>').css('padding', '0px').append(inputUOMCode));

        //input Rate

        var inputRate = $('<input>').attr('type', 'number').css(columnStyles).val(uom.uomRate);
        newRow.append($('<td>').css('padding', '0px').append(inputRate));

        //input UnitCost

        var inputUnitCost = $('<input>').attr('type', 'number').css(columnStyles).val(uom.unitCost);
        newRow.append($('<td>').css('padding', '0px').append(inputUnitCost));

        //input Price

        var inputPrice = $('<input>').attr('type', 'number').css(columnStyles).val(uom.sellingPrice);
        newRow.append($('<td>').css('padding', '0px').append(inputPrice));

        newRow.on('keypress', function (event) {
          if (event.keyCode === 13) {
            addStockUOMItem();
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
  $('#hiddenId').text(itemId);

}

function saveStockUOMItems() {

  const modal = $('#stockUOMItemsModal');
  const shouldProceed = window.confirm("Are you sure of saving record?");

  if (shouldProceed) {
    const itemId = $('#hiddenId').text();

    const tableData = [];
    const table = document.getElementById('stockUOMItemsTable');

    for (let i = 1; i < table.rows.length; i++) {
      const row = table.rows[i];
      const rowData = [];
      for (let j = 0; j < row.cells.length; j++) {
        const cellContent = getCellContent(row.cells[j]);
        rowData.push(cellContent);
      }
      tableData.push(rowData);
    }


    var inputData = {
      itemId: itemId,
      stockUOMItems: tableData
    }

    $.ajax({
      type: 'POST',
      url: "/StockUOM/SaveStockUOMItems",
      data: inputData,
      success: function () {
        modal.hide();
        location.reload();
      },
      error: function () {
        alert('Error fetching stocks.');
      }
    });
  }
}

function getCellContent(cell) {
  const select = cell.querySelector("select");
  if (select) {
    return select.value;
  }
  const input = cell.querySelector("input");
  if (input) {
    return input.value;
  }
  return cell.textContent;
}

function clearStockUOMItems() {
  $('#stockUOMItemsListId').empty();
}

function closeStockUOMItemsModal() {
  $('#stockUOMItemsModal').hide();
}

