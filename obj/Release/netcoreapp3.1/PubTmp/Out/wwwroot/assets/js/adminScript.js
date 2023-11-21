$(function () {
    'use strict';

    $('#program').on('change', function () {

        var demovalue = $(this).val();
        $("#category").empty();
        //Load Categories
        //---------------
        var urlPrograms = '/Admin/ProgramCategoriesByInstitution';
       
        $.post(urlPrograms, { institutionId: demovalue }, function (data) {
            var items = '';
            $("#category").empty();

            $.each(data, function (i, response) {

                items += "<option value = '" + response.value + "'>" + response.text + "</option>";
            });

            $("#category").html(items);
        });

    });

    $('#country').on('change', function () {
        var countryId = $(this).val();
        //alert(countryId);

        var url = '/Home/statesByCountryId';;
        //alert(url);

        $.post(url, { CountryId: countryId }, function (data) {
            var items = '';
            $("#state").empty();

            $.each(data, function (i, response) {

                items += "<option value = '" + response.value + "'>" + response.text + "</option>";
            });

            $("#state").html(items);
        });
    });

    $('#state').on('change', function () {
        var cityId = $(this).val();

        var url = '/Home/citiesByStateId';

        $.post(url, { StateId: cityId }, function (data) {
            var items = '';
            $("#city").empty();

            $.each(data, function (i, response) {

                items += "<option value = '" + response.value + "'>" + response.text + "</option>";
            });

            $("#city").html(items);
        });
    });

    //$('#countryIns').on('change', function () {
    //    var countryId = $(this).val();
    //    alert(countryId);
    //    var url = $(this).data('url');


    //    $.post(url, { CountryId: countryId }, function (data) {
    //        var items = '';
    //        $("#stateIns").empty();

    //        $.each(data, function (i, response) {

    //            items += "<option value = '" + response.value + "'>" + response.text + "</option>";
    //        });

    //        $("#stateIns").html(items);
    //    });
    //});

    //$('#stateIns').on('change', function () {
    //    var cityId = $(this).val();

    //    var url = $(this).data('url');

    //    $.post(url, { StateId: cityId }, function (data) {
    //        var items = '';
    //        $("#cityIns").empty();

    //        $.each(data, function (i, response) {

    //            items += "<option value = '" + response.value + "'>" + response.text + "</option>";
    //        });

    //        $("#cityIns").html(items);
    //    });
    //});
});