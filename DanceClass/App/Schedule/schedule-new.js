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
    var _self = this;
    BaseController.call(this);

    const TIME_SLOTS = [
        { hours: 9, minutes: 0 },
        { hours: 10, minutes: 0 },
        { hours: 11, minutes: 0 },
        { hours: 12, minutes: 0 },
        { hours: 17, minutes: 0 },
        { hours: 18, minutes: 0 },
        { hours: 18, minutes: 30 },
        { hours: 19, minutes: 0 },
        { hours: 19, minutes: 35 },
        { hours: 20, minutes: 0 },
    ];

    this.currentDaysOfWeek = [];
    this.scheduleDetails = [];

    this.run = function () {
        collapseSideBar();
        renderUserRemainingSessions();
        initCreateScheduleForm();
    }

    function renderUserRemainingSessions() {
        let user = _self.currentUser;
        if (user && user.activePackage) {
            let remainingSessions = user.activePackage.remainingSessions;
            $('.calendar-remaining-sessions').html(formatRemainingSessions(remainingSessions));
        } else if (user && user.roleNames.includes("Member")) {
            $('.calendar-remaining-sessions').html(0);
        } else {
            $('.calendar-remaining-sessions-area').hide();
        }
    }
}

var ctrl = new ScheduleController();
ctrl.initialize();