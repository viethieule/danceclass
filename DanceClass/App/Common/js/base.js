function BaseController() {
    this.currentUser = null;

    this.initialize = function () {
        UserService.getCurrentUser().then(function (user) {
            this.currentUser = user;
            this.run();
        }.bind(this));
    };
}