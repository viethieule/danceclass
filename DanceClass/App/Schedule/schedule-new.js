function BaseController() {
    this.currentUser = null;
    this.initialize = function (callback) {
        userService.getCurrentUser().then(function (user) {
            this.currentUser = user;
            callback();
        }.bind(this));
    };
}

function ScheduleController() {
    BaseController.call(this);
    this.initialize(function () {
        console.log(this.currentUser);
    }.bind(this));
}

new ScheduleController();