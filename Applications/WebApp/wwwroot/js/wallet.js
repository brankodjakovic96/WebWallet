$(document).on('keydown', '#Amount', function (e) {
    var input = $(this);
    var oldVal = input.val();
    var regex = new RegExp(input.attr('pattern'), 'g');

    setTimeout(function () {
        var newVal = input.val();
        if (!regex.test(newVal)) {
            input.val(oldVal);
        }
    }, 0);
});

function isEmpty(str) {
    return (!str || str.length === 0);
}

$(document).on('keydown', '#TransferAmount', function (e) {
    var input = $(this);
    var oldVal = input.val();
    var regex = new RegExp(input.attr('pattern'), 'g');

    setTimeout(function () {
        var newVal = input.val();
        if (!regex.test(newVal)) {
            input.val(oldVal);
        }
        else if (!isEmpty(document.getElementById('JmbgFrom').value) && !isEmpty(document.getElementById('PasswordFrom').value) && !isEmpty(newVal)) {
            $.ajax({
                type: "POST",
                url: '/Wallet/CalculateFee',
                contentType: "application/json",
                data: JSON.stringify({ jmbg: document.getElementById('JmbgFrom').value, password: document.getElementById('PasswordFrom').value, amount: parseFloat(newVal) }),
                dataType: "json",
                success: function (data) { document.getElementById('Fee').value = parseFloat(data).toFixed(2) },
                error: function (data) {
                    $('#modalTitle').html("<p class=\"text-danger\">Failed to calculate fee</p>");
                    $('#modalContent').html(data.responseJSON.errorMessage);
                    $('#modalCenter').modal('show');
                }
            });
        }
    }, 0);
});

function CalculateFee() {
    if (!isEmpty(document.getElementById('JmbgFrom').value) && !isEmpty(document.getElementById('PasswordFrom').value) && !isEmpty(document.getElementById('TransferAmount').value)) {
        $.ajax({
            type: "POST",
            url: '/Wallet/CalculateFee',
            contentType: "application/json",
            data: JSON.stringify({ jmbg: document.getElementById('JmbgFrom').value, password: document.getElementById('PasswordFrom').value, amount: parseFloat(document.getElementById('TransferAmount').value) }),
            dataType: "json",
            success: function (data) { document.getElementById('Fee').value = data },
            error: function (data) {
                $('#modalTitle').html("<p class=\"text-danger\">Failed to calculate fee</p>");
                $('#modalContent').html(data.responseJSON.errorMessage);
                $('#modalCenter').modal('show');
            }
        });
    }
}


function ShowMessage(successStatus, successMessage, errorMessage) {
    if (successStatus == "yes") {
            $(document).ready(function () {
                $('#modalTitle').html("<p class=\"text-success\">Success</p>");
                $('#modalContent').html(successMessage);
                $('#modalCenter').modal('show');
            });
    }
    if (successStatus == "no") {
            $(document).ready(function () {
                $('#modalTitle').html("<p class=\"text-danger\">Failed</p>");
                $('#modalContent').html(errorMessage);
                $('#modalCenter').modal('show');
            });
    }
}

