function callAddSupplierController() {
  $.ajax({
    url: "/Supplier/AddSupplierPartial",
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

function callEditSupplierController(ApId) {
  var inputData = {
    apId: ApId
  };

  $.ajax({
    url: "/Supplier/EditSupplierPartial",
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

function callDeleteSupplierController(ApId) {
  var inputData = {
    apId: ApId
  };

  $.ajax({
    url: "/Supplier/DeleteSupplierPartial",
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

function addSupplierItem() {
  const tBody = $('#supplierItemsListId');
  const modal = $('#supplierItemsModal');

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

  //select ItemId

  var selectItemId = $('<select>').css(leftColumnStyles);

  $.ajax({
    url: "/Supplier/GetStocks",
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
      url: "/Supplier/GetStockById",
      data: { itemId: this.value },
      success: function (stock) {
        tdItemDesc.text(stock.itemDesc);
      },
      error: function () {
        alert('Error fetching Stocks.');
      }
    });
  });

  newRow.append($('<td>').css('padding', '0px').append(selectItemId));

  //td itemDesc
  var tdItemDesc = $('<td>').css({ 'padding': '5px', 'textAlign': 'left' });
  newRow.append(tdItemDesc.text(''));

  newRow.on('keypress', function (event) {
    if (event.keyCode === 13) {
      addSupplierItem();
    }
  });

  newRow.on('keydown', function (event) {
    if (event.keyCode === 46) {
      newRow.remove();
    }
  });
  tBody.append(newRow);
}

function editSupplierItems(apId, apNme) {
  clearSupplierItems();

  const tBody = $('#supplierItemsListId');
  const modal = $('#supplierItemsModal');

  var inputData = { apId: apId };

  $.ajax({
    type: 'GET',
    url: '/Supplier/GetSupplierItemsList',
    data: inputData,
    success: function (list) {

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

      list.forEach(function (item) {
        const newRow = $('<tr>').css({ 'font-size': '14px', 'text-align': 'center' });

        //select ItemId

        var selectItemId = $('<select>').css(leftColumnStyles);

        $.ajax({
          url: "/Supplier/GetStocks",
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
            url: "/Supplier/GetStockById",
            data: { itemId: this.value },
            success: function (stock) {
              tdItemDesc.text(stock.itemDesc);
            },
            error: function () {
              alert('Error fetching itemDesc.');
            }
          });
        });

        newRow.append($('<td>').css('padding', '0px').append(selectItemId));

        //text itemDesc
        var tdItemDesc = $('<td>').css({ 'padding': '5px', 'textAlign': 'left' });
        newRow.append(tdItemDesc.text(item.itemDesc));

        newRow.on('keypress', function (event) {
          if (event.keyCode === 13) {
            addSupplierItem();
          }
        });

        newRow.on('keydown', function (event) {
          if (event.keyCode === 46) {
            newRow.remove();
          }
        });
        tBody.append(newRow);

      });
      $('#spanId').text(apNme);
      console.log(apId);
      $('#hiddenId').text(apId);
      modal.show();
    },
    error: function () {
      alert('Error occured.');
    }
  });


}

function saveSupplierItems() {

  const modal = $('#supplierItemsModal');
  const shouldProceed = window.confirm("Are you sure of saving record?");

  if (shouldProceed) {

    const apId = $('#hiddenId').text();
    const tableData = [];
    const table = document.getElementById('supplierItemsTable');

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
      apId: apId,
      supplierItems: tableData
    }

    $.ajax({
      type: 'POST',
      url: "/Supplier/SaveSupplierItems",
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

function clearSupplierItems() {
  $('#supplierItemsListId').empty();
}

function closeSupplierItemModal() {
  $('#supplierItemsModal').hide();
}
