$(document).on('keydown', '#DepositAmount', function (e) {
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