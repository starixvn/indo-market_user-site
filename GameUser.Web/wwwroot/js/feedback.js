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
                //parent.postMessage("------------hello!-----cocos---------", "*")
                parent.postMessage({ showSuccess: true }, "*")
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
        const modal = $('#successModal');
        if (!name) {
            name = "";
        }

        modal.modal('show');
    }

    closeSuccessModal() {
        console.log("close");
        parent.postMessage({closeSuccess: true}, "*")
    }

    openSelectFile() {
        $('.to-add-input').trigger('click');
    }

    onFileInputChanged(e) {
        var file = e.files[0];       

        $(e).parent().parent().find('.to-add-name').html(file.name);
        $(e).removeClass('to-add-input');
        $(e).parent().parent().removeAttr('hidden');
        $('<li class="added-image" hidden><div><img class="attach-img"/><img class="remove-img" onclick="feedback.removeAttachment(this)"/><br/><span class="to-add-name"></span><input type="file" accept="image/x-png,image/gif,image/jpeg" class="to-add-input" name="Feedback.Files" hidden onchange="feedback.onFileInputChanged(this)"></div></li>').insertBefore('.to-add-image');

        var container = $('.image-container ul');
        container.animate({ scrollLeft: container[0].scrollWidth });
        feedback.updateAttachmentCount();
    }

    onDescKeyUp() {
        console.log("upp");
        feedback.validateContent($('#gameForm'));
    }

    removeAttachment(e) {
        $(e).parent().parent().remove();
        feedback.updateAttachmentCount();
    }

    updateAttachmentCount() {
        $('.attachment-count').html($('.added-image').length - 1);
    }
}

var feedback = new Feedback();

$('#successModal').on('hidden.bs.modal', function () {
    feedback.closeSuccessModal();
});
