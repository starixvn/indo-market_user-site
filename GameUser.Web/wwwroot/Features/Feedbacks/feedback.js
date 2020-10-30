class Feedback {
    postToParentFromIframe(data) {
        parent.postMessage(data, "*");
    }

    submit() {
        const modal = $('#gameForm');

        if (!this.validateContent(modal)) {
            return;
        }

        console.log(window.rootUrl);

        let formData = new FormData(modal[0]);     
        modal.find('#submitBtn').prop('disabled', true);

        this.showLoading();
        $.ajax({
            method: "POST",
            url: `${window.rootUrl}/feedback/Add`,
            data: formData,
            processData: false,
            contentType: false
        }).done(function (result) {
            this.hideLoading();

            if (result.success) {
                

                modal.find('#descErrors').hide();
                feedback.showSuccessModal();
                modal.find('#submitBtn').prop('disabled', false);
                feedback.postToParentFromIframe({ showSuccess: true });
            }
            else {
                modal.find('#descErrors').show();
                modal.find('#descErrorsContent').html(result.message);
                modal.find('#submitBtn').prop('disabled', false);
            }            

            
        }.bind(this));
    }

    showLoading() {
        $('#loadingModal').modal('show');
    }

    hideLoading() {
        console.log("hiding");
        $('#loadingModal').modal('hide');
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

    validateDescs(modal) {
        var desc = $('#descText').val();

        console.log("before length ", desc.length);

        if (desc === "") {
            modal.find('#descErrors').show();
            modal.find('#descErrorsContent').html("(The description can't be empty)");
            $('#descText').addClass('error-border');

            setTimeout(function () {
                modal.find('#descErrors').hide();
                $('#descText').removeClass('error-border');
            }, 3000);

            return false;
        }
        else if (desc.length > 500) {

            $('#descText').val(desc.substring(0, 500));
            console.log($('#descText').val().length);

            modal.find('#descErrors').show();
            modal.find('#descErrorsContent').html("(You have reached the maximum number of 500 characters)");
            $('#descText').addClass('error-border');

            setTimeout(function () {
                modal.find('#descErrors').hide();
                $('#descText').removeClass('error-border');
            }, 3000);

            return false;
        }

        modal.find('#descErrors').hide();
        $('#descText').removeClass('error-border')
        return true;
    }
    
    validateAttachments(modal, currentFile) {
        var totalLength = currentFile.size;

        $('.added-image:visible input[type=file]').each(function (i, obj) {
            let file = obj.files[0].size;
            totalLength += file;
            console.log(file);
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

    showSuccessModal() {
        $('#successModal').modal('show');

        setTimeout(function () {
            this.hideSuccessModal();
            this.hideLoading();
        }.bind(this), 3000);
    }

    hideSuccessModal() {
        $('#successModal').modal('hide');
    }

    closeSuccessModal() {
        console.log("closing");
        feedback.resetForm();
        feedback.postToParentFromIframe({ closeSuccess: true });
    }

    openSelectFile() {
        $('.to-add-input').trigger('click');
    }

    onFileInputChanged(e) {
        var file = e.files[0];       

        if (!this.validateAttachments($('#gameForm'), file)) {
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

    onDescKeyUp() {
        console.log("upp");
        feedback.validateContent($('#gameForm'));
    }

    onDescPaste() {
        setTimeout(function () {
            this.onDescKeyUp();
        }.bind(this), 100);
    }

    removeAttachment(e) {
        $(e).parent().parent().remove();
        feedback.updateAttachmentCount();
    }

    updateAttachmentCount() {
        $('.attachment-count').html($('.added-image').length - 1);
    }

    registerEventHandlers() {
        $('#successModal').on('hidden.bs.modal', function () {
            feedback.closeSuccessModal();
        });
    }
}

var feedback = new Feedback();
feedback.registerEventHandlers();