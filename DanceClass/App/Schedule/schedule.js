let m_currentDaysOfWeek = [];
let m_scheduleDetails = [];

let m_user = null;

$(async function () {
    registerEvent();
    initWeek();
    await initUser();
    renderUserRemainingSessions();
    await renderCalendar();
});

async function initUser() {
    m_user = await userService.getCurrentUser();
}

function initWeek(currentDate) {
    currentDate = moment(currentDate || new Date());

    var weekStart = currentDate.clone().startOf('isoWeek');

    for (var i = 0; i <= 6; i++) {
        m_currentDaysOfWeek.push(moment(weekStart).add(i, 'days'));
    }
}

function renderUserRemainingSessions() {
    if (m_user && m_user.activePackage) {
        let remainingSessions = m_user.activePackage.remainingSessions;
        $('.calendar-remaining-sessions').html(formatRemainingSessions(remainingSessions));
    } else if (m_user && m_user.roleNames.includes("Member")) {
        $('.calendar-remaining-sessions').html(0);
    } else {
        $('.calendar-remaining-sessions-area').hide();
    }
}

function formatRemainingSessions(remainingSessions) {
    return remainingSessions < 10 && remainingSessions !== 0 ? "0" + remainingSessions : remainingSessions;
}

function registerEvent() {
    $('.btn-calendar-navigate').click(e => {
        console.log(e);
        if (e.target.id === 'prev') {
            m_currentDaysOfWeek = m_currentDaysOfWeek.map(day => day.subtract(7, 'days'));
        } else if (e.target.id === 'next') {
            m_currentDaysOfWeek = m_currentDaysOfWeek.map(day => day.add(7, 'days'));
        }
        renderCalendar();
    })

    $('#modal-register').on('show.bs.modal', function (event) {
        let div = $(event.relatedTarget);
        const id = div.data('id') // Extract info from data-* attributes
        // If necessary, you could initiate an AJAX request here (and then do the updating in a callback).
        // Update the modal's content. We'll use jQuery here, but you could use a data binding library or other methods instead.

        let $modal = $(this);
        let modalTitle = $modal.find('.modal-title');
        let modalBodyInfo = $modal.find('.modal-body-info');
        let btnAction = $modal.find('#btn-action');

        modalBodyInfo.empty();
        btnAction.off('click');

        $('.modal-body-message').empty();
        $('.modal-body-remaining-sessions').html(m_user && m_user.activePackage ? m_user.activePackage.remainingSessions : 0);

        const scheduleDetail = m_scheduleDetails.find(x => x.id === parseInt(id));
        if (scheduleDetail.isCurrentUserRegistered) {
            modalTitle.text('Hủy đăng ký');
            modalBodyInfo.text('Bạn có chắc muốn hủy đăng ký?');
            btnAction.html('Hủy');
            btnAction.on('click', async function (e) {
                handleUnregisterScheduleClick(scheduleDetail.currentUserRegistration.id, $modal)
            });
        } else {
            modalTitle.text('Bạn có chắc chắn muốn đăng ký?');

            $('<p>').text('Lớp: ' + scheduleDetail.schedule.class.name).appendTo(modalBodyInfo);
            $('<p>').text('Bài: ' + scheduleDetail.schedule.song).appendTo(modalBodyInfo).appendTo(modalBodyInfo);
            $('<p>').text('Thời gian: ' + capitalizeFirstLetter(moment(scheduleDetail.date).locale('vi').format('dddd D/M'))).appendTo(modalBodyInfo);
            $('<p>').text('Địa điểm: ' + scheduleDetail.schedule.branch).appendTo(modalBodyInfo);

            btnAction.html('Đăng ký');
            btnAction.on('click', async function (e) {
                await handleRegisterScheduleClick(scheduleDetail, $modal)
            });
        }
    })

    $('#modal-manage').on('shown.bs.modal', function (event) {

        let div = $(event.relatedTarget);
        const id = div.data('id');

        const scheduleDetail = m_scheduleDetails.find(x => x.id === parseInt(id));
        const { schedule, registrations, date, totalRegistered, sessionNo } = scheduleDetail;

        let $modal = $(this);

        let [hour, minute, ...rest] = schedule.startTime.split(':');
        let timeStart = capitalizeFirstLetter(moment(date).hours(parseInt(hour)).minute(parseInt(minute)).locale('vi').format('dddd D/M HH:mm'));
        $modal.find('.modal-title').text(schedule.class.name + ' - ' + timeStart);

        const { song, branch, sessions: totalSessions } = schedule;
        $('.session-general-info')
            .empty()
            .append(renderSessionInfoGroup('Bài múa', song))
            .append(renderSessionInfoGroup('Buổi', sessionNo + ' / ' + totalSessions))
            .append(renderSessionInfoGroup('Thời gian', timeStart))
            .append(renderSessionInfoGroup('Địa điểm', branch))
            .append(renderSessionInfoGroup('Số học viên đăng ký', totalRegistered + ' / 20'));

        let $registrationList = registrations.reduce((jObject, registration, index) => {
            jObject = jObject.add(renderRegistrationRow(registration, index));
            return jObject;
        }, $());

        $('.session-registration-list').empty().append($registrationList);

        $('#modal-manage').off('hide.bs.hide.bs.modal').on('hide.bs.hide.bs.modal', async function (event) {
            if (registrations.some(r => r.isModified)) {
                await renderCalendar();
            }
        })
    });

    $('.session-registrations').slimscroll({
        distance: '5px'
    });
}

function renderSessionInfoGroup(label, value) {
    return $('<div>', { class: 'session-info-group' })
        .append($('<p>', { class: 'session-info-label' }).text(label))
        .append($('<p>', { class: 'session-info' }).text(value));
}

function renderRegistrationRow(registration, index) {
    let tdNo = $('<td>').html(index + 1);
    let tdName = $('<td>').html(registration.user.fullName);
    let confirmBtn = $('<button>', { class: 'btn btn-success btn-xs btn-label' })
        .html('Đến lớp')
        .on('click', { registration }, handleConfirmRegistration);

    let cancelBtn = $('<button>', { class: 'btn btn-danger btn-xs btn-label' })
        .html('Hủy')
        .on('click', { registration }, handleCancelRegistration);

    if (registration.status && registration.status.value === 1) {
        confirmBtn
            .prop('disabled', true)
            .off('click')
            .prepend($('<i>', { class: 'fa fa-check' }))

        cancelBtn.prop('disabled', true).hide();
    } else if (registration.status && registration.status.value === 2) {
        confirmBtn.prop('disabled', true).hide();

        cancelBtn
            .html('Nghỉ')
            .off('click')
            .prop('disabled', true)
            .prepend($('<i>', { class: 'fa fa-times' }))
    }

    let tdActionBtns = $('<td>').append(confirmBtn).append(cancelBtn);

    return $('<tr>').append(tdNo).append(tdName).append(tdActionBtns)
}

async function handleConfirmRegistration(event) {
    try {
        let $confirmBtn = $(event.target);
        $confirmBtn
            .prop('disabled', true)
            .empty()
            .append($('<li>', { class: 'fa fa-circle-o-notch fa-spin' }));

        let $cancelBtn = $confirmBtn.closest('td').find('.btn-danger');
        $cancelBtn.prop('disabled', true).hide();

        let { registration } = event.data;
        await confirmRegistration(event.data.registration.id);

        registration.status.value = 1;
        registration.status.name = 'Đến lớp';

        $confirmBtn
            .empty()
            .append($('<i>', { class: 'fa fa-check' }))
            .append('Đến lớp')
            .off('click');

    } catch (ex) {
        console.log(ex);
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

    let { registration } = event.data;
    try {
        await unregisterSchedule(registration.id);

        registration.status.value = 2;
        registration.status.name = 'Nghỉ';
        registration.isModified = true;

        $cancelBtn
            .empty()
            .append($('<i>', { class: 'fa fa-times' }))
            .append('Nghỉ')
            .off('click');
    } catch (ex) {
        console.log(ex);
        $confirmBtn.prop('disabled', false).show();
        $cancelBtn
            .empty()
            .append('Hủy')
            .prop('disabled', false);

        $cancelBtn.closest('td').append(ex.responseJSON.ExceptionMessage);
    }

}

async function handleUnregisterScheduleClick(registrationId, $modal) {
    try {
        await unregisterSchedule(registrationId, true);
        $('.modal-body-message').css('color', 'green').text('Hủy thành công');
        setTimeout(async function () {
            $modal.modal('hide');
            updateUserRemainingSessions(false);
            await renderSchedule();
        }, 1000);
    } catch (ex) {
        console.log(ex);
        $('.modal-body-message').css('color', 'red').text(ex.responseJSON.ExceptionMessage);
    }
}

async function handleRegisterScheduleClick(scheduleDetail, $modal) {
    try {
        const rs = await registerSchedule(scheduleDetail.id);
        if (rs && rs.registration) {
            $('.modal-body-message').css('color', 'green').text('Đăng ký thành công');
            setTimeout(async function () {
                $modal.modal('hide');
                updateUserRemainingSessions(true);
                await renderSchedule();
            }, 1000);
        }
    } catch (ex) {
        console.log(ex);
        $('.modal-body-message').css('color', 'red').text(ex.responseJSON.ExceptionMessage);
    }
}

function updateUserRemainingSessions(isRegistration) {
    if (m_user && m_user.activePackage) {
        if (isRegistration) {
            m_user.activePackage.remainingSessions--;
        }
        else {
            m_user.activePackage.remainingSessions++;
        }
        renderUserRemainingSessions();
    }
}

async function renderCalendar() {
    // render days of current week header
    renderDaysOfWeek();
    // render schedule data
    await renderSchedule();
}

function renderDaysOfWeek() {
    $('.calendar-control-current-week').html(
        `${m_currentDaysOfWeek[0].locale('vi').format('DD/MM')} - ${m_currentDaysOfWeek[6].locale('vi').format('DD/MM')}`
    );
    $('#calendarHead').empty();

    var weekTableData = m_currentDaysOfWeek
        .map(day => {
            var dayLocale = capitalizeFirstLetter(day.locale('vi').format('dddd D/M'));
            return `<th>${dayLocale}</th>`;
        })
        .join('');

    var weekTableHead = `<tr><th></th>${weekTableData}</th>`;
    $('#calendarHead').append(weekTableHead);
}

async function renderSchedule() {
    $('#calendarBody').empty();

    const eventsByTime = await getSchedule();

    eventsByTime.forEach((eventGroup) => {
        let tr = $('<tr></tr>');
        let tdTime = $('<td></td>')
            .html(
                `${formatHhMm(eventGroup.hours, eventGroup.minutes)} <br />- ${formatHhMm(eventGroup.hours + 1, eventGroup.minutes)}`
            )
            .appendTo(tr);

        m_currentDaysOfWeek.forEach(date => {
            let tdEvents = $('<td></td>');
            let events = eventGroup.events.filter(e => (new Date(e.date)).getDay() === date.day())
            if (events && events.length > 0) {
                events
                    .reduce((jObject, event) => {
                        jObject = jObject.add(renderEventTag(event));
                        return jObject;
                    }, $())
                    .appendTo(tdEvents);
            } else {
                if (userService.isAdmin()) {
                    tdEvents
                        .on('click', function (event) { console.log(event) })
                        .hover(function () {
                            $(this)
                                .addClass('cell-admin-add-schedule')
                                .html('Tạo<br />lịch học')
                        }, function () {
                            $(this)
                                .removeClass('cell-admin-add-schedule')
                                .html('')
                        })
                }
            }

            tdEvents.appendTo(tr);
        });

        tr.appendTo('#calendarBody');
    });
}

function renderEventTag(event) {
    const { schedule } = event;

    var div = $('<div></div>', {
        'class': 'mistake-event mistake-event-' + schedule.branch.toLowerCase(),
        'data-toggle': 'modal',
        'data-target': userService.isAdmin() ? '#modal-manage' : userService.isMember() ? '#modal-register' : '',
        'data-id': event.id
    });

    div.hover(function () { $(this).css('cursor', 'pointer') });

    $('<p></p>', {
        class: 'mistake-event-class',
    })
        .text(schedule.class.name)
        .appendTo(div);

    $('<p></p>', {
        class: 'mistake-event-song',
    })
        .text(schedule.song)
        .appendTo(div);

    var infoDiv = $('<div></div>', {
        class: 'mistake-event-info-container'
    });

    $('<span></span>', {
        class: 'mistake-event-info',
    })
        .text(`${event.sessionNo}/${schedule.sessions}`)
        .appendTo(infoDiv);

    $('<span></span>', {
        class: 'mistake-event-info',
    })
        .text(`${event.totalRegistered}/20`)
        .appendTo(infoDiv);

    $('<span></span>', {
        class: 'mistake-event-info',
    })
        .text(schedule.branch)
        .appendTo(infoDiv);

    infoDiv.appendTo(div);

    if (userService.isMember()) {
        let isRegistered = event.isCurrentUserRegistered && event.currentUserRegistration.status.value === 0;
        let action = isRegistered ? 'Hủy đăng ký' : 'Đăng ký';
        let btnClass = isRegistered ? 'btn-danger' : 'btn-success';
        $('<div>', { class: 'mistake-event-action' })
            .append($('<button>', { class: 'btn btn-xs ' + btnClass }).html(action))
            .appendTo(div);
    }

    return div;
}

async function getSchedule() {
    try {
        const data = await ajaxSchedule(m_currentDaysOfWeek[0].toDate());
        console.log(data);

        if (data && data.scheduleDetails) {
            m_scheduleDetails = data.scheduleDetails;
            const eventsByTime = m_scheduleDetails.reduce((result, ele) => {
                let [hours, minutes, ...rest] = ele.schedule.startTime.split(":");

                hours = parseInt(hours);
                minutes = parseInt(minutes);

                var eventGroup = result.find(
                    (r) => r.hours === hours && r.minutes === minutes
                );
                if (!eventGroup) {
                    result.push({
                        hours,
                        minutes,
                        events: [ele],
                    });
                } else {
                    eventGroup.events.push(ele);
                }

                return result;
            }, []).sort(compareTime);

            console.log(eventsByTime);
            return eventsByTime;
        }
    } catch (err) {
        console.log(err);
        return [];
    }
}

function formatHhMm(hours, minutes) {
    const formatLeadingZero = (timePart) =>
        `${
        timePart.toString().length === 1
            ? '0' + timePart
            : timePart.toString()
        }`;
    return `${formatLeadingZero(hours)}:${formatLeadingZero(minutes)}`;
}

function compareTime(t1, t2) {
    const { hours: h1, minutes: m1 } = t1;
    const { hours: h2, minutes: m2 } = t2;
    return h1 > h2 ? 1 : h1 < h2 ? -1 : m1 > m2 ? 1 : m1 < m2 ? -1 : 0;
}

function capitalizeFirstLetter(string) {
    return string.charAt(0).toUpperCase() + string.slice(1);
}

async function confirmRegistration(registrationId) {
    return $.ajax({
        method: 'PUT',
        async: true,
        url: '/api/registration/confirmAttendance/' + registrationId
    });
}

async function registerSchedule(scheduleDetailId) {
    return $.ajax({
        method: 'POST',
        data: {
            registration: { scheduleDetailId }
        },
        async: true,
        url: '/api/registration/create',
    });
}

async function unregisterSchedule(id, isDelete) {
    return $.ajax({
        method: 'POST',
        data: {
            registrationId: id,
            isDelete
        },
        async: true,
        url: '/api/registration/cancel',
    });
}

async function ajaxSchedule(weekStart) {
    return $.ajax({
        method: 'POST',
        data: { start: weekStart.toISOString() },
        async: true,
        url: '/api/schedule/getdetail',
    });
}