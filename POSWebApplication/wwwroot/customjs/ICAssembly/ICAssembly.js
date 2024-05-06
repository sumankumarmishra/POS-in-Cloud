
// js functions for Inventory Assembly

const tBody = $('#inventoryAssemblyDetailsBodyId');

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

  // select Issue location
  var selectIssueLocation = $('<select>').css(leftColumnStyles);
  $.ajax({
    url: "/ICAssembly/GetLocations",
    success: function (locations) {
      locations.forEach(location => {
        $("<option>").val(location.locCde).text(location.locCde).appendTo(selectIssueLocation);
      });
    }
  });
  newRow.append($('<td>').css('padding', '0px').append(selectIssueLocation));

  // select Receive location
  var selectReceiveLocation = $('<select>').css(leftColumnStyles);
  $.ajax({
    url: "/ICAssembly/GetLocations",
    success: function (locations) {
      locations.forEach(location => {
        $("<option>").val(location.locCde).text(location.locCde).appendTo(selectReceiveLocation);
      });
    }
  });
  newRow.append($('<td>').css('padding', '0px').append(selectReceiveLocation));

  //select ItemId
  var selectItemId = $('<select>').css(leftColumnStyles);
  $.ajax({
    url: "/ICAssembly/GetStocks",
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
      url: "/ICAssembly/GetStocksByItemId",
      data: inputData,
      success: function (stock) {
        inputItemDesc.val(stock.itemDesc);
        $.ajax({
          url: "/ICAssembly/GetStockUOMs",
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
      url: "/ICAssembly/GetStockUOMsByUOMCde",
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

function updateInventoryAssembly() {
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
    const table = document.getElementById('inventoryAssemblyDetailTable');

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
        url: '/ICAssembly/AddInventoryAssemblyDetails',
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
        ASMRefNo: $('#inputRefNo').val()
      };

      $.ajax({
        type: 'POST',
        url: '/ICAssembly/UpdateInventoryAssemblyDetails',
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
  var numberColumnCells = $('#inventoryAssemblyDetailsBodyId tr td:first-child');
  numberColumnCells.each(function (index) {
    $(this).text(index + 1);
  });

}

//Edit Inventory Assembly
function editInventoryAssembly(icmoveId) {
  clearScreen();
  $('#btnPrintReview').show();

  var inputData = {
    icMoveId: icmoveId
  };

  $.ajax({
    type: 'GET',
    url: '/ICAssembly/FindICAssemblyDetails',
    data: inputData,
    success: function (list) {
      list.forEach(function (icAssemblyDetail) {
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
        }).text(icAssemblyDetail.ordNo));

        // select issue location
        var selectIssueLocation = $('<select>').css(leftColumnStyles);
        $.ajax({
          url: "/ICAssembly/GetLocations",
          success: function (locations) {
            locations.forEach(location => {
              $("<option>").val(location.locCde).text(location.locCde).appendTo(selectIssueLocation);
            });
            selectIssueLocation.val(icAssemblyDetail.fromLoc);
          },
          error: function () {
            alert('error');
          }
        });
        newRow.append($('<td>').css('padding', '0px').append(selectIssueLocation));

        // select receive location
        var selectReceiveLocation = $('<select>').css(leftColumnStyles);
        $.ajax({
          url: "/ICAssembly/GetLocations",
          success: function (locations) {
            locations.forEach(location => {
              $("<option>").val(location.locCde).text(location.locCde).appendTo(selectReceiveLocation);
            });
            selectReceiveLocation.val(icAssemblyDetail.toLoc);
          },
          error: function () {
            alert('error');
          }
        });
        newRow.append($('<td>').css('padding', '0px').append(selectReceiveLocation));

        //select ItemId
        var selectItemId = $('<select>').css(leftColumnStyles);
        $.ajax({
          url: "/ICAssembly/GetStocks",
          success: function (stocks) {
            var fragment = document.createDocumentFragment();
            stocks.forEach(stock => {
              $("<option>").val(stock.itemId).text(stock.itemId).appendTo(fragment);
            });
            selectItemId.append(fragment);
            selectItemId.val(icAssemblyDetail.itemId);
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
            url: "/ICAssembly/GetStocksByItemId",
            data: inputData,
            success: function (stock) {
              inputItemDesc.val(stock.itemDesc);
              $.ajax({
                url: "/ICAssembly/GetStockUOMs",
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
        var inputItemDesc = $('<input>').css(leftColumnStyles).val(icAssemblyDetail.itemDesc);
        newRow.append($('<td>').css('padding', '0px').append(inputItemDesc));

        //select UOM
        var selectUOM = $('<select>').css(leftColumnStyles);
        var inputData = {
          itemId: icAssemblyDetail.itemId
        }
        $.ajax({
          url: "/ICAssembly/GetStockUOMs",
          data: inputData,
          dataType: "html",
          success: function (uoms) {
            var parsedUOM = JSON.parse(uoms);
            parsedUOM.forEach(uom => {
              $("<option>").val(uom.uomCde).text(uom.uomCde).appendTo(selectUOM);
            });
            selectUOM.val(icAssemblyDetail.uom);
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
            url: "/ICAssembly/GetStockUOMsByUOMCde",
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
        var tdRate = $('<td>').text(icAssemblyDetail.uomRate).css('padding', '5px');
        newRow.append(tdRate);

        //input Qty
        var inputQty = $('<input>').attr('type', 'number').css(columnStyles).val(parseFloat(icAssemblyDetail.qty));
        inputQty.on('input', function () {
          tdAmount.text(calculateAmount());
          calculateTotalAmount();
        })
        newRow.append($('<td>').css('padding', '0px').append(inputQty));

        //input unitCost
        var inputUnitCost = $('<input>').attr('type', 'number').css(columnStyles).val(icAssemblyDetail.unitCost);
        inputUnitCost.on('input', function () {
          tdAmount.text(calculateAmount());
          calculateTotalAmount();
        })
        newRow.append($('<td>').css('padding', '0px').append(inputUnitCost));

        //input discount
        var inputDiscount = $('<input>').attr('type', 'number').css(columnStyles).val(icAssemblyDetail.discAmt);
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
      findInventoryAssemblyH(icmoveId);
      calculateTotalAmount();
    },
    error: function (error) {
      alert('Error occured.');
    }
  });
}

function findInventoryAssemblyH(icmoveId) {
  var inputData = {
    icMoveId: icmoveId
  };

  $.ajax({
    url: "/ICAssembly/FindICAssemblyH",
    data: inputData,
    success: function (gRH) {
      $('#inputIcMoveId').val(gRH.icMoveId);
      $('#inputRefNo').val(gRH.icRefNo2);
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

//Print
function printReview() {
  var arapId = $('#inputIcMoveId').val();
  if (arapId === '') {
    alert('choose saved data first');
  }
  else {
    $.ajax({
      type: 'POST',
      url: '/ICAssembly/PrintReview',
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
  var lastRow = $('#inventoryAssemblyDetailsBodyId tr:last td:first-child');
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
  const tableCells = $('#inventoryAssemblyDetailTable tr td:last-child');
  tableCells.each(function (index) {
    billTotal = parseFloat(billTotal) + parseFloat($(this).text().replace(',',''));
  })
  $('#inputBillAmt').val(billTotal.toLocaleString());
}

function clearScreen() {
  $('#inputIcMoveId').val('');
  $('#inventoryAssemblyDetailsBodyId').empty();
}

function refresh() {
  location.reload();
}
