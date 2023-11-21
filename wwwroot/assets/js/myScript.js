$(function () {
    'use strict';

    //$('#programCSD').on('change', function () {

    //    var demovalue = $(this).val();
       
    //    $("#categoryCSD").empty();
    //    $("#coursesCSD").empty();
    //    //$("#divCourseOptions").empty();
    //   // $("#divCertificateOptions").empty();
    //    //Load Programs
    //    //-------------
    //    var urlPrograms = '/Home/GetProgramCategoriess';

    //    $.post(urlPrograms, { ProgramId: demovalue }, function (data) {
    //        var items = '';
    //        $("#categoryCSD").empty();

    //        $.each(data, function (i, response) {

    //            items += "<option value = '" + response.value + "'>" + response.text + "</option>";
    //        });

    //        $("#categoryCSD").html(items);
    //    });

    //});

    //$('#categoryCSD').on('change', function () {

    //    var demovalue = $(this).val();
    //    // var t = document.getElementById("category");
    //    // var selectedText = t.options[t.selectedIndex].text;
    //    // ProgramDe(selectedText);
    //    //$("#divCourseOptions").empty();
    //   // $("#divCertificateOptions").empty();
    //   // document.getElementById('certificationOnly').disabled = false;

    //    /*$("#subjectOptions").css("display", "none");*/
    //    ////Load Program Price
    //    ////------------------
    //    //var url = '/Home/programPriceByProgramId';

    //    //$.post(url, { ProgramId: demovalue }, function (data) {

    //    //    const result1 = data.split('=');
    //    //    var re = parseInt(result1[3]);

    //    //    //Show payment description div
    //    //    //----------------------------
    //    //    if (re > 0) {

    //    //        $("#paymentDes").css("display", "block");
    //    //        $("#displ").css("display", "block");

    //    //        document.getElementById("deposit").innerHTML = '₦' + result1[1].bold();
    //    //        document.getElementById("price").innerHTML = selectedText.bold() + ' program tuition is ₦' + result1[0].bold() + ' for ' + result1[2].bold();
    //    //        document.getElementById("maximumPrice").innerHTML = '₦' + result1[0].bold();
    //    //        document.getElementById("deposit2").innerHTML = '₦' + result1[1].bold();
    //    //        document.getElementById("deposit3").innerHTML = '  (₦' + result1[1].bold() + ')';
    //    //        document.getElementById("fullTuition").innerHTML = '  (₦' + result1[0].bold() + ')';
    //    //        document.getElementById("amountF").value = result1[3];
    //    //        document.getElementById("amountD").value = result1[4];
    //    //        document.getElementById("fullP").innerHTML = '₦' + result1[0].bold();
    //    //    }

    //    //});


    //    //Load Program Options
    //    //--------------------
    //    var urlOptions = '/Home/GetCoursesbyProgramCat';

    //    $.post(urlOptions, { CategoryId: demovalue }, function (data) {

    //        var items = '';
    //        $("#coursesCSD").empty();

    //        $.each(data, function (i, response) {

    //            items += "<option value = '" + response.value + "'>" + response.text + "</option>";
    //        });

    //        $("#coursesCSD").html(items);
    //    });
    //});

    //$('#coursesCSD').on('change', function () {

    //    var demovalue = $(this).val();
    //    $("#divCourseOptionDates").empty();
    //    $("#divCourseOptions").empty();

    //    $("#institutionD").css("display", "block");
    //    $("#courseOptions").css("display", "block");
    //    $("#coursePrices").css("display", "none")
    //    $("#divCourseFee").css("display", "none");
    //    $("#divPayment").css("display", "none");
    //    $("#divPaymentDetails").css("display", "none");

    //    var url = '/Home/GetInstitutionbyCourseId';


    //    $.post(url, { CourseId: demovalue }, function (data) {

    //        document.getElementById("institution").innerHTML = data.bold();
    //    });


    //    let divCO = $("#divCourseOptionDates");

    //    //Load Course Price Options
    //    //-------------------------
    //    var urlSubjects = '/Home/GetCourseOptionsDatesbyCourseId';

    //    $.post(urlSubjects, { CourseId: demovalue }, function (data) {

    //        $("#divCourseOptionDates").empty();

    //        $.each(data, function (i, response) {
    //            let myHtml = '<div style="display: inline-flex;align-items: center;margin-right: 0.75rem;"><label class="container">' + response.text + '<input type = "radio" id="DateClicked" name="optionsRadiosDate" value="' + response.value + '"><span class="checkmark"></span></label></div>';
    //            divCO.append(myHtml);
    //        });
    //    });       
    //});

    //Clicking of Course date to load the other coursetype options
    //------------------------------------------------------------
    $(document).on('click', '#DateClicked', function () {
        var demovalue = $(this).val();
        $("#divCourseOptions").empty();
        $("#divCourseFee").css("display", "none");
        $("#divPayment").css("display", "none");
        $("#divPaymentDetails").css("display", "none");

        let divCourO = $("#divCourseOptions");

        var urlCerts = '/Home/GetCourseOptionsbyOptionDate';

        $.post(urlCerts, { OptionDate: demovalue }, function (data) {

            $("#divCourseOptions").empty();

            $.each(data, function (i, response) {
                let myHtml = '<div style="display: inline-flex;align-items: center;margin-right: 0.75rem;"><label class="container">' + response.text + '<input type = "radio" name="optionsRadiosCO" id="CoursePriceClicked" value="' + response.value + '"><span class="checkmark"></span></label></div>';
                divCourO.append(myHtml);
            });
        });
        $("#coursePrices").css("display", "block")

    });

    //Clicking of Course date to load the other coursetype options
    //------------------------------------------------------------
    $(document).on('click', '#CoursePriceClicked', function () {
        //checkbox label text
        //-------------------
        var demoText = $(this).parent().text().trim();
        document.getElementById("newCourseOptionId").value = $(this).val();

        var coursefee = demoText.split(' - ')[2];
        var NewcourFeeOnly = coursefee.substring(1);
        NewcourFeeOnly = NewcourFeeOnly.replace(',', '');
        var CurrentcourFee = document.getElementById("currentCFee").value.substring(1);
        CurrentcourFee = CurrentcourFee.replace(',', '');

        $("#divCourseFee").css("display", "block");
        $("#divPayment").css("display", "block");
        $("#divPaymentDetails").css("display", "block");

        document.getElementById("fees").innerHTML = "Your current course date fee is " + document.getElementById("currentCFee").value.bold() +" and new course date fee is " + coursefee.bold();
       
        if (parseFloat(NewcourFeeOnly) > parseFloat(CurrentcourFee)) {
           // alert('greater');
            $("#divOption").css("display", "block");

            var balance = 0;
            balance = parseFloat(NewcourFeeOnly) - parseFloat(CurrentcourFee);
            document.getElementById("amountToPay").value = balance;
            var valu = "₦ " + parseFloat(balance).toLocaleString() + ".00";

            document.getElementById("pDetailsCSD").innerHTML = "Amount to pay is " + valu.bold() ;
        }
        else {
            $("#divOption").css("display", "none");

           // alert('less');
            var balance = 0;
            balance = parseFloat(NewcourFeeOnly) - parseFloat(CurrentcourFee);
            document.getElementById("amountToPay").value = balance;
            // alert(balance);
            //var v = 0;
           // v = Math.abs(balance);
           // alert(v);
            balance *= -1
            //alert(balance);

            var valu = "₦ " + parseFloat(balance).toLocaleString() + ".00";
           // alert(valu);

            document.getElementById("pDetailsCSD").innerHTML = valu.bold() +  " will be added to your wallet";
        }

    });

    $('#optionsOfflineCSD').on('change', function () {
       
        if (this.checked) {
            $("#offlineAccountCSD").css("display", "block");
            document.getElementById("paymentMethodDTOCSD").value = "Offline";
            var offlinePR = (new Date().getTime()) + "5";

            document.getElementById("transactionRefCSD").innerHTML = offlinePR;
            document.getElementById("OfflinePaymentRefDTOCSD").value = offlinePR;
        }
        else {
            $("#offlineAccountCSD").css("display", "none");
            document.getElementById("paymentMethodDTOCSD").value = "Card";
            document.getElementById("transactionRefCSD").innerHTML = "";
            document.getElementById("OfflinePaymentRefDTOCSD").value = "";
        }
    });
    $('#optionCardCSD').on('change', function () {
        if (this.checked) {
            $("#offlineAccountCSD").css("display", "none");
            document.getElementById("paymentMethodDTOCSD").value = "Card";
            document.getElementById("transactionRefCSD").innerHTML = "";
            document.getElementById("OfflinePaymentRefDTOCSD").value = "";
        }
        else {
            $("#offlineAccountCSD").css("display", "block");
            document.getElementById("paymentMethodDTOCSD").value = "Offline";
            var offlinePR = (new Date().getTime()) + "5";
            document.getElementById("transactionRefCSD").innerHTML = offlinePR;
            document.getElementById("OfflinePaymentRefDTOCSD").value = offlinePR;
        }
    });

    $('#optionsOfflineMP').on('change', function () {

        if (this.checked) {
            $("#offlineAccountMP").css("display", "block");
            document.getElementById("paymentMethodDTOMP").value = "Offline";
            var offlinePR = (new Date().getTime()) + "5";

            document.getElementById("transactionRefMP").innerHTML = offlinePR;
            document.getElementById("OfflinePaymentRefDTOMP").value = offlinePR;
        }
        else {
            $("#offlineAccountMP").css("display", "none");
            document.getElementById("paymentMethodDTOMP").value = "Card";
            document.getElementById("transactionRefMP").innerHTML = "";
            document.getElementById("OfflinePaymentRefDTOMP").value = "";
        }
    });
    $('#optionCardMP').on('change', function () {
        if (this.checked) {
            $("#offlineAccountMP").css("display", "none");
            document.getElementById("paymentMethodDTOMP").value = "Card";
            document.getElementById("transactionRefMP").innerHTML = "";
            document.getElementById("OfflinePaymentRefDTOMP").value = "";
        }
        else {
            $("#offlineAccountMP").css("display", "block");
            document.getElementById("paymentMethodDTOMP").value = "Offline";
            var offlinePR = (new Date().getTime()) + "5";
            document.getElementById("transactionRefMP").innerHTML = offlinePR;
            document.getElementById("OfflinePaymentRefDTOMP").value = offlinePR;
        }
    });
});