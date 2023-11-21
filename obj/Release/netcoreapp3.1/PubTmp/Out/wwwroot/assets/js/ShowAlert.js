function ShowPopup() {

    var type = 'basic'
    showSwal = function (type) {
        'use strict';
        swal.fire({
            text: 'Payment successful',
            confirmButtonText: 'Close',
            confirmButtonClass: 'btn btn-danger',
        })
    }
}