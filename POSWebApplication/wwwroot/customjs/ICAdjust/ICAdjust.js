
// js functions for Inventory Adjust

const tBody = $('#inventoryAdjustDetailsBodyId');

function addNewDetailsRow() {
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
    textAlign: 'left'
  };

  // auto NO.
  var number = lastNumber();
  newRow.append($('<td>').css('padding', '5px').text(number));

  // select Adjust
  var selectAdjust = $('<select>').css(columnStyles);
  $("<option>").val('+').text('+').appendTo(selectAdjust);
  $("<option>").val('-').text('-').appendTo(selectAdjust);
  newRow.append($('<td>').css('padding', '0px').append(selectAdjust));

  // select Location
  var selectFromLocation = $('<select>').css(leftColumnStyles);
  $.ajax({
    url: "/ICAdjust/GetLocations",
    success: function (locations) {
      locations.forEach(location => {
        $("<option>").val(location.locCde).text(location.locCde).appendTo(selectFromLocation);
      });
    }
  });
  newRow.append($('<td>').css('padding', '0px').append(selectFromLocation));

  //select ItemId
  var selectItemId = $('<select>').css(leftColumnStyles);
  $.ajax({
    url: "/ICAdjust/GetStocks",
    success: function (stocks) {
      var fragment = document.createDocumentFragment();
      stocks.forEach(stock => {
        $("<option>").val(stock.itemId).text(stock.itemId).appendTo(fragment);
      });
      selectItemId.append(fragment);
    }
  });
  selectItemId.on('change', function () {
    var inputData = {
      itemId: this.value
    }
    $.ajax({
      url: "/ICAdjust/GetStocksByItemId",
      data: inputData,
      success: function (stock) {
        inputItemDesc.val(stock.itemDesc);
        $.ajax({
          url: "/ICAdjust/GetStockUOMs",
          data: inputData,
          success: function (uoms) {
            selectUOM.find("option").remove();
            uoms.forEach(uom => {
              $("<option>").val(uom.uomCde).text(uom.uomCde).appendTo(selectUOM);
            });
          },
        });
        tdRate.text(1);
        inputQty.val(1);
        inputUnitCost.val(stock.unitCost);
        tdAmount.text(calculateAmount());
        calculateTotalAmount();
      },
      error: function () {
        alert('error');
      }
    });
  });
  newRow.append($('<td>').css('padding', '0px').append(selectItemId));

  //input itemDesc
  var inputItemDesc = $('<input>').css(leftColumnStyles);
  newRow.append($('<td>').css('padding', '0px').append(inputItemDesc));

  //select UOM
  var selectUOM = $('<select>').css(leftColumnStyles);
  selectUOM.on('change', function () {
    var inputData = {
      itemId: selectItemId.val(),
      uomCde: this.value
    }
    $.ajax({
      url: "/ICAdjust/GetStockUOMsByUOMCde",
      data: inputData,
      success: function (uom) {
        inputUnitCost.val(uom.unitCost);
        tdAmount.text(calculateAmount());
        calculateTotalAmount();
      },
    });
  })
  newRow.append($('<td>').css('padding', '0px').append(selectUOM));

  //td rate
  var tdRate = $('<td>').text(0).css('padding', '5px');
  newRow.append(tdRate);

  //input Qty
  var inputQty = $('<input>').attr('type', 'number').css(columnStyles).val(0);
  inputQty.on('input', function () {
    tdAmount.text(calculateAmount());
    calculateTotalAmount();
  })
  newRow.append($('<td>').css('padding', '0px').append(inputQty));

  //input unitCost
  var inputUnitCost = $('<input>').attr('type', 'number').css(columnStyles).val(0);
  inputUnitCost.on('input', function () {
    tdAmount.text(calculateAmount());
    calculateTotalAmount();
  })
  newRow.append($('<td>').css('padding', '0px').append(inputUnitCost));

  const calculateAmount = () => {
    return (parseFloat(inputQty.val()) * parseFloat(inputUnitCost.val())).toLocaleString();
  }

  //td amount
  var result = calculateAmount();
  var tdAmount = $('<td>').text(result).css('padding', '5px')
  newRow.append(tdAmount);

  newRow.on('keypress', function (event) {
    if (event.keyCode === 13) {
      addNewDetailsRow();
      calculateTotalAmount();
    }
  })
  newRow.on('keydown', function (event) {
    if (event.keyCode === 46) {
      newRow.remove();
      changeNumberColumn();
      calculateTotalAmount();
    }
  })
  tBody.append(newRow);
}

function updateInventoryAdjust() {
  const shouldProceed = window.confirm("Are you sure of saving record?");
  var checkedCancelFlg = $('#checkboxCancelFlg:checked').length > 0;
  if (shouldProceed) {
    var formData = {
      refNo: $('#inputRefNo').val(),
      cancelFlg: checkedCancelFlg.toString(),
      tranDte: $('#inputTranDte').val(),
      refNo2: $('#inputRefNo2').val(),
      reasonId: $('#selectReason').val(),
      remark: $('#textareaRemark').val()
    };

    const tableData = [];
    const table = document.getElementById('inventoryAdjustDetailTable');

    for (let i = 1; i < table.rows.length; i++) {
      const row = table.rows[i];
      const rowData = [];
      for (let j = 0; j < row.cells.length; j++) {
        const cellContent = getCellContent(row.cells[j]);
        rowData.push(cellContent);
      }
      tableData.push(rowData);
    }
    var icMoveId = $('#inputIcMoveId').val();
    if (icMoveId === '') {
      const allData = {
        TableData: tableData,
        FormData: formData
      };

      $.ajax({
        type: 'POST',
        url: '/ICAdjust/AddInventoryAdjustDetails',
        data: JSON.stringify(allData),
        contentType: 'application/json',
        success: function (status) {
          refresh();
        },
        error: function (error) {
          alert('Error occured.');
        }
      });
    }
    else {
      const allData = {
        TableData: tableData,
        FormData: formData,
        icMoveId: icMoveId
      };

      $.ajax({
        type: 'POST',
        url: '/ICAdjust/UpdateInventoryAdjustDetails',
        data: JSON.stringify(allData),
        contentType: 'application/json',
        success: function (status) {
          refresh();
        },
        error: function (error) {
          alert('Error occured.');
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
}

function changeNumberColumn() {
  var numberColumnCells = $('#inventoryAdjustDetailsBodyId tr td:first-child');
  numberColumnCells.each(function (index) {
    $(this).text(index + 1);
  });

}

//Edit InventoryTranfer
function editInventoryAdjust(icmoveId) {

  clearScreen();
  $('#btnPrintReview').show();
  $('#btnDelete').show();

  var inputData = {
    icMoveId: icmoveId
  };

  $.ajax({
    type: 'GET',
    url: '/ICAdjust/FindICAdjustDetails',
    data: inputData,
    success: function (list) {
      list.forEach(function (icAdjustDetail) {
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
          textAlign: 'left'
        };

        // auto NO.
        newRow.append($('<td>').css({
          'padding': '5px'
        }).text(icAdjustDetail.ordNo));

        // select Adjust
        var selectAdjust = $('<select>').css(columnStyles);
        $("<option>").val('+').text('+').appendTo(selectAdjust);
        $("<option>").val('-').text('-').appendTo(selectAdjust);
        if (parseFloat(icAdjustDetail.qty) >= 0) {
          selectAdjust.val('+');
        }
        else {
          selectAdjust.val('-');
        }
        newRow.append($('<td>').css('padding', '0px').append(selectAdjust));

        // select Location
        var selectFromLocation = $('<select>').css(leftColumnStyles);
        $.ajax({
          url: "/ICAdjust/GetLocations",
          success: function (locations) {
            locations.forEach(location => {
              $("<option>").val(location.locCde).text(location.locCde).appendTo(selectFromLocation);
            });
            selectFromLocation.val(icAdjustDetail.fromLoc);
          },
          error: function () {
            alert('error');
          }
        });
        newRow.append($('<td>').css('padding', '0px').append(selectFromLocation));

        //select ItemId
        var selectItemId = $('<select>').css(leftColumnStyles);
        $.ajax({
          url: "/ICAdjust/GetStocks",
          success: function (stocks) {
            var fragment = document.createDocumentFragment();
            stocks.forEach(stock => {
              $("<option>").val(stock.itemId).text(stock.itemId).appendTo(fragment);
            });
            selectItemId.append(fragment);
            selectItemId.val(icAdjustDetail.itemId);
          },
          error: function () {
            alert('error');
          }
        });
        selectItemId.on('change', function () {
          var inputData = {
            itemId: this.value
          }
          $.ajax({
            url: "/ICAdjust/GetStocksByItemId",
            data: inputData,
            success: function (stock) {
              inputItemDesc.val(stock.itemDesc);
              $.ajax({
                url: "/ICAdjust/GetStockUOMs",
                data: inputData,
                success: function (uoms) {
                  selectUOM.find("option").remove();
                  uoms.forEach(uom => {
                    $("<option>").val(uom.uomCde).text(uom.uomCde).appendTo(selectUOM);
                  });
                },
              });
              tdRate.text(1);
              inputQty.val(1);
              inputUnitCost.val(stock.unitCost);
              tdAmount.text(calculateAmount());
              calculateTotalAmount();
            },
            error: function () {
              alert('error');
            }
          });
        });
        newRow.append($('<td>').css('padding', '0px').append(selectItemId));

        //input itemDesc
        var inputItemDesc = $('<input>').css(leftColumnStyles).val(icAdjustDetail.itemDesc);
        newRow.append($('<td>').css('padding', '0px').append(inputItemDesc));

        //select UOM
        var selectUOM = $('<select>').css(leftColumnStyles);
        var inputData = {
          itemId: icAdjustDetail.itemId
        }
        $.ajax({
          url: "/ICAdjust/GetStockUOMs",
          data: inputData,
          dataType: "html",
          success: function (uoms) {
            var parsedUOM = JSON.parse(uoms);
            parsedUOM.forEach(uom => {
              $("<option>").val(uom.uomCde).text(uom.uomCde).appendTo(selectUOM);
            });
            selectUOM.val(icAdjustDetail.uom);
          },
          error: function () {
            alert('error');
          }
        });
        selectUOM.on('change', function () {
          var inputData = {
            itemId: selectItemId.val(),
            uomCde: this.value
          }
          $.ajax({
            url: "/ICAdjust/GetStockUOMsByUOMCde",
            data: inputData,
            success: function (uom) {
              inputUnitCost.val(uom.unitCost);
              tdAmount.text(calculateAmount());
              calculateTotalAmount();
            },
          });
        })
        newRow.append($('<td>').css('padding', '0px').append(selectUOM));

        //td rate
        var tdRate = $('<td>').text(icAdjustDetail.uomRate).css('padding', '5px');
        newRow.append(tdRate);

        //input Qty
        if (icAdjustDetail.qty < 0) { //change - to + 
          icAdjustDetail.qty = (-1) * icAdjustDetail.qty;
        }
        var inputQty = $('<input>').attr('type', 'number').css(columnStyles).val(parseFloat(icAdjustDetail.qty));
        inputQty.on('input', function () {
          tdAmount.text(calculateAmount());
          calculateTotalAmount();
        })
        newRow.append($('<td>').css('padding', '0px').append(inputQty));

        //input unitCost
        var inputUnitCost = $('<input>').attr('type', 'number').css(columnStyles).val(icAdjustDetail.unitCost);
        inputUnitCost.on('input', function () {
          tdAmount.text(calculateAmount());
          calculateTotalAmount();
        })
        newRow.append($('<td>').css('padding', '0px').append(inputUnitCost));

        const calculateAmount = () => {
          return (parseFloat(inputQty.val()) * parseFloat(inputUnitCost.val())).toLocaleString();
        }

        //td amount
        var result = calculateAmount();
        var tdAmount = $('<td>').text(result).css('padding', '5px')
        newRow.append(tdAmount);

        newRow.on('keypress', function (event) {
          if (event.keyCode === 13) {
            addNewDetailsRow();
          }
        })
        newRow.on('keydown', function (event) {
          if (event.keyCode === 46) {
            newRow.remove();
            changeNumberColumn();
            calculateTotalAmount();
          }
        })
        tBody.append(newRow);
      });
      findInventoryAdjustH(icmoveId);
      calculateTotalAmount();
    },
    error: function (error) {
      alert('Error occured.');
    }
  });
}

function findInventoryAdjustH(icmoveId) {
  var inputData = {
    icMoveId: icmoveId
  };

  $.ajax({
    url: "/ICAdjust/FindICAdjustH",
    data: inputData,
    success: function (gRH) {
      $('#inputIcMoveId').val(gRH.icMoveId);
      $('#inputRefNo').val(gRH.icRefNo);
      $('#checkboxCancelFlg').prop('checked', gRH.cancelFlg);
      var formattedTranDate = gRH.tranDte.split('T')[0];
      $('#inputTranDte').val(formattedTranDate);
      $('#inputRefNo2').val(gRH.icRefNo2);
      $('#selectReason').val(gRH.reasonId);
      $('#textareaRemark').val(gRH.remark);
    },
    error: function () {
      alert('error');
    }
  });
}

//Delete
function deleteInventoryAdjust() {
  var icMoveId = $("#inputIcMoveId").val();
  var inputRefNo = $('#inputRefNo').val();
  const shouldProceed = window.confirm("Are you sure you want to delete " + inputRefNo + " ?");
  if (shouldProceed) {
    $.ajax({
      type: 'POST',
      url: '/ICAdjust/DeleteICAdjustDetails',
      data: { icMoveId: icMoveId },
      success: function () {
        refresh();
      },
      error: function (error) {
        alert('Error occured.');
      }
    });
  }
}

//Print
function printReview() {
  var arapId = $('#inputIcMoveId').val();
  if (arapId === '') {
    alert('choose saved data first');
  }
  else {
    $.ajax({
      type: 'POST',
      url: '/ICAdjust/PrintReview',
      data: {
        refNo: $('#inputRefNo').val()
      },
      success: function (htmlData) {
        var newWindow = window.open('');
        newWindow.document.write(htmlData);
      },
      error: function () {
        alert('An error occurred while generating the report. Please try again later.');
      }
    });
  }
}

//Common Function
function lastNumber() { // To check what is last number of No. column in table
  var lastRow = $('#inventoryAdjustDetailsBodyId tr:last td:first-child');
  if (lastRow.length <= 0) {
    return 1;
  }
  else {
    if (lastRow.text() == '') {
      return 1;
    }
    return parseFloat(lastRow.text()) + 1;
  }

}

function calculateTotalAmount() {
  var billTotal = 0;
  const tableCells = $('#inventoryAdjustDetailTable tr td:last-child');
  tableCells.each(function () {
    billTotal = parseFloat(billTotal) + parseFloat($(this).text().replace(',', ''));
  })
  $('#inputBillAmt').val(billTotal.toLocaleString());
}

function clearScreen() {
  $('#inputIcMoveId').val('');
  $('#inventoryAdjustDetailsBodyId').empty();
}

function refresh() {
  location.reload();
}
