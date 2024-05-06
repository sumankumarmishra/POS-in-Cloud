
// js functions for GoodReceive

const tBody = $('#goodReceiveDetailsBodyId');
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
    url: "/GoodReceive/GetLocations",
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
    url: "/GoodReceive/GetStocksByApid",
    data: { apId: $('#selectApId').val() },
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
      url: "/GoodReceive/GetStocksByItemId",
      data: inputData,
      success: function (stock) {
        inputItemDesc.val(stock.itemDesc);
        $.ajax({
          url: "/GoodReceive/GetStockUOMs",
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
      url: "/GoodReceive/GetStockUOMsByUOMCde",
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
    var amount = parseFloat(inputQty.val()) * parseFloat(inputUnitCost.val()) - parseFloat(inputDiscount.val());
    return amount.toLocaleString();
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

function updateGoodReceive() {
  const shouldProceed = window.confirm("Are you sure of saving record?");
  if (shouldProceed) {
    var checkedCancelFlg = $('#checkboxCancelFlg:checked').length > 0;
    var formData = {
      refNo: $('#inputRefNo').val(),
      cancelFlg: checkedCancelFlg.toString(),
      tranDte: $('#inputTranDte').val(),
      refNo2: $('#inputRefNo2').val(),
      depositDte: $('#inputDepositDte').val(),
      apId: $('#selectApId').val(),
      arapDesc: $('#inputArapDesc').val(),
      depositAmt: $('#inputDepositAmt').val(),
      tradeCurrCde: $('#selectTradeCurrCde').val(),
      billTerm: $('#inputBillTerm').val(),
      billTermDay: $('#inputBillTermDay').val(),
      otherChrgAmt: $('#inputOtherChrgAmt').val(),
      remark: $('#textareaRemark').val(),
      tenderCde: $('#selectTenderCde').val(),
      billDiscAmt: $('#inputBillDiscAmt').val(),
      billAmt: $('#inputBillAmt').val()
    };
    const tableData = [];
    const table = document.getElementById('goodReceiveDetailTable');

    for (let i = 1; i < table.rows.length; i++) {
      const row = table.rows[i];
      const rowData = [];
      for (let j = 0; j < row.cells.length; j++) {
        const cellContent = getCellContent(row.cells[j]);
        rowData.push(cellContent);
      }
      tableData.push(rowData);
    }
    var arapId = $('#inputArapId').val();
    if (arapId === '') {
      const allData = {
        TableData: tableData,
        FormData: formData
      };

      $.ajax({
        type: 'POST',
        url: '/GoodReceive/AddGoodReceiveDetails',
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
        arapId: arapId
      };

      $.ajax({
        type: 'POST',
        url: '/GoodReceive/UpdateGoodReceiveDetails',
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
  var numberColumnCells = $('#goodReceiveDetailsBodyId tr td:first-child');
  numberColumnCells.each(function (index) {
    $(this).text(index + 1);
  });

}

function changeItemId() {

  var arapid = $('#inputArapId').val();
  if (arapid != "") {
    var apid = $('#selectApId').val();
    editGoodReceive(arapid, apid);
  }
  else {
    clearScreen();
  }

}

//Edit GoodReceive
function editGoodReceive(arapid, apId) {

  if (apId === 0) {
    clearScreen();
    findGoodReceiveH(arapid);
  }

  $('#goodReceiveDetailsBodyId').empty();
  $('#btnPrintReview').show();
  $('#btnDelete').show();

  var inputData = {
    arapId: arapid
  }

  $.ajax({
    type: 'GET',
    url: '/GoodReceive/FindGoodReceiveDetails',
    data: inputData,
    success: function (list) {
      list.forEach(function (goodReceiveDetail) {
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
        }).text(goodReceiveDetail.ordNo));

        // select location
        var selectLocation = $('<select>').css(leftColumnStyles);
        $.ajax({
          url: "/GoodReceive/GetLocations",
          success: function (locations) {
            locations.forEach(location => {
              $("<option>").val(location.locCde).text(location.locCde).appendTo(selectLocation);
            });
            selectLocation.val(goodReceiveDetail.fromLoc);
          },
          error: function () {
            alert('error');
          }
        });
        newRow.append($('<td>').css('padding', '0px').append(selectLocation));

        //select ItemId
        var selectItemId = $('<select>').css(leftColumnStyles);

        if (apId != 0) { // Check ApId is there or not, apid is there it is not first click
          $.ajax({
            url: "/GoodReceive/GetStocksByApId",
            data: { apId: apId },
            success: function (stocks) {
              var fragment = document.createDocumentFragment();
              stocks.forEach(stock => {
                $("<option>").val(stock.itemId).text(stock.itemId).appendTo(fragment);
              });
              selectItemId.append(fragment);
              selectItemId.val(goodReceiveDetail.itemId);
            },
            error: function () {
              alert('error');
            }
          });
        }
        else {
          $.ajax({
            url: "/GoodReceive/GetStocksByArapId",
            data: { arapId: arapid },
            success: function (stocks) {
              var fragment = document.createDocumentFragment();
              stocks.forEach(stock => {
                $("<option>").val(stock.itemId).text(stock.itemId).appendTo(fragment);
              });
              selectItemId.append(fragment);
              selectItemId.val(goodReceiveDetail.itemId);
            },
            error: function () {
              alert('error');
            }
          });
        }

        selectItemId.on('change', function () {
          var inputData = {
            itemId: this.value
          }
          $.ajax({
            url: "/GoodReceive/GetStocksByItemId",
            data: inputData,
            success: function (stock) {
              inputItemDesc.val(stock.itemDesc);
              $.ajax({
                url: "/GoodReceive/GetStockUOMs",
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
        var inputItemDesc = $('<input>').css(leftColumnStyles).val(goodReceiveDetail.itemDesc);
        newRow.append($('<td>').css('padding', '0px').append(inputItemDesc));

        //select UOM
        var selectUOM = $('<select>').css(leftColumnStyles);
        var inputData = {
          itemId: goodReceiveDetail.itemId
        }
        $.ajax({
          url: "/GoodReceive/GetStockUOMs",
          data: inputData,
          dataType: "html",
          success: function (uoms) {
            var parsedUOM = JSON.parse(uoms);
            parsedUOM.forEach(uom => {
              $("<option>").val(uom.uomCde).text(uom.uomCde).appendTo(selectUOM);
            });
            selectUOM.val(goodReceiveDetail.uom);
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
            url: "/GoodReceive/GetStockUOMsByUOMCde",
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
        var tdRate = $('<td>').text(goodReceiveDetail.uomRate).css('padding', '5px');
        newRow.append(tdRate);

        //input Qty
        var inputQty = $('<input>').attr('type', 'number').css(columnStyles).val(goodReceiveDetail.qty);
        inputQty.on('input', function () {
          tdAmount.text(calculateAmount());
          calculateTotalAmount();
        })
        newRow.append($('<td>').css('padding', '0px').append(inputQty));

        //input unitCost
        var inputUnitCost = $('<input>').attr('type', 'number').css(columnStyles).val(goodReceiveDetail.unitCost);
        inputUnitCost.on('input', function () {
          tdAmount.text(calculateAmount());
          calculateTotalAmount();
        })
        newRow.append($('<td>').css('padding', '0px').append(inputUnitCost));

        //input discount
        var inputDiscount = $('<input>').attr('type', 'number').css(columnStyles).val(goodReceiveDetail.discAmt);
        inputDiscount.on('input', function () {
          tdAmount.text(calculateAmount());
          calculateTotalAmount();
        })
        newRow.append($('<td>').css('padding', '0px').append(inputDiscount));

        const calculateAmount = () => {
          var amount = parseFloat(inputQty.val()) * parseFloat(inputUnitCost.val()) - parseFloat(inputDiscount.val());
          return amount.toLocaleString();
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

    },
    error: function (error) {
      alert('Error occured.');
    }
  });
}

function findGoodReceiveH(arapid) {
  var inputData = {
    arapId: arapid
  };

  $.ajax({
    url: "/GoodReceive/FindGoodReceiveH",
    data: inputData,
    success: function (gRH) {
      $('#inputArapId').val(gRH.arapId);
      $('#inputRefNo').val(gRH.icRefNo);
      $('#checkboxCancelFlg').prop('checked', gRH.cancelFlg);
      var formattedTranDate = gRH.tranDte.split('T')[0];
      $('#inputTranDte').val(formattedTranDate);
      $('#inputRefNo2').val(gRH.refNo2);
      var formattedTranDate = gRH.depositDte.split('T')[0];
      $('#inputDepositDte').val(formattedTranDate);
      $('#selectApId').val(gRH.apId);
      $('#inputArapDesc').val(gRH.arapDesc);
      $('#inputDepositAmt').val(gRH.depositAmt);
      $('#selectTradeCurrCde').val(gRH.tradeCurrCde);
      $('#inputBillTerm').val(gRH.billTerm);
      $('#inputBillTermDay').val(gRH.billTermDay);
      $('#inputOtherChrgAmt').val(gRH.otherChrgAmt);
      $('#textareaRemark').val(gRH.remark);
      $('#selectTenderCde').val(gRH.tenderCde);
      $('#inputBillDiscAmt').val(gRH.billDiscAmt);
      $('#inputBillAmt').val(gRH.billAmt.toLocaleString());
      $('#inputPrintRefNo').val(gRH.icRefNo);
    },
    error: function () {
      alert('error');
    }
  });
}

//Delete
function deleteGoodReceive() {
  var arapId = $("#inputArapId").val();

  if (arapId === '' || arapId === null) {
    alert('choose saved data first');
  }
  else {
    var inputRefNo = $('#inputRefNo').val();
    const shouldProceed = window.confirm("Are you sure you want to delete " + inputRefNo + " ?");
    if (shouldProceed) {
      $.ajax({
        type: 'POST',
        url: '/GoodReceive/DeleteGoodReceiveDetails',
        data: { arapId: arapId },
        success: function (status) {
          refresh();
        },
        error: function (error) {
          alert('Error occured.');
        }
      });
    }
  }
}

//Print
function printReview() {
  var arapId = $('#inputArapId').val();
  if (arapId === '') {
    alert('choose saved data first');
  }
  else {
    $.ajax({
      type: 'POST',
      url: '/GoodReceive/PrintReview',
      data: {
        refNo: $('#inputRefNo').val()
      },
      success: function (htmlData) {
        var newWindow = window.open(''); // open new empty window for bill slip
        newWindow.document.write(htmlData); // write return htmlcontent in the new empty window
      },
      error: function () {
        alert('An error occurred while generating the report. Please try again later.');
      }
    });
  }
}

//Common Function
function lastNumber() { // To check what is last number of No. column in table
  var lastRow = $('#goodReceiveDetailsBodyId tr:last td:first-child');
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
  const tableCells = $('#goodReceiveDetailTable tr td:last-child');
  tableCells.each(function (index) {
    var amount = $(this).text();
    billTotal = parseFloat(billTotal) + parseFloat(amount.replace(',', ''));

  })
  var realBillTotal = parseFloat(billTotal) - parseFloat($('#inputBillDiscAmt').val());
  $('#inputBillAmt').val(realBillTotal.toLocaleString());
}

function calculateDiscount() {
  const billDiscPercField = $('#inputBillDiscPerc');
  const billDisField = $('#inputBillDiscAmt');
  var billTotal = 0;
  const tableCells = $('#goodReceiveDetailTable tr td:last-child');
  tableCells.each(function (index) {
    billTotal = parseFloat(billTotal) + parseFloat($(this).text().replace(',', ''));
  })
  var billDiscount = (parseFloat(billTotal) * parseFloat(billDiscPercField.val())) / 100;
  billDisField.val(parseFloat(billDiscount));
  calculateTotalAmount();
}

function clearScreen() {
  $('#inputArapId').val('');
  $('#inputPrintRefNo').val('');
  $('#goodReceiveDetailsBodyId').empty();
  $('#inputBillDiscPerc').val('');
}

function refresh() {
  location.reload();
}
