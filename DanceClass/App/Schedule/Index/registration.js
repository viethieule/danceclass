function RegistrationManager() {
    var _self = this;

    this.initRegistrationMng = function () {
        registerEvent();
    }

    this.unregisterSchedule = function (id) {
        return ApiService.post('/api/registration/cancel', { registrationId: id });
    }

    this.registerSchedule = function (scheduleDetailId, userId) {
        return ApiService.post('/api/registration/create', {
            registration: {
                scheduleDetailId,
                userId
            }
        });
    }

    this.confirmRegistration = function (registrationId) {
        return ApiService.put('/api/registration/confirmAttendance/' + registrationId);
    }

    this.reloadManageModal = function (scheduleDetailId, alertType, message) {
        handleShowingModalManage(scheduleDetailId, alertType, message); 
    }

    function registerEvent() {
        $('#modal-manage').on('shown.bs.modal', function (event) {
            $('#modal-manage .modal-body').alert(false);

            let div = $(event.relatedTarget);
            const id = parseInt(div.data('id'));

            handleShowingModalManage(id);
        });

        $('.session-registrations').slimscroll({
            distance: '5px'
        });
    }

    function handleShowingModalManage(scheduleDetailId, alertType, message) {
        if (message) {
            $('#modal-manage .modal-body').alert(true, alertType, message);
        }

        _self.selectedScheduleDetails = _self.scheduleDetails.find(x => x.id === scheduleDetailId);
        const { schedule, registrations, date, totalRegistered, sessionNo } = _self.selectedScheduleDetails;
        _self.selectedSchedule = schedule;

        let [hour, minute, ...rest] = schedule.startTime.split(':');
        let timeStart = Utils.capitalizeFirstLetter(moment(date).hours(parseInt(hour)).minute(parseInt(minute)).locale('vi').format('dddd D/M HH:mm'));

        // Update modal title to schedule's class name
        $('#modal-manage').find('.modal-title').html(schedule.class.name + ' - ' + timeStart);

        const { song, branch, sessions: totalSessions } = schedule;
        $('.session-general-info')
            .empty()
            .append(renderSessionInfoGroup('Bài múa', song))
            .append(renderSessionInfoGroup('Buổi', sessionNo + (totalSessions ? ' / ' + totalSessions : '')))
            .append(renderSessionInfoGroup('Thời gian', timeStart))
            .append(renderSessionInfoGroup('Địa điểm', branch))
            .append(renderSessionInfoGroup('Số học viên đăng ký', totalRegistered + ' / 20'));

        renderRegistrationList(registrations);

        $('.session-user-search-result').hide();
        $('.session-search-message').text('').hide();
        $('#session-user-search').val('');

        $('.session-add-registration button').off('click').on('click', async function (event) {
            let $addBtn = $(event.target);
            let $input = $addBtn.closest('div').find('input');
            let phoneNumber = $input.val();
            if (!phoneNumber) {
                return;
            }

            $addBtn.find('i').removeClass('fa fa-plus').addClass('fa fa-circle-o-notch fa-spin').prop('disabled', true);
            try {
                const user = await UserService.get({ phoneNumber });
                $('.session-user-search-result').show();
                if (user) {
                    $('.session-search-message').empty();
                    if (registrations.some(r => r.userId === user.id)) {
                        $('.session-search-message').text('Học viên ' + user.fullName + ' đã đăng ký!').show();
                        $('.session-user-search-result tbody').empty();
                        return;
                    }

                    let registerBtn = $('<button>', { class: 'btn btn-block btn-success btn-xs btn-label' })
                        .html('Đăng ký')
                        .on('click', { id: scheduleDetailId }, async function (event) {
                            let $registerBtn = $(event.target);
                            $registerBtn
                                .prop('disabled', true)
                                .empty()
                                .append($('<i>', { class: 'fa fa-circle-o-notch fa-spin' }))
                            try {
                                const response = await _self.registerSchedule(event.data.id, user.id);
                                if (response && response.registration) {
                                    response.registration.isModified = true;
                                    registrations.push(response.registration);

                                    // rerender
                                    renderRegistrationList(registrations);

                                    // hide search result
                                    $('.session-user-search-result').hide();
                                }
                            } catch (ex) {
                                console.log(ex);
                                $('.session-search-message').text(ex.responseJSON ? ex.responseJSON.ExceptionMessage : ex).show();
                            } finally {
                                // empty search result
                                $('.session-user-search-result tbody').empty();
                            }
                        });

                    let searchResult = $('<tr>')
                        .append($('<td>').text(user.fullName))
                        .append($('<td>').text(user.userName))
                        .append($('<td>').text(user.phoneNumber))
                        .append($('<td>').append(registerBtn));

                    $('.session-user-search-result tbody').empty().append(searchResult);
                } else {
                    $('.session-search-message').text('Không tìm thấy học viên!').show();
                    $('.session-user-search-result tbody').empty();
                }
            } catch (ex) {
                console.log(ex);
                $('.session-search-message').text(ex.responseJSON ? ex.responseJSON.ExceptionMessage : ex).show();
                $('.session-user-search-result tbody').empty();
            } finally {
                $addBtn.find('i')
                    .removeClass('fa fa-circle-o-notch fa-spin').addClass('fa fa-plus')
                    .prop('disabled', false);
            }
        })

        $('#modal-manage').off('hide.bs.hide.bs.modal').on('hide.bs.hide.bs.modal', async function (event) {
            if (_self.selectedScheduleDetails &&
                (registrations.some(r => r.isModified) || totalRegistered !== registrations.length)) {
                _self.renderSchedule();
            } else if (_self.selectedSchedule === null) {
                _self.renderSchedule();
            }
        });

        $('.btn-schedule-update').off('click').on('click', function (event) {
            _self.openScheduleCreateModal('edit', $(this));
        });

        $('.btn-schedule-delete-create').off('click').on('click', function (event) {
            _self.openScheduleCreateModal('deleteCreate', $(this));
        });

        $('.btn-session-delete').off('click touchend').on('click touchend', function (event) {
            var pWarning = $('<p>').text('Lưu ý');
            var ul = $('<ul>')
                .append(
                    $('<li>').text('Toàn bộ buổi học sau buổi này sẽ bị xóa.')
                )
                .append(
                    $('<li>').text('Toàn bộ học viên của các buổi bị xóa sẽ được hoàn lại buổi học đã đăng ký.')
                );
            var pConfirm = $('<p>').text('Bạn có chắc chắn muốn xóa?')

            var message = pWarning.html() + ul.html() + pConfirm.html();
            _self.showAlert(message, async function (event) {
                try {
                    var rs = await ApiService.del('/api/schedule/deleteSession/' + _self.selectedScheduleDetails.id);
                    handlePostDeleteScheduleSession(rs)
                } catch (ex) {
                    console.log(ex);
                    $('#modal-manage .modal-body').alert(true, 'danger', ex);
                }
            });
        });

        $('.btn-schedule-delete').off('click touchend').on('click touchend', function (event) {
            var message = 'Toàn bộ học viên có đăng ký các buổi thuộc lịch học này sẽ được hoàn lại buổi học sau khi xóa.<br />Bạn có chắc chắn muốn xóa lịch học này?';
            _self.showAlert(message, async function (event) {
                try {
                    var rs = await ApiService.del('/api/schedule/delete/' + _self.selectedSchedule.id);
                    handlePostDeleteScheduleSession(rs);
                } catch (ex) {
                    console.log(ex);
                    $('#modal-manage .modal-body').alert(true, 'danger', ex);
                }                
            })
        });
    }

    function handlePostDeleteScheduleSession(rs) {
        var $modalBody = $('#modal-manage .modal-body');
        if (rs) {
            var isClosedModal = false;
            if (rs.success && rs.isUserGetSessionBack) {
                $modalBody.alert(true, 'warning', 'Xóa thành công. Vui lòng thông báo học viên đã được hoàn lại buổi đã đăng ký sau khi xóa.');
            } else {
                $modalBody.alert(true, 'success', 'Xóa thành công');
                isClosedModal = true;
            }
            _self.selectedSchedule = null;
            _self.selectedScheduleDetails = null;
            if (isClosedModal) {
                setTimeout(function () { $('#modal-manage').modal('hide'); }, 1500);
            }
        } else {
            $modalBody.alert(true, 'danger', 'Xóa không thành công');
        }
    }

    function renderSessionInfoGroup(label, value) {
        return $('<div>', { class: 'session-info-group' })
            .append($('<p>', { class: 'session-info-label' }).text(label))
            .append($('<p>', { class: 'session-info' }).text(value));
    }

    function renderRegistrationList(registrations) {
        let $registrationList = registrations.reduce((jObject, registration, index, arr) => {
            jObject = jObject.add(renderRegistrationRow(registration, index, registrations));
            return jObject;
        }, $());

        $('.session-registration-list').empty().append($registrationList);
    }

    function renderRegistrationRow(registration, index, registrations) {
        let tdNo = $('<td>').html(index + 1);
        let tdName = $('<td>').html(registration.user.fullName);
        let confirmBtn = $('<button>', { class: 'btn btn-success btn-xs btn-label' })
            .html('Đến lớp')
            .on('click', { registration }, handleConfirmRegistration);

        let cancelBtn = $('<button>', { class: 'btn btn-danger btn-xs btn-label' })
            .html('Hủy')
            .on('click', { registration, registrations }, handleCancelRegistration);

        if (registration.status && registration.status.value === 1) {
            confirmBtn
                .prop('disabled', true)
                .off('click')
                .prepend($('<i>', { class: 'fa fa-check' }))

            cancelBtn.prop('disabled', true).hide();
        }

        let tdActionBtns = $('<td>').append(confirmBtn).append(cancelBtn);

        return $('<tr>').append(tdNo).append(tdName).append(tdActionBtns)
    }

    async function handleConfirmRegistration(event) {
        let $confirmBtn = $(event.target);
        $confirmBtn
            .prop('disabled', true)
            .empty()
            .append($('<li>', { class: 'fa fa-circle-o-notch fa-spin' }));

        let $cancelBtn = $confirmBtn.closest('td').find('.btn-danger');
        $cancelBtn.prop('disabled', true).hide();

        let { registration } = event.data;
        try {
            await _self.confirmRegistration(registration.id);

            registration.status.value = 1;
            registration.status.name = 'Đến lớp';

            $confirmBtn
                .empty()
                .append($('<i>', { class: 'fa fa-check' }))
                .append('Đến lớp')
                .off('click');

        } catch (ex) {
            console.log(ex);
            $cancelBtn.prop('disabled', false).show();
            $confirmBtn
                .empty()
                .append('Đăng ký')
                .prop('disabled', false);

            $cancelBtn.closest('td').append(ex.responseJSON ? ex.responseJSON.ExceptionMessage : 'Đã có lỗi xảy ra');
        }
    }

    async function handleCancelRegistration(event) {
        let $cancelBtn = $(event.target);
        $cancelBtn
            .prop('disabled', true)
            .empty()
            .append($('<i>', { class: 'fa fa-circle-o-notch fa-spin' }));

        let $confirmBtn = $cancelBtn.closest('td').find('.btn-success');
        $confirmBtn.prop('disabled', true).hide();

        let { registration, registrations } = event.data;
        try {
            await _self.unregisterSchedule(registration.id);

            let index = registrations.findIndex(r => r.id === registration.id);
            registrations.splice(index, 1);
            $cancelBtn.closest('tr').remove();
        } catch (ex) {
            console.log(ex);
            $confirmBtn.prop('disabled', false).show();
            $cancelBtn
                .empty()
                .append('Hủy')
                .prop('disabled', false);

            $cancelBtn.closest('td').append(ex.responseJSON ? ex.responseJSON.ExceptionMessage : 'Đã có lỗi xảy ra');
        }
    }
}