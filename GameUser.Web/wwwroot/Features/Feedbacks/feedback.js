class Feedback {
    constructor() {
        this.mainForm = $('#gameForm');
        this.successModal = $('#successModal');
        this.errorModal = $('#errorModal');
        this.loadingModal = $('#loadingModal');

        this.submitButton = this.mainForm.find('#submitBtn');

        this.registerEventHandlers();
    }

    // ****** Common functions *******
    postToParentFromIframe(data) {
        parent.postMessage(data, "*");
    }

    submit() {
        if (!this.validateContent(this.mainForm)) {
            return;
        }

        let formData = new FormData(this.mainForm[0]);
        this.submitButton.prop('disabled', true);

        this.showLoading();
        $.ajax({
            method: "POST",
            url: `${window.rootUrl}feedback/Add`,
            data: formData,
            processData: false,
            contentType: false
        }).done(function (result) {
            this.hideLoading();
            this.submitButton.prop('disabled', false);

            if (result.success) {
                feedback.showSuccessModal();
                feedback.postToParentFromIframe({ showBackdrop: true });
            }
            else {
                feedback.showErrorModal(result.message);
                feedback.postToParentFromIframe({ showBackdrop: true });
            }
        }.bind(this));
    }

    resetForm() {
        $('#Feedback_Category').val('newgame');
        $('#descText').val('');
        $('.added-image:visible').remove();
        $('.attachment-count').html(0);
        $('#descErrors').hide();
        $('#attachmentErrors').hide();
    }

    validateContent(modal) {
        return this.validateDescs(modal);// && this.validateAttachments(modal);
    }

    registerEventHandlers() {
        this.successModal.on('hidden.bs.modal', function () {
            feedback.onCloseSuccessModal();
        });

        this.errorModal.on('hidden.bs.modal', function () {
            feedback.onCloseErrorModal();
        });
    }
    //  ******* End common functions  *******


    //  ******* Description  *******
    validateDescs(modal) {
        var desc = $('#descText').val();

        if (desc === "") {
            modal.find('#descErrors').show();
            modal.find('#descErrorsContent').html("(The description can't be empty)");
            $('#descText').addClass('error-border');

            this.hideDescErrors(modal);
            return false;
        }
        else if (desc.length > 500) {
            $('#descText').val(desc.substring(0, 500));
            modal.find('#descErrors').show();
            modal.find('#descErrorsContent').html("(You have reached the maximum number of 500 characters)");
            $('#descText').addClass('error-border');

            this.hideDescErrors(modal);
            return false;
        }

        modal.find('#descErrors').hide();
        $('#descText').removeClass('error-border')
        return true;
    }

    hideDescErrors(modal) {
        setTimeout(function () {
            modal.find('#descErrors').hide();
            $('#descText').removeClass('error-border');
        }, 3000);
    }

    onDescKeyUp() {
        feedback.validateContent(this.mainForm);
    }

    onDescPaste() {
        setTimeout(function () {
            this.onDescKeyUp();
        }.bind(this), 100);
    }
    //  ******* End Description  *******


    //  ******* Attachment  *******
    validateAttachments(modal, currentFile) {
        var totalLength = currentFile.size;

        $('.added-image:visible input[type=file]').each(function (i, obj) {
            let file = obj.files[0].size;
            totalLength += file;
        });

        if (totalLength > 15728640) {
            modal.find('#attachmentErrors').show();
            modal.find('#attachmentErrorsContent').html("(You can't attach more than 15M in total)")
            $('.image-container').addClass('error-border');

            setTimeout(function () {
                modal.find('#attachmentErrors').hide();
                $('.image-container').removeClass('error-border');
            }, 3000);

            return false;
        }

        modal.find('#attachmentErrors').hide();
        return true;
    }

    openSelectFile() {
        $('.to-add-input').trigger('click');
    }

    onFileInputChanged(e) {
        var file = e.files[0];

        if (!this.validateAttachments(this.mainForm, file)) {
            $(e).parent().parent().remove();
        }
        else {
            $(e).parent().parent().find('.to-add-name').html(file.name);
            $(e).removeClass('to-add-input');
            $(e).parent().parent().removeAttr('hidden');
        }

        $('<li class="added-image" hidden><div><img class="attach-img"/><img class="remove-img" onclick="feedback.removeAttachment(this)"/><br/><span class="to-add-name"></span><input type="file" accept="image/x-png,image/gif,image/jpeg" class="to-add-input" name="Feedback.Files" hidden onchange="feedback.onFileInputChanged(this)"></div></li>').insertBefore('.to-add-image');

        var container = $('.image-container ul');
        container.animate({ scrollLeft: container[0].scrollWidth });
        feedback.updateAttachmentCount();
    }

    removeAttachment(e) {
        $(e).parent().parent().remove();
        feedback.updateAttachmentCount();
    }

    updateAttachmentCount() {
        $('.attachment-count').html($('.added-image').length - 1);
    }
    //  ******* End Attachment  *******



    //  ******* Success Modal  *******
    showSuccessModal() {
        this.successModal.modal('show');

        setTimeout(function () {
            this.hideSuccessModal();
            this.hideLoading();
        }.bind(this), 3000);
    }

    hideSuccessModal() {
        this.successModal.modal('hide');
    }

    onCloseSuccessModal() {
        feedback.resetForm();
        feedback.postToParentFromIframe({ hideBackdrop: true });
    }
    //  ******* End Success Modal  *******


    //  ******* Error Modal  *******
    showErrorModal(message) {
        this.errorModal.modal('show');
        this.errorModal.find('#errorContent').html(message);

        setTimeout(function () {
            this.hideErrorModal();
            this.hideLoading();
        }.bind(this), 4000);
    }

    hideErrorModal() {
        this.errorModal.modal('hide');
    }

    onCloseErrorModal() {
        feedback.postToParentFromIframe({ hideBackdrop: true });
    }

    //  ******* End Error Modal  *******


    //  ******* Loading Modal  *******
    showLoading() {
        this.loadingModal.modal('show');
    }

    hideLoading() {
        this.loadingModal.modal('hide');
    }
    //  ******* Loading Modal  *******    
}

var feedback = new Feedback();