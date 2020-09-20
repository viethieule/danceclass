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
            let modalMessage = $('.modal-body-message');
            let btnAction = $modal.find('#btn-action');
            let user = _self.currentUser;

            $modal.find('.modal-body').alert(false);
            modalBodyInfo.empty();
            btnAction.off('click');

            $('.modal-body-remaining-sessions').html(user && user.activePackage ? user.activePackage.remainingSessions : 0);

            const scheduleDetail = _self.scheduleDetails.find(x => x.id === parseInt(id));
            if (!scheduleDetail) {
                return;
            }

            const { isCurrentUserRegistered, currentUserRegistration } = scheduleDetail;
            if (isCurrentUserRegistered && currentUserRegistration && currentUserRegistration.status.value === 0) {
                modalTitle.text('Hủy đăng ký');
                modalBodyInfo.text('Bạn có chắc muốn hủy đăng ký?');
                modalMessage.text('Bạn sẽ được hoàn lại 1 buổi trong gói tập hiện tại sau khi hủy').css('color', '#00a65a');
                btnAction.html('Hủy');
                btnAction.on('click', function (e) {
                    handleUnregisterScheduleClick(currentUserRegistration.id, $modal)
                });
            } else {
                modalTitle.text('Bạn có chắc chắn muốn đăng ký?');
                modalMessage.text('Lưu ý: Bạn sẽ dùng 1 buổi trong gói tập hiện tại sau khi đăng ký').css('color', '#dd4b39');

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
        var $modalBody = $modal.find('.modal-body');
        try {
            await _self.unregisterSchedule(registrationId);
            $modalBody.alert(true, 'success', 'Hủy thành công', 1000);
            setTimeout(function () {
                $modal.modal('hide');
                updateUserRemainingSessions(false);
                _self.renderSchedule();
            }, 1000);
        } catch (ex) {
            console.log(ex);
            $modalBody.alert(true, 'danger', ex);
        }
    }

    async function handleRegisterScheduleClick(scheduleDetail, $modal) {
        var $modalBody = $modal.find('.modal-body');
        try {
            const rs = await _self.registerSchedule(scheduleDetail.id, _self.currentUser.id);
            if (rs && rs.registration) {
                $modalBody.alert(true, 'success', 'Đăng ký thành công', 1000);
                setTimeout(async function () {
                    $modal.modal('hide');
                    updateUserRemainingSessions(true);
                    _self.renderSchedule();
                }, 1000);
            }
        } catch (ex) {
            console.log(ex);
            $modalBody.alert(true, 'danger', ex);
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