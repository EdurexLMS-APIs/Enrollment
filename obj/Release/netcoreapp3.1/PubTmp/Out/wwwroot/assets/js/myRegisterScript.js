$(function () {
    'use strict';
    //Enrollment page 1 script
    //-----------------------
    $('#optionPersonal').on('change', function () {

        var demovalue = $(this).val();
        if (this.checked) {
            document.getElementById("acctTyBus").value = "";
            document.getElementById("acctTyPers").value = demovalue;
            $("#personalSection").css("display", "block");
            $("#businessSection").css("display", "none");
        }
        else {
            document.getElementById("acctTyPers").value = "";
            document.getElementById("acctTyBus").value = demovalue;
            $("#businessSection").css("display", "block");
            $("#personalSection").css("display", "none");
        }
    });
    $('#optionsBusiness').on('change', function () {
        var demovalue = $(this).val();
        if (this.checked) {
            document.getElementById("acctTyPers").value = "";
            document.getElementById("acctTyBus").value = demovalue;
            $("#businessSection").css("display", "block");
            $("#personalSection").css("display", "none");
        }
        else {
            document.getElementById("acctTyBus").value = "";
            document.getElementById("acctTyPers").value = demovalue;
            $("#personalSection").css("display", "block");
            $("#businessSection").css("display", "none");
        }
    });
    //$('#reg1AcctType').on('change', function () {

    //    var demovalue = $(this).val();
    //    document.getElementById("acctTyBus").value = demovalue;
    //    document.getElementById("acctTyPers").value = demovalue;
    //    if (demovalue == "Personal") {
    //        $("#personalSection").css("display", "block");
    //        $("#businessSection").css("display", "none");
    //        //document.getElementById("acctTyPers").value = demovalue;
    //    }
    //    else {
    //        $("#businessSection").css("display", "block");
    //        $("#personalSection").css("display", "none");
    //    }
    //});

    $('#busiCountry').on('change', function () {
        var countryId = $(this).val();

        var url = '/Home/statesByCountryId';

        $.post(url, { CountryId: countryId }, function (data) {
            var items = '';
            $("#busiState").empty();

            $.each(data, function (i, response) {

                items += "<option value = '" + response.value + "'>" + response.text + "</option>";
            });

            $("#busiState").html(items);
        });
    });

    $('#busiState').on('change', function () {
        var cityId = $(this).val();

        var url = '/Home/citiesByStateId';

        $.post(url, { StateId: cityId }, function (data) {
            var items = '';
            $("#busiCity").empty();

            $.each(data, function (i, response) {

                items += "<option value = '" + response.value + "'>" + response.text + "</option>";
            });

            $("#busiCity").html(items);
        });
    });

    $('#persCountry').on('change', function () {
        var countryId = $(this).val();
        //alert(countryId);
        var url = '/Home/statesByCountryId';

        $.post(url, { CountryId: countryId }, function (data) {
            var items = '';
            $("#persState").empty();

            $.each(data, function (i, response) {

                items += "<option value = '" + response.value + "'>" + response.text + "</option>";
            });

            $("#persState").html(items);
        });
    });

    $('#persState').on('change', function () {
        var cityId = $(this).val();
        //alert(cityId);
        var url = '/Home/citiesByStateId';

        $.post(url, { StateId: cityId }, function (data) {
            var items = '';
            $("#persCity").empty();

            $.each(data, function (i, response) {

                items += "<option value = '" + response.value + "'>" + response.text + "</option>";
            });

            $("#persCity").html(items);
        });
    });

    //function ProgramDe(ProgramSelected) {

    //    if (ProgramSelected.includes("UTME")) {
    //        $("#choices").css("display", "block");

    //        //Load Institutions
    //        //-----------------
    //        var urlInstitutions = '/Home/GetInstitutions';

    //        $.post(urlInstitutions, function (data) {
    //            var items = '';
    //            $(".institutions").empty();

    //            $.each(data, function (i, response) {

    //                items += "<option value = '" + response.value + "'>" + response.text + "</option>";
    //            });

    //            $(".institutions").html(items);
    //        });

    //        //Load Courses
    //        //-----------------
    //        var urlCourses = '/Home/GetCourses';

    //        $.post(urlCourses, function (data) {
    //            var items = '';

    //            $(".courses").empty();

    //            $.each(data, function (i, response) {

    //                items += "<option value = '" + response.value + "'>" + response.text + "</option>";
    //            });

    //            $(".courses").html(items);
    //        });
    //    }
    //    else {
    //        $("#choices").css("display", "none");
    //    }
    //}

    //End of Enrollment page 1 script
    //-------------------------------
    //Enrollment page 2 script
    //-------------------------
    $('#program').on('change', function () {

        var demovalue = $(this).val();
        $("#category").empty();
        $("#courses").empty();
        $("#divCourseOptions").empty();
        $("#divCertifications").empty();
        $("#divCertOptions").empty();
        $("#divCertificateOptions").empty();
        $("#divCourseOptionDates").empty();
        $("#courseTypeCourierFee").css("display", "none");
        $("#courseOptions").css("display", "none");
        $("#certificateOptions").css("display", "none");
        //Load Programs
        //-------------
        var urlPrograms = '/Home/GetProgramCategoriess';

        $.post(urlPrograms, { ProgramId: demovalue }, function (data) {
            var items = '';
            $("#category").empty();

            $.each(data, function (i, response) {

                items += "<option value = '" + response.value + "'>" + response.text + "</option>";
            });

            $("#category").html(items);
        });

    });

    $('#category').on('change', function () {

        //var InterSwitchTR = (new Date().getTime()) + "6";
        //document.getElementById("txnref").value = InterSwitchTR;
        //document.getElementById("OfflinePaymentRefDTO").value = InterSwitchTR;

        var demovalue = $(this).val();
       // var t = document.getElementById("category");
       // var selectedText = t.options[t.selectedIndex].text;
       // ProgramDe(selectedText);
        $("#courses").empty();
        $("#divCourseOptions").empty();
        $("#divCertifications").empty();
        $("#divCertOptions").empty();
        $("#divCertificateOptions").empty();
        $("#divCourseOptionDates").empty();
        $("#courseTypeCourierFee").css("display", "none");
        $("#courseOptions").css("display", "none");
        $("#certificateOptions").css("display", "none");

        document.getElementById('certificationOnly').disabled = false;

        if (document.getElementById('certificationOnly').checked) {
           
            $("#courseDiv").css("display", "none");
            $("#certificateOptions").css("display", "block");
            $("#divCourseOptionDates").empty();
            $("#divCertifications").empty();
            $("#divCertOptions").empty();
            $("#divCourseOptions").empty();
            $("#paymentDes").css("display", "none");
            $("#certPrices").css("display", "none")
            $("#coursePrices").css("display", "none")
           // $("#institutionD").css("display", "none");

            //Load certifications by categoryId
            //---------------------------------
            //var demovalue = document.getElementById("category").value;


            let divCert = $("#divCertifications");
            var urlCerts = '/Home/GetCertificatesbyCategoryId';
            alert(demovalue);

            $.post(urlCerts, { CategoryId: demovalue }, function (data) {

                $("#divCertifications").empty();

                $.each(data, function (i, response) {
                    let myHtml = '<div style="display: inline-flex;align-items: center;margin-right: 0.75rem;"><label class="container">' + response.text + '<input type = "radio" name="optionsRadiosCert" id="CertClicked" value="' + response.value + '"><span class="checkmark"></span></label></div>';
                    divCert.append(myHtml);
                });
            });

            $("#courseTypeCourierFee").css("display", "none");
            $("#certificateOptions").css("display", "block");
            document.getElementById("courseFee").value = "";           
            document.getElementById("certificateFee").value = "";
            document.getElementById("courierFee").value = "";
            document.getElementById("selectedType").value = "";
            document.getElementById("fees").innerHTML = "";
        }
        /*$("#subjectOptions").css("display", "none");*/
        ////Load Program Price
        ////------------------
        //var url = '/Home/programPriceByProgramId';

        //$.post(url, { ProgramId: demovalue }, function (data) {

        //    const result1 = data.split('=');
        //    var re = parseInt(result1[3]);
           
        //    //Show payment description div
        //    //----------------------------
        //    if (re > 0) {

        //        $("#paymentDes").css("display", "block");
        //        $("#displ").css("display", "block");

        //        document.getElementById("deposit").innerHTML = '₦' + result1[1].bold();
        //        document.getElementById("price").innerHTML = selectedText.bold() + ' program tuition is ₦' + result1[0].bold() + ' for ' + result1[2].bold();
        //        document.getElementById("maximumPrice").innerHTML = '₦' + result1[0].bold();
        //        document.getElementById("deposit2").innerHTML = '₦' + result1[1].bold();
        //        document.getElementById("deposit3").innerHTML = '  (₦' + result1[1].bold() + ')';
        //        document.getElementById("fullTuition").innerHTML = '  (₦' + result1[0].bold() + ')';
        //        document.getElementById("amountF").value = result1[3];
        //        document.getElementById("amountD").value = result1[4];
        //        document.getElementById("fullP").innerHTML = '₦' + result1[0].bold();
        //    }
            
        //});


        //Load Program Options
        //--------------------
        var urlOptions = '/Home/GetCoursesbyProgramCat';

        $.post(urlOptions, { CategoryId: demovalue }, function (data) {

            var items = '';
            $("#courses").empty();

            $.each(data, function (i, response) {

                items += "<option value = '" + response.value + "'>" + response.text + "</option>";
            });

            $("#courses").html(items);
        });


        //else {
        //    $("#courseDiv").css("display", "block")
        //    $("#certificateOptions").css("display", "none")

        //    //Load Courses by Category
        //    //-----------------------
        //    //var demovalue = document.getElementById("category").value;

        //    var urlOptions = '/Home/GetCoursesbyProgramCat';

        //    $.post(urlOptions, { CategoryId: demovalue }, function (data) {

        //        var items = '';
        //        $("#courses").empty();

        //        $.each(data, function (i, response) {

        //            items += "<option value = '" + response.value + "'>" + response.text + "</option>";
        //        });

        //        $("#courses").html(items);
        //    });
        //}
       // //Enable payment buttons
       // //-----------------------
        //document.getElementById("cardButton").disabled = false;
        //document.getElementById("transferButton").disabled = false;
        //document.getElementById("bankButton").disabled = false;
    });

    $('#courses').on('change', function () {

        var demovalue = $(this).val(); 
        $("#divCourseOptionDates").empty();
        $("#divCertifications").empty();
        $("#divCertOptions").empty();
        $("#divCourseOptions").empty();
        $("#divCertificateOptions").empty();

        $("#courseTypeCourierFee").css("display", "none");
        $("#courseOptions").css("display", "none");
        $("#certificateOptions").css("display", "none");

       // $("#imagSec").css("display", "block");
       // $("#institutionD").css("display", "block");
       

        document.getElementById("courseFee").value = "";
        document.getElementById("certificateFee").value = "";
        document.getElementById("courierFee").value = "";
        document.getElementById("selectedType").value = "";
        document.getElementById("fees").innerHTML = "";

        $("#certificationFeeInfo").css("display", "none")
        document.getElementById("certificationsFeeInfo").value = "";
        document.getElementById("CertifeesInfo").innerHTML = "";

        var url = '/Home/GetInstitutionbyCourseId';
        

        $.post(url, { CourseId: demovalue }, function (data) {

            document.getElementById("institution").innerHTML = data.bold();
        });


        let divCO = $("#divCourseOptionDates");

        //Load Course Price Options
        //-------------------------
        var urlSubjects = '/Home/GetCourseOptionsDatesbyCourseId';

        $.post(urlSubjects, { CourseId: demovalue }, function (data) {

            $("#divCourseOptionDates").empty();

            $.each(data, function (i, response) {
                let myHtml = '<div style="display: inline-flex;align-items: center;margin-right: 0.75rem;"><label class="container">' + response.text + '<input type = "radio" id="DateClicked" name="optionsRadiosDate" value="' + response.value + '"><span class="checkmark"></span></label></div>';
                divCO.append(myHtml);
            });

            $("#courseOptions").css("display", "block");
            $("#certificateOptions").css("display", "block");
            $("#certPrices").css("display", "none")
            $("#coursePrices").css("display", "none")
            $("#courseTypeCourierFee").css("display", "none")
        });


        //Load Course related certifications
        //-------------------------
      
        let divCert = $("#divCertifications");
        var urlCerts = '/Home/GetCertificatesbyCourseId';

        $.post(urlCerts, { CourseId: demovalue }, function (data) {

            $("#divCertifications").empty();
            
            $.each(data, function (i, response) {
                let myHtml = '<div style="display: inline-flex;align-items: center;margin-right: 0.75rem;"><label class="container">' + response.text + '<input type = "radio" name="optionsRadiosCert" id="CertClicked" value="' + response.value + '"><span class="checkmark"></span></label></div>';
                divCert.append(myHtml);
            });
        });
    });

    //Clicking of Course date to load the other coursetype options
    //------------------------------------------------------------
    $(document).on('click', '#DateClicked' , function () {
        var demovalue = $(this).val();
        $("#divCourseOptions").empty();

        let divCourO = $("#divCourseOptions");
        var courseId = document.getElementById("courses").value;
        //alert(courseId);
        var urlCerts = '/Home/GetCourseOptionsbyOptionDate';

        $.post(urlCerts, { OptionDate: demovalue, selectedCourseId: courseId }, function (data) {

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
        //alert('hi');
        document.getElementById('paymentButton').disabled = false;
        var demoText = $(this).parent().text().trim();
        //alert(demoText);

        var coursefee = demoText.split(' - ')[2];
        //alert(coursefee);

        document.getElementById("courseOptionIdDTO").value = $(this).val();
        var courFeeOnly = coursefee.substring(1);
        //alert(courFeeOnly);

        courFeeOnly = courFeeOnly.replace(',', '');
        //alert(courFeeOnly);
        //alert(parseFloat(courFeeOnly));
        var percOffer = document.getElementById("percentageO").value;
       
        percOffer = parseInt(percOffer);
        

        var newCourseFee = 0;
        if (percOffer > 0) {
            newCourseFee= (parseFloat(courFeeOnly) * percOffer)/100;
            newCourseFee = parseFloat(courFeeOnly) - newCourseFee;
        }
        else {
            newCourseFee = parseFloat(courFeeOnly);
        }
        

        document.getElementById("amountPayable").value = newCourseFee;
       // alert(document.getElementById("amountPayable").value);
       
        //Add value to table payment
        //--------------------------
        var table = document.getElementById("tblPayment");
        var rows = table.rows;
        var total = 0;
        var cell;
        for (var i = 1, iLen = rows.length - 1; i <= iLen; i++) {
            rows[1].cells[2].innerText = parseFloat(newCourseFee).toLocaleString() + ".00"; //coursefee.substring(1);
            rows[1].cells[3].innerText = parseFloat(newCourseFee).toLocaleString() + ".00"; //coursefee.substring(1);

            cell = rows[i].cells[3];
            total += parseFloat(cell.textContent.replace(/,/g, '') || cell.innerText.replace(/,/g, ''));
        }
        document.getElementById("totalPayment").innerText = "₦ " + parseFloat(total).toLocaleString() + ".00";
        //var InterSwitchAmount = parseFloat(document.getElementById("totalPayment").innerText.replace(/,/g, ''));
        //document.getElementById("amount").value = InterSwitchAmount * 100;
        document.getElementById("courseFee").value = coursefee;
        document.getElementById("fees").innerHTML = "Course fee is " + coursefee.bold();
        $("#courseTypeCourierFee").css("display", "block")

        //$("#divcertificateType").empty();
        //$("#certificateTypes").css("display", "block")

        //let divCertType = $("#divcertificateType");

        //var urlCerts = '/Home/GetCertificateType';

        //$.post(urlCerts, function (data) {

        //    $("#divcertificateType").empty();

        //    $.each(data, function (i, response) {
        //        let myHtml = '<div style="display: inline-flex;align-items: center;margin-right: 0.75rem;"><label class="container">' + response.text + '<input type = "radio" name="optionsRadiosCType" id="certTypeClicked" value="' + response.value + '"><span class="checkmark"></span></label></div>';
        //        divCertType.append(myHtml);
        //    });
        //});

        //$("#certificateTypes").css("display", "block")

    });
    //////Clicking of certification type to load the countries, states and typeFee
    //////------------------------------------------------------------------------
    ////$(document).on('click', '#certTypeClicked', function () {

    ////    var demovalue = $(this).val();
      
    ////    var demoText = $(this).parent().text().trim();
    ////    document.getElementById("selectedType").value = demoText;
    ////    document.getElementById("courseOptionCertificateTypeIdDTO").value = demovalue;

        
    ////    if (demoText != "Electronic") {
    ////        //Load Certificate type fee
    ////        //-------------------------
    ////        var urlCertTypes = '/Home/GetCertificateFeeByTypeId';
           
    ////        $.post(urlCertTypes, { TypeId: demovalue }, function (data) {

    ////            document.getElementById("certificateFee").value = data;
                
    ////            document.getElementById("fees").innerHTML = "Course fee is " + document.getElementById("courseFee").value.bold() + ", " + demoText.bold() + " certificate fee is " + data.bold();

    ////            //Add value to table payment
    ////            //--------------------------
    ////            var table = document.getElementById("tblPayment");
    ////            var rows = table.rows;
    ////            var total = 0;
    ////            var cell;
    ////            for (var i = 1, iLen = rows.length - 1; i <= iLen; i++) {
    ////                rows[2].cells[2].innerText = data.substring(1);
    ////                rows[2].cells[3].innerText = data.substring(1);

    ////                cell = rows[i].cells[3];
    ////                total += parseFloat(cell.textContent.replace(/,/g, '') || cell.innerText.replace(/,/g, ''));
    ////            }
    ////            document.getElementById("totalPayment").innerText = "₦ " + parseFloat(total).toLocaleString() + ".00";
    ////            var InterSwitchAmount = parseFloat(document.getElementById("totalPayment").innerText.replace(/,/g, ''));
    ////            document.getElementById("amount").value = InterSwitchAmount*100;

    ////        });


    ////        //$("#delieveryAddress").css("display", "block")

    ////        //Load Countries
    ////        //--------------
    ////        var urlCountries = '/Home/GetCountries';

    ////        $.post(urlCountries, function (data) {
    ////            var items = '';
    ////            $("#country").empty();

    ////            $.each(data, function (i, response) {

    ////                items += "<option value = '" + response.value + "'>" + response.text + "</option>";
    ////            });

    ////            $("#country").html(items);
    ////            $('#country').val('163');
    ////            //automaticallySelectNigeria('country', '163');
    ////        });

    ////        //Load Nigeria states by default
    ////        //------------------------------
    ////        var urlState = '/Home/statesByCountryId';

    ////        $.post(urlState, { CountryId: 163 }, function (data) {
    ////            var items = '';
    ////            $("#stateNew").empty();

    ////            $.each(data, function (i, response) {

    ////                items += "<option value = '" + response.value + "'>" + response.text + "</option>";
    ////            });

    ////            $("#stateNew").html(items);
    ////        });

    ////    }
    ////    else {
    ////        $("#country").empty();
    ////        $("#stateNew").empty();
    ////        $("#delieveryAddress").css("display", "none")
    ////        document.getElementById("certificateFee").value = "";
    ////        document.getElementById("fees").innerHTML = "Course fee is " + document.getElementById("courseFee").value.bold() + ", " + demoText.bold() + " certificate fee is " + "FREE".bold();

    ////        //Add value to table payment
    ////        //--------------------------
    ////        var table = document.getElementById("tblPayment");
    ////        var rows = table.rows;
    ////        var total = 0;
    ////        var cell;
    ////        for (var i = 1, iLen = rows.length - 1; i <= iLen; i++) {
    ////            rows[2].cells[2].innerText ="0.00";
    ////            rows[2].cells[3].innerText = "0.00";
    ////            rows[3].cells[2].innerText = "0.00";
    ////            rows[3].cells[3].innerText = "0.00";

    ////            cell = rows[i].cells[3];
    ////            total += parseFloat(cell.textContent.replace(/,/g, '') || cell.innerText.replace(/,/g, ''));
    ////        }
    ////        document.getElementById("totalPayment").innerText = "₦ " + parseFloat(total).toLocaleString() + ".00";
    ////        var InterSwitchAmount = parseFloat(document.getElementById("totalPayment").innerText.replace(/,/g, ''));
    ////        document.getElementById("amount").value = InterSwitchAmount * 100;

    ////    }

       
    ////});

    //Clicking of certification to load the certification options
    //-----------------------------------------------------------
    $(document).on('click', '#CertClicked', function () {
        var demovalue = $(this).val();
        $("#divCertOptions").empty();

        let divCourO = $("#divCertOptions");

        var urlCerts = '/Home/GetCertificatesOptionsbyId';

        $.post(urlCerts, { CertId: demovalue }, function (data) {

            $("#divCertOptions").empty();

            $.each(data, function (i, response) {
                let myHtml = '<div style="display: inline-flex;align-items: center;margin-right: 0.75rem;"><label class="container">' + response.text + '<input type = "radio" name="optionsRadiosCertOp" id="certDateClicked" value="' + response.value + '"><span class="checkmark"></span></label></div>';
                divCourO.append(myHtml);
            });
        });
        $("#certPrices").css("display", "block")

    });

    //Clicking of certification to load the certification options
    //-----------------------------------------------------------
    $(document).on('click', '#certDateClicked', function () {
        
        var demoText = $(this).parent().text().trim();
        var demovalue = $(this).val();
        document.getElementById("certificationPriceOptionIdDTO").value = demovalue;
       
        var urlCerts = '/Home/GetCertificationConvertedValuebyCertOptId';
        
        $.post(urlCerts, { CertOptId: demovalue }, function (data) {
            
            var amount = data.split(',')[0];
            var rate = data.split(',')[1];
            var charges = data.split(',')[2];
            var currency = data.split(',')[3];
            
            var totalFee = 0;
            if (currency != "₦") {
                totalFee = parseFloat(amount) * parseFloat(rate) + parseFloat(charges);
            }
            else {
                totalFee = parseFloat(amount) + parseFloat(charges);
            }

            document.getElementById("CertifeesInfo").innerHTML = "Certification fee is ₦ " + totalFee.toString().bold() + " ( " + demoText.split(' - ')[1].bold() + " Amount * " + rate.bold() + " Exchange rate + " + charges.bold()+" Charges ) ";
            //Add value to table payment
            //--------------------------
            var table = document.getElementById("tblPayment");
            var rows = table.rows;
            var total = 0;
            var cell;
            for (var i = 1, iLen = rows.length - 1; i <= iLen; i++) {
                rows[4].cells[2].innerText = totalFee.toLocaleString() + ".00";
                rows[4].cells[3].innerText = totalFee.toLocaleString() + ".00";
                cell = rows[i].cells[3];

                total += parseFloat(cell.textContent.replace(/,/g, '') || cell.innerText.replace(/,/g, ''));
            }
            document.getElementById("totalPayment").innerText = "₦ " + parseFloat(total).toLocaleString() + ".00";
            var InterSwitchAmount = parseFloat(document.getElementById("totalPayment").innerText.replace(/,/g, ''));
            document.getElementById("amount").value = InterSwitchAmount * 100;

        });

        var certfee = demoText.split(' - ')[1];
        document.getElementById("certificationsFeeInfo").value = certfee;
        
        //var certfee = demoText.split(' - ')[1];

        $("#certificationFeeInfo").css("display", "block")        
    });

    $('#certificationOnly').on('change', function () {
        if (this.checked) {

            $("#courseOptions").css("display", "none")
            $("#courseDiv").css("display", "none")
            $("#certificateOptions").css("display", "block")
            $("#divCourseOptionDates").empty();
            $("#divCertifications").empty();
            $("#divCertOptions").empty();
            $("#divCourseOptions").empty();
            $("#paymentDes").css("display", "none");
            $("#certPrices").css("display", "none")
            $("#coursePrices").css("display", "none")
           // $("#institutionD").css("display", "none");


            $("#courseTypeCourierFee").css("display", "none")
            document.getElementById("courseFee").value = "";
            document.getElementById("certificateFee").value = "";
            document.getElementById("courierFee").value = "";
            document.getElementById("selectedType").value = "";
            document.getElementById("fees").innerHTML = "";

            //Load certifications by categoryId
            //---------------------------------
            var demovalue = document.getElementById("category").value;
           
            let divCert = $("#divCertifications");
            var urlCerts = '/Home/GetCertificatesbyCategoryId';

            $.post(urlCerts, { CategoryId: demovalue }, function (data) {

                $("#divCertifications").empty();

                $.each(data, function (i, response) {
                    let myHtml = '<div style="display: inline-flex;align-items: center;margin-right: 0.75rem;"><label class="container">' + response.text + '<input type = "radio" name="optionsRadiosCert" id="CertClicked" value="' + response.value + '"><span class="checkmark"></span></label></div>';
                    divCert.append(myHtml);
                });
            });            
        }
        else
        {
            $("#courseDiv").css("display", "block")
            $("#certificateOptions").css("display", "none")

            //Load Courses by Category
            //-----------------------
            var demovalue = document.getElementById("category").value;

            var urlOptions = '/Home/GetCoursesbyProgramCat';

            $.post(urlOptions, { CategoryId: demovalue }, function (data) {

                var items = '';
                $("#courses").empty();

                $.each(data, function (i, response) {

                    items += "<option value = '" + response.value + "'>" + response.text + "</option>";
                });

                $("#courses").html(items);
            });
        }
    });

    //Clicking of certification to load the certification options
    //-----------------------------------------------------------
    $(document).on('click', '#radDataClicked', function () {

        var demoText = $(this).val();
        var id = "data" + demoText;
        document.getElementById("dataIdDTO").value = demoText;

        var previousElValue = document.getElementById(id).previousElementSibling.innerHTML;

        $("#datamodemSec").css("display", "block")
        document.getElementById("dataselected").value = demoText;
       
        if (document.getElementById("modemselectedAmount").value === "" || document.getElementById("modemselectedAmount").value === null) {
            if (previousElValue.slice(1, -1) === "FREE") {
                
                document.getElementById("dataselectedAmount").value = "₦0.00";
            }
            else {

                document.getElementById("dataselectedAmount").value = previousElValue.slice(1, -1);                
            }
            document.getElementById("datamodemInfo").innerHTML = "Selected data fee is " + previousElValue.slice(1, -1).bold();
        }
        else {

            if (previousElValue.slice(1, -1) === "FREE") {
                document.getElementById("dataselectedAmount").value = "₦0.00";
            }
            else {
                document.getElementById("dataselectedAmount").value = previousElValue.slice(1, -1);
            }
            if (document.getElementById("modemselectedAmount").value == "₦0.00") {
                var feeValu = "FREE";
                document.getElementById("datamodemInfo").innerHTML = "Selected data fee is " + previousElValue.slice(1, -1).bold() + " and selected modem fee is " + feeValu.bold();

            }
            else {
                document.getElementById("datamodemInfo").innerHTML = "Selected data fee is " + previousElValue.slice(1, -1).bold() + " and selected modem fee is " + document.getElementById("modemselectedAmount").value.bold();

            }
        }

        //Add value to table payment
        //--------------------------
        var table = document.getElementById("tblPayment");
        var rows = table.rows;
        var total = 0;
        var cell;
        for (var i = 1, iLen = rows.length - 1; i <= iLen; i++) {
            rows[5].cells[2].innerText = document.getElementById("dataselectedAmount").value.substring(1);
            rows[5].cells[3].innerText = document.getElementById("dataselectedAmount").value.substring(1);

            cell = rows[i].cells[3];

            total += parseFloat(cell.textContent.replace(/,/g, '') || cell.innerText.replace(/,/g, ''));
        }
        document.getElementById("totalPayment").innerText = "₦ " + parseFloat(total).toLocaleString() + ".00";
        var InterSwitchAmount = parseFloat(document.getElementById("totalPayment").innerText.replace(/,/g, ''));
        document.getElementById("amount").value = InterSwitchAmount * 100;

    });

    //Clicking of certification to load the certification options
    //-----------------------------------------------------------
    $(document).on('click', '#radModemClicked', function () {

        var demoText = $(this).val();

        $("#datamodemSec").css("display", "block")
        document.getElementById("modemselected").value = demoText;
        document.getElementById("modemIdDTO").value = demoText;

        var id = "modem" + demoText;
        var previousElValue = document.getElementById(id).previousElementSibling.innerHTML;
                
        if (document.getElementById("dataselectedAmount").value === "" || document.getElementById("dataselectedAmount").value === null) {
            if (previousElValue.slice(1, -1) === "FREE") {

                document.getElementById("modemselectedAmount").value = "₦0.00";
            }
            else {

                document.getElementById("modemselectedAmount").value = previousElValue.slice(1, -1);
            }
            document.getElementById("datamodemInfo").innerHTML = "Selected modem fee is " + previousElValue.slice(1, -1).bold();
        }
        else {

            if (previousElValue.slice(1, -1) === "FREE") {
                document.getElementById("modemselectedAmount").value = "₦0.00";
            }
            else {
                document.getElementById("modemselectedAmount").value = previousElValue.slice(1, -1);
            }
            
            if (document.getElementById("dataselectedAmount").value == "₦0.00") {
                var feeValu = "FREE";
                document.getElementById("datamodemInfo").innerHTML = "Selected data fee is " + feeValu.bold() + " and selected modem fee is " + previousElValue.slice(1, -1).bold();

            }
            else {
                document.getElementById("datamodemInfo").innerHTML = "Selected data fee is " + document.getElementById("dataselectedAmount").value.bold() + " and selected modem fee is " + previousElValue.slice(1, -1).bold();

            }
        }

        //Add value to table payment
        //--------------------------
        var table = document.getElementById("tblPayment");
        var rows = table.rows;
        var total = 0;
        var cell;
        for (var i = 1, iLen = rows.length - 1; i <= iLen; i++) {
            rows[6].cells[2].innerText = document.getElementById("modemselectedAmount").value.substring(1);
            rows[6].cells[3].innerText = document.getElementById("modemselectedAmount").value.substring(1);

            cell = rows[i].cells[3];

            total += parseFloat(cell.textContent.replace(/,/g, '') || cell.innerText.replace(/,/g, ''));
        }
        document.getElementById("totalPayment").innerText = "₦ " + parseFloat(total).toLocaleString() + ".00";

        var InterSwitchAmount = parseFloat(document.getElementById("totalPayment").innerText.replace(/,/g, ''));
        document.getElementById("amount").value = InterSwitchAmount * 100;

    });


    $('#country').on('change', function () {
        var countryId = $(this).val();

        var url = '/Home/statesByCountryId';

        $.post(url, { CountryId: countryId }, function (data) {
            var items = '';
            $("#state").empty();
            $("#stateNew").empty();

            $.each(data, function (i, response) {

                items += "<option value = '" + response.value + "'>" + response.text + "</option>";
            });

            $("#state").html(items);
            $("#stateNew").html(items);
        });
    });   

    $('#stateNew').on('change', function () {

        var demoValue = $(this).val();
        //var TypeText = $('#certTypeClicked').parent().text().trim();
        document.getElementById("courseOptiondeliveryStateIdDTO").value = demoValue;

        var t = document.getElementById("stateNew");
        var selectedStateText = t.options[t.selectedIndex].text;

        var tCountry = document.getElementById("country");
        var selectedCountryText = tCountry.options[tCountry.selectedIndex].text;

        var urlOptions = '/Home/GetCourierFeeByStateId';

        $.post(urlOptions, { StateId: demoValue }, function (data) {

            document.getElementById("courierFee").value = data;
            document.getElementById("fees").innerHTML = "Course fee is " + document.getElementById("courseFee").value.bold() + ", " + document.getElementById("selectedType").value.bold() + " certificate fee is " + document.getElementById("certificateFee").value.bold() + " and delivery fee to " + selectedCountryText.bold() + "/" + selectedStateText.bold() + " is " + data.bold();

            //Add value to table payment
            //--------------------------
            var table = document.getElementById("tblPayment");
            var rows = table.rows;
            var total = 0;
            var cell;
            for (var i = 1, iLen = rows.length - 1; i <= iLen; i++) {
                rows[3].cells[2].innerText = data.substring(1);
                rows[3].cells[3].innerText = data.substring(1);

                cell = rows[i].cells[3];

                total += parseFloat(cell.textContent.replace(/,/g, '') || cell.innerText.replace(/,/g, ''));
                
            }
            document.getElementById("totalPayment").innerText = "₦ " + parseFloat(total).toLocaleString() + ".00";

            var InterSwitchAmount = parseFloat(document.getElementById("totalPayment").innerText.replace(/,/g, ''));
            document.getElementById("amount").value = InterSwitchAmount * 100;


        });

    });

    $('#optionsPart').on('change', function () {
        if (this.checked) {
            document.getElementById('payable').style = 'display:table-row;';
        }
    });
    $('#optionsFull').on('change', function () {
        if (this.checked) {
            document.getElementById('payable').style = 'display:none;';
            $("#paymentButton").css("display", "block");

            var table = document.getElementById("tblPayment");
            var rows = table.rows;
            var total = 0;
            var cell;
            for (var i = 1, iLen = rows.length - 1; i <= iLen; i++) {
                rows[1].cells[3].innerText = rows[1].cells[2].innerText;

                cell = rows[i].cells[3];
                total += parseFloat(cell.textContent.replace(/,/g, '') || cell.innerText.replace(/,/g, ''));
            }
            document.getElementById("totalPayment").innerText = "₦ " + parseFloat(total).toLocaleString() + ".00";
            var InterSwitchAmount = parseFloat(document.getElementById("totalPayment").innerText.replace(/,/g, ''));
            document.getElementById("amount").value = InterSwitchAmount * 100;

        }
    });
    $('#optionsOffline').on('change', function () {
        if (this.checked) {
           // alert("Offline");
            $("#offlineAccount").css("display", "block");
            $("#divInterSwitchPayment").css("display", "block");
            //$("#divInterSwitchPayment").css("display", "none");
           // $("#divOfflinePayment").css("display", "block");
            document.getElementById("paymentMethodDTO").value = "Offline";
            var offlinePR = (new Date().getTime()) + "5";
           // alert(offlinePR);
            document.getElementById("transactionRef").innerHTML = offlinePR;
            document.getElementById("OfflinePaymentRefDTO").value = offlinePR;
        }
        else {
            $("#offlineAccount").css("display", "none");
           // $("#divOfflinePayment").css("display", "none");
            //document.getElementById("paymentMethodDTO").value = "Card";
            document.getElementById("paymentMethodDTO").value = "Card";
            //document.getElementById("transactionRef").innerHTML = "";
            //var InterSwitchTR = (new Date().getTime()) + "6";
            //document.getElementById("txnref").value = InterSwitchTR;
           // document.getElementById("OfflinePaymentRefDTO").value = InterSwitchTR;
            document.getElementById("OfflinePaymentRefDTO").value = "";
            $("#divInterSwitchPayment").css("display", "block");

        }
    });
    $('#optionCard').on('change', function () {
        if (this.checked) {
            $("#offlineAccount").css("display", "none");
            //document.getElementById("paymentMethodDTO").value = "Card";
            document.getElementById("paymentMethodDTO").value = "Card";
            //var InterSwitchTR = (new Date().getTime()) + "6";
            //document.getElementById("txnref").value = InterSwitchTR;
            //document.getElementById("OfflinePaymentRefDTO").value = InterSwitchTR;
            document.getElementById("transactionRef").innerHTML = "";
            document.getElementById("OfflinePaymentRefDTO").value = "";
            $("#divInterSwitchPayment").css("display", "block");
        }
        else {
            $("#divInterSwitchPayment").css("display", "block");
            $("#offlineAccount").css("display", "block");
            document.getElementById("paymentMethodDTO").value = "Offline";
            var offlinePR = (new Date().getTime()) + "5";
            document.getElementById("transactionRef").innerHTML = offlinePR;
            document.getElementById("OfflinePaymentRefDTO").value = offlinePR;
           // alert(document.getElementById("OfflinePaymentRefDTO").value);
        }
    });

    //Profile Page
    //------------
    $("#btnEditProfile").click(function () {
        //alert('hi');
        document.getElementById("editFirstname").contentEditable = true;
        document.getElementById("editLastname").contentEditable = true;
        document.getElementById("editEmail").contentEditable = true;
        document.getElementById("editPhone").contentEditable = true;
        document.getElementById("editAltPhone").contentEditable = true;

    });

    //$('#programoption').on('change', function () {

    //    var demovalue = $(this).val();
    //    var optionName = document.getElementById("programoption");
    //    var selectedText = optionName.options[optionName.selectedIndex].text;

    //    var programName = document.getElementById("program");
    //    var selectedProgramText = programName.options[programName.selectedIndex].text;

    //    ProgramDe(selectedText);

    //    //Load Program Price
    //    //------------------
    //    var url = '/Home/GetPriceByProgramOptionId';

    //    $.post(url, { ProgramOptionId: demovalue }, function (data) {

    //        const result1 = data.split('=');

    //        document.getElementById("deposit").innerHTML = '₦' + result1[1].bold();
    //        document.getElementById("price").innerHTML = selectedProgramText.bold() + ' ' + selectedText.bold() + ' tuition is ₦' + result1[0].bold() + ' for ' + result1[2].bold();
    //        document.getElementById("maximumPrice").innerHTML = '₦' + result1[0].bold();
    //        document.getElementById("deposit2").innerHTML = '₦' + result1[1].bold();
    //        document.getElementById("deposit3").innerHTML = ' (₦' + result1[1].bold() +')';
    //        document.getElementById("fullTuition").innerHTML = ' (₦' + result1[0].bold() + ')';
    //        document.getElementById("amountF").value = result1[3];
    //        document.getElementById("amountD").value =result1[4];
    //        document.getElementById("fullP").innerHTML = '₦' + result1[0].bold();
    //        document.getElementById("usd").innerHTML = '$' + result1[6].bold();


    //       // if (parseInt(result1[5]) > 0) {
    //            document.getElementById("maxSub").innerHTML = "(You can only select " + result1[5].bold() + " options)";
    //        }
    //        else {
    //            document.getElementById("maxSub").innerHTML = "";
    //        }


    //    });
    //    let divS = $("#divSubjects");

       

    //    //Load Option Subjects
    //    //--------------------
    //    var urlSubjects = '/Home/GetProgramSubjects';

    //    $.post(urlSubjects, { OptionId: demovalue }, function (data) {
    //        //var items = '';
    //        $("#subjects").empty();
    //        $("#divSubjects").empty();

    //        $.each(data, function (i, response) {
    //            let myHtml = '<div style="display: inline-flex;align-items: center;margin-right: 0.75rem;"><label class="container">' + response.text + '<input type = "checkbox" value="' + response.value +'"><span class="checkmark"></span></label></div>';
    //            divS.append(myHtml);
    //            /*items += "<option value = '" + response.value + "'>" + response.text + "</option>";*/
    //        });

    //       // divS.append(innerDiv);
    //       // $("#subjects").html(items);
    //    });

    //    //Show payment description div
    //    //----------------------------
    //    $("#subjectOptions").css("display", "block");
    //    $("#paymentDes").css("display", "block"); 
    //    $("#displ").css("display", "block");
    //    //Enable payment buttons
    //    //-----------------------
    //    document.getElementById("cardButton").disabled = false;
    //    document.getElementById("transferButton").disabled = false;
    //    document.getElementById("bankButton").disabled = false;
    //});

    ////$('#optionsPart').on('change', function () {
    ////    if (this.checked) {
    ////        document.getElementById('amount').readOnly = false;
    ////        document.getElementById("amount").value = "";
    ////    }        
    ////});
    ////$('#optionDeposit').on('change', function () {
    ////    if (this.checked) {
    ////        document.getElementById('amount').readOnly = true;
    ////        document.getElementById("amount").value = document.getElementById("amountD").value;
    ////    }
    ////});
    ////$('#optionsFull').on('change', function () {
    ////    if (this.checked) {
    ////        document.getElementById('amount').readOnly = true;
    ////        document.getElementById("amount").value = document.getElementById("amountF").value;
    ////    }
    ////});

    //$("#cardButton").click(function () {
    //    Subjects();
    //});
    //$("#transferButton").click(function () {
    //    Subjects();
    //});
    //$("#bankButton").click(function () {
    //    Subjects();
    //});
    //$("#offlineButton").click(function () {
    //    Subjects();
    //});

    //function Subjects() {
    //    var items = '';
    //    $("#subjects").empty();

    //    var aInput = $("div :checkbox:checked");
    //    for (var i = 0; i < aInput.length; i++) {
    //        if (aInput[i].checked) {
    //            var res = aInput[i].value;
    //            //alert(res);
    //            items += "<option selected value = '" + res + "'>" + res + "</option>";
    //        }
    //    }
    //    $("#subjects").html(items);
    //}

    //$('#optionCard').on('change', function () {
    //    if (this.checked) {
    //        $("#cardButton").css("display", "block");
    //        $("#bankButton").css("display", "none");
    //        $("#transferButton").css("display", "none");
    //        $("#offlineButton").css("display", "none");
    //        $("#offlineAccount").css("display", "none");

    //    }
    //    else {
    //        $("#cardButton").css("display", "none");

    //    }
    //});
    //$('#optionsTransfer').on('change', function () {
    //    if (this.checked) {
    //        $("#transferButton").css("display", "block");
    //        $("#bankButton").css("display", "none");
    //        $("#cardButton").css("display", "none");
    //        $("#offlineButton").css("display", "none");
    //        $("#offlineAccount").css("display", "none");

    //    }
    //    else {
    //        $("#transferButton").css("display", "none");
    //    }
    //});
    //$('#optionsBank').on('change', function () {
    //    if (this.checked) {
    //        $("#bankButton").css("display", "block");
    //        $("#cardButton").css("display", "none");
    //        $("#transferButton").css("display", "none");
    //        $("#offlineButton").css("display", "none");
    //        $("#offlineAccount").css("display", "none");

    //    }
    //    else {
    //        $("#bankButton").css("display", "none");
    //    }
    //});
    //$('#optionsOffline').on('change', function () {
    //    if (this.checked) {
    //        $("#offlineButton").css("display", "block");
    //        $("#cardButton").css("display", "none");
    //        $("#bankButton").css("display", "none");
    //        $("#transferButton").css("display", "none");
    //        $("#offlineAccount").css("display", "block");            
    //    }
    //    else {
    //        $("#offlineButton").css("display", "none");
    //        $("#offlineAccount").css("display", "none");

    //    }
    //});
    //$('#amount').on('onkeyup', function () {
    //    alert('hi');
    //    ValidateAmount();

    //});
    
});