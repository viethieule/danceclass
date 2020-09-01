function ScheduleController() {
    BaseController.call(this);
    CalendarManager.call(this);
    RegistrationManager.call(this);
    UserRegistrationManager.call(this);
    ScheduleCreate.call(this);

    this.currentDaysOfWeek = [];
    this.scheduleDetails = [];

    this.run = function () {
        Utils.collapseSideBar();
        Utils.setNavBarTitle();

        this.initCalendar();
        this.initRegistrationMng();
        this.initUserRegistration();
        this.initScheduleCreate();
    }
}