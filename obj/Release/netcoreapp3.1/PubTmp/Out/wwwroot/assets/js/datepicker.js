$(function() {
  'use strict';

  if($('#datePickerExample').length) {
    var date = new Date();
    var today = new Date(date.getFullYear(), date.getMonth(), date.getDate());
    $('#datePickerExample').datepicker({
      format: "mm/dd/yyyy",
      todayHighlight: true,
      autoclose: true
    });
    $('#datePickerExample').datepicker('setDate', today);
  }

if ($('#datePickerFrom').length) {
    var date = new Date();
    var today = new Date(date.getFullYear(), date.getMonth(), date.getDate());
    $('#datePickerFrom').datepicker({
        format: "mm/dd/yyyy",
        todayHighlight: true,
        autoclose: true
    });
    $('#datePickerFrom').datepicker('setDate', today);
}

if ($('#datePickerTo').length) {
    var date = new Date();
    var today = new Date(date.getFullYear(), date.getMonth(), date.getDate());
    $('#datePickerTo').datepicker({
        format: "mm/dd/yyyy",
        todayHighlight: true,
        autoclose: true
    });
    $('#datePickerTo').datepicker('setDate', today);
}


if ($('.myDatePicker').length) {
    var date = new Date();
    var today = new Date(date.getFullYear(), date.getMonth(), date.getDate());
    $('.myDatePicker').datepicker({
        format: "mm/dd/yyyy",
        todayHighlight: true,
        autoclose: true
    });
    $('.myDatePicker').datepicker('setDate', today);
}
});