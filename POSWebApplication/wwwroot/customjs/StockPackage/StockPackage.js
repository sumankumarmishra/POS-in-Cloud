// js functions for Stock Package

function callAddStockPackageController() {
  var loadingScreen = document.getElementById('loadingTextId');
  loadingScreen.style.display = "block";

  $.ajax({
    url: "/StockPackage/AddStockPackagePartial",
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

function callEditStockPackageController(pkgHId) {
  var loadingScreen = document.getElementById('loadingTextId');
  loadingScreen.style.display = "block";

  var inputData = {
    pkgHId: pkgHId
  };

  $.ajax({
    url: "/StockPackage/EditStockPackagePartial",
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

function callDeleteStockPackageController(pkgHId) {
  var loadingScreen = document.getElementById('loadingTextId');
  loadingScreen.style.display = "block";

  var inputData = {
    pkgHId: pkgHId
  };

  $.ajax({
    url: "/StockPackage/DeleteStockPackagePartial",
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

function addPackageItems() {
  const tBody = $('#packageItemsListId');
  const modal = $('#packageItemsModal');

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
    url: "/StockPackage/GetStocks",
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
      url: "/StockPackage/GetStockById",
      data: { itemId: this.value },
      success: function (stock) {
        inputItemDesc.val(stock.itemDesc);
      },
      error: function () {
        alert('Error fetching Stocks.');
      }
    });

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

  //input itemDesc
  var inputItemDesc = $('<input>').css(leftColumnStyles);
  newRow.append($('<td>').css('padding', '0px').append(inputItemDesc));

  //select UOM

  var selectUOM = $('<select>').css(leftColumnStyles);

  $.ajax({
    url: "/GoodReceive/GetStockUOMs",
    data: { itemId: selectItemId.val() },
    success: function (uoms) {
      uoms.forEach(uom => {
        $("<option>").val(uom.uomCde).text(uom.uomCde).appendTo(selectUOM);
      });
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
      addPackageItems();
    }
  });


  newRow.on('keydown', function (event) {
    if (event.keyCode === 46) {
      newRow.remove();
    }
  });
  tBody.append(newRow);
}

function editPackageItems(pkgHId, pkgNme) {
  clearPackageItems();

  const tBody = $('#packageItemsListId');
  const modal = $('#packageItemsModal');

  var inputData = { pkgHId: pkgHId };

  $.ajax({
    type: 'GET',
    url: '/StockPackage/GetPackageItemsList',
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
          url: "/StockPackage/GetStocks",
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

          $.ajax({
            url: "/StockPackage/GetStockById",
            data: { itemId: this.value },
            success: function (stock) {
              inputItemDesc.val(stock.itemDesc);
            },
            error: function () {
              alert('Error fetching itemDesc.');
            }
          });
        });

        newRow.append($('<td>').css('padding', '0px').append(selectItemId));

        //input itemDesc

        var inputItemDesc = $('<input>').css(leftColumnStyles).val(item.itemDesc);
        newRow.append($('<td>').css('padding', '0px').append(inputItemDesc));


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
            addPackageItems();
          }
        });

        newRow.on('keydown', function (event) {
          if (event.keyCode === 46) {
            newRow.remove();
          }
        });
        tBody.append(newRow);

      });
      $('#spanId').text(pkgNme);
      $('#hiddenId').text(pkgHId);
      modal.show();
    },
    error: function () {
      alert('Error occured.');
    }
  });


}

function savePackageItems() {

  const modal = $('#packageItemsModal');
  const shouldProceed = window.confirm("Are you sure of saving record?");

  if (shouldProceed) {
    const pkgHId = $('#hiddenId').text();

    const tableData = [];
    const table = document.getElementById('packageItemsTable');

    for (let i = 1; i < table.rows.length; i++) {
      const row = table.rows[i];
      const rowData = [];
      for (let j = 0; j < row.cells.length; j++) {
        const cellContent = getCellContent(row.cells[j]);
        rowData.push(cellContent);
      }
      tableData.push(rowData);
    }

    console.log(tableData);

    var inputData = {
      pkgHId: pkgHId,
      packageItems: tableData
    }

    $.ajax({
      type: 'POST',
      url: "/StockPackage/SavePackageItems",
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

function clearPackageItems() {
  $('#packageItemsListId').empty();
}

function closePackageItemModal() {
  $('#packageItemsModal').hide();
}


