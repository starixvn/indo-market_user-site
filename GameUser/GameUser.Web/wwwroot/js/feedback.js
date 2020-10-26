class Feedback {
    submit() {
        const modal = $('#gameForm');

        if (!this.validateContent(modal)) {
            return;
        }

        modal.find('#submitBtn').prop('disabled', true);
        $.ajax({
            method: "POST",
            url: "/feedback/AddFeedback",
            dataType: 'json',
            data: modal.serialize(),
        }).done(function (result) {
            if (result.success) {
                modal.find('#failedMsg').hide();
                parent.postMessage("------------hello!-----cocos---------", "*")
                feedback.showSuccessModal('');
                modal.find('#submitBtn').prop('disabled', false);
            }
            else {
                modal.find('#failedMsg').show();

                modal.find('#failedMsgContent').html(result.message);
                modal.find('#submitBtn').prop('disabled', false);
            }            
        });
    }

    validateContent(modal) {
        var desc = $('#descText').val();
        if (desc === "") {
            modal.find('#failedMsg').show();
            modal.find('#failedMsgContent').html("(The description can't be empty)");
            return false;
        }

        modal.find('#failedMsg').hide();
        return true;
    }

    showSuccessModal(text) {
        const modal = $('#editModal');
        if (!name) {
            name = "";
        }

        modal.modal('show');
    }

    openSelectFile() {
        $('#myfile').trigger('click');
    }
}

var feedback = new Feedback();
$("#descText").keyup(function () {
    feedback.validateContent($('#gameForm'));
});
var chosenFiles = [];
$('#myfile').on('change', function (e) {
    var file = e.target.files[0];
    chosenFiles.push(file);
    console.log(file.name);
    console.log(chosenFiles);
});