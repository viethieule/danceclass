function MemberIndexController() {
    BaseController.call(this);
    UserInfoController.call(this);
    CreatePackageController.call(this);
    EditPackageController.call(this);

    this.member = null;

    this.run = function () {
        this.initUserInfo();
        this.initCreatePackage();
        this.initEditPackage();
    }
}