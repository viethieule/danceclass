function UserRegistrationManager() {
    var _self = this;

    this.initUserRegistration = function () {
        registerEvent();
    }

    function registerEvent() {
        $('#modal-register').on('show.bs.modal', function (event) {
            let div = $(event.relatedTarget);
            const id = div.data('id');

            let $modal = $(this);
            let modalTitle = $modal.find('.modal-title');
            let modalBodyInfo = $modal.find('.modal-body-info');
            let btnAction = $modal.find('#btn-action');
            let user = _self.currentUser;

            modalBodyInfo.empty();
            btnAction.off('click');

            $('.modal-body-message').empty();
            $('.modal-body-remaining-sessions').html(user && user.activePackage ? user.activePackage.remainingSessions : 0);

            const scheduleDetail = _self.scheduleDetails.find(x => x.id === parseInt(id));
            if (!scheduleDetail) {
                return;
            }

            const { isCurrentUserRegistered, currentUserRegistration } = scheduleDetail;
            if (isCurrentUserRegistered && currentUserRegistration && currentUserRegistration.status.value === 0) {
                modalTitle.text('Hủy đăng ký');
                modalBodyInfo.text('Bạn có chắc muốn hủy đăng ký?');
                btnAction.html('Hủy');
                btnAction.on('click', function (e) {
                    handleUnregisterScheduleClick(currentUserRegistration.id, $modal)
                });
            } else {
                modalTitle.text('Bạn có chắc chắn muốn đăng ký?');

                $('<p>').text('Lớp: ' + scheduleDetail.schedule.class.name).appendTo(modalBodyInfo);
                $('<p>').text('Bài: ' + scheduleDetail.schedule.song).appendTo(modalBodyInfo).appendTo(modalBodyInfo);
                $('<p>').text('Thời gian: ' + Utils.capitalizeFirstLetter(moment(scheduleDetail.date).locale('vi').format('dddd D/M'))).appendTo(modalBodyInfo);
                $('<p>').text('Địa điểm: ' + scheduleDetail.schedule.branch).appendTo(modalBodyInfo);

                btnAction.html('Đăng ký');
                btnAction.on('click', async function (e) {
                    await handleRegisterScheduleClick(scheduleDetail, $modal)
                });
            }
        })
    }

    async function handleUnregisterScheduleClick(registrationId, $modal) {
        try {
            await _self.unregisterSchedule(registrationId);
            $('.modal-body-message').css('color', 'green').text('Hủy thành công');
            setTimeout(function () {
                $modal.modal('hide');
                updateUserRemainingSessions(false);
                _self.renderSchedule();
            }, 1000);
        } catch (ex) {
            console.log(ex);
            $('.modal-body-message').css('color', 'red').text(ex.responseJSON ? ex.responseJSON.ExceptionMessage : 'Đã có lỗi xảy ra!');
        }
    }

    async function handleRegisterScheduleClick(scheduleDetail, $modal) {
        try {
            const rs = await _self.registerSchedule(scheduleDetail.id, _self.currentUser.id);
            if (rs && rs.registration) {
                $('.modal-body-message').css('color', 'green').text('Đăng ký thành công');
                setTimeout(async function () {
                    $modal.modal('hide');
                    updateUserRemainingSessions(true);
                    _self.renderSchedule();
                }, 1000);
            }
        } catch (ex) {
            console.log(ex);
            $('.modal-body-message').css('color', 'red').text(ex.responseJSON ? ex.responseJSON.ExceptionMessage : 'Đã có lỗi xảy ra!');
        }
    }

    function updateUserRemainingSessions(isRegistration) {
        var user = _self.currentUser;
        if (user && user.activePackage) {
            if (isRegistration) {
                user.activePackage.remainingSessions--;
            }
            else {
                user.activePackage.remainingSessions++;
            }
            _self.renderUserRemainingSessions();
        }
    }
}