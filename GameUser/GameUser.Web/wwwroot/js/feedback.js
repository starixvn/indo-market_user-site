class Feedback {
    submit() {
        const modal = $('#gameForm');

        if (!this.validateContent(modal)) {
            return;
        }

        let formData = new FormData(modal[0]);     
        modal.find('#submitBtn').prop('disabled', true);
        $.ajax({
            method: "POST",
            url: "/feedback/AddFeedback",
            //dataType: 'json',
            data: formData,// modal.serialize(),
            processData: false,  // tell jQuery not to process the data
            contentType: false
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
        $('.to-add-input').trigger('click');
    }

    onFileInputChanged(e) {
        $(e).removeClass('to-add-input');
        $(e).parent().removeAttr('hidden');
        $('<li class="added-image" hidden><img /><input type="file" class="to-add-input" name="Feedback.Files" hidden onchange="feedback.onFileInputChanged(this)"></li>').insertBefore('.to-add-image');
    }

    onDescKeyUp() {
        console.log("upp");
        feedback.validateContent($('#gameForm'));
    }
}

var feedback = new Feedback();
