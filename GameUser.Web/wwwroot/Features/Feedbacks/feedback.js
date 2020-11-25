const SUPPORTED_FILE_TYPES = ["image/jpeg", "image/png", "image/gif"];

class Feedback {
    
    constructor() {
        this.mainForm = $('#gameForm');
        this.successModal = $('#successModal');
        this.errorModal = $('#errorModal');
        this.loadingModal = $('#loadingModal');
        this.submitButton = this.mainForm.find('#submitBtn');
        this.backToLobbyLink = this.mainForm.find('#backlink');
        this.wordCount = this.mainForm.find('#wordCount');

        this.viewResources = viewResources;

        this.registerEventHandlers();
        this.loadAvatar(avatar);
        this.clearSessionFromUrl();
    }

    // ****** Common functions *******  

    backToLobby() {
        window.close();
    }

    loadAvatar(avatar) {
        let canvas = document.getElementById("avatar");
        canvas.style.backgroundPosition = ` -${avatar.trimX*46/128}px -${avatar.trimY*46/128}px`;
    }

    disableSubmitButton(isDisabled) {
        this.submitButton.prop('disabled', isDisabled);
    }

    refreshAntiforgeryToken(culture, submitForm) {
        $.ajax({
            method: "GET",
            url: `${window.rootUrl}${culture}/feedback/RefreshToken`
        }).then(function (result) {
            if (!!result["unauthorized"]) {
                window.location.href = `${window.rootUrl}Error/NotAuthorized`
            }
            else {
                var tokenValue = $('<div />').html(result).find('input[type="hidden"]').val();
                submitForm(culture, tokenValue);
            }
        });
    }

    submitForm(culture, tokenValue) {
        $('input[name="__RequestVerificationToken"]').val(tokenValue);
        let formData = new FormData(this.mainForm[0]);
        $.ajax({
            method: "POST",
            url: `${window.rootUrl}${culture}/feedback/Add`,
            data: formData,
            processData: false,
            contentType: false
        }).done(function (result) {
            this.hideLoading();
            this.disableSubmitButton(false);

            if (result.isSuccess) {
                this.showSuccessModal();
            }
            else {
                this.showErrorModal(result.message);
            }
        }.bind(this));
    }

    submit() {        
        if (!this.validateDescs()) {
            return;
        }
        
        this.disableSubmitButton(true);        
        this.showLoading();
        let culture = $("#Culture").val();
        this.refreshAntiforgeryToken(culture, this.submitForm.bind(this));
    }

    resetForm() {
        this.mainForm.find('#Feedback_Category').val('NewGame');
        this.mainForm.find('#descText').val('');
        this.mainForm.find('.added-image:visible').remove();
        this.mainForm.find('.attachment-count').html(0);
        this.mainForm.find('#descErrors').hide();
        this.mainForm.find('#attachmentErrors').hide();
        this.updateWordCount();
    }    

    registerEventHandlers() {
        this.successModal.on('hidden.bs.modal', this.onCloseSuccessModal.bind(this));
        this.errorModal.on('hidden.bs.modal', this.onCloseErrorModal.bind(this));
        this.submitButton.on('click', this.submit.bind(this));
        this.mainForm.find('.to-add-image').on('click', this.openSelectFile.bind(this));
        this.mainForm.find('#descText').on('keyup', this.onDescKeyUp.bind(this));
        this.mainForm.find('#descText').on('paste', this.onDescPaste.bind(this));
        this.backToLobbyLink.on('click', this.backToLobby.bind(this));
        this.registerAttachmentHandlers();
    }

    clearSessionFromUrl() {
        var url = location.href;

        history.pushState(null, "", url.split('?')[0]);
    }

    //  ******* End common functions  *******


    //  ******* Description  *******
    validateDescs() {
        var desc = this.mainForm.find('#descText').val();

        if (desc === "") {
            this.mainForm.find('#descErrors').show();
            this.mainForm.find('#descErrorsContent').html(`(${this.viewResources.Description_Cant_Empty})`);
            this.mainForm.find('#descText').addClass('error-border');

            this.hideDescErrors();
            return false;
        }
        else if (desc.length > 500) {
            this.mainForm.find('#descText').val(desc.substring(0, 500));
            this.mainForm.find('#descErrors').show();
            this.mainForm.find('#descErrorsContent').html(`(${this.viewResources.Description_Max_Characters})`);
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
        this.updateWordCount();
    }

    updateWordCount() {
        var desc = this.mainForm.find('#descText').val();
        this.wordCount.html(`${desc.length}/500`);
    }

    onDescPaste() {
        setTimeout(function () {
            this.onDescKeyUp();
        }.bind(this), 100);
    }
    //  ******* End Description  *******


    //  ******* Attachment  *******
    validateAttachments(currentFile) {
        let errorMessage = "";
        let totalLength = currentFile.size;

        this.mainForm.find('.added-image:visible input[type=file]').each(function (i, obj) {
            let file = obj.files[0].size;
            totalLength += file;
        });

        if (totalLength > 15728640) {
            errorMessage = this.viewResources.Attachment_Max_Size;
        }
        else if (!SUPPORTED_FILE_TYPES.includes(currentFile.type)) {
            errorMessage = this.viewResources.Attachment_Supported_Types;
        }

        if (errorMessage === "") {
            this.mainForm.find('#attachmentErrors').hide();
            return true;
        }
        else {
            this.mainForm.find('#attachmentErrors').show();
            this.mainForm.find('#attachmentErrorsContent').html(`(${errorMessage})`)
            this.mainForm.find('.image-container').addClass('error-border');

            setTimeout(function () {
                this.mainForm.find('#attachmentErrors').hide();
                this.mainForm.find('.image-container').removeClass('error-border');
            }.bind(this), 3000);

            return false;
        }
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
            $(e).parent().parent().find('.to-add-name').html(file.name).attr("title", file.name);
            $(e).parent().parent().find('.attach-img').attr("title", file.name);
            $(e).removeClass('to-add-input');
            $(e).parent().parent().removeAttr('hidden');
        }

        $(`<li class="added-image" hidden>
                <div>
                    <span class="attach-img"></span><span class="remove-img"></span>
                    <p class="to-add-name"></p>
                    <input type="file" accept="image/x-png,image/jpeg" class="to-add-input" name="Feedback.Files" hidden>
                </div>
           </li>`).insertAfter('.to-add-image');

        var container = this.mainForm.find('.image-container ul');
        container.animate({ scrollLeft: 0 });
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
        }.bind(this), 3000);
    }

    hideSuccessModal() {
        this.successModal.modal('hide');
    }

    onCloseSuccessModal() {
        this.resetForm();
    }
    //  ******* End Success Modal  *******


    //  ******* Error Modal  *******
    showErrorModal(message) {
        this.errorModal.modal('show');
        this.errorModal.find('#errorContent').html(message);       
    }

    hideErrorModal() {
        this.errorModal.modal('hide');
    }

    onCloseErrorModal() {
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