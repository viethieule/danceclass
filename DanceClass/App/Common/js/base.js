function BaseController() {
    this.currentUser = null;

    this.initialize = function () {
        UserService.getCurrentUser().then(function (user) {
            this.currentUser = user;
            this.run();
        }.bind(this));
    };

    this.showAlert = function (message, confirmHandler) {
        $('#mistake-modal-confirm .modal-body p').html(message);
        $('.btn-mistake-modal-confirm').off('click').on('click', async function (event) {
            $(this).prop('disabled', true);
            await confirmHandler(event);
            $('#mistake-modal-confirm').modal('hide');
            $(this).prop('disabled', false);
        });
        $('#mistake-modal-confirm').modal('show');
    }
}