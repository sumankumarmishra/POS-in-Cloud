
// js functions for Inventory Issues

const tBody = $('#inventoryIssueDetailsBodyId');

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

  // select location
  var selectLocation = $('<select>').css(leftColumnStyles);
  $.ajax({
    url: "/ICIssue/GetLocations",
    success: function (locations) {
      locations.forEach(location => {
        $("<option>").val(location.locCde).text(location.locCde).appendTo(selectLocation);
      });
    }
  });
  newRow.append($('<td>').css('padding', '0px').append(selectLocation));

  //select ItemId
  var selectItemId = $('<select>').css(leftColumnStyles);
  $.ajax({
    url: "/ICIssue/GetStocks",
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
      url: "/ICIssue/GetStocksByItemId",
      data: inputData,
      success: function (stock) {
        inputItemDesc.val(stock.itemDesc);
        $.ajax({
          url: "/ICIssue/GetStockUOMs",
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
      url: "/ICIssue/GetStockUOMsByUOMCde",
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

  //input discount
  var inputDiscount = $('<input>').attr('type', 'number').css(columnStyles).val(0);
  inputDiscount.on('input', function () {
    tdAmount.text(calculateAmount());
    calculateTotalAmount();
  })
  newRow.append($('<td>').css('padding', '0px').append(inputDiscount));

  const calculateAmount = () => {
    return ((parseFloat(inputQty.val()) * parseFloat(inputUnitCost.val())) - parseFloat(inputDiscount.val())).toLocaleString();
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

function updateInventoryIssue() {
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
    const table = document.getElementById('inventoryIssueDetailTable');

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
        url: '/ICIssue/AddInventoryIssueDetails',
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
        url: '/ICIssue/UpdateInventoryIssueDetails',
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
  var numberColumnCells = $('#inventoryIssueDetailsBodyId tr td:first-child');
  numberColumnCells.each(function (index) {
    $(this).text(index + 1);
  });

}

//Edit InventoryIssue
function editInventoryIssue(icmoveId) {

  clearScreen();
  $('#btnPrintReview').show();
  $('#btnDelete').show();

  var inputData = {
    icMoveId: icmoveId
  };

  $.ajax({
    type: 'GET',
    url: '/ICIssue/FindICIssueDetails',
    data: inputData,
    success: function (list) {
      list.forEach(function (icIssueDetail) {
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
        }).text(icIssueDetail.ordNo));

        // select location
        var selectLocation = $('<select>').css(leftColumnStyles);
        $.ajax({
          url: "/ICIssue/GetLocations",
          success: function (locations) {
            locations.forEach(location => {
              $("<option>").val(location.locCde).text(location.locCde).appendTo(selectLocation);
            });
            selectLocation.val(icIssueDetail.fromLoc);
          },
          error: function () {
            alert('error');
          }
        });
        newRow.append($('<td>').css('padding', '0px').append(selectLocation));

        //select ItemId
        var selectItemId = $('<select>').css(leftColumnStyles);
        $.ajax({
          url: "/ICIssue/GetStocks",
          success: function (stocks) {
            var fragment = document.createDocumentFragment();
            stocks.forEach(stock => {
              $("<option>").val(stock.itemId).text(stock.itemId).appendTo(fragment);
            });
            selectItemId.append(fragment);
            selectItemId.val(icIssueDetail.itemId);
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
            url: "/ICIssue/GetStocksByItemId",
            data: inputData,
            success: function (stock) {
              inputItemDesc.val(stock.itemDesc);
              $.ajax({
                url: "/ICIssue/GetStockUOMs",
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
        var inputItemDesc = $('<input>').css(leftColumnStyles).val(icIssueDetail.itemDesc);
        newRow.append($('<td>').css('padding', '0px').append(inputItemDesc));

        //select UOM
        var selectUOM = $('<select>').css(leftColumnStyles);
        var inputData = {
          itemId: icIssueDetail.itemId
        }
        $.ajax({
          url: "/ICIssue/GetStockUOMs",
          data: inputData,
          dataType: "html",
          success: function (uoms) {
            var parsedUOM = JSON.parse(uoms);
            parsedUOM.forEach(uom => {
              $("<option>").val(uom.uomCde).text(uom.uomCde).appendTo(selectUOM);
            });
            selectUOM.val(icIssueDetail.uom);
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
            url: "/ICIssue/GetStockUOMsByUOMCde",
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
        var tdRate = $('<td>').text(icIssueDetail.uomRate).css('padding', '5px');
        newRow.append(tdRate);

        //input Qty
        var inputQty = $('<input>').attr('type', 'number').css(columnStyles).val(-1 * parseFloat(icIssueDetail.qty)); // show back + if qty is -
        inputQty.on('input', function () {
          tdAmount.text(calculateAmount());
          calculateTotalAmount();
        })
        newRow.append($('<td>').css('padding', '0px').append(inputQty));

        //input unitCost
        var inputUnitCost = $('<input>').attr('type', 'number').css(columnStyles).val(icIssueDetail.unitCost);
        inputUnitCost.on('input', function () {
          tdAmount.text(calculateAmount());
          calculateTotalAmount();
        })
        newRow.append($('<td>').css('padding', '0px').append(inputUnitCost));

        //input discount
        var inputDiscount = $('<input>').attr('type', 'number').css(columnStyles).val(icIssueDetail.discAmt);
        inputDiscount.on('input', function () {
          tdAmount.text(calculateAmount());
          calculateTotalAmount();
        })
        newRow.append($('<td>').css('padding', '0px').append(inputDiscount));

        const calculateAmount = () => {
          return ((parseFloat(inputQty.val()) * parseFloat(inputUnitCost.val())) - parseFloat(inputDiscount.val())).toLocaleString();
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
      findInventoryIssueH(icmoveId);
      calculateTotalAmount();
    },
    error: function (error) {
      alert('Error occured.');
    }
  });
}

function findInventoryIssueH(icmoveId) {
  var inputData = {
    icMoveId: icmoveId
  };

  $.ajax({
    url: "/ICIssue/FindICIssueH",
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
function deleteInventoryIssue() {
  var icMoveId = $("#inputIcMoveId").val();
  var inputRefNo = $('#inputRefNo').val();
  const shouldProceed = window.confirm("Are you sure you want to delete " + inputRefNo + " ?");
  if (shouldProceed) {
    $.ajax({
      type: 'POST',
      url: '/ICIssue/DeleteICIssueDetails',
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
      url: '/ICIssue/PrintReview',
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
  var lastRow = $('#inventoryIssueDetailsBodyId tr:last td:first-child');
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
  const tableCells = $('#inventoryIssueDetailTable tr td:last-child');
  tableCells.each(function (index) {
    billTotal = parseFloat(billTotal) + parseFloat($(this).text().replace(',', ''));
  })
  $('#inputBillAmt').val(billTotal.toLocaleString());
}

function clearScreen() {
  $('#inputIcMoveId').val('');
  $('#inventoryIssueDetailsBodyId').empty();
}

function refresh() {
  location.reload();
}
