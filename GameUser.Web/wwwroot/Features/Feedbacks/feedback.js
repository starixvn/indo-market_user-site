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

    disableSubmitButton(isDisabled) {
        this.submitButton.prop('disabled', isDisabled);
    }

    submit() {
        if (!this.validateDescs()) {
            return;
        }

        let formData = new FormData(this.mainForm[0]);
        this.disableSubmitButton(true);        
        this.showLoading();
        let culture = $("#Culture").val();
        $.ajax({
            method: "POST",
            url: `${window.rootUrl}${culture}/feedback/Add`,
            data: formData,
            processData: false,
            contentType: false
        }).done(function (result) {
            this.hideLoading();
            this.disableSubmitButton(false);

            console.log(result);
            if (result.isSuccess) {
                this.showSuccessModal();
                this.postToParentFromIframe({ showBackdrop: true });
            }
            else {
                this.showErrorModal(result.message);
                this.postToParentFromIframe({ showBackdrop: true });
            }
        }.bind(this));
    }

    resetForm() {
        this.mainForm.find('#Feedback_Category').val('newgame');
        this.mainForm.find('#descText').val('');
        this.mainForm.find('.added-image:visible').remove();
        this.mainForm.find('.attachment-count').html(0);
        this.mainForm.find('#descErrors').hide();
        this.mainForm.find('#attachmentErrors').hide();
    }    

    registerEventHandlers() {
        this.successModal.on('hidden.bs.modal', this.onCloseSuccessModal.bind(this));
        this.errorModal.on('hidden.bs.modal', this.onCloseErrorModal.bind(this));
        this.submitButton.on('click', this.submit.bind(this));
        this.mainForm.find('.to-add-image').on('click', this.openSelectFile.bind(this));
        this.mainForm.find('#descText').on('keyup', this.onDescKeyUp.bind(this));
        this.mainForm.find('#descText').on('paste', this.onDescPaste.bind(this));
        this.registerAttachmentHandlers();
    }
    //  ******* End common functions  *******


    //  ******* Description  *******
    validateDescs() {
        var desc = this.mainForm.find('#descText').val();

        if (desc === "") {
            this.mainForm.find('#descErrors').show();
            this.mainForm.find('#descErrorsContent').html("(The description can't be empty)");
            this.mainForm.find('#descText').addClass('error-border');

            this.hideDescErrors();
            return false;
        }
        else if (desc.length > 500) {
            this.mainForm.find('#descText').val(desc.substring(0, 500));
            this.mainForm.find('#descErrors').show();
            this.mainForm.find('#descErrorsContent').html("(You have reached the maximum number of 500 characters)");
            this.mainForm.find('#descText').addClass('error-border');

            this.hideDescErrors();
            return false;
        }

        this.mainForm.find('#descErrors').hide();
        this.mainForm.find('#descText').removeClass('error-border')
        return true;
    }

    hideDescErrors() {
        setTimeout(function () {
            this.mainForm.find('#descErrors').hide();
            this.mainForm.find('#descText').removeClass('error-border');
        }.bind(this), 3000);
    }

    onDescKeyUp() {
        console.log("key up");
        this.validateDescs(this.mainForm);
    }

    onDescPaste() {
        setTimeout(function () {
            this.onDescKeyUp();
        }.bind(this), 100);
    }
    //  ******* End Description  *******


    //  ******* Attachment  *******
    validateAttachments(currentFile) {
        var totalLength = currentFile.size;

        this.mainForm.find('.added-image:visible input[type=file]').each(function (i, obj) {
            let file = obj.files[0].size;
            totalLength += file;
        });

        if (totalLength > 15728640) {
            this.mainForm.find('#attachmentErrors').show();
            this.mainForm.find('#attachmentErrorsContent').html("(You can't attach more than 15M in total)")
            this.mainForm.find('.image-container').addClass('error-border');

            setTimeout(function () {
                this.mainForm.find('#attachmentErrors').hide();
                this.mainForm.find('.image-container').removeClass('error-border');
            }.bind(this), 3000);

            return false;
        }

        this.mainForm.find('#attachmentErrors').hide();
        return true;
    }

    openSelectFile() {
        this.mainForm.find('.to-add-input').trigger('click');
    }

    onFileInputChanged(event) {
        var e = event.target;
        var file = e.files[0];

        if (!this.validateAttachments(file)) {
            $(e).parent().parent().remove();
        }
        else {
            $(e).parent().parent().find('.to-add-name').html(file.name);
            $(e).removeClass('to-add-input');
            $(e).parent().parent().removeAttr('hidden');
        }

        $(`<li class="added-image" hidden>
                <div>
                    <span class="attach-img"></span><span class="remove-img"></span>
                    <p class="to-add-name"></p>
                    <input type="file" accept="image/x-png,image/gif,image/jpeg" class="to-add-input" name="Feedback.Files" hidden>
                </div>
           </li>`).insertBefore('.to-add-image');

        var container = this.mainForm.find('.image-container ul');
        container.animate({ scrollLeft: container[0].scrollWidth });
        this.updateAttachmentCount();

        this.registerAttachmentHandlers();
    }

    registerAttachmentHandlers() {
        this.mainForm.find('#imageContainer .remove-img').off('click');
        this.mainForm.find('#imageContainer .remove-img').on('click', this.removeAttachment.bind(this));
        this.mainForm.find('#imageContainer .to-add-input').off('change');
        this.mainForm.find('#imageContainer .to-add-input').on('change', this.onFileInputChanged.bind(this));
    }

    removeAttachment(event) {
        var e = event.target;
        $(e).parent().parent().remove();
        this.updateAttachmentCount();
    }

    updateAttachmentCount() {
        this.mainForm.find('.attachment-count').html($('.added-image').length - 1);
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
        this.resetForm();
        this.postToParentFromIframe({ hideBackdrop: true });
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
        this.postToParentFromIframe({ hideBackdrop: true });
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