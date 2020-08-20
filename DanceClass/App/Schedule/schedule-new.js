function BaseController() {
    this.currentUser = null;

    this.initialize = function () {
        userService.getCurrentUser().then(function (user) {
            this.currentUser = user;
            this.run();
        }.bind(this));
    };
}

function ScheduleController() {
    BaseController.call(this);

    this.run = function () {
        console.log(this.currentUser);
    }
}

var ctrl = new ScheduleController();
ctrl.initialize();